using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactGraph.Graph;
using ReactGraph.Instrumentation;
using ReactGraph.NodeInfo;

namespace ReactGraph
{
    public class DependencyEngine
    {
        readonly DirectedGraph<INodeInfo> graph;
        readonly NodeRepository nodeRepository;
        readonly EngineInstrumenter engineInstrumenter;
        readonly ExpressionAdder expressionAdder;
        bool isExecuting;

        public DependencyEngine()
        {
            graph = new DirectedGraph<INodeInfo>();
            nodeRepository = new NodeRepository();
            expressionAdder = new ExpressionAdder(graph, nodeRepository);
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

        public bool ValueHasChanged(object instance, string pathToChangedValue)
        {
            if (!nodeRepository.Contains(instance) || isExecuting) return false;

            var changedInstance = nodeRepository.Get(instance);

            // The idea of this is for a expression viewModel.Foo.Bar
            // When Foo, "Bar" is passed into this method, we lookup the node with value of Foo
            // Then get the successors and filter by path to get the intended changed node.
            var successors = graph.SuccessorsOf(changedInstance)
                .Where(v => v.Data.Path.EndsWith(pathToChangedValue))
                .ToArray();
            // TODO Should make this visible via instrumentation

            INodeInfo node;
            if (successors.Length > 1)
                node = changedInstance;
            else if (successors.Length == 1)
                node = successors[0].Data;
            else
                return false;

            try
            {
                isExecuting = true;
                var orderToReeval = new Queue<Vertex<INodeInfo>>(graph.TopologicalSort(node));
                var firstVertex = orderToReeval.Dequeue();
                engineInstrumenter.DependecyWalkStarted(pathToChangedValue, firstVertex.Id);
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
                var vertex = nodes.First();
                if (vertex.Data.Type == NodeType.Member)
                    nodes.Add(vertex);
                else
                    nodes.Insert(0, nodes.Last());

                sb.AppendLine(string.Join(" --> ", nodes.Select(v => v.Data.ToString())));
            }

            throw new CycleDetectedException(sb.ToString().Trim());
        }

        public void AddExpression<T>(ISourceDefinition<T> source, ITargetDefinition<T> target, Action<Exception> onError)
        {
            expressionAdder.Add(source, target, onError);
        }
    }
}