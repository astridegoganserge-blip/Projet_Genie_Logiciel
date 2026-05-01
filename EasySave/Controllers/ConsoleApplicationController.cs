using System;
using EasyLog;
using EasySave.Services;
using EasySave.Views;

namespace EasySave.Controllers
{
    public class ConsoleApplicationController
    {
        private readonly JobController _jobController;
        private readonly SettingsController _settingsController;

        public ConsoleApplicationController(
            JobController jobController,
            SettingsController settingsController)
        {
            _jobController = jobController;
            _settingsController = settingsController;
        }

        public void Run(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                LoadSavedLanguage();
                ExecuteCommandLine(args[0]);
                return;
            }

            InitializeLanguage();
            RunMenu();
        }

        private void InitializeLanguage()
        {
            string language = LanguageView.AskLanguage();
            _settingsController.UpdateLanguage(language);
            LanguageView.ShowLanguageSelected(language);
        }

        private void LoadSavedLanguage()
        {
            var settings = _settingsController.GetSettings();
            LanguageManager.LoadLanguage(settings.Language);
        }

        private void ExecuteCommandLine(string command)
        {
            bool commandSuccess = _jobController.ExecuteSequential(command);

            if (commandSuccess)
            {
                JobView.ShowBackupCompleted();
            }
            else
            {
                JobView.ShowBackupFailed();
            }
        }

        private void RunMenu()
        {
            while (true)
            {
                JobView.ShowMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateJob();
                        break;

                    case "2":
                        ExecuteSingleJob();
                        break;

                    case "3":
                        ExecuteSequential();
                        break;

                    case "4":
                        JobView.ShowJobList(_jobController.GetAllJobs());
                        JobView.Pause();
                        break;

                    case "5":
                        DeleteJob();
                        break;

                    case "6":
                        UpdateSettings();
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

        private void CreateJob()
        {
            var form = JobView.ReadCreateJobForm();

            bool success = _jobController.CreateJob(
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

        private void ExecuteSingleJob()
        {
            JobView.ShowJobList(_jobController.GetAllJobs());

            int id = JobView.ReadJobId();

            bool success = _jobController.ExecuteJob(id);

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

        private void ExecuteSequential()
        {
            string command = JobView.ReadSequentialCommand();

            bool success = _jobController.ExecuteSequential(command);

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

        private void DeleteJob()
        {
            JobView.ShowJobList(_jobController.GetAllJobs());

            int id = JobView.ReadJobId();

            bool success = _jobController.DeleteJob(id);

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

        private void UpdateSettings()
        {
            LogFormat format = SettingsView.AskLogFormat();
            _settingsController.UpdateLogFormat(format);
            SettingsView.ShowSettingsSaved();
            JobView.Pause();
        }
    }
}