using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    // EN: Strategy that copies all files regardless of previous state
    // FR: Stratégie qui copie tous les fichiers indépendamment de l'état précédent
    public class CompleteBackupStrategy : IBackupStrategy
    {
        // EN: Executes complete backup: copies every file from source to target
        // FR: Exécute la sauvegarde complète : copie chaque fichier de source vers cible
        public void Execute(BackupJob job, EasyLog.EasyLog logger)

        {
            // Sécurisation : éviter un crash si les chemins sont null
            if (string.IsNullOrWhiteSpace(job.SourcePath) || string.IsNullOrWhiteSpace(job.TargetPath))
                throw new ArgumentException("SourcePath or TargetPath cannot be null.");

            // EN: Get all files recursively from source directory
            // FR: Récupère tous les fichiers récursivement depuis le répertoire source
            var files = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories);
            long totalSize = files.Sum(f => new FileInfo(f).Length);

            // EN: Set total counters for progress tracking
            // FR: Définit les compteurs totaux pour le suivi de progression
            StateTracker.Initialize(job, files.Length, totalSize);

            // EN: Loop through each file and copy it
            // FR: Boucle sur chaque fichier et le copie
            foreach (var file in files)
            {
                string relativePath = file.Substring(job.SourcePath.Length).TrimStart('\\', '/');
                string destFile = Path.Combine(job.TargetPath, relativePath);
                string? destDir = Path.GetDirectoryName(destFile);
                if (string.IsNullOrWhiteSpace(destDir))
                    continue;

                // EN: Create target directory if it doesn't exist
                // FR: Crée le répertoire cible s'il n'existe pas
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                // EN: Measure transfer time
                // FR: Mesure le temps de transfert
                var startTime = DateTime.Now;
                File.Copy(file, destFile, true);
                var transferTime = (long)(DateTime.Now - startTime).TotalMilliseconds;

                var fileInfo = new FileInfo(file);


                // EN: Log the file transfer to daily JSON log
                // FR: Journalise le transfert de fichier dans le log JSON journalier
                logger.LogFileTransfer(job.Name, file, destFile, fileInfo.Length, transferTime);

                // EN: Update real-time progress state
                // FR: Met à jour l'état de progression temps réel
                StateTracker.UpdateProgress(file, destFile, fileInfo.Length);
            }

            // EN: Mark backup as completed
            // FR: Marque la sauvegarde comme terminée
            StateTracker.MarkAsCompleted();
        }
    }
}