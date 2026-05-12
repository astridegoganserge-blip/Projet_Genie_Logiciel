using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
        private EasyLog.EasyLog? _sharedLogger;



        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;



            var settingsRepository = new JsonSettingsRepository();
            var jobRepository = new JsonJobRepository();



            var backupManager = new BackupManager(
            jobRepository,
            settingsRepository);



            AppSettings settings = settingsRepository.Load();



            string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            _sharedLogger = new EasyLog.EasyLog(logDirectory, settings.LogFormat);
            backupManager.SetLogger(_sharedLogger);



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



            if (_sharedLogger != null)
            {
                _sharedLogger.Flush();
                _sharedLogger.Dispose();
            }



            base.OnExit(e);
        }



        private void OnDispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            LogFatalException("Dispatcher", e.Exception);

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\n" +
                "The application will keep running. See crash.log for details.",
                "EasySave - Unexpected error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }



        private static void OnAppDomainUnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogFatalException("AppDomain", ex);
            }
        }



        private static void OnUnobservedTaskException(
            object? sender,
            UnobservedTaskExceptionEventArgs e)
        {
            LogFatalException("UnobservedTask", e.Exception);
            e.SetObserved();
        }



        private static void LogFatalException(string source, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "crash.log");
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] {ex}\n\n";
                File.AppendAllText(logPath, line);
            }
            catch
            {
                // If we can't even log, there is nothing more to do.
            }
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
