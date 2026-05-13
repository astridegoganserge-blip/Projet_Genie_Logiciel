using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Core.Services;

namespace EasySave.GUI.ViewModels
{
    public class JobListViewModel : BaseViewModel
    {
        private readonly BackupManager _backupManager;
        private readonly DispatcherTimer _stateRefreshTimer;

        private string _statusMessage = "Ready";
        private bool _isExecuting;
        private BackupJob? _selectedJob;
        private JobListItemViewModel? _selectedJobItem;

        public JobListViewModel()
            : this(new BackupManager(new JsonJobRepository(), new JsonSettingsRepository()))
        {
        }

        public JobListViewModel(BackupManager backupManager)
        {
            _backupManager = backupManager;

            Jobs = new ObservableCollection<BackupJob>();
            JobItems = new ObservableCollection<JobListItemViewModel>();
            JobStates = new ObservableCollection<JobState>();

            RefreshCommand = new RelayCommand(_ => RefreshJobs());

            ExecuteJobCommand = new RelayCommand(
                parameter => _ = ExecuteSelectedJobAsync(parameter),
                parameter => !IsExecuting && !IsEditLocked && GetJobFromParameter(parameter) != null);

            ExecuteAllCommand = new RelayCommand(
                _ => _ = ExecuteAllJobsAsync(),
                _ => Jobs.Count > 0 && !IsExecuting && !IsEditLocked);

            DeleteJobCommand = new RelayCommand(
                parameter => DeleteSelectedJob(parameter),
                parameter => !IsExecuting && !IsEditLocked && GetJobFromParameter(parameter) != null);

            EditJobCommand = new RelayCommand(
                parameter => EditSelectedJob(parameter),
                parameter => !IsExecuting && !IsEditLocked && GetJobFromParameter(parameter) != null);

            CreateJobCommand = new RelayCommand(_ => CreateJob(), _ => !IsExecuting);

            PauseJobCommand = new RelayCommand(
                parameter => PauseJob(parameter),
                parameter => GetJobFromParameter(parameter) != null);

            ResumeJobCommand = new RelayCommand(
                parameter => ResumeJob(parameter),
                parameter => GetJobFromParameter(parameter) != null);

            StopJobCommand = new RelayCommand(
                parameter => StopJob(parameter),
                parameter => GetJobFromParameter(parameter) != null);

            PauseAllCommand = new RelayCommand(_ => PauseAll(), _ => Jobs.Count > 0);
            ResumeAllCommand = new RelayCommand(_ => ResumeAll(), _ => Jobs.Count > 0);
            StopAllCommand = new RelayCommand(_ => StopAll(), _ => Jobs.Count > 0);

            _stateRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _stateRefreshTimer.Tick += (_, _) =>
            {
                try
                {
                    RefreshRuntimeStates();
                }
                catch (Exception ex)
                {
                    StatusMessage = Tr("MsgProgressError", ex.Message);
                }
            };

            LoadJobs();
            RefreshRuntimeStates();
        }

        private static string Tr(string key, params object[] args)
        {
            object? resource = System.Windows.Application.Current?.TryFindResource(key);
            string template = resource?.ToString() ?? key;
            return args.Length > 0 ? string.Format(template, args) : template;
        }

        public event Action<object>? NavigationRequested;

        public ObservableCollection<BackupJob> Jobs { get; }

        public ObservableCollection<JobListItemViewModel> JobItems { get; }

        public ObservableCollection<JobState> JobStates { get; }

        public JobListItemViewModel? SelectedJobItem
        {
            get => _selectedJobItem;
            set
            {
                _selectedJobItem = value;
                SelectedJob = value?.Job;
                OnPropertyChanged();
            }
        }

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
            JobItems.Clear();

            var jobs = _backupManager
                .GetAllJobs()
                .OrderBy(job => job.Number)
                .ToList();

            foreach (BackupJob job in jobs)
            {
                Jobs.Add(job);
                JobItems.Add(new JobListItemViewModel(job));
            }

            RefreshRuntimeStates();
            OnPropertyChanged(nameof(IsEditLocked));
            RaiseCommandStates();

            if (updateStatusMessage)
            {
                StatusMessage = Tr("MsgJobsLoaded", Jobs.Count);
            }
        }

        private void RefreshRuntimeStates()
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

            foreach (JobListItemViewModel item in JobItems)
            {
                JobState? state = states.FirstOrDefault(s =>
                    string.Equals(s.BackupName, item.Name, StringComparison.OrdinalIgnoreCase));

                item.UpdateState(state);
            }
        }

        private void RefreshJobs()
        {
            LoadJobs(false);
            SelectedJobItem = null;
            SelectedJob = null;
            StatusMessage = Tr("MsgJobsRefreshed", Jobs.Count);
        }

        private async Task ExecuteSelectedJobAsync(object? parameter)
        {
            BackupJob? jobToExecute = GetJobFromParameter(parameter);

            if (jobToExecute == null)
            {
                StatusMessage = Tr("MsgSelectJob");
                return;
            }

            if (IsEditLocked)
            {
                StatusMessage = Tr("MsgExecutionLocked");
                return;
            }

            SelectedJob = jobToExecute;
            IsExecuting = true;
            StatusMessage = Tr("MsgExecuting", jobToExecute.Name);

            _stateRefreshTimer.Start();

            try
            {
                EasyLog.EasyLog? logger = _backupManager.SharedLogger;

                if (logger == null)
                {
                    StatusMessage = Tr("MsgLoggerNotInit");
                    return;
                }

                bool success = await Task.Run(() =>
                    _backupManager.ExecuteJob(jobToExecute.Id, logger));

                RefreshRuntimeStates();

                StatusMessage = success
                    ? Tr("MsgExecutionCompleted", jobToExecute.Name)
                    : Tr("MsgExecutionFailed", jobToExecute.Name);

                LoadJobs(false);
            }
            catch (Exception ex)
            {
                StatusMessage = Tr("MsgExecutionError", ex.Message);
            }
            finally
            {
                IsExecuting = false;
                _stateRefreshTimer.Stop();
                RefreshRuntimeStates();
            }
        }

        private async Task ExecuteAllJobsAsync()
        {
            if (Jobs.Count == 0)
            {
                StatusMessage = Tr("MsgNoJobAvailable");
                return;
            }

            if (IsEditLocked)
            {
                StatusMessage = Tr("MsgExecutionLocked");
                return;
            }

            IsExecuting = true;
            StatusMessage = Tr("MsgParallelStarted");

            _stateRefreshTimer.Start();

            try
            {
                EasyLog.EasyLog? logger = _backupManager.SharedLogger;

                if (logger == null)
                {
                    StatusMessage = Tr("MsgLoggerNotInit");
                    return;
                }

                var jobIds = Jobs
                    .OrderBy(job => job.Number)
                    .Select(job => job.Id)
                    .ToList();

                bool success = await _backupManager.ExecuteParallel(jobIds, logger);

                RefreshRuntimeStates();

                StatusMessage = success
                    ? Tr("MsgParallelCompleted")
                    : Tr("MsgParallelCompletedErrors");

                LoadJobs(false);
            }
            catch (Exception ex)
            {
                StatusMessage = Tr("MsgParallelError", ex.Message);
            }
            finally
            {
                IsExecuting = false;
                _stateRefreshTimer.Stop();
                RefreshRuntimeStates();
            }
        }

        private void PauseJob(object? parameter)
        {
            BackupJob? job = GetJobFromParameter(parameter);

            if (job == null)
            {
                StatusMessage = Tr("MsgNoJobSelected");
                return;
            }

            _backupManager.PauseJob(job.Name);
            RefreshRuntimeStates();
            StatusMessage = Tr("MsgPauseRequested", job.Name);
        }

        private void ResumeJob(object? parameter)
        {
            BackupJob? job = GetJobFromParameter(parameter);

            if (job == null)
            {
                StatusMessage = Tr("MsgNoJobSelected");
                return;
            }

            _backupManager.ResumeJob(job.Name);
            RefreshRuntimeStates();
            StatusMessage = Tr("MsgResumeRequested", job.Name);
        }

        private void StopJob(object? parameter)
        {
            BackupJob? job = GetJobFromParameter(parameter);

            if (job == null)
            {
                StatusMessage = Tr("MsgNoJobSelected");
                return;
            }

            _backupManager.StopJob(job.Name);
            RefreshRuntimeStates();
            StatusMessage = Tr("MsgStopRequested", job.Name);
        }

        private void PauseAll()
        {
            _backupManager.PauseAll();
            RefreshRuntimeStates();
            StatusMessage = Tr("MsgPauseAllRequested");
        }

        private void ResumeAll()
        {
            _backupManager.ResumeAll();
            RefreshRuntimeStates();
            StatusMessage = Tr("MsgResumeAllRequested");
        }

        private void StopAll()
        {
            _backupManager.StopAll();
            RefreshRuntimeStates();
            StatusMessage = Tr("MsgStopAllRequested");
        }

        private void DeleteSelectedJob(object? parameter)
        {
            BackupJob? jobToDelete = GetJobFromParameter(parameter);

            if (jobToDelete == null)
            {
                StatusMessage = Tr("MsgNoJobSelected");
                return;
            }

            if (IsEditLocked)
            {
                StatusMessage = Tr("MsgDeleteLocked");
                return;
            }

            _backupManager.RemoveJob(jobToDelete.Id);
            LoadJobs(false);
            SelectedJobItem = null;
            SelectedJob = null;
            StatusMessage = Tr("MsgJobDeleted", jobToDelete.Name);
        }

        private void CreateJob()
        {
            var viewModel = new JobEditViewModel(
                isCreation: true,
                job: null,
                backupManager: _backupManager);

            NavigationRequested?.Invoke(viewModel);
            StatusMessage = Tr("MsgCreateRequested");
        }

        private void EditSelectedJob(object? parameter)
        {
            BackupJob? jobToEdit = GetJobFromParameter(parameter);

            if (jobToEdit == null)
            {
                StatusMessage = Tr("MsgNoJobSelected");
                return;
            }

            var viewModel = new JobEditViewModel(
                isCreation: false,
                job: jobToEdit,
                backupManager: _backupManager);

            NavigationRequested?.Invoke(viewModel);
            OnPropertyChanged(nameof(IsEditLocked));
            RaiseCommandStates();
            StatusMessage = Tr("MsgEditRequested", jobToEdit.Name);
        }

        private BackupJob? GetJobFromParameter(object? parameter)
        {
            return parameter switch
            {
                BackupJob job => job,
                JobListItemViewModel item => item.Job,
                _ => SelectedJob
            };
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

    public class JobListItemViewModel : BaseViewModel
    {
        private JobState? _state;

        public JobListItemViewModel(BackupJob job)
        {
            Job = job;
        }

        public BackupJob Job { get; }
        public Guid Id => Job.Id;
        public int Number => Job.Number;
        public string Name => Job.Name;
        public string SourcePath => Job.SourcePath;
        public string TargetPath => Job.TargetPath;
        public BackupType Type => Job.Type;
        public DateTime? LastExecutionTime => Job.LastExecutionTime;

        public JobState? State
        {
            get => _state;
            private set
            {
                _state = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayStatus));
                OnPropertyChanged(nameof(Progression));
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public string DisplayStatus
        {
            get
            {
                string key = State == null ? "StatusDisponible" : $"Status{State.Status}";
                object? resource = System.Windows.Application.Current?.TryFindResource(key);
                return resource?.ToString() ?? (State?.Status.ToString() ?? "Disponible");
            }
        }

        public double Progression => State?.Progression ?? 0;

        public string ProgressText => State == null
            ? "0 %"
            : $"{State.Progression:0.##} %";

        public string ErrorMessage => State?.ErrorMessage ?? string.Empty;

        public bool HasError => State?.Status == JobStatus.Erreur
            && !string.IsNullOrEmpty(ErrorMessage);

        public void UpdateState(JobState? state)
        {
            State = state;
        }
    }
}