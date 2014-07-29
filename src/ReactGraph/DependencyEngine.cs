using System;
using System.Linq;
using System.Linq.Expressions;
using ReactGraph.Internals;

namespace ReactGraph
{
    public class DependencyEngine
    {
        private readonly DirectedGraph<NodeInfo> _graph;
        private readonly ExpressionParser _expressionParser;

        public DependencyEngine()
        {
            _graph = new DirectedGraph<NodeInfo>();
            _expressionParser = new ExpressionParser();
        }

        public event Action<object, string> SettingValue = (o, s) => { };

        public void PropertyChanged(object instance, string property)
        {
            var sourceVertex = _graph.Verticies.Single(v => v.Data.Instance == instance && v.Data.PropertyInfo.Name == property);
            var orderToReeval = _graph.TopologicalSort(sourceVertex.Data);
            foreach (var vertex in orderToReeval.Skip(1))
            {
                SettingValue(vertex.Data.Instance, vertex.Data.PropertyInfo.Name);
                vertex.Data.ReevalValue();
            }
        }

        public void Bind<TProp>(Expression<Func<TProp>> targetProperty, Expression<Func<TProp>> sourceFunction)
        {
            var targetVertex = _expressionParser.GetNodeInfo(targetProperty, sourceFunction);
            var sourceVertices = _expressionParser.GetSourceVerticies(sourceFunction);

            foreach (var sourceVertex in sourceVertices)
            {
                _graph.AddEdge(sourceVertex, targetVertex);
            }
        }
    }
}