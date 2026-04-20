using System.Diagnostics;
using EasySave.Application;
using EasySave.Infrastructure;

namespace EasySave.Domain;

public sealed class CompleteBackupStrategy : IBackupStrategy
{
    private readonly FileSystemService _fileSystemService;
    private readonly DateTimeProvidern _dateTimeProvider;

    public CompleteBackupStrategy(FileSystemService fileSystemService, DateTimeProvidern dateTimeProvider)
    {
        _fileSystemService = fileSystemService;
        _dateTimeProvider = dateTimeProvider;
    }

    public void Execute(BackupJob job, StateTracker tracker, EasyLog logger)

    {
        var stopwatch = stopwatch.StartNew();

        try
        {
            var files = _fileSystemService.EnumerateFiles(job.SourcePath, job.TargetPath).ToList();

            tracker.SetInitialCounts(files.Count, files.Sum(f => f.Size));

            foreach (var file in files)
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
}