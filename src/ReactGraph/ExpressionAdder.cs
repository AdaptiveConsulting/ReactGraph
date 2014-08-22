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
            var sourceNode = GetSourceNode(source);
            var targetNode = GetTargetNode(target);

            targetNode.SetSource(sourceNode, onError);
            graph.AddEdge(sourceNode, targetNode, source.NodeName, target.NodeName);
            if (!source.SourcePaths.Any())
            {
                var instance = sourceNode.GetValue().Value;
                nodeRepository.AddLookup(instance, sourceNode);
            }

            AddSourcePathExpressions(source, sourceNode);
        }

        void AddSourcePathExpressions(ISourceDefinition sourceDefinition, IValueSource sourceNode)
        {
            foreach (var sourcePath in sourceDefinition.SourcePaths)
            {
                var pathNode = definitionToNodeLookup.ContainsKey(sourcePath)
                    ? (IValueSource) definitionToNodeLookup[sourcePath]
                    : CreateSourceNode(sourcePath);

                graph.AddEdge(pathNode, sourceNode, sourcePath.NodeName, sourceDefinition.NodeName);
                // TODO We need some sort of value source, which notifications can hook into as well as lookups?
                if (!sourcePath.SourcePaths.Any())
                {
                    var instance = pathNode.GetValue().Value;
                    nodeRepository.AddLookup(instance, pathNode);
                }
                AddSourcePathExpressions(sourcePath, pathNode);
            }
        }

        IValueSource<T> GetSourceNode<T>(ISourceDefinition<T> source)
        {
            IValueSource<T> sourceNode;
            if (definitionToNodeLookup.ContainsKey(source))
            {
                sourceNode = (IValueSource<T>)definitionToNodeLookup[source];
            }
            else
            {
                sourceNode = CreateSourceNode(source);
                definitionToNodeLookup.Add(source, sourceNode);
            }

            return sourceNode;
        }

        ITakeValue<T> GetTargetNode<T>(ITargetDefinition<T> target)
        {
            ITakeValue<T> targetNode;
            if (definitionToNodeLookup.ContainsKey(target))
            {
                targetNode = (ITakeValue<T>)definitionToNodeLookup[target];
            }
            else
            {
                targetNode = CreateTargetNode(target);
                definitionToNodeLookup.Add(target, targetNode);
            }

            return targetNode;
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

        IValueSource CreateSourceNode(ISourceDefinition source)
        {
            var method = createSourceInfo.MakeGenericMethod(source.SourceType);
            return (IValueSource)method.Invoke(this, new object[] { source });
        }
    }
}