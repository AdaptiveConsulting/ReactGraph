using System.Text;
using ReactGraph.Internals.Graph;
using ReactGraph.Internals.NodeInfo;

namespace ReactGraph.Internals.Visualisation
{
    internal class DotVisualisation
    {
        readonly DirectedGraph<INodeInfo> graph;

        public DotVisualisation(DirectedGraph<INodeInfo> graph)
        {
            this.graph = graph;
        }

        public string Generate(string title)
        {
            var labels = new StringBuilder();
            var graphDefinition = new StringBuilder();

            foreach (var vertex in graph.Verticies)
            {
                var label = string.IsNullOrEmpty(vertex.Label) ? vertex.Data.ToString() : vertex.Label;
                var color = string.IsNullOrEmpty(vertex.Color) ? string.Empty : string.Format(", fillcolor=\"{0}\"", vertex.Color);
                labels.AppendFormat("     {0} [label=\"{1}\"{2}];", vertex.Data.GetHashCode(), label, color).AppendLine();
            }

            foreach (var edge in graph.Edges)
            {
                graphDefinition.AppendFormat("     {0} -> {1};",
                    edge.Source.Data.GetHashCode(), edge.Target.Data.GetHashCode())
                    .AppendLine();
            }

            return string.Format(@"digraph {0} {{
{1}
{2}}})", title, labels, graph);
        }
    }
}