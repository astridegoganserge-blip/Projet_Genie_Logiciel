namespace EasySave.Domain;

public sealed class BackupJob
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string TargetPath { get; init; } = string.Empty;
    public BackupType Type { get; init; }
    public DateTime LastExecutionTime { get; set; } = DateTime.MinValue;
    public bool ValidatePaths()
    {
        if (string.IsNullOrWhiteSpace(SourcePath)
            return false;
        if (string.IsNullOrWhiteSpace(TargetPath))
            return false;

        return Directory.Exists(SourcePath) && Directory.Exists(TargetPath);
    }

    public string IsExisting()
    {
        if (!Directory.Exists(SourcePath))
            return "Source path does not exist: {SourcePath}.";
        return "OK";
    }
    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, SourcePath: {SourcePath}, TargetPath: {TargetPath}, Type: {Type}, LastExecutionTime: {LastExecutionTime}";
    }
}