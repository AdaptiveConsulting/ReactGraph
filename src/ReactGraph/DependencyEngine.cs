using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
            nodeRepository = new NodeRepository();
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
                }
            }
            finally
            {
                isExecuting = false;
                engineInstrumenter.DependencyWalkEnded();
            }

            return true;
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
                ? (IValueSource<T>)nodeRepository.Get(source.Root, source.Path)
                : CreateSourceNode(source);
            var targetNode = nodeRepository.Contains(target.Root, target.Path)
                ? (ITakeValue<T>)nodeRepository.Get(target.Root, target.Path)
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
                AddSourcePathExpressions(sourcePath);
            }
        }

        ITakeValue<T> CreateTargetNode<T>(ITargetDefinition<T> target)
        {
            switch (target.NodeType)
            {
                case NodeType.Formula:
                    throw new ArgumentException("Formula nodes cannot be a value target");
                case NodeType.Member:
                    // TODO Figure out how to remove cast
                    var getValueDelegate = ((ISourceDefinition<T>)target).CreateGetValueDelegate();
                    var setValueDelegate = target.CreateSetValueDelegate();
                    return new ReadWriteNode<T>(getValueDelegate, setValueDelegate, target.Path);
                case NodeType.Action:
                    return new WriteOnlyNode<T>(target.CreateSetValueDelegate(), target.Path);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        IValueSource<T> CreateSourceNode<T>(ISourceDefinition<T> source)
        {
            switch (source.NodeType)
            {
                case NodeType.Formula:
                case NodeType.Action:
                    return new ReadOnlyNodeInfo<T>(source.CreateGetValueDelegate(), source.Path);
                case NodeType.Member:
                    // TODO Figure out how to remove cast
                    var getValueDelegate = source.CreateGetValueDelegate();
                    var setValueDelegate = ((ITargetDefinition<T>)source).CreateSetValueDelegate();
                    return new ReadWriteNode<T>(getValueDelegate, setValueDelegate, source.Path);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        readonly MethodInfo createSourceInfo = typeof (DependencyEngine).GetMethods(BindingFlags.Instance |
                                                                           BindingFlags.NonPublic)
            .Single(m => m.Name == "CreateSourceNode" && m.IsGenericMethodDefinition);

        INodeInfo CreateSourceNode(ISourceDefinition source)
        {
            var method = createSourceInfo.MakeGenericMethod(source.SourceType);
            return (INodeInfo)method.Invoke(this, new object[] { source });
        }
    }
}