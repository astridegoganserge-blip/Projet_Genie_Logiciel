hellowerld
hellowerld.
En ligne

camixoxo — 15/04/2026 10:40
https://apprendre-la-programmation.net/design-pattern/
Comprendre les Design Patterns | Apprendre la programmation
Cliquez ici pour comprendre et utiliser les design patterns. Il en existe 23 et je vous en conseille quelques-uns sur lesquels vous focalisez.
Comprendre les Design Patterns | Apprendre la programmation
hellowerld — 15/04/2026 10:40
https://docs.google.com/document/d/17GRmgNyABfMj7abzjDa7j-qlbFSBrS6PfJlYf11yhnA/edit?usp=sharing
Google Docs
Design Pattern : Structure
camixoxo — 15/04/2026 11:05
https://fr.scribd.com/document/888259876/Design-Pattern-Structuraux
Scribd
Introduction aux Design Patterns Structuraux | PDF | Interface (Inf...
Le document présente plusieurs patrons de conception structurels, notamment l'Adaptateur, le Pont, le Composite, le Décorateur et la Façade. Chaque patron est expliqué avec des exemples de problèmes qu'ils résolvent et des solutions qu'ils offrent, mettant en avant leurs avantages et inconvénients. Ces patrons visent à améliorer la stru...
Image
camixoxo — 19/04/2026 00:00
@hellowerld hello
how are you
did you do any thing  today concerning the project?
camixoxo — 10:48
LES CODES POUR TES DOSSIERS
EasyLog
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    public class EasyLog
    {
        private string _logDirectory;

        public EasyLog(string logDirectory)
        {
            _logDirectory = logDirectory;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void LogFileTransfer(string backupName, string sourceFile, string targetFile, long fileSize, long transferTimeMs)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs
            };

            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            string filePath = Path.Combine(_logDirectory, fileName);

            List<LogEntry> entries = new List<LogEntry>();

            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    entries = JsonSerializer.Deserialize<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
            }

            entries.Add(entry);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(entries, options);
            File.WriteAllText(filePath, json);
        }
    }
}
LogEntry
using System;

namespace EasyLog
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string BackupName { get; set; }
        public string SourceFile { get; set; }
        public string TargetFile { get; set; }
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
    }
}
ConsoleInterface
using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Managers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.UI
{
    public class ConsoleInterface
    {
        private BackupManager _manager;
        private EasyLog.EasyLog _logger;

        public ConsoleInterface()
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "logs");
            _logger = new EasyLog.EasyLog(logPath);
            _manager = new BackupManager();
        }

        public void Run(string[] args)
        {
            if (args.Length > 0)
            {
                RunCommandLine(args[0]);
                return;
            }

            while (true)
            {
                ShowMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateJob(); break;
                    case "2": ExecuteJob(); break;
                    case "3": ExecuteSequential(); break;
                    case "4": ShowJobs(); break;
                    case "5": DeleteJob(); break;
                    case "0": return;
                    default:
                        Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                        Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void RunCommandLine(string command)
        {
            bool success = _manager.ExecuteSequential(command, _logger);

            if (success)
                Console.WriteLine(LanguageManager.GetString("Backup Completed"));
            else
                Console.WriteLine(LanguageManager.GetString("Backup Failed"));
        }
        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.GetString("AppTitle"));
            Console.WriteLine(LanguageManager.GetString("MenuCreate"));
            Console.WriteLine(LanguageManager.GetString("MenuExecute"));
            Console.WriteLine(LanguageManager.GetString("MenuSequential"));
            Console.WriteLine(LanguageManager.GetString("MenuShow"));
            Console.WriteLine(LanguageManager.GetString("MenuDelete"));
            Console.WriteLine(LanguageManager.GetString("MenuExit"));
            Console.Write(LanguageManager.GetString("ChoicePrompt"));
        }

        private void CreateJob()
        {
            if (_manager.Jobs.Count >= 5)
            {
                Console.WriteLine(LanguageManager.GetString("MaxJobsReached"));
                Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            Console.Write(LanguageManager.GetString("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            if (!_manager.IsJobIdAvailable(id))
            {
                Console.WriteLine(LanguageManager.GetString("IdUsed"));
                Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                Console.ReadKey();
                return;
            }

... (125lignes restantes)

message.txt
9 Ko
ceci cest pour ConsoleInterface👆
LanguageManager
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Services;

public static class LanguageManager
{
    private static Dictionary<string, string> _currentTranslations = new Dictionary<string, string>();
    private static string _currentLanguage = "fr";

    public static void LoadLanguage(string languageCode)
    {
        _currentLanguage = languageCode;
        string fileName = languageCode == "en" ? "en.json" : "fr.json";
        string filePath = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            _currentTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
        }
    }

    public static string GetString(string key)
    {
        if (_currentTranslations.ContainsKey(key))
            return _currentTranslations[key];

        return $"[{key}]"; // Retourne la clé si traduction manquante
    }

    public static string CurrentLanguage => _currentLanguage;
}
program
using System;
using EasySave.Services;
using EasySave.UI;

namespace EasySave;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("\t \t Bienvenue sur le logiciel EasySave 1.0\n");
        Console.WriteLine("Veuillez sélectionner la langue d'affichage / Please select your display language: ");
        Console.WriteLine("1. Français");
        Console.WriteLine("2. English");
        Console.Write("Saisissez votre choix (1/2) :  ");

        string choice = Console.ReadLine();
        string lang = choice == "2" ? "en" : "fr";

        LanguageManager.LoadLanguage(lang);

        var ui = new ConsoleInterface();
        ui.Run(args);
    }
}
camixoxo — 10:55
en.json
{
  "AppTitle": "=== EasySave 1.0 ===",
  "MenuCreate": "1. Create new backup job",
  "MenuExecute": "2. Execute a backup job",
  "MenuSequential": "3. Sequential execution (e.g. 1-3 or 1;3)",
  "MenuShow": "4. Show all jobs",
  "MenuDelete": "5. Delete a job",
  "MenuExit": "0. Exit application",
  "ChoicePrompt": "Enter your selection: ",
  "InvalidChoice": "Unrecognized option. Please select a valid entry.",
  "MaxJobsReached": "Maximum 5 jobs reached!",
  "EnterJobId": "Job identifier (1 to 5):  ",
  "IdUsed": "This identifier is already assigned to an existing job.",
  "EnterName": "Job name: ",
  "EnterSource": "Source directory (local, external or UNC path): ",
  "EnterTarget": "Target directory (local, external or UNC path): ",
  "SourceNotFound": "Source path not found!",
  "TargetNotFound": "Target path not found!",
  "TargetCreated": "Target directory created.",
  "EnterType": "Backup Type (1=Complete, 2=Differential): ",
  "JobCreated": "Job created successfully!",
  "JobDeleted": "The backup job has been deleted.",
  "JobNotFound": "No job matches this identifier.",
  "BackupCompleted": "Backup completed!",
  "BackupFailed": "Backup failed!",
  "SequenceCompleted": "Backup sequence completed successfully.",
  "SequenceFailed": "The backup sequence failed. Check the job status and execution logs.",
  "PressAnyKey": "Press any key to continue..."
}
fr.json
{
  "AppTitle": " EasySave 1.0 ",
  "MenuCreate": "1. Créer un travail de sauvegarde",
  "MenuExecute": "2. Exécuter un travail",
  "MenuSequential": "3. Exécution séquentielle (ex: 1-3 ou 1;3)",
  "MenuShow": "4. Liste des travaux configurés",
  "MenuDelete": "5. Supprimer un travail",
  "MenuExit": "0. Quitter l'application",
  "ChoicePrompt": "Saisissez votre choix :  ",
  "InvalidChoice": "Option non reconnue. Veuillez sélectionner une entrée valide.",
  "MaxJobsReached": "La limite de 5 travaux de sauvegarde est atteinte. Supprimez un travail existant avant d'en créer un nouveau.",
  "EnterJobId": "Identifiant du travail (1 à 5) : ",
  "IdUsed": "Cet identifiant est déjà attribué à un travail existant.",
  "EnterName": "Nom du travail :",
  "EnterSource": "Répertoire source (chemin local, externe ou UNC) : ",
  "EnterTarget": "Répertoire cible (chemin local, externe ou UNC) : ",
  "SourceNotFound": "Chemin source introuvable !",
  "TargetNotFound": "Chemin cible introuvable !",
  "TargetCreated": "Répertoire cible créé.",
  "EnterType": "Type de sauvegarde (1=Complète, 2=Différentielle) : ",
  "JobCreated": "Travail de sauvegarde créé avec succès !",
  "JobDeleted": "Travail de sauvegarde supprimé !",
  "JobNotFound": "Aucun travail ne correspond à cet identifiant.",
  "BackupCompleted": "Sauvegarde terminée !",
  "BackupFailed": "Échec de la sauvegarde !",
  "SequenceCompleted": "Séquence de sauvegardes terminée avec succès.",
  "SequenceFailed": "La séquence de sauvegardes a échoué.",
  "PressAnyKey": "Appuyez sur une touche..."
}
﻿
camixoxo
anelka2305
using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Managers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.UI
{
    public class ConsoleInterface
    {
        private BackupManager _manager;
        private EasyLog.EasyLog _logger;

        public ConsoleInterface()
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "logs");
            _logger = new EasyLog.EasyLog(logPath);
            _manager = new BackupManager();
        }

        public void Run(string[] args)
        {
            if (args.Length > 0)
            {
                RunCommandLine(args[0]);
                return;
            }

            while (true)
            {
                ShowMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateJob(); break;
                    case "2": ExecuteJob(); break;
                    case "3": ExecuteSequential(); break;
                    case "4": ShowJobs(); break;
                    case "5": DeleteJob(); break;
                    case "0": return;
                    default:
                        Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                        Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void RunCommandLine(string command)
        {
            bool success = _manager.ExecuteSequential(command, _logger);

            if (success)
                Console.WriteLine(LanguageManager.GetString("Backup Completed"));
            else
                Console.WriteLine(LanguageManager.GetString("Backup Failed"));
        }
        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.GetString("AppTitle"));
            Console.WriteLine(LanguageManager.GetString("MenuCreate"));
            Console.WriteLine(LanguageManager.GetString("MenuExecute"));
            Console.WriteLine(LanguageManager.GetString("MenuSequential"));
            Console.WriteLine(LanguageManager.GetString("MenuShow"));
            Console.WriteLine(LanguageManager.GetString("MenuDelete"));
            Console.WriteLine(LanguageManager.GetString("MenuExit"));
            Console.Write(LanguageManager.GetString("ChoicePrompt"));
        }

        private void CreateJob()
        {
            if (_manager.Jobs.Count >= 5)
            {
                Console.WriteLine(LanguageManager.GetString("MaxJobsReached"));
                Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            Console.Write(LanguageManager.GetString("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            if (!_manager.IsJobIdAvailable(id))
            {
                Console.WriteLine(LanguageManager.GetString("IdUsed"));
                Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            Console.Write(LanguageManager.GetString("EnterName"));
            string name = Console.ReadLine();
            Console.Write(LanguageManager.GetString("EnterSource"));
            string source = Console.ReadLine();

            // EN: Verify source path exists before accepting
            // FR: Vérifie que le chemin source existe avant d'accepter
            if (!System.IO.Directory.Exists(source))
            {
                Console.WriteLine(LanguageManager.GetString("SourceNotFound"));
                Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            Console.Write(LanguageManager.GetString("EnterTarget"));
            string target = Console.ReadLine();

            // EN: Verify target path exists or can be created
            // FR: Vérifie que le chemin cible existe ou peut être créé
            if (!System.IO.Directory.Exists(target))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(target);
                    Console.WriteLine(LanguageManager.GetString("TargetCreated"));
                }
                catch
                {
                    Console.WriteLine(LanguageManager.GetString("TargetNotFound"));
                    Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                    Console.ReadKey();
                    return;
                }
            }
            Console.Write(LanguageManager.GetString("EnterType"));
            string typeChoice = Console.ReadLine();

            var job = new BackupJob
            {
                Id = id,
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = typeChoice == "2" ? BackupType.Differential : BackupType.Complete
            };

            if (_manager.AddJob(job))
                Console.WriteLine(LanguageManager.GetString("JobCreated"));
            else
                Console.WriteLine(LanguageManager.GetString("BackupFailed"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        private void ExecuteJob()
        {
            ShowJobs();
            Console.Write(LanguageManager.GetString("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            if (_manager.ExecuteJob(id, _logger))
                Console.WriteLine(LanguageManager.GetString("BackupCompleted"));
            else
                Console.WriteLine(LanguageManager.GetString("BackupFailed"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        private void ExecuteSequential()
        {
            Console.Write(LanguageManager.GetString("MenuSequential"));
            Console.Write(" ");
            string command = Console.ReadLine();

            if (_manager.ExecuteSequential(command, _logger))
                Console.WriteLine(LanguageManager.GetString("SequenceCompleted"));
            else
                Console.WriteLine(LanguageManager.GetString("SequenceFailed"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        private void ShowJobs()
        {
            var jobs = _manager.GetAllJobs();
            Console.WriteLine("\n--- " + LanguageManager.GetString("AppTitle") + " ---");
            foreach (var job in jobs)
            {
                Console.WriteLine(job.ToString());
            }
            Console.WriteLine("-------------------");
            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        private void DeleteJob()
        {
            ShowJobs();
            Console.Write(LanguageManager.GetString("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            if (_manager.RemoveJob(id))
                Console.WriteLine(LanguageManager.GetString("JobDeleted"));
            else
                Console.WriteLine(LanguageManager.GetString("JobNotFound"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }
    }
}
message.txt
9 Ko