using System.Linq;

namespace ReactGraph
{
    public class ReactEngine
    {
        private readonly DirectedGraph<NodeInfo> _graph;

        public ReactEngine(DirectedGraph<NodeInfo> graph)
        {
            _graph = graph;
        }

        public void PropertyChanged(object instance, string property)
        {
            var sourceVertex = _graph.Verticies.Single(v => v.Data.Instance == instance && v.Data.PropertyInfo.Name == property);
            var orderToReeval = _graph.TopologicalSort(sourceVertex.Data);
            foreach (var vertex in orderToReeval)
            {
                vertex.Data.ReevalValue();
            }
        }
    }
}