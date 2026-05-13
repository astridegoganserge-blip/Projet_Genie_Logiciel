using System;
using EasyLog;
using EasySave.Core.Models;
using EasySave.Infrastructure;
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
            AppSettings settings = _settingsController.GetSettings();
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

                    case "7":
                        ExecuteParallel();
                        break;

                    case "8":
                        PauseSingleJob();
                        break;

                    case "9":
                        ResumeSingleJob();
                        break;

                    case "10":
                        StopSingleJob();
                        break;

                    case "11":
                        PauseAllJobs();
                        break;

                    case "12":
                        ResumeAllJobs();
                        break;

                    case "13":
                        StopAllJobs();
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

        private void ExecuteParallel()
        {
            JobView.ShowJobList(_jobController.GetAllJobs());

            bool success = _jobController.ExecuteParallel();

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

        private void PauseSingleJob()
        {
            JobView.ShowJobList(_jobController.GetAllJobs());

            int id = JobView.ReadJobId();

            _jobController.PauseJob(id);

            JobView.ShowInfo(LanguageManager.T("PauseRequested"));
            JobView.Pause();
        }

        private void ResumeSingleJob()
        {
            JobView.ShowJobList(_jobController.GetAllJobs());

            int id = JobView.ReadJobId();

            _jobController.ResumeJob(id);

            JobView.ShowInfo(LanguageManager.T("ResumeRequested"));
            JobView.Pause();
        }

        private void StopSingleJob()
        {
            JobView.ShowJobList(_jobController.GetAllJobs());

            int id = JobView.ReadJobId();

            _jobController.StopJob(id);

            JobView.ShowInfo(LanguageManager.T("StopRequested"));
            JobView.Pause();
        }

        private void PauseAllJobs()
        {
            _jobController.PauseAll();
            JobView.ShowInfo(LanguageManager.T("PauseAllRequested"));
            JobView.Pause();
        }

        private void ResumeAllJobs()
        {
            _jobController.ResumeAll();
            JobView.ShowInfo(LanguageManager.T("ResumeAllRequested"));
            JobView.Pause();
        }

        private void StopAllJobs()
        {
            _jobController.StopAll();
            JobView.ShowInfo(LanguageManager.T("StopAllRequested"));
            JobView.Pause();
        }

        private void UpdateSettings()
        {
            while (true)
            {
                AppSettings settings = _settingsController.GetSettings();

                SettingsView.ShowSettingsMenu(settings);

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _settingsController.UpdateLogFormat(SettingsView.AskLogFormat());
                        SettingsView.ShowSettingsSaved();
                        JobView.Pause();
                        break;

                    case "2":
                        _settingsController.UpdateLogMode(SettingsView.AskLogMode());
                        SettingsView.ShowSettingsSaved();
                        JobView.Pause();
                        break;

                    case "3":
                        _settingsController.UpdateDockerLogServerUrl(SettingsView.AskDockerUrl());
                        SettingsView.ShowSettingsSaved();
                        JobView.Pause();
                        break;

                    case "4":
                        _settingsController.UpdateBusinessSoftware(SettingsView.AskBusinessSoftware());
                        SettingsView.ShowSettingsSaved();
                        JobView.Pause();
                        break;

                    case "5":
                        _settingsController.UpdateMaxFileSizeKb(SettingsView.AskMaxFileSizeKb());
                        SettingsView.ShowSettingsSaved();
                        JobView.Pause();
                        break;

                    case "6":
                        _settingsController.UpdateDeleteOrphanFilesInDifferential(SettingsView.AskDifferentialMirror());
                        SettingsView.ShowSettingsSaved();
                        JobView.Pause();
                        break;

                    case "7":
                        if (_settingsController.AddEncryptionExtension(SettingsView.AskExtension()))
                        {
                            SettingsView.ShowSettingsSaved();
                        }
                        else
                        {
                            SettingsView.ShowExtensionAlreadyExists();
                        }

                        JobView.Pause();
                        break;

                    case "8":
                        if (_settingsController.RemoveEncryptionExtension(SettingsView.AskExtension()))
                        {
                            SettingsView.ShowSettingsSaved();
                        }
                        else
                        {
                            SettingsView.ShowExtensionNotFound();
                        }

                        JobView.Pause();
                        break;

                    case "9":
                        if (_settingsController.AddPriorityExtension(SettingsView.AskExtension()))
                        {
                            SettingsView.ShowSettingsSaved();
                        }
                        else
                        {
                            SettingsView.ShowExtensionAlreadyExists();
                        }

                        JobView.Pause();
                        break;

                    case "10":
                        if (_settingsController.RemovePriorityExtension(SettingsView.AskExtension()))
                        {
                            SettingsView.ShowSettingsSaved();
                        }
                        else
                        {
                            SettingsView.ShowExtensionNotFound();
                        }

                        JobView.Pause();
                        break;

                    case "0":
                        return;

                    default:
                        SettingsView.ShowInvalidChoice();
                        JobView.Pause();
                        break;
                }
            }
        }
    }
}