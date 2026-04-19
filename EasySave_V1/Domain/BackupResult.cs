namespace EasySave.Domain;

public sealed BackupResult
{ 
    public string JobName { get; }
    public int TotalFiles { get; }
    public long TotalSize { get; }
    public TimeSpan Duration { get; }
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public BackupResult(string jobName, int totalFiles, long totalSize, TimeSpan duration, bool success, string? errorMessage = null)
    {
        JobName = jobName;
        TotalFiles = totalFiles;
        TotalSize = totalSize;
        Duration = duration;
        Success = success;
        ErrorMessage = errorMessage;
    }
    public override string ToString()
    {
        return $"JobName: {JobName}, TotalFiles: {TotalFiles}, TotalSize: {TotalSize} bytes, Duration: {Duration}, Success: {Success}, ErrorMessage: {ErrorMessage}";
}
}