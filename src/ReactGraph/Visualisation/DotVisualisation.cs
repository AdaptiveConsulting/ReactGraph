using System;
using System.Text;
using ReactGraph.Graph;
using ReactGraph.NodeInfo;

namespace ReactGraph.Visualisation
{
    internal class DotVisualisation : IVisualisation
    {
        readonly DirectedGraph<INodeInfo> graph;

        public DotVisualisation(DirectedGraph<INodeInfo> graph)
        {
            this.graph = graph;
        }

        public string Generate(string title, Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties = null, bool showRootAsClusters = false)
        {
            var labels = new StringBuilder();
            var graphDefinition = new StringBuilder();

            foreach (var vertex in graph.Verticies)
            {
                var properties = new VertexVisualProperties(vertex.Id)
                {
                    Label = vertex.Data.ToString()
                };

                if (vertex.Data.Type == NodeType.Formula)
                {
                    properties.Color = "lightblue";
                    properties.AddCustomProperty("style", "filled");
                    properties.AddCustomProperty("shape", "octagon");
                }
                else if (vertex.Data.Type == NodeType.WritableNode)
                {
                    properties.AddCustomProperty("shape", "box");
                    properties.AddCustomProperty("style", "filled");
                    properties.AddCustomProperty("style", "rounded");
                }

                if (overrideVisualProperties != null)
                {
                    properties = overrideVisualProperties(properties);
                }

                labels.AppendLine(properties.ToString());
            }

            foreach (var edge in graph.Edges)
            {
                graphDefinition.AppendFormat("     {0} -> {1};",
                    edge.Source.Id, edge.Target.Id)
                    .AppendLine();
            }

            return string.Format(@"digraph {0} {{
{1}
{2}}})", title, labels, graphDefinition);
        }
    }
}