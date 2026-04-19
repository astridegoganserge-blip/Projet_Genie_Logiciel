namespace EasySave.Domain;

public sealed class FileEntry
    {
    public string SourcePath { get; }
    public string DestinationPath { get; }
    public long Size { get; }
    public DateTime LastModified { get; }

    public FileEntry(string sourcePath, string destinationPath, long size, DateTime lastModified)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        Size = size;
        LastModified = lastModified;
    }
}