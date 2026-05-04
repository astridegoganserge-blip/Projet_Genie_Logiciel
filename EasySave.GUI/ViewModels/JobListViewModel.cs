using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyLog;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Core.Services;

namespace EasySave.GUI.ViewModels
{
    public class JobListViewModel : BaseViewModel
    {
        private string _statusMessage = "Ready";
        private bool _isExecuting;
        private readonly BackupManager _backupManager;


        public JobListViewModel()
        {
            _backupManager = new BackupManager(new JsonJobRepository(), new JsonSettingsRepository());

            Jobs = new ObservableCollection<BackupJob>();

            RefreshCommand = new RelayCommand(_ => RefreshJobs());
            ExecuteJobCommand = new RelayCommand(parameter => ExecuteSelectedJob(parameter), _ => !IsExecuting);
            ExecuteAllCommand = new RelayCommand(_ => ExecuteAllJobs(), _ => Jobs.Count > 0 && !IsExecuting);
            DeleteJobCommand = new RelayCommand(_ => DeleteSelectedJob(), _ => SelectedJob != null && !IsExecuting);
            CreateJobCommand = new RelayCommand(_ => CreateJob(), _ => !IsExecuting);

            LoadJobs();
        }

        public ObservableCollection<BackupJob> Jobs { get; }

        private BackupJob? _selectedJob;

        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged();
                ExecuteJobCommand.RaiseCanExecuteChanged();
                DeleteJobCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            set
            {
                _isExecuting = value;
                OnPropertyChanged();
                ExecuteJobCommand.RaiseCanExecuteChanged();
                DeleteJobCommand.RaiseCanExecuteChanged();
                CreateJobCommand.RaiseCanExecuteChanged();
                ExecuteAllCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand RefreshCommand { get; }

        public RelayCommand ExecuteJobCommand { get; }

        public RelayCommand ExecuteAllCommand { get; }

        public RelayCommand DeleteJobCommand { get; }

        public RelayCommand CreateJobCommand { get; }

        private void LoadJobs(bool updateStatusMessage = true)
        {
            Jobs.Clear();

            var jobs = _backupManager
                .GetAllJobs()
                .OrderBy(job => job.Number)
                .ToList();

            foreach (BackupJob job in jobs)
            {
                Jobs.Add(job);
            }

            if (updateStatusMessage)
            {
                StatusMessage = $"{Jobs.Count} job(s) loaded from {AppContext.BaseDirectory}";
            }
        }

        private void RefreshJobs()
        {
            LoadJobs();
            SelectedJob = null;
            StatusMessage = $"{Jobs.Count} job(s) refreshed.";
        }

        private async void ExecuteSelectedJob(object? parameter)
        {
            BackupJob? jobToExecute = parameter as BackupJob ?? SelectedJob;

            if (jobToExecute == null)
            {
                StatusMessage = "Please select a backup job before execution.";
                return;
            }

            SelectedJob = jobToExecute;

            var settings = _backupManager.GetSettings();

            if (BusinessSoftwareWatcher.IsRunning(settings.BusinessSoftware))
            {
                string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                var logger = new EasyLog.EasyLog(logDirectory, settings.LogFormat);

                logger.LogFileTransfer(
                    jobToExecute.Name,
                    $"Business software detected: {settings.BusinessSoftware}",
                    string.Empty,
                    0,
                    -1,
                    0);

                StatusMessage = $"Execution blocked: business software is running ({settings.BusinessSoftware})";
                return;
            }

            IsExecuting = true;
            StatusMessage = $"Executing: {jobToExecute.Name}";

            try
            {
                bool success = await Task.Run(() =>
                {
                    string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                    var logger = new EasyLog.EasyLog(logDirectory, settings.LogFormat);

                    return _backupManager.ExecuteJob(jobToExecute.Id, logger);
                });

                StatusMessage = success
                    ? $"Execution completed: {jobToExecute.Name}"
                    : $"Execution failed or interrupted: {jobToExecute.Name}";

                LoadJobs(false);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Execution error: {ex.Message}";
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private async void ExecuteAllJobs()
        {
            if (Jobs.Count == 0)
            {
                StatusMessage = "No backup job available for sequential execution.";
                return;
            }

            var settings = _backupManager.GetSettings();

            if (BusinessSoftwareWatcher.IsRunning(settings.BusinessSoftware))
            {
                string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                var logger = new EasyLog.EasyLog(logDirectory, settings.LogFormat);

                logger.LogFileTransfer(
                    "Sequential execution",
                    $"Business software detected: {settings.BusinessSoftware}",
                    string.Empty,
                    0,
                    -1,
                    0);

                StatusMessage = $"Sequential execution blocked: business software is running ({settings.BusinessSoftware})";
                return;
            }

            IsExecuting = true;
            StatusMessage = "Sequential execution started.";

            try
            {
                var orderedJobIds = Jobs
                    .OrderBy(job => job.Number)
                    .Select(job => job.Id)
                    .ToList();

                bool success = await Task.Run(() =>
                {
                    string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                    var logger = new EasyLog.EasyLog(logDirectory, settings.LogFormat);

                    return _backupManager.ExecuteSequential(orderedJobIds, logger);
                });

                StatusMessage = success
                    ? "Sequential execution completed."
                    : "Sequential execution completed with errors or interruptions.";

                LoadJobs(false);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Sequential execution error: {ex.Message}";
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private void DeleteSelectedJob()
        {
            if (SelectedJob == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            _backupManager.RemoveJob(SelectedJob.Id);
            LoadJobs();
            StatusMessage = "Job deleted.";
        }

        private void CreateJob()
        {
            StatusMessage = "Use the Create job menu to add a new backup job.";
        }
    }
}