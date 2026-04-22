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
            _manager.ExecuteSequential(command, _logger);
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
            Console.Write(LanguageManager.GetString("EnterTarget"));
            string target = Console.ReadLine();
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