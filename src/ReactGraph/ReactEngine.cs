using System;
using System.Diagnostics;
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
    }
}