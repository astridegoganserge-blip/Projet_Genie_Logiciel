using System.Diagnostics;
using EasySave.Application;
using EasySave.Infrastructure;

namespace EasySave.Domain;

public sealed class DifferentialBackupStrategy : IBackupStrategy
{
    private readonly FileSystemService _fileSystemService;
    private readonly DateTimeProvidern _dateTimeProvider;
    public DifferentialBackupStrategy(FileSystemService fileSystemService, DateTimeProvidern dateTimeProvider)
    {
        _fileSystemService = fileSystemService;
        _dateTimeProvider = dateTimeProvider;
    }
    public void Execute(BackupJob job, StateTracker tracker, EasyLog logger)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var souceFiles = _fileSystemService.EnumerateFiles(job.SourcePath, job.TargetPath).ToList();
            var filesToCopy = souceFiles.Where(f => ShouldCopyFile(f.SourcePath, f.TargetPath)).ToList();

            tracker.SetInitialCounts(filesToCopy.Count, filesToCopy.Sum(f => f.Size));

            foreach (var file in filesToCopy)
            {
                var fileStopwatch = Stopwatch.StartNew();
                _fileSystemService.CopyFile(file.SourcePath, file.TargetPath, overwrite: true);
                _fileSystemService.EnsureDirectoryExists(Path.GetDirectoryName(file.TargetPath)!);
                fileStopwatch.Stop();
                tracker.UpdateProgress(file.Size, file.SourcePath, file.TargetPath, fileStopwatch.ElapsedMilliseconds);
                logger.LogFileCopy(job.Name, file.SourcePath, file.TargetPath, file.Size, fileStopwatch.ElapsedMilliseconds);
            }

            tracker.MarkCompleted();
        }
        catch
        {
            tracker.MarkError();
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public bool ShouldCopyFile(string sourcePath, string targetPath)
    {
        if (!_fileSystemService.FileExists(targetPath))
            return true;
        var sourceInfo = _fileSystemService.GetFileInfo(sourcePath);
        var targetInfo = _fileSystemService.GetFileInfo(targetPath);
        return sourceInfo.LastModified > targetInfo.LastModified || sourceInfo.Size != targetInfo.Size;
    }
}