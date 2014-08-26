using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using PropertyChanged;

namespace ReactGraphViewer
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        public ObservableCollection<LogEntry> Entries { get; private set; }
        public ImageSource Image { get; private set; }

        public LogEntry SelectedLogEntry
        {
            get { return selectedLogEntry; }
            set
            {
                if (value != null)
                {
                    var text = GetLogEntryText(value);
                    var output = wrapper.GenerateGraph(text, Enums.GraphReturnType.Png);
                    var image = GetBitmapImage(output);

                    Image = image;
                }
                else
                {
                    Image = null;
                }
                
                selectedLogEntry = value;
            }
        }

        long currentFilePosition = 0;
        long logEntryIndex = 0;
        string filePath;
        GraphGeneration wrapper;
        LogEntry selectedLogEntry;
        readonly Dispatcher dispatcher;

        public MainViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            Entries = new ObservableCollection<LogEntry>();
            InitialiseDotWrapper();
            ListenLogFileChanges();
        }

        void InitialiseDotWrapper()
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            wrapper = new GraphGeneration(getStartProcessQuery, getProcessStartInfoQuery, registerLayoutPluginCommand);
        }

        void ListenLogFileChanges()
        {
            var logDirectory = ConfigurationManager.AppSettings["logDirectory"];
            var fileName = ConfigurationManager.AppSettings["fileName"];
            var watcher = new FileSystemWatcher(logDirectory, fileName);

            filePath = Path.Combine(logDirectory, fileName);
            watcher.Created += FileCreated;
            watcher.Changed += FileChanged;
            watcher.Deleted += FileDeleted;

            if (File.Exists(filePath))
            {
                FileCreated(null, null);
            }

            watcher.EnableRaisingEvents = true;
        }

        void FileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                dispatcher.Invoke(ReadFileToEnd);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }        
        }

        void FileDeleted(object sender, FileSystemEventArgs e)
        {
            dispatcher.Invoke(() =>
            {
                Entries.Clear();
                SelectedLogEntry = null;
                currentFilePosition = 0;
                logEntryIndex = 0;
            });
        }

        void FileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                dispatcher.Invoke(ReadFileToEnd);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }      
        }

        private void ReadFileToEnd()
        {
            var fileSize = new FileInfo(filePath).Length;
            var size = fileSize - currentFilePosition;
            if (size <= 0) return;

            var newEntries = new List<LogEntry>();

            using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open))
            using (var viewStream = mmf.CreateViewStream(currentFilePosition, size, MemoryMappedFileAccess.Read))
            using(var reader = new BinaryReader(viewStream))
            {
                var startPosition = currentFilePosition;
                var lastChar = ' ';

                while (reader.PeekChar() != -1)
                {
                    var newChar = reader.ReadChar();
                    if (lastChar == '\r' && newChar == '\n')
                    {
                        newEntries.Add(new LogEntry(startPosition, currentFilePosition - startPosition, logEntryIndex++));
                        startPosition = currentFilePosition;
                    }
                    lastChar = newChar;
                    currentFilePosition++;
                }
            }
            foreach (var newEntry in newEntries)
            {
                Entries.Add(newEntry);
            }
            SelectedLogEntry = Entries.LastOrDefault();
        }

        private string GetLogEntryText(LogEntry logEntry)
        {
            using (var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open))
            using (var viewStream = mmf.CreateViewStream(logEntry.Position, logEntry.Size, MemoryMappedFileAccess.Read))
            using (var reader = new StreamReader(viewStream))
            {
                return reader.ReadToEnd();
            }
        }

        private static BitmapImage GetBitmapImage(byte[] imageBytes)
        {
            var bitmapImage = new BitmapImage();
            if (imageBytes != null && imageBytes.Length > 0)
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(imageBytes);
                bitmapImage.EndInit();
            }
            
            return bitmapImage;
        }
    }
}