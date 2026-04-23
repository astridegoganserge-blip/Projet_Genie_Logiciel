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
        private readonly BackupManager _manager;
        private readonly EasyLog.EasyLog _logger;

        public ConsoleInterface()
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "logs");
            _logger = new EasyLog.EasyLog(logPath);
            _manager = new BackupManager();
        }

        public void Run(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                RunCommandLine(args[0]);
                return;
            }

            while (true)
            {
                ShowMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateJob(); break;
                    case "2": ExecuteJob(); break;
                    case "3": ExecuteSequential(); break;
                    case "4": ShowJobs(); break;
                    case "5": DeleteJob(); break;
                    case "0": return;
                    default:
                        Console.WriteLine(LanguageManager.T("InvalidChoice"));
                        Console.WriteLine(LanguageManager.T("PressAnyKey"));
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void RunCommandLine(string command)
        {
            bool success = _manager.ExecuteSequential(command, _logger);

            Console.WriteLine(success
                ? LanguageManager.T("BackupCompleted")
                : LanguageManager.T("BackupFailed"));
        }

        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.T("AppTitle"));
            Console.WriteLine(LanguageManager.T("MenuCreate"));
            Console.WriteLine(LanguageManager.T("MenuExecute"));
            Console.WriteLine(LanguageManager.T("MenuSequential"));
            Console.WriteLine(LanguageManager.T("MenuShow"));
            Console.WriteLine(LanguageManager.T("MenuDelete"));
            Console.WriteLine(LanguageManager.T("MenuExit"));
            Console.Write(LanguageManager.T("ChoicePrompt"));
        }

        private void CreateJob()
        {
            if (_manager.Jobs.Count >= 5)
            {
                Console.WriteLine(LanguageManager.T("MaxJobsReached"));
                Console.WriteLine(LanguageManager.T("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            Console.Write(LanguageManager.T("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.T("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            if (!_manager.IsJobIdAvailable(id))
            {
                Console.WriteLine(LanguageManager.T("IdUsed"));
                Console.WriteLine(LanguageManager.T("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            Console.Write(LanguageManager.T("EnterName"));
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) name = "Unnamed";

            Console.Write(LanguageManager.T("EnterSource"));
            string? source = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
            {
                Console.WriteLine(LanguageManager.T("SourceNotFound"));
                Console.ReadKey();
                return;
            }

            Console.Write(LanguageManager.T("EnterTarget"));
            string? target = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(target))
            {
                Console.WriteLine(LanguageManager.T("TargetNotFound"));
                Console.ReadKey();
                return;
            }

            if (!Directory.Exists(target))
            {
                try
                {
                    Directory.CreateDirectory(target);
                    Console.WriteLine(LanguageManager.T("TargetCreated"));
                }
                catch
                {
                    Console.WriteLine(LanguageManager.T("TargetNotFound"));
                    Console.ReadKey();
                    return;
                }
            }

            Console.Write(LanguageManager.T("EnterType"));
            string? typeChoice = Console.ReadLine();

            var job = new BackupJob
            {
                Id = id,
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = typeChoice == "2" ? BackupType.Differential : BackupType.Complete
            };

            Console.WriteLine(_manager.AddJob(job)
                ? LanguageManager.T("JobCreated")
                : LanguageManager.T("BackupFailed"));

            Console.WriteLine(LanguageManager.T("PressAnyKey"));
            Console.ReadKey();
        }

        private void ExecuteJob()
        {
            ShowJobs();
            Console.Write(LanguageManager.T("EnterJobId"));

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.T("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            Console.WriteLine(_manager.ExecuteJob(id, _logger)
                ? LanguageManager.T("BackupCompleted")
                : LanguageManager.T("BackupFailed"));

            Console.WriteLine(LanguageManager.T("PressAnyKey"));
            Console.ReadKey();
        }

        private void ExecuteSequential()
        {
            Console.Write(LanguageManager.T("MenuSequential"));
            Console.Write(" ");

            string? command = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(command))
            {
                Console.WriteLine(LanguageManager.T("SequenceFailed"));
                Console.ReadKey();
                return;
            }

            Console.WriteLine(_manager.ExecuteSequential(command, _logger)
                ? LanguageManager.T("SequenceCompleted")
                : LanguageManager.T("SequenceFailed"));

            Console.WriteLine(LanguageManager.T("PressAnyKey"));
            Console.ReadKey();
        }

        private void ShowJobs()
        {
            var jobs = _manager.GetAllJobs();
            Console.WriteLine("\n--- " + LanguageManager.T("AppTitle") + " ---");

            foreach (var job in jobs)
                Console.WriteLine(job);

            Console.WriteLine("-------------------");
            Console.WriteLine(LanguageManager.T("PressAnyKey"));
            Console.ReadKey();
        }

        private void DeleteJob()
        {
            ShowJobs();
            Console.Write(LanguageManager.T("EnterJobId"));

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.T("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            Console.WriteLine(_manager.RemoveJob(id)
                ? LanguageManager.T("JobDeleted")
                : LanguageManager.T("JobNotFound"));

            Console.WriteLine(LanguageManager.T("PressAnyKey"));
            Console.ReadKey();
        }
    }
}
