using System.Windows;
using EasySave.Core.Managers;
using EasySave.Core.Repositories;
using EasySave.GUI.Services;

namespace EasySave.GUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var backupManager = new BackupManager(
                new JsonJobRepository(),
                new JsonSettingsRepository());

            string language = backupManager.GetSettings().Language;
            LocalizationService.ApplyLanguage(language);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}