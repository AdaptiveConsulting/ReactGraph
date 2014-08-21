using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly ExpressionParser expressionParser;
        private readonly NodeRepository nodeRepository;
        private readonly EngineInstrumenter engineInstrumenter;
        private bool isExecuting;

        public DependencyEngine(IEngineInstrumentation engineInstrumentation = null)
        {
            graph = new DirectedGraph<INodeInfo>();
            nodeRepository = new NodeRepository(this);
            expressionParser = new ExpressionParser();
            engineInstrumenter = new EngineInstrumenter(engineInstrumentation);
        }

        public DirectedGraph<INodeMetadata> GetGraphSnapshot()
        {
            return graph.Clone(vertex => (INodeMetadata)new NodeMetadata(vertex.Data.Type, vertex.Data.ToString(), vertex.Id));
        } 

        public bool ValueHasChanged(object instance, string key)
        {
            if (!nodeRepository.Contains(instance, key) || isExecuting) return false;

            var node = nodeRepository.Get(instance, key);
            engineInstrumenter.DependecyWalkStarted(key);

            try
            {
                isExecuting = true;
                var orderToReeval = new Queue<Vertex<INodeInfo>>(graph.TopologicalSort(node));
                var firstVertex = orderToReeval.Dequeue();
                node.ValueChanged();
                NotificationStratgegyValueUpdate(firstVertex);
                while (orderToReeval.Count > 0)
                {
                    var vertex = orderToReeval.Dequeue();
                    var results = vertex.Data.Reevaluate();

                    switch (results)
                    {
                        case ReevaluationResult.NoChange:
                            break;
                        case ReevaluationResult.Error:
                            var nodesRelatedToError = graph.TopologicalSort(vertex.Data).ToDictionary(k => k.Data);
                            var newListToProcess = orderToReeval
                                .Where(remaining => !nodesRelatedToError.ContainsKey(remaining.Data))
                                .ToArray();
                            orderToReeval = new Queue<Vertex<INodeInfo>>(newListToProcess);
                            break;
                        case ReevaluationResult.Changed:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    engineInstrumenter.NodeEvaluated(vertex.Data.ToString(), results);

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

        public IExpressionDefinition<TProp> Expr<TProp>(Expression<Func<TProp>> sourceFunction, string expressionId = null)
        {
            var formulaNode = expressionParser.GetFormulaInfo(sourceFunction);
            return new ExpressionDefinition<TProp>(formulaNode, expressionId, expressionParser, graph, nodeRepository);
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
    }
}