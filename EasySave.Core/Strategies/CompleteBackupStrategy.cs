using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Core.Models;
using EasySave.Core.Services;



namespace EasySave.Core.Strategies
{
    public sealed class CompleteBackupStrategy : IBackupStrategy
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
            .OrderByDescending(file => PriorityFileFilter.IsPriorityFile(file, settings.PriorityExtensions))
            .ThenBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToArray();



            long totalSize = files.Sum(GetFileSize);
            int priorityFilesCount = files.Count(file =>
            PriorityFileFilter.IsPriorityFile(file, settings.PriorityExtensions));



            int pendingPriorityFilesForCurrentJob = priorityFilesCount;



            PriorityFileFilter.RegisterPriorityFiles(priorityFilesCount);
            StateTracker.Initialize(job, files.Length, totalSize);



            foreach (string sourceFile in files)
            {
                if (!context.WaitIfPaused())
                {
                    NotifyRemainingPriorityFiles(pendingPriorityFilesForCurrentJob);
                    StateTracker.MarkAsInterrupted(job.Name);
                    return false;
                }



                if (context.StopToken.IsCancellationRequested)
                {
                    NotifyRemainingPriorityFiles(pendingPriorityFilesForCurrentJob);
                    StateTracker.MarkAsInterrupted(job.Name);
                    return false;
                }



                if (!PriorityFileFilter.CanProcess(sourceFile, settings.PriorityExtensions, context))
                {
                    NotifyRemainingPriorityFiles(pendingPriorityFilesForCurrentJob);
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
                        PriorityFileFilter.NotifyPriorityFileCompleted();
                        pendingPriorityFilesForCurrentJob = Math.Max(
                        0,
                        pendingPriorityFilesForCurrentJob - 1);
                    }
                }
            }



            StateTracker.MarkAsCompleted(job.Name);
            return true;
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



        private static void NotifyRemainingPriorityFiles(int remainingPriorityFiles)
        {
            for (int index = 0; index < remainingPriorityFiles; index++)
            {
                PriorityFileFilter.NotifyPriorityFileCompleted();
            }
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