using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class CompleteBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, EasyLog.EasyLog logger)
        {
            // 1. Récupérer tous les fichiers récursivement
            var files = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);
            long totalSize = files.Sum(f => new FileInfo(f).Length);

            // 2. Initialiser le state avec les vrais chiffres
            StateTracker.Initialize(job, files.Length, totalSize);

            // 3. Boucle de copie
            foreach (var file in files)
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

                // 4. Logger dans EasyLog
                logger.LogFileTransfer(job.Name, file, destFile, fileInfo.Length, transferTime);

                // 5. Mettre à jour l'état temps réel
                StateTracker.UpdateProgress(file, destFile, fileInfo.Length);
            }

            // 6. Marquer comme terminé
            StateTracker.MarkAsCompleted();
        }
    }
}