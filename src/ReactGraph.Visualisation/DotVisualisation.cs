using System;
using System.Text;
using ReactGraph.Graph;
using ReactGraph.NodeInfo;

namespace ReactGraph.Visualisation
{
    internal class DotVisualisation
    {
        readonly DirectedGraph<INodeMetadata> graph;

        public DotVisualisation(DirectedGraph<INodeMetadata> graph)
        {
            this.graph = graph;
        }

        public string Generate(string title, Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties = null, bool indent = true)
        {
            var labels = new StringBuilder();
            var graphDefinition = new StringBuilder();

            foreach (var vertex in graph.Verticies)
            {
                var properties = new VertexVisualProperties(vertex.Id)
                {
                    Label = vertex.Data.Label
                };

                switch (vertex.Data.NodeType)
                {
                    case NodeType.Formula:
                        properties.Color = "lightblue";
                        properties.AddCustomProperty("style", "filled");
                        properties.AddCustomProperty("shape", "octagon");
                        break;
                    case NodeType.WritableNode:
                        properties.AddCustomProperty("shape", "box");
                        properties.AddCustomProperty("style", "filled");
                        properties.AddCustomProperty("style", "rounded");
                        break;
                    case NodeType.Action:
                        properties.AddCustomProperty("style", "filled");
                        properties.Color = "green";
                        break;
                }

                if (overrideVisualProperties != null)
                {
                    properties = overrideVisualProperties(properties);
                }

                labels.Append(properties);
                if (indent) labels.AppendLine();
            }

            foreach (var edge in graph.Edges)
            {
                graphDefinition.Append(indent ? "    " : string.Empty).AppendFormat("{0} -> {1};", edge.Source.Id, edge.Target.Id);
                if (indent) graphDefinition.AppendLine();
            }

            if (indent)
            {
                return string.Format(@"digraph {0} {{
{1}
{2}}})", title, labels, graphDefinition);
            }
            return string.Format(@"digraph {0} {{ {1} {2}}})", title, labels, graphDefinition);
        }
    }
}