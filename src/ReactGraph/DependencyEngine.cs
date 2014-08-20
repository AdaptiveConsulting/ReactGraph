using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Api;
using ReactGraph.Construction;
using ReactGraph.Graph;
using ReactGraph.Instrumentation;
using ReactGraph.NodeInfo;
using ReactGraph.Visualisation;

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
            Visualisation = new DotVisualisation(graph);
            engineInstrumenter = new EngineInstrumenter(engineInstrumentation);
        }

        public IVisualisation Visualisation { get; set; }

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

        public override string ToString()
        {
            var dotVisualisation = new DotVisualisation(graph);
            return dotVisualisation.Generate("DependencyGraph");
        }

        public IExpressionDefinition<TProp> Expr<TProp>(Expression<Func<TProp>> sourceFunction, string expressionId = null)
        {
            var formulaNode = expressionParser.GetFormulaInfo(sourceFunction);
            return new ExpressionDefinition<TProp>(formulaNode, expressionId, expressionParser, graph, nodeRepository);
        }
    }
}