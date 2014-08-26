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

        public DependencyEngine()
        {
            graph = new DirectedGraph<INodeInfo>();
            nodeRepository = new NodeRepository(this);
            expressionParser = new ExpressionParser();
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