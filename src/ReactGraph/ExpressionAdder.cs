using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReactGraph.Graph;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    class ExpressionAdder
    {
        readonly MethodInfo createSourceInfo = typeof(ExpressionAdder).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(m => m.Name == "CreateSourceNode" && m.IsGenericMethodDefinition);
        readonly Dictionary<IDefinitionIdentity, INodeInfo> definitionToNodeLookup;
        readonly DirectedGraph<INodeInfo> graph;
        readonly NodeRepository nodeRepository;

        public ExpressionAdder(DirectedGraph<INodeInfo> graph, NodeRepository nodeRepository)
        {
            var definitionComparer = new DefinitionComparer();
            definitionToNodeLookup = new Dictionary<IDefinitionIdentity, INodeInfo>(definitionComparer);
            this.graph = graph;
            this.nodeRepository = nodeRepository;
        }

        public void Add<T>(ISourceDefinition<T> source, ITargetDefinition<T> target, Action<Exception> onError)
        {
            var sourceNode = GetSourceNode(source, onError);
            var targetNode = GetTargetNode(target, onError);
            sourceNode.VisualisationInfo.IsDirectlyReferenced = true;
            targetNode.VisualisationInfo.IsDirectlyReferenced = true;

            targetNode.SetSource(sourceNode);
            var valueSource = (targetNode) as IValueSource<T>;
            if (valueSource != null)
            {
                var initialValue = valueSource.GetValue();
                sourceNode.SetTarget(targetNode, initialValue);
            }

            graph.AddEdge(sourceNode, targetNode, source.NodeName, target.NodeName);
            AddSourcePathExpressions(source, sourceNode, onError);
            var targetAsSource = target as ISourceDefinition<T>;
            if (targetAsSource != null)
                AddSourcePathExpressions(targetAsSource, (IValueSource) targetNode, onError);
        }

        void AddSourcePathExpressions(ISourceDefinition sourceDefinition, IValueSource sourceNode, Action<Exception> onError)
        {
            foreach (var sourcePath in sourceDefinition.SourcePaths)
            {
                IValueSource pathNode;
                if (definitionToNodeLookup.ContainsKey(sourcePath))
                    pathNode = (IValueSource) definitionToNodeLookup[sourcePath];
                else
                {
                    pathNode = CreateSourceNode(sourcePath, onError);
                    definitionToNodeLookup.Add(sourcePath, pathNode);
                }

                pathNode.VisualisationInfo.IsDirectlyReferenced = false;
                graph.AddEdge(pathNode, sourceNode, sourcePath.NodeName, sourceDefinition.NodeName);
                AddSourcePathExpressions(sourcePath, pathNode, onError);
            }
        }

        IValueSource<T> GetSourceNode<T>(ISourceDefinition<T> source, Action<Exception> onError)
        {
            IValueSource<T> sourceNode;
            if (definitionToNodeLookup.ContainsKey(source))
            {
                sourceNode = (IValueSource<T>)definitionToNodeLookup[source];
                sourceNode.TrackChanges();
            }
            else
            {
                sourceNode = CreateSourceNode(source, onError);
                definitionToNodeLookup.Add(source, sourceNode);
            }

            return sourceNode;
        }

        ITakeValue<T> GetTargetNode<T>(ITargetDefinition<T> target, Action<Exception> onError)
        {
            ITakeValue<T> targetNode;
            if (definitionToNodeLookup.ContainsKey(target))
            {
                targetNode = (ITakeValue<T>)definitionToNodeLookup[target];
            }
            else
            {
                targetNode = CreateTargetNode(target, onError);
                definitionToNodeLookup.Add(target, targetNode);
            }

            return targetNode;
        }

        ITakeValue<T> CreateTargetNode<T>(ITargetDefinition<T> target, Action<Exception> onError)
        {
            // TODO this smells: generally when you switch on a type like that it's that you should be doing a polymorphic call somewhere else. 
            // shouldn't it be like that instead:
            // - an ActionDefinition know how to create a WriteOnlyNode (which could be renamed ActionNode btw)
            // - a MemberDefinition know how to create ReadWriteNode (which could be renamed MemberNode)
            // - a FormulaDefinition know how to create ReadOnlyNode (which could be renamed FormulaNode)

            // Jake: If we do that the internals need to be opened up and the public API will start bleeding implementation details.
            // I think it is better to have the switch?
            switch (target.NodeType)
            {
                case NodeType.Formula:
                    throw new ArgumentException("Formula nodes cannot be a value target");
                case NodeType.Member:
                    var memberDefinition = (MemberDefinition<T>)target;
                    var getValueDelegate = memberDefinition.CreateGetValueDelegate();
                    var setValueDelegate = memberDefinition.CreateSetValueDelegate();
                    if (!memberDefinition.IsWritable)
                    {
                        throw new InvalidOperationException("A readonly member cannot be a target");
                    }

                    var shouldTrackChanges = !memberDefinition.SourceType.IsValueType;
                    var visualisationInfo = new VisualisationInfo(NodeType.Member)
                    {
                        IsRoot = !memberDefinition.SourcePaths.Any()
                    };
                    return new ReadWriteNode<T>(getValueDelegate, setValueDelegate, target.FullPath, memberDefinition.PathToParent, visualisationInfo, nodeRepository, shouldTrackChanges, onError);
                case NodeType.Action:
                    return new WriteOnlyNode<T>(target.CreateSetValueDelegate(), onError, target.FullPath);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        IValueSource<T> CreateSourceNode<T>(ISourceDefinition<T> source, Action<Exception> onError)
        {
            switch (source.NodeType)
            {
                case NodeType.Formula:
                    var getValue = source.CreateGetValueDelegateWithCurrentValue();
                    return new ReadOnlyNodeInfo<T>(getValue, source.FullPath, source.PathToParent, nodeRepository, false, onError, new VisualisationInfo(NodeType.Formula));
                case NodeType.Member:
                    var memberDefinition = (MemberDefinition<T>)source;
                    var getValueDelegate = memberDefinition.CreateGetValueDelegate();
                    var setValueDelegate = memberDefinition.CreateSetValueDelegate();

                    var shouldTrackChanges = !source.SourceType.IsValueType;
                    var visualisationInfo = new VisualisationInfo(NodeType.Member)
                    {
                        IsRoot = !memberDefinition.SourcePaths.Any()
                    };
                    if (memberDefinition.IsWritable)
                    {
                        return new ReadWriteNode<T>(getValueDelegate, setValueDelegate, source.FullPath, memberDefinition.PathToParent, visualisationInfo, nodeRepository, shouldTrackChanges, onError);
                    }

                    return new ReadOnlyNodeInfo<T>(_ => getValueDelegate(), memberDefinition.FullPath, memberDefinition.PathToParent, nodeRepository, shouldTrackChanges, onError, visualisationInfo);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        IValueSource CreateSourceNode(ISourceDefinition source, Action<Exception> onError)
        {
            var method = createSourceInfo.MakeGenericMethod(source.SourceType);
            return (IValueSource)method.Invoke(this, new object[] { source, onError });
        }
    }
}