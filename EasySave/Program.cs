using System;
using System.IO;
using EasyLog;
using EasySave.Controllers;
using EasySave.Repositories;
using EasySave.Views;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var settingsRepository = new JsonSettingsRepository();
            var settingsController = new SettingsController(settingsRepository);

            string language = LanguageView.AskLanguage();
            settingsController.UpdateLanguage(language);
            LanguageView.ShowLanguageSelected(language);

            string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            var jobRepository = new JsonJobRepository();
            var jobController = new JobController(jobRepository, settingsRepository, logDirectory);

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                bool commandSuccess = jobController.ExecuteSequential(args[0]);

                if (commandSuccess)
                {
                    JobView.ShowBackupCompleted();
                }
                else
                {
                    JobView.ShowBackupFailed();
                }

                return;
            }

            RunMenu(jobController, settingsController);
        }

        private static void RunMenu(JobController jobController, SettingsController settingsController)
        {
            while (true)
            {
                JobView.ShowMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateJob(jobController);
                        break;

                    case "2":
                        ExecuteSingleJob(jobController);
                        break;

                    case "3":
                        ExecuteSequential(jobController);
                        break;

                    case "4":
                        JobView.ShowJobList(jobController.GetAllJobs());
                        JobView.Pause();
                        break;

                    case "5":
                        DeleteJob(jobController);
                        break;

                    case "6":
                        UpdateSettings(settingsController);
                        break;

                    case "0":
                        return;

                    default:
                        JobView.ShowError("Invalid choice / Choix invalide");
                        JobView.Pause();
                        break;
                }
            }
        }

        private static void CreateJob(JobController jobController)
        {
            var form = JobView.ReadCreateJobForm();

            bool success = jobController.CreateJob(
                form.id,
                form.name,
                form.source,
                form.target,
                form.type);

            if (success)
            {
                JobView.ShowJobCreated();
            }
            else
            {
                JobView.ShowError("Unable to create job. Check id, paths, or maximum number of jobs.");
            }

            JobView.Pause();
        }

        private static void ExecuteSingleJob(JobController jobController)
        {
            JobView.ShowJobList(jobController.GetAllJobs());

            int id = JobView.ReadJobId();

            bool success = jobController.ExecuteJob(id);

            if (success)
            {
                JobView.ShowBackupCompleted();
            }
            else
            {
                JobView.ShowBackupFailed();
            }

            JobView.Pause();
        }

        private static void ExecuteSequential(JobController jobController)
        {
            string command = JobView.ReadSequentialCommand();

            bool success = jobController.ExecuteSequential(command);

            if (success)
            {
                JobView.ShowBackupCompleted();
            }
            else
            {
                JobView.ShowBackupFailed();
            }

            JobView.Pause();
        }

        private static void DeleteJob(JobController jobController)
        {
            JobView.ShowJobList(jobController.GetAllJobs());

            int id = JobView.ReadJobId();

            bool success = jobController.DeleteJob(id);

            if (success)
            {
                JobView.ShowJobDeleted();
            }
            else
            {
                JobView.ShowJobNotFound();
            }

            JobView.Pause();
        }

        private static void UpdateSettings(SettingsController settingsController)
        {
            LogFormat format = SettingsView.AskLogFormat();
            settingsController.UpdateLogFormat(format);
            SettingsView.ShowSettingsSaved();
            JobView.Pause();
        }
    }
}