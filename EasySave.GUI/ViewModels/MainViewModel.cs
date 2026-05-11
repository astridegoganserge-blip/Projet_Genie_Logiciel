
using System;
using EasySave.Core.Managers;
using EasySave.Core.Repositories;



namespace EasySave.GUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly BackupManager _backupManager;



        private object? _currentView;
        private string _applicationTitle = "EasySave v3.0";



        public MainViewModel()
        : this(new BackupManager(new JsonJobRepository(), new JsonSettingsRepository()))
        {
        }



        public MainViewModel(BackupManager backupManager)
        {
            _backupManager = backupManager;



            ShowJobsCommand = new RelayCommand(_ => ShowJobs());
            ShowHistoryCommand = new RelayCommand(_ => ShowHistory());
            ShowSettingsCommand = new RelayCommand(_ => ShowSettings());
            ShowExecutionCommand = new RelayCommand(_ => ShowExecution());
            ShowCreateJobCommand = new RelayCommand(_ => ShowCreateJob());



            ShowJobs();
        }



        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }



        public string ApplicationTitle
        {
            get => _applicationTitle;
            set
            {
                _applicationTitle = value;
                OnPropertyChanged();
            }
        }



        public RelayCommand ShowJobsCommand { get; }



        public RelayCommand ShowHistoryCommand { get; }



        public RelayCommand ShowSettingsCommand { get; }



        public RelayCommand ShowExecutionCommand { get; }



        public RelayCommand ShowCreateJobCommand { get; }



        private void ShowJobs()
        {
            var viewModel = new JobListViewModel(_backupManager);
            viewModel.NavigationRequested += NavigateTo;



            CurrentView = viewModel;
        }



        private void ShowHistory()
        {
            CurrentView = new JobHistoryViewModel(_backupManager);
        }



        private void ShowSettings()
        {
            CurrentView = new SettingsViewModel(_backupManager);
        }



        private void ShowExecution()
        {
            CurrentView = new JobExecutionViewModel();
        }



        private void ShowCreateJob()
        {
            var viewModel = new JobEditViewModel(
            isCreation: true,
            job: null,
            backupManager: _backupManager);



            viewModel.NavigationRequested += NavigateTo;



            CurrentView = viewModel;
        }



        private void NavigateTo(object viewModel)
        {
            if (viewModel is JobListViewModel jobListViewModel)
            {
                jobListViewModel.NavigationRequested += NavigateTo;
            }



            if (viewModel is JobEditViewModel jobEditViewModel)
            {
                jobEditViewModel.NavigationRequested += NavigateTo;
            }



            CurrentView = viewModel;
        }
    }
}

