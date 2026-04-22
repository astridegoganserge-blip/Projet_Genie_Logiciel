using EasySave.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Timers;

namespace EasySave.Services;


public static class StateTracker
{


    private static string _stateFilePath = Path.Combine(AppContext.BaseDirectory, "state.json");


    public static string BackupName { get; set; } = "";

    public static DateTime LastActionTime { get; set; }

    // EN: Current status: Active, Completed, or Error
    // FR: État actuel : Actif, Terminé, ou Erreur
    public static string Status { get; set; } = "Completed";

    // Si le travail est actif, ces champs sont remplis :
    public static int TotalFiles { get; set; }
    public static long TotalSize { get; set; }
    public static int RemainingFiles { get; set; }
    public static long RemainingSize { get; set; }

    // EN: Source path of the file currently being copied
    // FR: Chemin source du fichier en cours de copie
    public static string CurrentSourceFile { get; set; } = "";

    // EN: Target path of the file currently being copied
    // FR: Chemin cible du fichier en cours de copie
    public static string CurrentTargetFile { get; set; } = "";

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

        CurrentSourceFile = "";
        CurrentTargetFile = "";

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

        SaveState(); // Écrit immédiatement dans le fichier
    }

    // EN: Marks backup as completed
    // FR: Marque la sauvegarde comme terminée
    public static void MarkAsCompleted()
    {
        Status = "Completed";
        LastActionTime = DateTime.Now;
        CurrentSourceFile = "";
        CurrentTargetFile = "";
        SaveState();
    }

    // EN: Marks backup as completed
    // FR: Marque la sauvegarde comme terminée
    public static void MarkAsError(string errorMessage)
    {
        Status = "Error";
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