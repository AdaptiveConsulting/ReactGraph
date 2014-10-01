using System;

namespace ReactGraph.Visualisation
{
    public static class DependencyEngineVisualisationExtensions
    {
        public static string ToDotFormat(this DependencyEngine dependencyEngine, 
            string title = null, 
            Func<VertexVisualProperties, VertexVisualProperties> overrideVisualProperties = null,
            VisualisationOptions options = null)
        {
            var graphSnapshot = dependencyEngine.GetGraphSnapshot();
            var visualsaition = new DotVisualisation(graphSnapshot);
            return visualsaition.Generate(title, overrideVisualProperties ?? (v => v), true, options ?? new VisualisationOptions());
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