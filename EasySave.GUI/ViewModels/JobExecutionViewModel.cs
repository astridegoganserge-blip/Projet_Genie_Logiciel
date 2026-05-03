using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace EasySave.GUI.ViewModels
{
    public class JobExecutionViewModel : BaseViewModel
    {
        private readonly System.Timers.Timer _refreshTimer;
        private double _globalProgression;
        private string _monitoringStatus = "Monitoring stopped";

        public JobExecutionViewModel()
        {
            JobStates = new ObservableCollection<string>
            {
                "Test_Backup | Terminé | 100%",
                "Daily_Backup | Actif | 45%"
            };

            StartMonitoringCommand = new RelayCommand(_ => StartMonitoring());
            StopMonitoringCommand = new RelayCommand(_ => StopMonitoring());

            _refreshTimer = new System.Timers.Timer(500);
            _refreshTimer.Elapsed += (_, _) => RefreshStates();
        }

        public ObservableCollection<string> JobStates { get; }

        public double GlobalProgression
        {
            get => _globalProgression;
            set
            {
                _globalProgression = value;
                OnPropertyChanged();
            }
        }

        public string MonitoringStatus
        {
            get => _monitoringStatus;
            set
            {
                _monitoringStatus = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand StartMonitoringCommand { get; }

        public RelayCommand StopMonitoringCommand { get; }

        private void StartMonitoring()
        {
            MonitoringStatus = "Monitoring started";
            _refreshTimer.Start();
        }

        private void StopMonitoring()
        {
            MonitoringStatus = "Monitoring stopped";
            _refreshTimer.Stop();
        }

        private void RefreshStates()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (GlobalProgression >= 100)
                {
                    GlobalProgression = 0;
                }
                else
                {
                    GlobalProgression += 5;
                }

                MonitoringStatus = $"Last refresh: {DateTime.Now:HH:mm:ss}";

                if (JobStates.Any())
                {
                    JobStates[0] = $"Test_Backup | Actif | {GlobalProgression}%";
                }
            });
        }
    }
}