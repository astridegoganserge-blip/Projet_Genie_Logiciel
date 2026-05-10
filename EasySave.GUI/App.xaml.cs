
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EasySave.Core.Managers;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Core.Services;
using EasySave.GUI.Services;
using EasySave.GUI.ViewModels;



namespace EasySave.GUI
{
    public partial class App : Application
    {
        private readonly CancellationTokenSource _businessSoftwareWatcherTokenSource = new();



        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);



            var settingsRepository = new JsonSettingsRepository();
            var jobRepository = new JsonJobRepository();



            var backupManager = new BackupManager(
            jobRepository,
            settingsRepository);



            AppSettings settings = settingsRepository.Load();



            LocalizationService.ApplyLanguage(settings.Language);



            if (settings.LogMode != LogMode.Local)
            {
                LogCentralizer.Configure(settings.DockerLogServerUrl);
            }



            StartBusinessSoftwareWatcherIfNeeded(
            backupManager,
            settings);



            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel(backupManager)
            };



            mainWindow.Show();
        }



        protected override void OnExit(ExitEventArgs e)
        {
            _businessSoftwareWatcherTokenSource.Cancel();
            _businessSoftwareWatcherTokenSource.Dispose();



            base.OnExit(e);
        }



        private void StartBusinessSoftwareWatcherIfNeeded(
        BackupManager backupManager,
        AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.BusinessSoftware))
            {
                return;
            }



            Task.Run(() =>
            BusinessSoftwareWatcher.StartContinuousWatch(
            settings.BusinessSoftware,
            onDetected: backupManager.PauseAll,
            onCleared: backupManager.ResumeAll,
            token: _businessSoftwareWatcherTokenSource.Token));
        }
    }
}

