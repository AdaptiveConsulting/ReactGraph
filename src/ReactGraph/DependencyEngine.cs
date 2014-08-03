using System;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Internals.Construction;
using ReactGraph.Internals.Graph;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph
{
    public class DependencyEngine
    {
        private readonly DirectedGraph<INodeInfo> graph;
        private readonly ExpressionParser expressionParser;
        private readonly NodeRepository nodeRepository;
        private bool isExecuting;

        public DependencyEngine()
        {
            graph = new DirectedGraph<INodeInfo>();
            nodeRepository = new NodeRepository(this);
            expressionParser = new ExpressionParser(nodeRepository);
        }

        public bool ValueHasChanged(object instance, string key)
        {
            if (!nodeRepository.Contains(instance, key) || isExecuting) return false;

            var node = nodeRepository.Get(instance, key);
            
            try
            {
                isExecuting = true;
                var orderToReeval = graph.TopologicalSort(node).ToArray();
                node.ValueChanged();
                var firstVertex = orderToReeval[0];
                PropagateParentValue(firstVertex);
                foreach (var vertex in orderToReeval.Skip(1))
                {
                    vertex.Data.Reevaluate();
                    PropagateParentValue(vertex);
                }
            }
            finally
            {
                isExecuting = false;
            }

            return true;
        }

        static void PropagateParentValue(Vertex<INodeInfo> firstVertex)
        {
            foreach (var successor in firstVertex.Successors)
            {
                firstVertex.Data.ParentInstance = successor.Source.Data.GetValue();
            }
        }

        public override string ToString()
        {
            return graph.ToDotLanguage("DependencyGraph");
        }

        public IExpressionDefinition Expr<TProp>(Expression<Func<TProp>> sourceFunction)
        {
            var formulaNode = expressionParser.GetNodeInfo(sourceFunction);
            return new ExpressionDefinition(formulaNode, expressionParser, graph);
        }
    }
}