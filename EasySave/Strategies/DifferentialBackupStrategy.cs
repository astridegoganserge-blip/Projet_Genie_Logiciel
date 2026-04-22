using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, EasyLog.EasyLog logger)
        {
            // 1. Récupérer les fichiers qui doivent vraiment être copiés
            var allFiles = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);
            var filesToCopy = allFiles.Where(f => ShouldCopyFile(f, job.SourcePath, job.TargetPath)).ToArray();
            long totalSize = filesToCopy.Sum(f => new FileInfo(f).Length);

            // 2. Initialiser le state
            StateTracker.Initialize(job, filesToCopy.Length, totalSize);

            // 3. Boucle de copie
            foreach (var file in filesToCopy)
            {
                string relativePath = file.Substring(job.SourcePath.Length).TrimStart('\\', '/');
                string destFile = Path.Combine(job.TargetPath, relativePath);
                string destDir = Path.GetDirectoryName(destFile);

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                var startTime = DateTime.Now;
                File.Copy(file, destFile, true);
                var transferTime = (long)(DateTime.Now - startTime).TotalMilliseconds;

                var fileInfo = new FileInfo(file);

                // 4. Logger
                logger.LogFileTransfer(job.Name, file, destFile, fileInfo.Length, transferTime);

                // 5. Mettre à jour l'état
                StateTracker.UpdateProgress(file, destFile, fileInfo.Length);
            }

            // 6. Terminer
            StateTracker.MarkAsCompleted();
        }

        private bool ShouldCopyFile(string sourceFile, string sourcePath, string targetPath)
        {
            string relativePath = sourceFile.Substring(sourcePath.Length).TrimStart('\\', '/');
            string destFile = Path.Combine(targetPath, relativePath);

            if (!File.Exists(destFile)) return true;

            var sourceInfo = new FileInfo(sourceFile);
            var destInfo = new FileInfo(destFile);

            return sourceInfo.LastWriteTime > destInfo.LastWriteTime;
        }
    }
}