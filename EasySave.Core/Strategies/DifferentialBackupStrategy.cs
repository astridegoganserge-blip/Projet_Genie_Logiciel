using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Core.Models;
using EasySave.Core.Services;



namespace EasySave.Core.Strategies
{
    public sealed class DifferentialBackupStrategy : IBackupStrategy
    {
        public bool Execute(
        BackupJob job,
        EasyLog.EasyLog logger,
        AppSettings settings,
        JobExecutionContext context)
        {
            if (!job.ValidatePaths())
            {
                return false;
            }



            string[] files = Directory
            .GetFiles(job.SourcePath, "*", SearchOption.AllDirectories)
            .Where(file => ShouldCopyFile(file, job.SourcePath, job.TargetPath))
            .OrderByDescending(file => PriorityFileFilter.IsPriorityFile(file, settings.PriorityExtensions))
            .ThenBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToArray();



            long totalSize = files.Sum(GetFileSize);
            int priorityFilesCount = files.Count(file =>
            PriorityFileFilter.IsPriorityFile(file, settings.PriorityExtensions));



            int pendingPriorityFilesForCurrentJob = priorityFilesCount;



            PriorityFileFilter.RegisterPriorityFiles(job.Name, priorityFilesCount);
            StateTracker.Initialize(job, files.Length, totalSize);



            foreach (string sourceFile in files)
            {
                if (!context.WaitIfPaused())
                {
                    PriorityFileFilter.ResetJob(job.Name);
                    StateTracker.MarkAsInterrupted(job.Name);
                    return false;
                }



                if (context.StopToken.IsCancellationRequested)
                {
                    PriorityFileFilter.ResetJob(job.Name);
                    StateTracker.MarkAsInterrupted(job.Name);
                    return false;
                }



                if (!PriorityFileFilter.CanProcess(sourceFile, settings.PriorityExtensions, context))
                {
                    PriorityFileFilter.ResetJob(job.Name);
                    StateTracker.MarkAsInterrupted(job.Name);
                    return false;
                }



                bool isPriorityFile = PriorityFileFilter.IsPriorityFile(sourceFile, settings.PriorityExtensions);
                bool wasLargeFileSlotAcquired = false;



                string targetFile = BuildTargetPath(job, sourceFile);
                long fileSize = GetFileSize(sourceFile);



                try
                {
                    wasLargeFileSlotAcquired = LargeFileThrottle.Acquire(
                    fileSize,
                    settings.MaxFileSizeKb);



                    EnsureTargetDirectoryExists(targetFile);



                    DateTime transferStartTime = DateTime.Now;
                    File.Copy(sourceFile, targetFile, true);
                    long transferTimeMs = Math.Max(
                    0,
                    (long)(DateTime.Now - transferStartTime).TotalMilliseconds);



                    long encryptionTimeMs = CryptoService.Encrypt(
                    targetFile,
                    settings.ExtensionsToEncrypt);



                    logger.LogFileTransfer(
                    job.Name,
                    sourceFile,
                    targetFile,
                    fileSize,
                    transferTimeMs,
                    encryptionTimeMs);



                    SendCentralizedLogIfRequired(
                    settings,
                    job.Name,
                    sourceFile,
                    targetFile,
                    fileSize,
                    transferTimeMs,
                    encryptionTimeMs);



                    StateTracker.UpdateProgress(
                    job.Name,
                    sourceFile,
                    targetFile,
                    fileSize);
                }
                catch
                {
                    logger.LogFileTransfer(
                    job.Name,
                    sourceFile,
                    targetFile,
                    fileSize,
                    -1,
                    0);



                    SendCentralizedLogIfRequired(
                    settings,
                    job.Name,
                    sourceFile,
                    targetFile,
                    fileSize,
                    -1,
                    0);



                    StateTracker.MarkAsError(job.Name);
                    return false;
                }
                finally
                {
                    LargeFileThrottle.Release(wasLargeFileSlotAcquired);



                    if (isPriorityFile)
                    {
                        PriorityFileFilter.NotifyPriorityFileCompleted(job.Name);
                        pendingPriorityFilesForCurrentJob = Math.Max(
                        0,
                        pendingPriorityFilesForCurrentJob - 1);
                    }
                }
            }



            if (settings.DeleteOrphanFilesInDifferential)
            {
                bool deleteSuccess = DeleteOrphanFiles(job, settings, logger, context);

                if (!deleteSuccess)
                {
                    return false;
                }
            }



            StateTracker.MarkAsCompleted(job.Name);
            return true;
        }



        private static bool DeleteOrphanFiles(
        BackupJob job,
        AppSettings settings,
        EasyLog.EasyLog logger,
        JobExecutionContext context)
        {
            if (!Directory.Exists(job.TargetPath))
            {
                return true;
            }



            string[] targetFiles = Directory.GetFiles(
            job.TargetPath,
            "*",
            SearchOption.AllDirectories);



            foreach (string targetFile in targetFiles)
            {
                if (!context.WaitIfPaused())
                {
                    return false;
                }



                if (context.StopToken.IsCancellationRequested)
                {
                    return false;
                }



                string relativePath = Path.GetRelativePath(job.TargetPath, targetFile);
                string expectedSourceFile = Path.Combine(job.SourcePath, relativePath);



                if (!File.Exists(expectedSourceFile))
                {
                    try
                    {
                        long fileSize = GetFileSize(targetFile);

                        File.Delete(targetFile);

                        logger.LogFileTransfer(
                        job.Name,
                        "[DELETED_FROM_TARGET_ORPHAN]",
                        targetFile,
                        fileSize,
                        0,
                        0);

                        SendCentralizedLogIfRequired(
                        settings,
                        job.Name,
                        "[DELETED_FROM_TARGET_ORPHAN]",
                        targetFile,
                        fileSize,
                        0,
                        0);
                    }
                    catch
                    {
                        logger.LogFileTransfer(
                        job.Name,
                        "[DELETE_ORPHAN_FAILED]",
                        targetFile,
                        0,
                        -1,
                        0);

                        SendCentralizedLogIfRequired(
                        settings,
                        job.Name,
                        "[DELETE_ORPHAN_FAILED]",
                        targetFile,
                        0,
                        -1,
                        0);

                        StateTracker.MarkAsError(job.Name);
                        return false;
                    }
                }
            }



            foreach (string directory in Directory
            .GetDirectories(job.TargetPath, "*", SearchOption.AllDirectories)
            .OrderByDescending(d => d.Length))
            {
                if (Directory.Exists(directory) &&
                !Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    try
                    {
                        Directory.Delete(directory);
                    }
                    catch
                    {
                    }
                }
            }



            return true;
        }



        private static bool ShouldCopyFile(
        string sourceFile,
        string sourcePath,
        string targetPath)
        {
            string relativePath = Path.GetRelativePath(sourcePath, sourceFile);
            string targetFile = Path.Combine(targetPath, relativePath);



            if (!File.Exists(targetFile))
            {
                return true;
            }



            var sourceInfo = new FileInfo(sourceFile);
            var targetInfo = new FileInfo(targetFile);



            return sourceInfo.LastWriteTime > targetInfo.LastWriteTime
            || sourceInfo.Length != targetInfo.Length;
        }



        private static string BuildTargetPath(BackupJob job, string sourceFile)
        {
            string relativePath = Path.GetRelativePath(job.SourcePath, sourceFile);
            return Path.Combine(job.TargetPath, relativePath);
        }



        private static void EnsureTargetDirectoryExists(string targetFile)
        {
            string? targetDirectory = Path.GetDirectoryName(targetFile);



            if (!string.IsNullOrWhiteSpace(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
        }



        private static long GetFileSize(string filePath)
        {
            return File.Exists(filePath)
            ? new FileInfo(filePath).Length
            : 0;
        }




        private static void SendCentralizedLogIfRequired(
        AppSettings settings,
        string backupName,
        string sourceFile,
        string targetFile,
        long fileSize,
        long transferTimeMs,
        long encryptionTimeMs)
        {
            if (settings.LogMode == LogMode.Local)
            {
                return;
            }



            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs,
                EncryptionTimeMs = encryptionTimeMs
            };



            _ = LogCentralizer.SendAsync(entry, Environment.MachineName);
        }
    }
}