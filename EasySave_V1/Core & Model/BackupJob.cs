namespace EasySave.Models;

public class BackupJob
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SourcePath { get; set; }
    public string TargetPath { get; set; }
    public BackupType Type { get; set; }
    public DateTime LastExecutionTime { get; set; }
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