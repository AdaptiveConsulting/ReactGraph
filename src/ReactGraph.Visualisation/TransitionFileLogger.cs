using System;
using System.IO;

namespace ReactGraph.Visualisation
{
    public class TransitionFileLogger : IDisposable
    {
        readonly DependencyEngineListener listener;
        readonly StreamWriter streamWriter;

        public TransitionFileLogger(DependencyEngine dependencyEngine, string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            streamWriter = new StreamWriter(fileStream);
            listener = new DependencyEngineListener(dependencyEngine, OnTransition);
        }

        void OnTransition(string dotRepresentation)
        {
            streamWriter.WriteLine(dotRepresentation.Replace(Environment.NewLine, " "));
        }

        public void Dispose()
        {
            listener.Dispose();
            streamWriter.Dispose();
        }
    }
}