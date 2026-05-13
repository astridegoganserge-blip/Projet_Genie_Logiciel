using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Core.Services;



namespace EasySave.GUI.ViewModels
{
    public class JobHistoryViewModel : BaseViewModel
    {
        private readonly BackupManager _backupManager;
        private bool _sortDescending = true;
        private string _sortDirectionLabel = "↓ Plus récent";
        private string _statusMessage = string.Empty;
        private JobHistoryItem? _selectedHistoryItem;
        private bool _isExecuting;



        public JobHistoryViewModel()
        : this(new BackupManager(new JsonJobRepository(), new JsonSettingsRepository()))
        {
        }



        public JobHistoryViewModel(BackupManager backupManager)
        {
            _backupManager = backupManager;



            History = new ObservableCollection<JobHistoryItem>();



            SortByDateCommand = new RelayCommand(_ => SortByDate());
            RefreshCommand = new RelayCommand(_ => LoadHistory());
            ReExecuteCommand = new RelayCommand(
            parameter => _ = ReExecuteAsync(parameter),
            _ => SelectedHistoryItem != null && !IsExecuting);



            LoadHistory();
        }



        private static string Tr(string key, params object[] args)
        {
            object? resource = System.Windows.Application.Current?.TryFindResource(key);
            string template = resource?.ToString() ?? key;
            return args.Length > 0 ? string.Format(template, args) : template;
        }



        public ObservableCollection<JobHistoryItem> History { get; }



        public JobHistoryItem? SelectedHistoryItem
        {
            get => _selectedHistoryItem;
            set
            {
                _selectedHistoryItem = value;
                OnPropertyChanged();
                ReExecuteCommand.RaiseCanExecuteChanged();
            }
        }



        public bool SortDescending
        {
            get => _sortDescending;
            set
            {
                _sortDescending = value;
                OnPropertyChanged();
            }
        }



        public string SortDirectionLabel
        {
            get => _sortDirectionLabel;
            set
            {
                _sortDirectionLabel = value;
                OnPropertyChanged();
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
                ReExecuteCommand.RaiseCanExecuteChanged();
            }
        }



        public RelayCommand SortByDateCommand { get; }



        public RelayCommand RefreshCommand { get; }



        public RelayCommand ReExecuteCommand { get; }



        private void LoadHistory()
        {
            History.Clear();



            List<JobState> states = StateTracker.GetAllStates();



            IEnumerable<BackupJob> jobs = _backupManager
            .GetAllJobs()
            .Where(job => job.LastExecutionTime != null);



            jobs = SortDescending
            ? jobs.OrderByDescending(job => job.LastExecutionTime)
            : jobs.OrderBy(job => job.LastExecutionTime);



            foreach (BackupJob job in jobs)
            {
                JobState? state = states.FirstOrDefault(currentState =>
                string.Equals(
                currentState.BackupName,
                job.Name,
                StringComparison.OrdinalIgnoreCase));



                History.Add(new JobHistoryItem(job, state));
            }



            StatusMessage = Tr("MsgHistoryLoaded", History.Count);
        }



        private void SortByDate()
        {
            SortDescending = !SortDescending;
            SortDirectionLabel = SortDescending
            ? "↓ Plus récent"
            : "↑ Plus ancien";



            LoadHistory();
        }



        private async Task ReExecuteAsync(object? parameter)
        {
            JobHistoryItem? item = parameter as JobHistoryItem ?? SelectedHistoryItem;



            if (item == null)
            {
                StatusMessage = Tr("MsgHistoryNoSelection");
                return;
            }



            if (_backupManager.IsEditLocked)
            {
                StatusMessage = Tr("MsgHistoryExecutionLocked");
                return;
            }



            IsExecuting = true;
            StatusMessage = Tr("MsgHistoryReExecuting", item.Name);



            try
            {
                EasyLog.EasyLog? logger = _backupManager.SharedLogger;

                if (logger == null)
                {
                    AppSettings settings = _backupManager.GetSettings();
                    string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                    logger = new EasyLog.EasyLog(logDirectory, settings.LogFormat);
                }



                bool success = await Task.Run(() =>
                _backupManager.ExecuteJob(item.Id, logger));



                StatusMessage = success
                ? Tr("MsgHistoryReExecutionCompleted", item.Name)
                : Tr("MsgHistoryReExecutionFailed", item.Name);



                LoadHistory();
            }
            catch (Exception ex)
            {
                StatusMessage = Tr("MsgHistoryReExecutionError", ex.Message);
            }
            finally
            {
                IsExecuting = false;
            }
        }
    }



    public class JobHistoryItem
    {
        public JobHistoryItem(BackupJob job, JobState? state)
        {
            Id = job.Id;
            Number = job.Number;
            Name = job.Name;
            Type = job.Type;
            LastExecutionTime = job.LastExecutionTime;
            Status = state?.Status ?? JobStatus.Terminé;
        }



        public Guid Id { get; }



        public int Number { get; }



        public string Name { get; }



        public BackupType Type { get; }



        public DateTime? LastExecutionTime { get; }



        public JobStatus Status { get; }
    }
}