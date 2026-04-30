using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Repositories
{
    public class JsonJobRepository : IJobRepository
    {
        private readonly string _filePath;

        public JsonJobRepository()
            : this(Path.Combine(AppContext.BaseDirectory, "jobs.config.json"))
        {
        }

        public JsonJobRepository(string filePath)
        {
            _filePath = filePath;
        }

        public List<BackupJob> GetAll()
        {
            if (!File.Exists(_filePath))
            {
                return new List<BackupJob>();
            }

            string json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<BackupJob>();
            }

            return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
        }

        public BackupJob? GetById(int id)
        {
            return GetAll().FirstOrDefault(job => job.Id == id);
        }

        public void Save(List<BackupJob> jobs)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(jobs, options);
            File.WriteAllText(_filePath, json);
        }

        public void Delete(int id)
        {
            List<BackupJob> jobs = GetAll();
            jobs.RemoveAll(job => job.Id == id);
            Save(jobs);
        }
    }
}