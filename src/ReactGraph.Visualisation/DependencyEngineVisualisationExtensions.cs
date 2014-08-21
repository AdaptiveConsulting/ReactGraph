using System;

namespace ReactGraph.Visualisation
{
    public static class DependencyEngineVisualisationExtensions
    {
        public static string ToDotFormat(this DependencyEngine dependencyEngine, string title, Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties = null)
        {
            var graphSnapshot = dependencyEngine.GetGraphSnapshot();
            var visualsaition = new DotVisualisation(graphSnapshot);
            return visualsaition.Generate(title, overrideVisualProperties);
        }

        public static IDisposable OnWalkComplete(this DependencyEngine dependencyEngine, Action<string> onWalkComplete)
        {
            return new DependencyEngineListener(dependencyEngine, onWalkComplete);
        }

        public static IDisposable LogTransitionsInDotFormat(this DependencyEngine dependencyEngine, string filePath)
        {
            return new TransitionFileLogger(dependencyEngine, filePath);
        }
    }
}