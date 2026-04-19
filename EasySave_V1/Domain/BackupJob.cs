namespace EasySave.Domain;

public sealed class BackupJob
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public BackupType Type { get; init; }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, SourcePath: {SourcePath}, DestinationPath: {DestinationPath}, Type: {Type}";
    }
}