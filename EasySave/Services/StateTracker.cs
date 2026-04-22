using System;
using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services
{
    public static class StateTracker
    {
        // Le fichier state.json est unique et stocké à côté de l'exécutable
        private static string _stateFilePath = Path.Combine(AppContext.BaseDirectory, "state.json");

        // === Attributs demandés par le cahier des charges ===
        public static string BackupName { get; set; } = "";
        public static DateTime LastActionTime { get; set; }
        public static string Status { get; set; } = "Non Actif"; // "Actif" ou "Non Actif" ou "Error"

        // Si le travail est actif, ces champs sont remplis :
        public static int TotalFiles { get; set; }
        public static long TotalSize { get; set; }
        public static int RemainingFiles { get; set; }
        public static long RemainingSize { get; set; }
        public static string CurrentSourceFile { get; set; } = "";
        public static string CurrentTargetFile { get; set; } = "";

        // === Méthodes ===

        // Appelé au début d'une sauvegarde
        public static void Initialize(BackupJob job, int totalFilesCount, long totalSizeBytes)
        {
            BackupName = job.Name;
            Status = "Actif";
            LastActionTime = DateTime.Now;

            TotalFiles = totalFilesCount;
            TotalSize = totalSizeBytes;
            RemainingFiles = totalFilesCount;
            RemainingSize = totalSizeBytes;

            CurrentSourceFile = "";
            CurrentTargetFile = "";

            SaveState(); // Écrit immédiatement dans le fichier
        }

        // Appelé après chaque fichier copié
        public static void UpdateProgress(string sourceFile, string targetFile, long fileSize)
        {
            LastActionTime = DateTime.Now;
            CurrentSourceFile = sourceFile;
            CurrentTargetFile = targetFile;

            // On décrémente les compteurs
            RemainingFiles = Math.Max(0, RemainingFiles - 1);
            RemainingSize = Math.Max(0, RemainingSize - fileSize);

            SaveState(); // Écrit immédiatement dans le fichier
        }

        // Appelé quand tout est terminé
        public static void MarkAsCompleted()
        {
            Status = "Non Actif";
            LastActionTime = DateTime.Now;
            CurrentSourceFile = "";
            CurrentTargetFile = "";
            SaveState();
        }

        // Appelé si une erreur survient
        public static void MarkAsError(string errorMessage)
        {
            Status = "Error";
            LastActionTime = DateTime.Now;
            SaveState();
        }

        // Écrit le fichier JSON avec retours à la ligne (pour Notepad)
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
                CurrentTargetFile
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true  // Retours à la ligne pour lisibilité Notepad
            };

            string json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(_stateFilePath, json);
        }
    }
}