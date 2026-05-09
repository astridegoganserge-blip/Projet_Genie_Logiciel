namespace EasySave.GUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object? _currentView;
        private string _applicationTitle = "EasySave v3.0";



        public MainViewModel()
        {
            ShowJobsCommand = new RelayCommand(_ => ShowJobs());
            ShowHistoryCommand = new RelayCommand(_ => ShowHistory());
            ShowSettingsCommand = new RelayCommand(_ => ShowSettings());
            ShowExecutionCommand = new RelayCommand(_ => ShowExecution());
            ShowCreateJobCommand = new RelayCommand(_ => ShowCreateJob());



            CurrentView = new JobListViewModel();
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
            CurrentView = new JobListViewModel();
        }



        private void ShowHistory()
        {
            CurrentView = new JobHistoryViewModel();
        }



        private void ShowSettings()
        {
            CurrentView = new SettingsViewModel();
        }



        private void ShowExecution()
        {
            CurrentView = new JobExecutionViewModel();
        }



        private void ShowCreateJob()
        {
            CurrentView = new JobEditViewModel(isCreation: true);
        }
    }
}