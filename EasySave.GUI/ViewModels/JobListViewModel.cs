using System.Collections.ObjectModel;
using EasySave.Core.Models;
using EasySave.Core.Repositories;

namespace EasySave.GUI.ViewModels
{
    public class JobListViewModel : BaseViewModel
    {
        private string _statusMessage = "Ready";
        private bool _isExecuting;
        private readonly IJobRepository _jobRepository;

        public JobListViewModel()
        {
            _jobRepository = new JsonJobRepository();

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

            foreach (BackupJob job in _jobRepository.GetAll())
            {
                Jobs.Add(job);
            }

            StatusMessage = $"{Jobs.Count} job(s) loaded.";
        }

        private void RefreshJobs()
        {
            LoadJobs();
        }

        private void ExecuteSelectedJob()
        {
            if (SelectedJob == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            StatusMessage = $"Execution requested for: {SelectedJob.Name}";
        }

        private void DeleteSelectedJob()
        {
            if (SelectedJob == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            _jobRepository.Delete(SelectedJob.Id);
            LoadJobs();
            StatusMessage = "Job deleted.";
        }

        private void CreateJob()
        {
            StatusMessage = "Use the Create job menu to add a new backup job.";
        }
    }
}