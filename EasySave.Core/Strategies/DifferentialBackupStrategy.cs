using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasySave.Core.Models;
using EasySave.Core.Services;



namespace EasySave.Core.Strategies
{
    /// <summary>
    /// Differential backup: copies only files that are new or have changed
    /// since the last backup. Optionally deletes orphan files from the target
    /// (files present in target but no longer in source).
    /// All shared logic (priority files, pause/stop, throttle, encrypt, log,
    /// state tracking) lives in BackupStrategyBase.
    /// </summary>
    public sealed class DifferentialBackupStrategy : BackupStrategyBase
    {
        protected override IEnumerable<string> SelectSourceFiles(
            BackupJob job,
            AppSettings settings)
        {
            return Directory
                .GetFiles(job.SourcePath, "*", SearchOption.AllDirectories)
                .Where(file => ShouldCopyFile(file, job.SourcePath, job.TargetPath));
        }



        protected override bool RunPostCopyStep(
            BackupJob job,
            AppSettings settings,
            EasyLog.EasyLog logger,
            JobExecutionContext context)
        {
            if (!settings.DeleteOrphanFilesInDifferential)
            {
                return true;
            }

            return DeleteOrphanFiles(job, settings, logger, context);
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
                if (!context.WaitIfPaused() || context.StopToken.IsCancellationRequested)
                {
                    return false;
                }

                string relativePath = Path.GetRelativePath(job.TargetPath, targetFile);
                string expectedSourceFile = Path.Combine(job.SourcePath, relativePath);

                if (File.Exists(expectedSourceFile))
                {
                    continue;
                }

                if (!TryDeleteOrphan(job, settings, logger, targetFile))
                {
                    return false;
                }
            }

            RemoveEmptyDirectories(job.TargetPath);
            return true;
        }



        private static bool TryDeleteOrphan(
            BackupJob job,
            AppSettings settings,
            EasyLog.EasyLog logger,
            string targetFile)
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

                return true;
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



        private static void RemoveEmptyDirectories(string targetPath)
        {
            foreach (string directory in Directory
                .GetDirectories(targetPath, "*", SearchOption.AllDirectories)
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
                        // Best-effort cleanup of empty target subdirectories.
                    }
                }
            }
        }
    }
}