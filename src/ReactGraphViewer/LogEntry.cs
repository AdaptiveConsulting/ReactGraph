namespace ReactGraphViewer
{
    public class LogEntry
    {
        public long Position { get; set; }
        public long Size { get; set; }
        public long LogEntryIndex { get; set; }

        public LogEntry(long position, long size, long logEntryIndex)
        {
            Position = position;
            Size = size;
            LogEntryIndex = logEntryIndex;
        }
    }
}