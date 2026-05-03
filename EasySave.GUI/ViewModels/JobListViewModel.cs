using System.Collections.ObjectModel;

namespace EasySave.GUI.ViewModels
{
    public class JobListViewModel : BaseViewModel
    {
        private string _statusMessage = "Ready";
        private bool _isExecuting;

        public JobListViewModel()
        {
            Jobs = new ObservableCollection<string>
            {
                "Job 1 - Complete backup",
                "Job 2 - Differential backup"
            };

            RefreshCommand = new RelayCommand(_ => RefreshJobs());
            ExecuteJobCommand = new RelayCommand(_ => ExecuteSelectedJob(), _ => SelectedJob != null && !IsExecuting);
            DeleteJobCommand = new RelayCommand(_ => DeleteSelectedJob(), _ => SelectedJob != null && !IsExecuting);
            CreateJobCommand = new RelayCommand(_ => CreateJob(), _ => !IsExecuting);
        }

        public ObservableCollection<string> Jobs { get; }

        private string? _selectedJob;

        public string? SelectedJob
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

        private void RefreshJobs()
        {
            StatusMessage = "Job list refreshed.";
        }

        private void ExecuteSelectedJob()
        {
            if (SelectedJob == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            StatusMessage = $"Execution requested for: {SelectedJob}";
        }

        private void DeleteSelectedJob()
        {
            if (SelectedJob == null)
            {
                StatusMessage = "No job selected.";
                return;
            }

            Jobs.Remove(SelectedJob);
            StatusMessage = "Job removed from the list.";
        }

        private void CreateJob()
        {
            int nextIndex = Jobs.Count + 1;
            Jobs.Add($"Job {nextIndex} - New backup job");
            StatusMessage = "New job added.";
        }
    }
}