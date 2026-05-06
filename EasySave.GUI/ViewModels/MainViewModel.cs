using EasySave.GUI.Views;

namespace EasySave.GUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object? _currentView;
        private string _applicationTitle = "EasySave v2.0";

        public MainViewModel()
        {
            ShowJobsCommand = new RelayCommand(_ => ShowJobs());
            ShowSettingsCommand = new RelayCommand(_ => ShowSettings());
            ShowExecutionCommand = new RelayCommand(_ => ShowExecution());
            ShowCreateJobCommand = new RelayCommand(_ => ShowCreateJob());

            CurrentView = new JobListView();
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

        public RelayCommand ShowSettingsCommand { get; }

        public RelayCommand ShowExecutionCommand { get; }

        public RelayCommand ShowCreateJobCommand { get; }

        private void ShowJobs()
        {
            CurrentView = new JobListView();
        }

        private void ShowSettings()
        {
            CurrentView = new SettingsView();
        }

        private void ShowExecution()
        {
            CurrentView = new JobExecutionView();
        }

        private void ShowCreateJob()
        {
            CurrentView = new JobEditView();
        }
    }
}