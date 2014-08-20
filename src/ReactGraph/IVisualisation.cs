using System;
using ReactGraph.Visualisation;

namespace ReactGraph
{
    public interface IVisualisation
    {
        string Generate(string title, Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties = null, bool showRootAsClusters = false);
    }
}