using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services
{
    public static class ConfigurationManager
    {
        private static string _configPath = Path.Combine(AppContext.BaseDirectory, "jobs.config.json");

        public static void SaveJobs(List<BackupJob> jobs)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(jobs, options);
            File.WriteAllText(_configPath, json);
        }

        public static List<BackupJob> LoadJobs()
        {
            if (!File.Exists(_configPath))
                return new List<BackupJob>();

            string json = File.ReadAllText(_configPath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<BackupJob>();

            return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
        }
    }
}