using System.Collections.Generic;
using System.Linq;
using EasySave.Models;
using EasySave.Repositories;

namespace EasySave.GUI.ViewModels
{
    public class JobEditViewModel : BaseViewModel
    {
        private readonly IJobRepository _jobRepository;

        private string _jobName = string.Empty;
        private string _sourcePath = string.Empty;
        private string _targetPath = string.Empty;
        private BackupType _selectedType = BackupType.Complete;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public JobEditViewModel()
        {
            _jobRepository = new JsonJobRepository();

            AvailableTypes = new List<BackupType>
            {
                BackupType.Complete,
                BackupType.Differential
            };

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => ClearForm());
        }

        public string JobName
        {
            get => _jobName;
            set
            {
                _jobName = value;
                OnPropertyChanged();
            }
        }

        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        public string TargetPath
        {
            get => _targetPath;
            set
            {
                _targetPath = value;
                OnPropertyChanged();
            }
        }

        public BackupType SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged();
            }
        }

        public List<BackupType> AvailableTypes { get; }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SaveCommand { get; }

        public RelayCommand CancelCommand { get; }

        private void Save()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (!ValidateForm())
            {
                return;
            }

            List<BackupJob> jobs = _jobRepository.GetAll();

            int nextId = jobs.Count == 0
                ? 1
                : jobs.Max(job => job.Id) + 1;

            var job = new BackupJob
            {
                Id = nextId,
                Name = JobName,
                SourcePath = SourcePath,
                TargetPath = TargetPath,
                Type = SelectedType
            };

            jobs.Add(job);
            _jobRepository.Save(jobs);

            SuccessMessage = "Backup job created successfully.";
            ClearFormFieldsOnly();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(JobName))
            {
                ErrorMessage = "Job name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                ErrorMessage = "Source path is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                ErrorMessage = "Target path is required.";
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            ClearFormFieldsOnly();
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        private void ClearFormFieldsOnly()
        {
            JobName = string.Empty;
            SourcePath = string.Empty;
            TargetPath = string.Empty;
            SelectedType = BackupType.Complete;
        }
    }
}