using System;
using ReactGraph.Internals.Visualisation;

namespace ReactGraph
{
    public interface IVisualisation
    {
        string Generate(string title, Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties = null, bool showRootAsClusters = false);
    }
}