using System;

namespace EasyLog
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public required string BackupName { get; set; }
        public required string SourceFile { get; set; }
        public required string TargetFile { get; set; }
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
        public long EncryptionTimeMs { get; set; }
    }
}