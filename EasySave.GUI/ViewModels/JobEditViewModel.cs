
using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using Microsoft.Win32;



namespace EasySave.GUI.ViewModels
{
    public class JobEditViewModel : BaseViewModel
    {
        private readonly BackupManager _backupManager;
        private readonly bool _isCreation;



        private Guid? _jobId;
        private int _jobNumber;
        private DateTime? _lastExecutionTime;



        private string _jobName = string.Empty;
        private string _sourcePath = string.Empty;
        private string _targetPath = string.Empty;
        private BackupType _selectedType = BackupType.Complete;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private string _formTitle = "Create backup job";



        public JobEditViewModel()
        : this(isCreation: true)
        {
        }



        public JobEditViewModel(bool isCreation = true, BackupJob? job = null)
        : this(
        isCreation,
        job,
        new BackupManager(new JsonJobRepository(), new JsonSettingsRepository()))
        {
        }



        public JobEditViewModel(
        bool isCreation,
        BackupJob? job,
        BackupManager backupManager)
        {
            _backupManager = backupManager;
            _isCreation = isCreation;



            AvailableTypes = new List<BackupType>
 {
 BackupType.Complete,
 BackupType.Differential
 };



            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
            BrowseSourceCommand = new RelayCommand(_ => BrowseSource());
            BrowseTargetCommand = new RelayCommand(_ => BrowseTarget());



            if (!_isCreation)
            {
                _backupManager.LockForEdit();
                FormTitle = "Edit backup job";
            }



            if (job != null)
            {
                LoadJob(job);
            }
        }



        public event Action<object>? NavigationRequested;



        public Guid? JobId
        {
            get => _jobId;
            private set
            {
                _jobId = value;
                OnPropertyChanged();
            }
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



        public string FormTitle
        {
            get => _formTitle;
            set
            {
                _formTitle = value;
                OnPropertyChanged();
            }
        }



        public RelayCommand SaveCommand { get; }



        public RelayCommand CancelCommand { get; }



        public RelayCommand BrowseSourceCommand { get; }



        public RelayCommand BrowseTargetCommand { get; }



        private void LoadJob(BackupJob job)
        {
            JobId = job.Id;
            _jobNumber = job.Number;
            _lastExecutionTime = job.LastExecutionTime;



            JobName = job.Name;
            SourcePath = job.SourcePath;
            TargetPath = job.TargetPath;
            SelectedType = job.Type;
        }



        private void Save()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;



            if (!ValidateForm())
            {
                return;
            }



            BackupJob job = BuildBackupJob();
            bool saved;



            if (_isCreation)
            {
                saved = _backupManager.AddJob(job);
            }
            else
            {
                _backupManager.UnlockForEdit();
                saved = _backupManager.UpdateJob(job);



                if (!saved)
                {
                    _backupManager.LockForEdit();
                }
            }



            if (!saved)
            {
                ErrorMessage = _isCreation
                ? "Unable to create backup job. Check the source path."
                : "Unable to update backup job. Check the source path.";



                return;
            }



            SuccessMessage = _isCreation
            ? "Backup job created successfully."
            : "Backup job updated successfully.";



            if (_isCreation)
            {
                ClearFormFieldsOnly();
            }
            else
            {
                NavigationRequested?.Invoke(new JobListViewModel(_backupManager));
            }
        }



        private BackupJob BuildBackupJob()
        {
            return new BackupJob
            {
                Id = JobId ?? Guid.NewGuid(),
                Number = _jobNumber,
                Name = JobName.Trim(),
                SourcePath = SourcePath.Trim(),
                TargetPath = TargetPath.Trim(),
                Type = SelectedType,
                LastExecutionTime = _lastExecutionTime
            };
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



            if (!Directory.Exists(SourcePath))
            {
                ErrorMessage = "Source path does not exist.";
                return false;
            }



            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                ErrorMessage = "Target path is required.";
                return false;
            }



            return true;
        }



        private void Cancel()
        {
            if (!_isCreation)
            {
                _backupManager.UnlockForEdit();
            }



            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;



            NavigationRequested?.Invoke(new JobListViewModel(_backupManager));
        }



        private void BrowseSource()
        {
            string? selectedPath = BrowseFolder("Select source folder");



            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                SourcePath = selectedPath;
            }
        }



        private void BrowseTarget()
        {
            string? selectedPath = BrowseFolder("Select target folder");



            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                TargetPath = selectedPath;
            }
        }



        private static string? BrowseFolder(string title)
        {
            var dialog = new OpenFolderDialog
            {
                Title = title,
                Multiselect = false
            };



            bool? result = dialog.ShowDialog();



            return result == true
            ? dialog.FolderName
            : null;
        }



        private void ClearFormFieldsOnly()
        {
            JobId = null;
            _jobNumber = 0;
            _lastExecutionTime = null;



            JobName = string.Empty;
            SourcePath = string.Empty;
            TargetPath = string.Empty;
            SelectedType = BackupType.Complete;
        }
    }
}

