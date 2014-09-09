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
            nodeRepository = new NodeRepository(this);
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


        // TODO we need to profile and make sure we are optimal on this code path, it's the main hot path of the library
        public bool ValueHasChanged(object instance, string pathToChangedValue)
        {
            if (!nodeRepository.Contains(instance) || isExecuting) return false;

            var changedInstance = nodeRepository.Get(instance);

            IValueSource node;
            if (!FindChangedNode(pathToChangedValue, changedInstance, out node))
            {
                return false;
            }

            try
            {
                isExecuting = true;
                // TODO I need to review topological sort and speed it up, it's on the hot path
                // TODO we may want to add another instrumentation call before the topo sort, so we can instrument the time it takes to calculate it and compare it with the time it takes to actually propagate (so we can optimize the costly part)
                var orderToReeval = new Queue<Vertex<INodeInfo>>(graph.TopologicalSort(node));
                var firstVertex = orderToReeval.Dequeue();
                engineInstrumenter.DependecyWalkStarted(pathToChangedValue, firstVertex.Id);

                node.UnderlyingValueHasBeenChanged();
                while (orderToReeval.Count > 0)
                {
                    var vertex = orderToReeval.Dequeue();

                    // TODO here I'm wondering if it's better to manage all the nodes the same way:
                    // - if the node is a formula, we could evaluate the formula and set the target member here (or call the target action)
                    // - if the node is a property/member, what does it actually mean to reevaluate it?
                    // I have the feeling that trying to make all this very generic is making the whole thing more confusing and complex than it actually is

                    var reevaluationResult = vertex.Data.Reevaluate();

                    switch (reevaluationResult)
                    {
                        case ReevaluationResult.NoChange:
                            engineInstrumenter.NodeEvaluated(vertex.Data.ToString(), vertex.Id, reevaluationResult);
                            break;
                        case ReevaluationResult.Error:

                            // TODO this should be refactored, maybe some algo in the graph?
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

        // TODO I tried pretty hard to understand this but couldn't. Lookups based on "Path.EndsWith(pathToChangedValue)" looks really flaky to me
        // TODO Also looks like there is a bug (see new failing tests for repro)
        // TODO it's also doing lots of iterations and lookups, and it's happening every single time UnderlyingValueHasBeenChanged is called
        bool FindChangedNode(string pathToChangedValue, INodeInfo changedInstance, out IValueSource node)
        {
            // The idea of this is for a expression viewModel.Foo.Bar
            // When Foo, "Bar" is passed into this method, we lookup the node with value of Foo
            // Then get the successors and filter by path to get the intended changed node.
            var successors = graph.SuccessorsOf(changedInstance)
                .Where(v => v.Data.Path.EndsWith(pathToChangedValue))
                .ToArray();
            // TODO Should make this visible via instrumentation

            if (successors.Length > 1)
                node = (IValueSource) changedInstance;
            else if (successors.Length == 1)
                node = (IValueSource) successors[0].Data;
            else
            {
                // When path is contained in a formula, i.e Foo.Bar and Bar changes but
                // Formula is Calc(Foo)
                successors = graph.SuccessorsOf(changedInstance)
                    .Where(v => v.Data.Path.Contains(changedInstance.Path))
                    .ToArray();
                if (successors.Length > 1)
                    node = (IValueSource) changedInstance;
                else if (successors.Length == 1)
                    node = (IValueSource) successors[0].Data;
                else
                {
                    node = null;
                    return false;
                }
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