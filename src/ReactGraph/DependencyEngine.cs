using System;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Internals;

namespace ReactGraph
{
    public class DependencyEngine
    {
        private readonly DirectedGraph<NodeInfo> graph;
        private readonly ExpressionParser expressionParser;

        public DependencyEngine()
        {
            graph = new DirectedGraph<NodeInfo>();
            expressionParser = new ExpressionParser();
        }

        public event Action<object, string> SettingValue = (o, s) => { };

        public void PropertyChanged(object instance, string property)
        {
            var sourceVertex = graph.Verticies.Single(v => v.Data.Instance == instance && v.Data.PropertyInfo.Name == property);
            var orderToReeval = graph.TopologicalSort(sourceVertex.Data);
            foreach (var vertex in orderToReeval.Skip(1))
            {
                SettingValue(vertex.Data.Instance, vertex.Data.PropertyInfo.Name);
                vertex.Data.ReevalValue();
            }
        }

        public void Bind<TProp>(Expression<Func<TProp>> targetProperty, Expression<Func<TProp>> sourceFunction)
        {
            var targetVertex = expressionParser.GetNodeInfo(targetProperty, sourceFunction);
            var sourceVertices = expressionParser.GetSourceVerticies(sourceFunction);

            foreach (var sourceVertex in sourceVertices)
            {
                graph.AddEdge(sourceVertex, targetVertex);
            }
        }
    }
}