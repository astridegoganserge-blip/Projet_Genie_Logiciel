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
            AppSettings settings)
        {
            if (!job.ValidatePaths())
            {
                return false;
            }

            if (BusinessSoftwareWatcher.IsRunning(settings.BusinessSoftware))
            {
                logger.LogFileTransfer(job.Name, string.Empty, string.Empty, 0, -1, 0);
                return false;
            }

            string[] files = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string relativePath = file.Substring(job.SourcePath.Length).TrimStart('\\', '/');
                string targetFile = Path.Combine(job.TargetPath, relativePath);
                string? targetDirectory = Path.GetDirectoryName(targetFile);

                if (!string.IsNullOrWhiteSpace(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                try
                {
                    DateTime startTime = DateTime.Now;

                    File.Copy(file, targetFile, true);

                    long transferTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds;
                    long encryptionTimeMs = CryptoService.Encrypt(targetFile, settings.ExtensionsToEncrypt);
                    long fileSize = new FileInfo(file).Length;

                    logger.LogFileTransfer(
                        job.Name,
                        file,
                        targetFile,
                        fileSize,
                        transferTimeMs,
                        encryptionTimeMs);
                }
                catch
                {
                    long fileSize = File.Exists(file) ? new FileInfo(file).Length : 0;

                    logger.LogFileTransfer(
                        job.Name,
                        file,
                        targetFile,
                        fileSize,
                        -1,
                        0);

                    return false;
                }

                if (BusinessSoftwareWatcher.IsRunning(settings.BusinessSoftware))
                {
                    logger.LogFileTransfer(job.Name, file, targetFile, 0, -1, 0);
                    return false;
                }
            }

            return true;
        }
    }
}