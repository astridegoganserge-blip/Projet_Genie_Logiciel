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
        private string _monitoringStatus = string.Empty;
        private bool _isMonitoring;

        public JobExecutionViewModel()
        {
            JobStates = new ObservableCollection<JobState>();

            StartMonitoringCommand = new RelayCommand(_ => StartMonitoring(), _ => !IsMonitoring);
            StopMonitoringCommand = new RelayCommand(_ => StopMonitoring(), _ => IsMonitoring);

            _refreshTimer = new System.Timers.Timer(500)
            {
                AutoReset = true
            };

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

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set
            {
                _isMonitoring = value;
                OnPropertyChanged();
                StartMonitoringCommand.RaiseCanExecuteChanged();
                StopMonitoringCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand StartMonitoringCommand { get; }

        public RelayCommand StopMonitoringCommand { get; }

        private void StartMonitoring()
        {
            IsMonitoring = true;
            MonitoringStatus = Tr("MsgMonitoringStarted");

            RefreshStates();
            _refreshTimer.Start();
        }

        private void StopMonitoring()
        {
            _refreshTimer.Stop();
            IsMonitoring = false;
            MonitoringStatus = Tr("MsgMonitoringStopped");
        }

        private static string Tr(string key, params object[] args)
        {
            object? resource = System.Windows.Application.Current?.TryFindResource(key);
            string template = resource?.ToString() ?? key;
            return args.Length > 0 ? string.Format(template, args) : template;
        }

        private void RefreshStates()
        {
            Application? application = Application.Current;

            if (application?.Dispatcher == null)
            {
                return;
            }

            application.Dispatcher.Invoke(() =>
            {
                JobStates.Clear();

                foreach (JobState state in StateTracker.GetAllStates())
                {
                    JobStates.Add(state);
                }

                GlobalProgression = JobStates.Count == 0
                    ? 0
                    : Math.Round(JobStates.Average(state => state.Progression), 2);

                MonitoringStatus = IsMonitoring
                    ? Tr("MsgLastRefresh", DateTime.Now.ToString("HH:mm:ss"))
                    : Tr("MsgMonitoringStopped");
            });
        }
    }
}