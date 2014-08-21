using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ReactGraph.Api;
using ReactGraph.Construction;
using ReactGraph.Graph;
using ReactGraph.Instrumentation;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class DependencyEngine
    {
        private readonly DirectedGraph<INodeInfo> graph;
        private readonly NodeRepository nodeRepository;
        private readonly EngineInstrumenter engineInstrumenter;
        private bool isExecuting;

        public DependencyEngine()
        {
            graph = new DirectedGraph<INodeInfo>();
            nodeRepository = new NodeRepository(this);
            engineInstrumenter = new EngineInstrumenter();
        }

        public void AddInstrumentation(IEngineInstrumentation engineInstrumentation)
        {
            engineInstrumenter.Add(engineInstrumentation);
        }

        public void RemoveInstrumentation(IEngineInstrumentation engineInstrumentation)
        {
            engineInstrumenter.Remove(engineInstrumentation);
        }

        public DirectedGraph<INodeMetadata> GetGraphSnapshot()
        {
            return graph.Clone(vertex => (INodeMetadata)new NodeMetadata(vertex.Data.Type, vertex.Data.ToString(), vertex.Id));
        } 

        public bool ValueHasChanged(object instance, string key)
        {
            if (!nodeRepository.Contains(instance, key) || isExecuting) return false;

            var node = nodeRepository.Get(instance, key);

            try
            {
                isExecuting = true;
                var orderToReeval = new Queue<Vertex<INodeInfo>>(graph.TopologicalSort(node));
                var firstVertex = orderToReeval.Dequeue();
                engineInstrumenter.DependecyWalkStarted(key, firstVertex.Id);
                node.ValueChanged();
                NotificationStratgegyValueUpdate(firstVertex);
                while (orderToReeval.Count > 0)
                {
                    var vertex = orderToReeval.Dequeue();
                    var reevaluationResult = vertex.Data.Reevaluate();

                    switch (reevaluationResult)
                    {
                        case ReevaluationResult.NoChange:
                            engineInstrumenter.NodeEvaluated(vertex.Data.ToString(), vertex.Id, reevaluationResult);
                            break;
                        case ReevaluationResult.Error:
                            var nodesRelatedToError = graph.TopologicalSort(vertex.Data).ToDictionary(k => k.Data);

                            foreach (var vertex1 in nodesRelatedToError)
                            {
                                engineInstrumenter.NodeEvaluated(vertex1.Value.Data.ToString(), vertex1.Value.Id, ReevaluationResult.Error);
                            }

                            var newListToProcess = orderToReeval
                                .Where(remaining => !nodesRelatedToError.ContainsKey(remaining.Data))
                                .ToArray();
                            orderToReeval = new Queue<Vertex<INodeInfo>>(newListToProcess);
                            break;
                        case ReevaluationResult.Changed:
                            engineInstrumenter.NodeEvaluated(vertex.Data.ToString(), vertex.Id, reevaluationResult);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    NotificationStratgegyValueUpdate(vertex);
                }
            }
            finally
            {
                isExecuting = false;
                engineInstrumenter.DependencyWalkEnded();
            }

            return true;
        }

        static void NotificationStratgegyValueUpdate(Vertex<INodeInfo> firstVertex)
        {
            foreach (var successor in firstVertex.Successors)
            {
                firstVertex.Data.UpdateSubscriptions(successor.Source.Data.GetValue());
            }
        }

        public void CheckCycles()
        {
            var cycles = graph.DetectCyles().ToList();
            if (cycles.Count == 0) return;

            var sb = new StringBuilder();
            sb.AppendFormat("{0} cycles found:", cycles.Count).AppendLine();
            foreach (var cycle in cycles)
            {
                var nodes = cycle.Reverse().ToList();
                nodes.Add(nodes.First());

                sb.AppendLine(string.Join(" --> ", nodes.Select(v => v.Data.ToString().Replace("() => ", string.Empty))));
            }

            throw new CycleDetectedException(sb.ToString().Trim());
        }

        public void AddExpression<T>(ISourceDefinition<T> source, ITargetDefinition<T> target, Action<Exception> onError)
        {
            // Repository should have getsource and get target?
            var sourceNode = nodeRepository.Contains(source.Root, source.Path)
                ? (INodeInfo<T>)nodeRepository.Get(source.Root, source.Path)
                : CreateSourceNode(source);
            var targetNode = nodeRepository.Contains(target.Root, target.Path)
                ? (IWritableNodeInfo<T>)nodeRepository.Get(target.Root, target.Path)
                : CreateTargetNode(target);

            targetNode.SetSource(sourceNode, onError);
            graph.AddEdge(sourceNode, targetNode, source.NodeName, target.NodeName);

            AddSourcePathExpressions(source);
        }

        void AddSourcePathExpressions(ISourceDefinition sourceDefinition) 
        {
            var sourceNode = nodeRepository.Contains(sourceDefinition.Root, sourceDefinition.Path)
                ? nodeRepository.Get(sourceDefinition.Root, sourceDefinition.Path)
                : CreateSourceNode(sourceDefinition);

            foreach (var sourcePath in sourceDefinition.SourcePaths)
            {
                var pathNode = nodeRepository.Contains(sourcePath.Root, sourcePath.Path)
                ? nodeRepository.Get(sourcePath.Root, sourcePath.Path)
                : CreateSourceNode(sourcePath);

                graph.AddEdge(pathNode, sourceNode, sourcePath.NodeName, sourceDefinition.NodeName);
            }
        }

        IWritableNodeInfo<T> CreateTargetNode<T>(ITargetDefinition<T> target)
        {
            switch (target.NodeType)
            {
                case NodeType.Formula:
                    return new WritableNodeInfo<T>(target.ParentInstance, );
                    break;
                case NodeType.Member:
                    break;
                case NodeType.Action:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        INodeInfo<T> CreateSourceNode<T>(ISourceDefinition<T> source)
        {

        }

        INodeInfo CreateSourceNode(ISourceDefinition source)
        {

        }
    }

    public interface ITargetDefinition<in T> : IDefinitionIdentity
    {
        Action<T> CreateSetValueDelegate();
    }

    public interface ISourceDefinition<out T> : ISourceDefinition
    {
        Func<T> CreateGetValueDelegate();
    }

    public interface ISourceDefinition : IDefinitionIdentity
    {
        ISourceDefinition[] SourcePaths { get; }
    }

    public interface IDefinitionIdentity
    {
        object Root { get; }

        object ParentInstance { get; }

        string Path { get; }

        string NodeName { get; }

        NodeType NodeType { get; }
    }

    public static class RegistrationExtensions
    {
        public static WhenFormulaChangesBuilder<TProp> When<TProp>(this DependencyEngine engine, Expression<Func<TProp>> sourceFunction, string expressionId = null)
        {
            return new WhenFormulaChangesBuilder<TProp>(sourceFunction, expressionId, engine);
        }

        public static AssignPropertyBuilder<TProp> Assign<TProp>(this DependencyEngine engine, Expression<Func<TProp>> targetMemberExpression, string expressionId = null)
        {
            return new AssignPropertyBuilder<TProp>(engine, targetMemberExpression, expressionId);
        }
    }

    public class AssignPropertyBuilder<T> : BuilderBase
    {
        readonly MemberDefinition<T> targetMemberDefinition;
        readonly DependencyEngine engine;

        public AssignPropertyBuilder(DependencyEngine engine, Expression<Func<T>> targetMemberExpression, string nodeId)
        {
            var memberExpression = targetMemberExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression must be a member (field/property) accessor, for example foo.Bar", "targetMemberExpression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            var fieldInfo = memberExpression.Member as FieldInfo;

            if (propertyInfo == null && fieldInfo == null)
                throw new ArgumentException("Only fields and properties are supported", "targetMemberExpression");
            if (propertyInfo != null && !propertyInfo.CanWrite)
                throw new ArgumentException("Property must be writable", "targetMemberExpression");
            if (fieldInfo != null && fieldInfo.IsInitOnly)
                throw new ArgumentException("Field cannot be read-only", "targetMemberExpression");

            this.engine = engine;
            targetMemberDefinition = CreateMemberDefinition(targetMemberExpression, nodeId);

        }

        public WhenFormulaChangesBuilder<T> From(Expression<Func<T>> sourceExpression, Action<Exception> onError, string nodeId = null)
        {
            ISourceDefinition<T> sourceDefinition;
            if (IsWritable(sourceExpression))
            {
                sourceDefinition = CreateMemberDefinition(sourceExpression, nodeId);
            }
            else
            {
                sourceDefinition = new FormulaDefinition<T>(sourceExpression, nodeId);
            }

            engine.AddExpression(sourceDefinition, targetMemberDefinition);
            return new WhenFormulaChangesBuilder<T>(sourceExpression, nodeId, engine);
        }
    }

    public class FormulaDefinition<T> : ExpressionDefinition<T>, ISourceDefinition<T>
    {
        readonly Expression<Func<T>> sourceExpression;

        public FormulaDefinition(Expression<Func<T>> sourceExpression, string nodeId) : base(sourceExpression, NodeType.Formula, nodeId)
        {
            this.sourceExpression = sourceExpression;
        }

        public Func<T> CreateGetValueDelegate()
        {
            return sourceExpression.Compile();
        }
    }

    public class ExpressionDefinition<T> : IDefinitionIdentity
    {
        public ExpressionDefinition(Expression expression, NodeType nodeType, string nodeName)
        {
            NodeType = nodeType;
            NodeName = nodeName;
            Path = ExpressionStringBuilder.ToString(expression);
            Root = 
        }

        public object Root { get; private set; }
        public string Path { get; private set; }
        public string NodeName { get; private set; }
        public NodeType NodeType { get; private set; }
    }

    public class BuilderBase
    {
        protected bool IsWritable<T>(Expression<Func<T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) return false;
            var propertyInfo = memberExpression.Member as PropertyInfo;
            var fieldInfo = memberExpression.Member as FieldInfo;

            if (propertyInfo == null && fieldInfo == null) return false;
            if (propertyInfo != null && !propertyInfo.CanWrite) return false;
            if (fieldInfo != null && fieldInfo.IsInitOnly) return false;

            return true;
        }

        protected MemberDefinition<T> CreateMemberDefinition<T>(Expression<Func<T>> expression, string nodeId)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var targetAssignmentLambda = Expression.Lambda<Action<T>>(Expression.Assign(expression.Body, parameterExpression), parameterExpression);

            return new MemberDefinition<T>(expression, targetAssignmentLambda, nodeId);
        }
    }

    public class MemberDefinition<T> : ExpressionDefinition<T>, ISourceDefinition<T>, ITargetDefinition<T>
    {
        readonly Expression<Func<T>> targetMemberExpression;
        readonly Expression<Action<T>> assignmentLambda;

        public MemberDefinition(Expression<Func<T>> targetMemberExpression, Expression<Action<T>> assignmentLambda, string targetMemberId) : base(targetMemberExpression, NodeType.Member, targetMemberId)
        {
            this.targetMemberExpression = targetMemberExpression;
            this.assignmentLambda = assignmentLambda;
        }

        public Func<T> CreateGetValueDelegate()
        {
            return targetMemberExpression.Compile();
        }

        public Action<T> CreateSetValueDelegate()
        {
            return assignmentLambda.Compile();
        }
    }

    public class WhenFormulaChangesBuilder<T> : BuilderBase
    {
        readonly ISourceDefinition<T> sourceDefinition;
        readonly DependencyEngine dependencyEngine;

        public WhenFormulaChangesBuilder(Expression<Func<T>> sourceFunction, string nodeId, DependencyEngine dependencyEngine)
        {
            this.dependencyEngine = dependencyEngine;
            if (IsWritable(sourceFunction))
                sourceDefinition = CreateMemberDefinition(sourceFunction, nodeId);
            else
                sourceDefinition = new FormulaDefinition<T>(sourceFunction, nodeId);
        }

        public void Do(Expression<Action<T>> action, Action<Exception> onError, string actionId = null)
        {
            dependencyEngine.AddExpression(sourceDefinition, new ActionDefinition<T>(action, actionId));
        }
    }

    public class ActionDefinition<T> : ExpressionDefinition<T>, ITargetDefinition<T>
    {
        readonly Expression<Action<T>> expression;

        public ActionDefinition(Expression<Action<T>> expression, string nodeName)
            : base(expression, NodeType.Action, nodeName)
        {
            this.expression = expression;
        }

        public Action<T> CreateSetValueDelegate()
        {
            return expression.Compile();
        }
    }
}