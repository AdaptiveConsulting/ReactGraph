using System;

namespace ReactGraph.Visualisation
{
    public static class DependencyEngineExtensions
    {
        public static string ToDotFormat(this DependencyEngine dependencyEngine, string title, Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties = null)
        {
            var graphSnapshot = dependencyEngine.GetGraphSnapshot();
            var visualsaition = new DotVisualisation(graphSnapshot);
            return visualsaition.Generate(title, overrideVisualProperties);
        }
    }
}