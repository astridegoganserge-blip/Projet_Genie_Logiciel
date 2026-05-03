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

        private void ShowJobs()
        {
            CurrentView = new JobListView();
        }

        private void ShowSettings()
        {
            CurrentView = "Settings view will be implemented later.";
        }

        private void ShowExecution()
        {
            CurrentView = "Execution monitoring view will be implemented later.";
        }
    }
}