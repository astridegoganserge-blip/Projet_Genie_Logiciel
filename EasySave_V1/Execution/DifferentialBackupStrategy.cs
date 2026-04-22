using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    // EN: Strategy that copies only new or modified files
    // FR: Stratégie qui copie uniquement les fichiers nouveaux ou modifiés
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        // EN: Executes differential backup: copies only changed files
        // FR: Exécute la sauvegarde différentielle : copie uniquement les fichiers modifiés
        public void Execute(BackupJob job, EasyLog.EasyLog logger)
        {
            // EN: Get all files from source
            // FR: Récupère tous les fichiers depuis la source
            var allFiles = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);

            // EN: Filter only files that need to be copied
            // FR: Filtre uniquement les fichiers qui doivent être copiés
            var filesToCopy = allFiles.Where(f => ShouldCopyFile(f, job.SourcePath, job.TargetPath)).ToArray();
            long totalSize = filesToCopy.Sum(f => new FileInfo(f).Length);

            // EN: Initialize state with filtered file count
            // FR: Initialise l'état avec le nombre de fichiers filtrés
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

                // EN:Logger
                // FR:Logger
                logger.LogFileTransfer(job.Name, file, destFile, fileInfo.Length, transferTime);

                // EN: Update
                // FR:Mettre à jour l'état
                StateTracker.UpdateProgress(file, destFile, fileInfo.Length);
            }
            // EN: Completed
            // FR: Terminer
            StateTracker.MarkAsCompleted();
        }
        // EN: Determines if a file should be copied based on modification date
        // FR: Détermine si un fichier doit être copié selon sa date de modification
        private bool ShouldCopyFile(string sourceFile, string sourcePath, string targetPath)
        {
            string relativePath = sourceFile.Substring(sourcePath.Length).TrimStart('\\', '/');
            string destFile = Path.Combine(targetPath, relativePath);

            // EN: If file doesn't exist in target, must copy
            // FR: Si le fichier n'existe pas dans la cible, doit copier
            if (!File.Exists(destFile)) return true;

            var sourceInfo = new FileInfo(sourceFile);
            var destInfo = new FileInfo(destFile);
            // EN: Copy only if source is newer than target
            // FR: Copie uniquement si la source est plus récente que la cible
            return sourceInfo.LastWriteTime > destInfo.LastWriteTime;
        }
    }
}