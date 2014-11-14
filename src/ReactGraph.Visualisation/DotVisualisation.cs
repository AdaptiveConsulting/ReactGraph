using System;
using System.Text;
using ReactGraph.Graph;
using ReactGraph.NodeInfo;

namespace ReactGraph.Visualisation
{
    using System.Linq;

    internal class DotVisualisation
    {
        readonly DirectedGraph<INodeMetadata> graph;

        public DotVisualisation(DirectedGraph<INodeMetadata> graph)
        {
            this.graph = graph;
        }

        public string Generate(string title, Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties, bool indent, VisualisationOptions visualisationOptions)
        {
            var labels = new StringBuilder();
            var graphDefinition = new StringBuilder();
            var currentGraph = graph;
            var thisNodes = graph.Verticies.Where(v => v.Data.Label == "this").ToArray();
            foreach (var source in thisNodes)
            {
                currentGraph.DeleteVertex(source.Data);
            }
            if (!visualisationOptions.ShowFormulas)
            {
                currentGraph = ExcludeFormulaNodes(currentGraph);
            }

            foreach (var vertex in currentGraph.Verticies)
            {
                var properties = new VertexVisualProperties(vertex.Id)
                {
                    Label = vertex.Data.Label
                };

                if (!visualisationOptions.ShowRoot && vertex.Data.VisualisationInfo.IsRoot) continue;

                switch (vertex.Data.VisualisationInfo.NodeType)
                {
                    case NodeType.Formula:
                        properties.Color = "lightblue";
                        properties.AddCustomProperty("style", "filled");
                        properties.AddCustomProperty("shape", "octagon");
                        break;
                    case NodeType.Member:
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

            foreach (var edge in currentGraph.Edges)
            {
                if (!visualisationOptions.ShowRoot && (edge.Source.Data.VisualisationInfo.IsRoot || edge.Target.Data.VisualisationInfo.IsRoot))
                    continue;

                graphDefinition.Append(indent ? "    " : string.Empty).AppendFormat("{0} -> {1};", edge.Source.Id, edge.Target.Id);
                if (indent) graphDefinition.AppendLine();
            }

            if (indent)
            {
                return string.Format(@"digraph {0} {{
{1}
{2}}}", title, labels, graphDefinition);
            }
            return string.Format(@"digraph {0} {{ {1} {2}}}", title, labels, graphDefinition);
        }

        DirectedGraph<INodeMetadata> ExcludeFormulaNodes(DirectedGraph<INodeMetadata> directedGraph)
        {
            var newGraph = directedGraph.Clone(v => v.Data);
            var formulaNodes = newGraph.Verticies.Where(v => v.Data.VisualisationInfo.NodeType == NodeType.Formula).ToArray();
            // Add edge between formula successors and predecessors
            foreach (var formulaNode in formulaNodes)
            {
                var predecessors = formulaNode.Predecessors.ToArray();
                var successors = formulaNode.Successors.ToArray();
                newGraph.DeleteVertex(formulaNode.Data);

                foreach (var predecessor in predecessors)
                {
                    foreach (var successor in successors)
                    {
                        var source = predecessor.Source;
                        var target = successor.Target;
                        newGraph.AddEdge(source.Data, target.Data, source.Id, target.Id);
                    }
                }
            }

            return newGraph;
        }
    }
}