using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Core.Services;

namespace EasySave.GUI.ViewModels
{
    public class JobListViewModel : BaseViewModel
    {
        private readonly BackupManager _backupManager;
        private string _statusMessage = "Ready";
        private bool _isExecuting;
        private BackupJob? _selectedJob;

        public JobListViewModel()
            : this(new BackupManager(new JsonJobRepository(), new JsonSettingsRepository()))
        {
        }

        public JobListViewModel(BackupManager backupManager)
        {
            _backupManager = backupManager;

            Jobs = new ObservableCollection<BackupJob>();
            JobStates = new ObservableCollection<JobState>();

            RefreshCommand = new RelayCommand(_ => RefreshJobs());
            ExecuteJobCommand = new RelayCommand(parameter => _ = ExecuteSelectedJobAsync(parameter), _ => CanExecuteJobAction());
            ExecuteAllCommand = new RelayCommand(_ => _ = ExecuteAllJobsAsync(), _ => Jobs.Count > 0 && !IsExecuting && !IsEditLocked);
            DeleteJobCommand = new RelayCommand(_ => DeleteSelectedJob(), _ => SelectedJob != null && !IsExecuting && !IsEditLocked);
            EditJobCommand = new RelayCommand(parameter => EditSelectedJob(parameter), _ => SelectedJob != null && !IsExecuting);
            CreateJobCommand = new RelayCommand(_ => CreateJob(), _ => !IsExecuting);
            PauseJobCommand = new RelayCommand(parameter => PauseJob(parameter), _ => SelectedJob != null);
            ResumeJobCommand = new RelayCommand(parameter => ResumeJob(parameter), _ => SelectedJob != null);
            StopJobCommand = new RelayCommand(parameter => StopJob(parameter), _ => SelectedJob != null);
            PauseAllCommand = new RelayCommand(_ => PauseAll(), _ => Jobs.Count > 0);
            ResumeAllCommand = new RelayCommand(_ => ResumeAll(), _ => Jobs.Count > 0);
            StopAllCommand = new RelayCommand(_ => StopAll(), _ => Jobs.Count > 0);

            LoadJobs();
        }

        public event Action<object>? NavigationRequested;

        public ObservableCollection<BackupJob> Jobs { get; }

        public ObservableCollection<JobState> JobStates { get; }

        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged();
                RaiseCommandStates();
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
                RaiseCommandStates();
            }
        }

        public bool IsEditLocked => _backupManager.IsEditLocked;

        public RelayCommand RefreshCommand { get; }

        public RelayCommand ExecuteJobCommand { get; }

        public RelayCommand ExecuteAllCommand { get; }

        public RelayCommand DeleteJobCommand { get; }

        public RelayCommand EditJobCommand { get; }

        public RelayCommand CreateJobCommand { get; }

        public RelayCommand PauseJobCommand { get; }

        public RelayCommand ResumeJobCommand { get; }

        public RelayCommand StopJobCommand { get; }

        public RelayCommand PauseAllCommand { get; }

        public RelayCommand ResumeAllCommand { get; }

        public RelayCommand StopAllCommand { get; }

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

            LoadStates();

            OnPropertyChanged(nameof(IsEditLocked));
            RaiseCommandStates();

            if (updateStatusMessage)
            {
                StatusMessage = $"{Jobs.Count} job(s) loaded.";
            }
        }

        private void LoadStates()
        {
            JobStates.Clear();

            var states = StateTracker
                .GetAllStates()
                .OrderBy(state => state.BackupName)
                .ToList();

            foreach (JobState state in states)
            {
                JobStates.Add(state);
            }
        }

        private void RefreshJobs()
        {
            LoadJobs(false);
            SelectedJob = null;
            StatusMessage = $"{Jobs.Count} job(s) refreshed.";
        }

        private async Task ExecuteSelectedJobAsync(object? parameter)
        {
            BackupJob? jobToExecute = GetJobFromParameter(parameter);

            if (jobToExecute == null)
            {
                StatusMessage = "Please select a backup job before execution.";
                return;
            }

            if (IsEditLocked)
            {
                StatusMessage = "Execution blocked: a job is currently being edited.";
                return;
            }

            SelectedJob = jobToExecute;
            IsExecuting = true;
            StatusMessage = $"Executing: {jobToExecute.Name}";

            try
            {
                AppSettings settings = _backupManager.GetSettings();
                EasyLog.EasyLog logger = CreateLogger(settings);

                bool success = await Task.Run(() =>
                    _backupManager.ExecuteJob(jobToExecute.Id, logger));

                StatusMessage = success
                    ? $"Execution completed: {jobToExecute.Name}"
                    : $"Execution failed, paused, stopped or blocked: {jobToExecute.Name}";

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

        private async Task ExecuteAllJobsAsync()
        {
            if (Jobs.Count == 0)
            {
                StatusMessage = "No backup job available for execution.";
                return;
            }

            if (IsEditLocked)
            {
                StatusMessage = "Execution blocked: a job is currently being edited.";
                return;
            }

            IsExecuting = true;
            StatusMessage = "Parallel execution started.";

            try
            {
                AppSettings settings = _backupManager.GetSettings();
                EasyLog.EasyLog logger = CreateLogger(settings);

                var jobIds = Jobs
                    .OrderBy(job => job.Number)
                    .Select(job => job.Id)
                    .ToList();

                bool success = await _backupManager.ExecuteParallel(jobIds, logger);

                StatusMessage = success
                    ? "Parallel execution completed."
                    : "Parallel execution completed with errors, stops or interruptions.";

                LoadJobs(false);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Parallel execution error: {ex.Message}";
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private void PauseJob(object? parameter)
        {
            BackupJob? job = GetJobFromParameter(parameter);

            if (job == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            _backupManager.PauseJob(job.Name);
            LoadStates();
            StatusMessage = $"Pause requested: {job.Name}";
        }

        private void ResumeJob(object? parameter)
        {
            BackupJob? job = GetJobFromParameter(parameter);

            if (job == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            _backupManager.ResumeJob(job.Name);
            LoadStates();
            StatusMessage = $"Resume requested: {job.Name}";
        }

        private void StopJob(object? parameter)
        {
            BackupJob? job = GetJobFromParameter(parameter);

            if (job == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            _backupManager.StopJob(job.Name);
            LoadStates();
            StatusMessage = $"Stop requested: {job.Name}";
        }

        private void PauseAll()
        {
            _backupManager.PauseAll();
            LoadStates();
            StatusMessage = "Pause requested for all active jobs.";
        }

        private void ResumeAll()
        {
            _backupManager.ResumeAll();
            LoadStates();
            StatusMessage = "Resume requested for all paused jobs.";
        }

        private void StopAll()
        {
            _backupManager.StopAll();
            LoadStates();
            StatusMessage = "Stop requested for all active jobs.";
        }

        private void DeleteSelectedJob()
        {
            if (SelectedJob == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            if (IsEditLocked)
            {
                StatusMessage = "Delete blocked: a job is currently being edited.";
                return;
            }

            _backupManager.RemoveJob(SelectedJob.Id);
            LoadJobs(false);
            SelectedJob = null;
            StatusMessage = "Job deleted.";
        }

        private void CreateJob()
        {
            var viewModel = new JobEditViewModel(
            isCreation: true,
            job: null,
            backupManager: _backupManager);



            NavigationRequested?.Invoke(viewModel);
            StatusMessage = "Create job view requested.";
        }


        private void EditSelectedJob(object? parameter)
        {
            BackupJob? jobToEdit = GetJobFromParameter(parameter);



            if (jobToEdit == null)
            {
                StatusMessage = "No job selected.";
                return;
            }



            var viewModel = new JobEditViewModel(
            isCreation: false,
            job: jobToEdit,
            backupManager: _backupManager);



            NavigationRequested?.Invoke(viewModel);
            OnPropertyChanged(nameof(IsEditLocked));
            RaiseCommandStates();
            StatusMessage = $"Edit requested: {jobToEdit.Name}";
        }



        private BackupJob? GetJobFromParameter(object? parameter)
        {
            return parameter as BackupJob ?? SelectedJob;
        }

        private bool CanExecuteJobAction()
        {
            return !IsExecuting && !IsEditLocked && SelectedJob != null;
        }

        private EasyLog.EasyLog CreateLogger(AppSettings settings)
        {
            string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            return new EasyLog.EasyLog(logDirectory, settings.LogFormat);
        }

        private void RaiseCommandStates()
        {
            ExecuteJobCommand.RaiseCanExecuteChanged();
            ExecuteAllCommand.RaiseCanExecuteChanged();
            DeleteJobCommand.RaiseCanExecuteChanged();
            EditJobCommand.RaiseCanExecuteChanged();
            CreateJobCommand.RaiseCanExecuteChanged();
            PauseJobCommand.RaiseCanExecuteChanged();
            ResumeJobCommand.RaiseCanExecuteChanged();
            StopJobCommand.RaiseCanExecuteChanged();
            PauseAllCommand.RaiseCanExecuteChanged();
            ResumeAllCommand.RaiseCanExecuteChanged();
            StopAllCommand.RaiseCanExecuteChanged();
        }
    }
}