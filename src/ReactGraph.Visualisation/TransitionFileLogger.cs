using System;
using System.IO;

namespace ReactGraph.Visualisation
{
    public class TransitionFileLogger : IDisposable
    {
        readonly string filePath;
        readonly DependencyEngineListener listener;

        public TransitionFileLogger(DependencyEngine dependencyEngine, string filePath)
        {
            this.filePath = filePath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            listener = new DependencyEngineListener(dependencyEngine, OnTransition, false);
        }

        void OnTransition(string dotRepresentation)
        {
            using(var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.WriteLine(dotRepresentation);
            }
        }

        public void Dispose()
        {
            listener.Dispose();
        }
    }
}