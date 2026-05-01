using System;
using System.IO;
using EasySave.Controllers;
using EasySave.Repositories;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var settingsRepository = new JsonSettingsRepository();
            var settingsController = new SettingsController(settingsRepository);

            string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            var jobRepository = new JsonJobRepository();

            var jobController = new JobController(
                jobRepository,
                settingsRepository,
                logDirectory);

            var applicationController = new ConsoleApplicationController(
                jobController,
                settingsController);

            applicationController.Run(args);
        }
    }
}