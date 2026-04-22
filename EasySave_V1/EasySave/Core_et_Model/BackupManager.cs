using System;
using System.Collections.Generic;
using System.Linq;
using EasySave.Models;
using EasySave.Services;
using EasySave.Strategies;

namespace EasySave.Managers
{
    public class BackupManager
    {
        private List<BackupJob> _jobs = new List<BackupJob>();
        private const int MAX_JOBS = 5;

        public List<BackupJob> Jobs => _jobs;

        public BackupManager()
        {
            LoadJobs();
        }

        public bool AddJob(BackupJob job)
        {
            if (_jobs.Count >= MAX_JOBS) return false;
            if (job.Id <= 0 || job.Id > MAX_JOBS) return false;
            if (_jobs.Any(j => j.Id == job.Id)) return false;

            _jobs.Add(job);
            SaveJobs();
            return true;
        }

        public bool RemoveJob(int id)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return false;

            _jobs.Remove(job);
            SaveJobs();
            return true;
        }

        public bool UpdateJob(BackupJob updatedJob)
        {
            var index = _jobs.FindIndex(j => j.Id == updatedJob.Id);
            if (index == -1) return false;

            _jobs[index] = updatedJob;
            SaveJobs();
            return true;
        }

        public BackupJob? GetJob(int id)
        {
            return _jobs.FirstOrDefault(j => j.Id == id);
        }

        public List<BackupJob> GetAllJobs()
        {
            return _jobs.OrderBy(j => j.Id).ToList();
        }

        public bool ExecuteJob(int id, EasyLog.EasyLog logger)
        {
            var job = GetJob(id);
            if (job == null) return false;

            IBackupStrategy strategy = job.Type == BackupType.Complete
                ? new CompleteBackupStrategy()
                : new DifferentialBackupStrategy();

            strategy.Execute(job, logger);
            job.LastExecutionTime = DateTime.Now;
            SaveJobs();
            return true;
        }

        public bool ExecuteSequential(string command, EasyLog.EasyLog logger)
        {
            var parser = new CommandLineParser(command);
            var ids = parser.ParseJobIds();

            if (ids == null || ids.Count == 0) return false;

            foreach (var id in ids)
            {
                if (!ExecuteJob(id, logger))
                    return false;
            }
            return true;
        }

        public bool IsJobIdAvailable(int id)
        {
            return !_jobs.Any(j => j.Id == id);
        }

        public void SaveJobs()
        {
            ConfigurationManager.SaveJobs(_jobs);
        }

        public void LoadJobs()
        {
            _jobs = ConfigurationManager.LoadJobs();
        }
    }
}