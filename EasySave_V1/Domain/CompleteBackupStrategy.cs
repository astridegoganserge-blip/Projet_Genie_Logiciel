using System.Diagnostics;
using EasySave.Application;
using EasySave.Infrastructure;
using EasySave.Shared;

namespace EasySave.Domain;

public sealed class CompleteBackupStrategy : IBackupStrategy
{
    private readonly FileSystemService _fileSystemService;
    private readonly DateTimeProvidern _dateTimeProvider;
    private readonly LogService _logService;
    private readonly StateService _stateService;

    public CompleteBackupStrategy(FileSystemService fileSystemService, DateTimeProvidern dateTimeProvider, LogService logService, StateService stateService)
    {
        _fileSystemService = fileSystemService;
        _dateTimeProvider = dateTimeProvider;
        _logService = logService;
        _stateService = stateService;
    }

    public BackupResult Execute(BackupJob job)
    {
        var stopwatch = Stopwatch.StartNew();
        var allFiles = _fileSystemService.EnumerateFiles(job.SourcePath).ToList();
        var totalFiles = allFiles.Count;
        var totalSize = allFiles.Sum(f => f.Size);
        var filesLeft = totalFiles;
        long bytesCopied = 0;

        try
        {
            _stateService.UpdateState(
                job.Name,
                JobState.Active,
                0,
                totalFiles,
                totalSize,
                filesLeft,
                null,
                null);

            foreach (var file in allFiles)
            {
                var fileStopwatch = Stopwatch.StartNew();
                _fileSystemService.EnsureDirectoryExists(Path.GetDirectoryName(file.DestinationPath)!);
                _fileSystemService.CopyFile(file.SourcePath, file.DestinationPath);
                fileStopwatch.Stop();
                bytesCopied += file.Size;
                filesLeft--;

                var progression = totalFiles == 0 ? 100 : (int)((totalFiles - filesLeft) * 100 / totalFiles);
                _stateService.UpdateState(
                    job.Name,
                    JobState.Active,
                    progression,
                    totalFiles,
                    totalSize,
                    filesLeft,
                    file.SourcePath,
                    file.DestinationPath);

                _logService.AppendFilelog(
                    job.Name,
                    file.SourcePath,
                    file.DestinationPath,
                    file.Size,
                    fileStopwatch.ElapsedMilliseconds);
            }
            stopwatch.Stop();
            _stateService.UpdateState(
                job.Name,
                JobState.Completed,
                100,
                totalFiles,
                totalSize,
                0,
                null,
                null);

            return new BackupResult(job.Name, totalFiles, totalSize, stopwatch.Elapsed, success: true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _stateService.UpdateJobState(
                job.Name,
                JobState.Failed,
                totalFiles,
                totalSize,
                filesLeft,
                totalFiles == 0 ? 0 : (int)((totalFiles - filesLeft) * 100 / totalFiles),
                null,
                null);
            return new BackupResult(job.Name, totalFiles, totalSize, stopwatch.Elapsed, success: false, errorMessage: ex.Message);
        }
    }
}