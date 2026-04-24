using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Core_et_Model;
using EasySave.Execution;
using EasySave.UI_et_LOG;

namespace EasySave.UI_et_LOG
{
    public class ConsoleInterface
    {
        private AppSettings _settings;
        private readonly string _logDirectory;

        public ConsoleInterface()
        {
            _settings = ConfigurationManager.LoadSettings();
            _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            // Load language at startup
            LanguageManager.LoadLanguage(_settings.Language ?? "fr");
        }

        public void Run(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                ExecuteSequential(args[0]);
                return;
            }

            while (true)
            {
                ShowMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateJob(); break;
                    case "2": ExecuteSingleJob(); break;
                    case "3": ExecuteSequentialPrompt(); break;
                    case "4": ShowJobs(); break;
                    case "5": DeleteJob(); break;
                    case "6": SettingsMenu(); break;
                    case "0": return;
                    default:
                        Console.WriteLine(LanguageManager.T("InvalidChoice"));
                        Pause();
                        break;
                }
            }
        }

        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.T("AppTitle"));
            Console.WriteLine("1. " + LanguageManager.T("MenuCreate"));
            Console.WriteLine("2. " + LanguageManager.T("MenuExecute"));
            Console.WriteLine("3. " + LanguageManager.T("MenuSequential"));
            Console.WriteLine("4. " + LanguageManager.T("MenuShow"));
            Console.WriteLine("5. " + LanguageManager.T("MenuDelete"));
            Console.WriteLine("6. " + LanguageManager.T("MenuSettings"));
            Console.WriteLine("0. " + LanguageManager.T("MenuExit"));
            Console.Write(LanguageManager.T("ChoicePrompt"));
        }

        private void CreateJob()
        {
            var jobs = ConfigurationManager.LoadJobs();

            if (jobs.Count >= 5)
            {
                Console.WriteLine(LanguageManager.T("MaxJobsReached"));
                Pause();
                return;
            }

            Console.Write(LanguageManager.T("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.T("InvalidChoice"));
                Pause();
                return;
            }

            if (jobs.Any(j => j.Id == id))
            {
                Console.WriteLine(LanguageManager.T("IdUsed"));
                Pause();
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
                Pause();
                return;
            }

            Console.Write(LanguageManager.T("EnterTarget"));
            string? target = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(target))
            {
                Console.WriteLine(LanguageManager.T("TargetNotFound"));
                Pause();
                return;
            }

            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

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

            jobs.Add(job);
            ConfigurationManager.SaveJobs(jobs);

            Console.WriteLine(LanguageManager.T("JobCreated"));
            Pause();
        }

        private void ExecuteSingleJob()
        {
            var jobs = ConfigurationManager.LoadJobs();
            ShowJobs();

            Console.Write(LanguageManager.T("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.T("InvalidChoice"));
                Pause();
                return;
            }

            var job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                Console.WriteLine(LanguageManager.T("JobNotFound"));
                Pause();
                return;
            }

            bool success = ExecuteJob(job);
            Console.WriteLine(success
                ? LanguageManager.T("BackupCompleted")
                : LanguageManager.T("BackupFailed"));

            Pause();
        }

        private void ExecuteSequentialPrompt()
        {
            Console.Write(LanguageManager.T("MenuSequential") + " ");
            string? command = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(command))
            {
                Console.WriteLine(LanguageManager.T("SequenceFailed"));
                Pause();
                return;
            }

            ExecuteSequential(command);
            Pause();
        }

        private void ExecuteSequential(string command)
        {
            var jobs = ConfigurationManager.LoadJobs();
            var parser = new CommandLineParser(new[] { command });
            var ids = parser.ParseJobIds();

            bool allSuccess = true;

            foreach (var id in ids)
            {
                var job = jobs.FirstOrDefault(j => j.Id == id);
                if (job == null)
                {
                    allSuccess = false;
                    continue;
                }

                if (!ExecuteJob(job))
                    allSuccess = false;
            }

            Console.WriteLine(allSuccess
                ? LanguageManager.T("SequenceCompleted")
                : LanguageManager.T("SequenceFailed"));
        }

        private bool ExecuteJob(BackupJob job)
        {
            var logger = new EasyLog.EasyLog(_logDirectory, _settings.LogFormat);

            IBackupStrategy strategy = job.Type switch
            {
                BackupType.Differential => new DifferentialBackupStrategy(),
                _ => new CompleteBackupStrategy()
            };

            try
            {
                strategy.Execute(job, logger);
                return true;
            }
            catch
            {
                return false;
            }
        }


        private void ShowJobs()
        {
            var jobs = ConfigurationManager.LoadJobs();

            Console.WriteLine("\n--- " + LanguageManager.T("AppTitle") + " ---");
            foreach (var job in jobs)
                Console.WriteLine(job);

            Console.WriteLine("-------------------");
        }

        private void DeleteJob()
        {
            var jobs = ConfigurationManager.LoadJobs();
            ShowJobs();

            Console.Write(LanguageManager.T("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.T("InvalidChoice"));
                Pause();
                return;
            }

            int removed = jobs.RemoveAll(j => j.Id == id);
            ConfigurationManager.SaveJobs(jobs);

            Console.WriteLine(removed > 0
                ? LanguageManager.T("JobDeleted")
                : LanguageManager.T("JobNotFound"));

            Pause();
        }

        private void SettingsMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.T("ChooseLogFormat"));
            Console.Write("1 = JSON, 2 = XML : ");

            string? choice = Console.ReadLine();
            _settings.LogFormat = choice == "2" ? LogFormat.Xml : LogFormat.Json;

            ConfigurationManager.SaveSettings(_settings);

            Console.WriteLine(LanguageManager.T("SettingsSaved"));
            Pause();
        }
        private static void Pause()
        {
            Console.WriteLine(LanguageManager.T("PressAnyKey"));
            Console.ReadKey();
        }
    }
}
