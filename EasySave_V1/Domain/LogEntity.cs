namespace EasySave.Domain;

public sealed class LogEntity
{
    public DateTime Timestamp { get; }
    public string JobName { get; }
    public string SourcePath { get; }
    public string TargetPath { get; }
    public long FileSize { get; }
    public int TransferTimeMs { get; }

    public LogEntity(DateTime timestamp, string jobName, string sourcePath, string targetPath, long fileSize, int transferTimeMs)
    {
        Timestamp = timestamp;
        JobName = jobName;
        SourcePath = sourcePath;
        TargetPath = targetPath;
        FileSize = fileSize;
        TransferTimeMs = transferTimeMs;
    }

    public override string ToString()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] Job: {JobName}, Source: {SourcePath}, Target: {TargetPath}, Size: {FileSize} bytes, Time: {TransferTimeMs} ms";
    }
}