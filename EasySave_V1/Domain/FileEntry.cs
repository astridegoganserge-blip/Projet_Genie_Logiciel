namespace EasySave.Domain;

public sealed class FileEntry
{
    public string SourcePath { get; init; } = string.Empty;
    public string TargetPath { get; init; } = string.Empty;
    public long Size { get; init; }
}