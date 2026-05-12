using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using EasySave.Core.Models;



namespace EasySave.Core.Repositories
{
    public class JsonJobRepository : IJobRepository
    {
        private static readonly ReaderWriterLockSlim Lock = new();



        private readonly string _filePath;



        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };



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
            Lock.EnterReadLock();



            try
            {
                return LoadJobsUnsafe();
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }



        public BackupJob? GetById(Guid id)
        {
            Lock.EnterReadLock();



            try
            {
                return LoadJobsUnsafe().FirstOrDefault(job => job.Id == id);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }



        public void Save(List<BackupJob> jobs)
        {
            Lock.EnterWriteLock();



            try
            {
                SaveJobsUnsafe(jobs);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }



        public void Delete(Guid id)
        {
            Lock.EnterWriteLock();



            try
            {
                List<BackupJob> jobs = LoadJobsUnsafe();
                jobs.RemoveAll(job => job.Id == id);
                SaveJobsUnsafe(jobs);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }



        private List<BackupJob> LoadJobsUnsafe()
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



            try
            {
                return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
            }
            catch (JsonException)
            {
                // jobs.config.json is corrupted (partial write from a previous crash).
                // Quarantine it and start fresh rather than crashing the app at startup.
                try
                {
                    string corruptedPath = _filePath + ".corrupted";
                    File.Copy(_filePath, corruptedPath, true);
                }
                catch
                {
                    // Best-effort quarantine.
                }

                return new List<BackupJob>();
            }
        }



        private void SaveJobsUnsafe(List<BackupJob> jobs)
        {
            string? directory = Path.GetDirectoryName(_filePath);



            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }



            string json = JsonSerializer.Serialize(jobs, JsonOptions);

            // Atomic write to avoid leaving the file half-written on a crash.
            string tempFilePath = _filePath + ".tmp";
            File.WriteAllText(tempFilePath, json);

            if (File.Exists(_filePath))
            {
                File.Replace(tempFilePath, _filePath, null);
            }
            else
            {
                File.Move(tempFilePath, _filePath);
            }
        }
    }
}