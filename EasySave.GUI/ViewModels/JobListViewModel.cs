using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using EasyLog;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;

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
            ExecuteJobCommand = new RelayCommand(_ => ExecuteSelectedJob(), _ => SelectedJob != null && !IsExecuting);
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
            }
        }

        public RelayCommand RefreshCommand { get; }

        public RelayCommand ExecuteJobCommand { get; }

        public RelayCommand DeleteJobCommand { get; }

        public RelayCommand CreateJobCommand { get; }

        private void LoadJobs()
        {
            Jobs.Clear();

            var jobs = _backupManager.GetAllJobs();

            foreach (BackupJob job in jobs)
            {
                Jobs.Add(job);
            }

            StatusMessage = $"{Jobs.Count} job(s) loaded from {AppContext.BaseDirectory}";
        }

        private void RefreshJobs()
        {
            LoadJobs();
            SelectedJob = null;
            StatusMessage = $"{Jobs.Count} job(s) refreshed.";
        }

        private async void ExecuteSelectedJob()
        {
            if (SelectedJob == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            IsExecuting = true;
            StatusMessage = $"Executing: {SelectedJob.Name}";

            try
            {
                bool success = await Task.Run(() =>
                {
                    var settings = _backupManager.GetSettings();
                    string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                    var logger = new EasyLog.EasyLog(logDirectory, settings.LogFormat);

                    return _backupManager.ExecuteJob(SelectedJob.Id, logger);
                });

                StatusMessage = success
                    ? $"Execution completed: {SelectedJob.Name}"
                    : $"Execution failed or interrupted: {SelectedJob.Name}";

                LoadJobs();
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