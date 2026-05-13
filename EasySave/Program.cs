using System;
using System.IO;
using EasySave.Controllers;
using EasySave.Core.Managers;
using EasySave.Core.Repositories;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var settingsRepository = new JsonSettingsRepository();
            var jobRepository = new JsonJobRepository();

            var backupManager = new BackupManager(
                jobRepository,
                settingsRepository);

            var settingsController = new SettingsController();

            string logDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "logs");

            var jobController = new JobController(
                backupManager,
                logDirectory);

            var applicationController = new ConsoleApplicationController(
                jobController,
                settingsController);

            applicationController.Run(args);
        }
    }
}