using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services
{
    public static class ConfigurationManager
    {
        private static readonly string JobsConfigPath = Path.Combine(AppContext.BaseDirectory, "jobs.config.json");
        private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");

        public static void SaveJobs(List<BackupJob> jobs)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(jobs, options);
            File.WriteAllText(JobsConfigPath, json);
        }

        public static List<BackupJob> LoadJobs()
        {
            if (!File.Exists(JobsConfigPath))
            {
                return new List<BackupJob>();
            }

            string json = File.ReadAllText(JobsConfigPath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<BackupJob>();
            }

            return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
        }

        public static void SaveSettings(AppSettings settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsPath, json);
        }

        public static AppSettings LoadSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(SettingsPath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppSettings();
            }

            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
    }
}