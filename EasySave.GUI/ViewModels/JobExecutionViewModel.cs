using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EasySave.Core.Models;
using EasySave.Core.Services;

namespace EasySave.GUI.ViewModels
{
    public class JobExecutionViewModel : BaseViewModel
    {
        private readonly System.Timers.Timer _refreshTimer;
        private double _globalProgression;
        private string _monitoringStatus = "Monitoring stopped";

        public JobExecutionViewModel()
        {
            JobStates = new ObservableCollection<JobState>();

            StartMonitoringCommand = new RelayCommand(_ => StartMonitoring());
            StopMonitoringCommand = new RelayCommand(_ => StopMonitoring());

            _refreshTimer = new System.Timers.Timer(500);
            _refreshTimer.Elapsed += (_, _) => RefreshStates();
        }

        public ObservableCollection<JobState> JobStates { get; }

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
            RefreshStates();
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
                JobStates.Clear();

                foreach (JobState state in StateTracker.GetAllStates())
                {
                    JobStates.Add(state);
                }

                GlobalProgression = JobStates.Count == 0
                    ? 0
                    : Math.Round(JobStates.Average(state => state.Progression), 2);

                MonitoringStatus = $"Last refresh: {DateTime.Now:HH:mm:ss}";
            });
        }
    }
}