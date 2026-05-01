using System;
using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services
{

    public static class StateTracker
    {
        private static readonly string StateFilePath = Path.Combine(AppContext.BaseDirectory, "state.json");
        public static string BackupName { get; set; } = string.Empty;
        public static DateTime LastActionTime { get; private set; }

        // EN: Current status: Active, Completed, or Error
        // FR: État actuel : Actif, Terminé, ou Erreur
        public static string Status { get; set; } = "Terminé";

        // Si le travail est actif, ces champs sont remplis :
        public static int TotalFiles { get; private set; }
        public static long TotalSize { get; private set; }
        public static int RemainingFiles { get; private set; }
        public static long RemainingSize { get; private set; }

        // EN: Source path of the file currently being copied
        // FR: Chemin source du fichier en cours de copie
        public static string CurrentSourceFile { get; set; } = string.Empty;

        // EN: Target path of the file currently being copied
        // FR: Chemin cible du fichier en cours de copie
        public static string CurrentTargetFile { get; set; } = string.Empty;

        public static double Progression { get; private set; }

        // EN: Initializes state at the beginning of a backup operation
        // FR: Initialise l'état au début d'une opération de sauvegarde
        public static void Initialize(BackupJob job, int totalFilesCount, long totalSizeBytes)
        {
            BackupName = job.Name;
            Status = "Actif";
            LastActionTime = DateTime.Now;

            TotalFiles = totalFilesCount;
            TotalSize = totalSizeBytes;
            RemainingFiles = totalFilesCount;
            RemainingSize = totalSizeBytes;

            CurrentSourceFile = string.Empty;
            CurrentTargetFile = string.Empty;
            Progression = 0.0;

            SaveState(); // Écrit immédiatement dans le fichier
        }

        // EN: Updates progress after each file transfer
        // FR: Met à jour la progression après chaque transfert de fichier
        public static void UpdateProgress(string sourceFile, string targetFile, long fileSize)
        {
            LastActionTime = DateTime.Now;
            CurrentSourceFile = sourceFile;
            CurrentTargetFile = targetFile;

            // On décrémente les compteurs
            RemainingFiles = Math.Max(0, RemainingFiles - 1);
            RemainingSize = Math.Max(0, RemainingSize - fileSize);

            if (TotalFiles > 0)
            {
                Progression = Math.Round((TotalFiles - RemainingFiles) * 100.0 / TotalFiles, 1);
            }
            else
            {
                Progression = 100.0;
            }

            SaveState(); // Écrit immédiatement dans le fichier
        }

        // EN: Marks backup as completed
        // FR: Marque la sauvegarde comme terminée
        public static void MarkAsCompleted()
        {
            Status = "Terminé";
            LastActionTime = DateTime.Now;
            CurrentSourceFile = string.Empty;
            CurrentTargetFile = string.Empty;
            Progression = 100.0;
            SaveState();
        }

        // EN: Marks backup as completed
        // FR: Marque la sauvegarde comme terminée
        public static void MarkAsError(string errorMessage)
        {
            Status = "Erreur";
            LastActionTime = DateTime.Now;
            SaveState();
        }

        // EN: Writes current state to JSON file with indentation for Notepad readability
        // FR: Écrit l'état actuel dans le fichier JSON avec indentation pour lisibilité Notepad
        public static void SaveState()
        {
            var state = new
            {
                BackupName,
                LastActionTime,
                Status,
                TotalFiles,
                TotalSize,
                RemainingFiles,
                RemainingSize,
                CurrentSourceFile,
                CurrentTargetFile,
                Progression
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true  // Retours à la ligne pour lisibilité Notepad
            };

            string json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(StateFilePath, json);
        }
    }
}