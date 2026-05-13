using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Core.Managers;
using EasySave.Core.Models;

namespace EasySave.Controllers
{
    public class JobController
    {
        private readonly BackupManager _backupManager;
        private readonly string _logDirectory;
        private const int MaxJobs = 5;

        public JobController(
            BackupManager backupManager,
            string logDirectory)
        {
            _backupManager = backupManager;
            _logDirectory = logDirectory;
        }

        public bool CreateJob(
            int number,
            string name,
            string source,
            string target,
            BackupType type)
        {
            List<BackupJob> jobs = _backupManager.GetAllJobs();

            if (jobs.Count >= MaxJobs)
            {
                return false;
            }

            if (number <= 0 || number > MaxJobs)
            {
                return false;
            }

            if (jobs.Any(job => job.Number == number))
            {
                return false;
            }

            var job = new BackupJob
            {
                Id = Guid.NewGuid(),
                Number = number,
                Name = string.IsNullOrWhiteSpace(name) ? "Unnamed" : name.Trim(),
                SourcePath = source,
                TargetPath = target,
                Type = type
            };

            return _backupManager.AddJob(job);
        }

        public bool DeleteJob(int number)
        {
            BackupJob? job = GetJobByNumber(number);

            if (job == null)
            {
                return false;
            }

            return _backupManager.RemoveJob(job.Id);
        }

        public List<BackupJob> GetAllJobs()
        {
            return _backupManager.GetAllJobs();
        }

        public bool ExecuteJob(int number)
        {
            BackupJob? job = GetJobByNumber(number);

            if (job == null)
            {
                return false;
            }

            EasyLog.EasyLog logger = CreateLogger();

            return _backupManager.ExecuteJob(job.Id, logger);
        }

        public bool ExecuteSequential(string command)
        {
            List<int> numbers = ParseJobNumbers(command);

            if (numbers.Count == 0)
            {
                return false;
            }

            EasyLog.EasyLog logger = CreateLogger();

            List<Guid> ids = numbers
                .Select(GetJobByNumber)
                .Where(job => job != null)
                .Select(job => job!.Id)
                .ToList();

            if (ids.Count == 0)
            {
                return false;
            }

            return _backupManager.ExecuteSequential(ids, logger);
        }

        public bool ExecuteParallel()
        {
            List<Guid> ids = _backupManager
                .GetAllJobs()
                .OrderBy(job => job.Number)
                .Select(job => job.Id)
                .ToList();

            if (ids.Count == 0)
            {
                return false;
            }

            EasyLog.EasyLog logger = CreateLogger();

            return _backupManager
                .ExecuteParallel(ids, logger)
                .GetAwaiter()
                .GetResult();
        }

        public void PauseJob(int number)
        {
            BackupJob? job = GetJobByNumber(number);

            if (job != null)
            {
                _backupManager.PauseJob(job.Name);
            }
        }

        public void ResumeJob(int number)
        {
            BackupJob? job = GetJobByNumber(number);

            if (job != null)
            {
                _backupManager.ResumeJob(job.Name);
            }
        }

        public void StopJob(int number)
        {
            BackupJob? job = GetJobByNumber(number);

            if (job != null)
            {
                _backupManager.StopJob(job.Name);
            }
        }

        public void PauseAll()
        {
            _backupManager.PauseAll();
        }

        public void ResumeAll()
        {
            _backupManager.ResumeAll();
        }

        public void StopAll()
        {
            _backupManager.StopAll();
        }

        private BackupJob? GetJobByNumber(int number)
        {
            return _backupManager
                .GetAllJobs()
                .FirstOrDefault(job => job.Number == number);
        }

        private EasyLog.EasyLog CreateLogger()
        {
            AppSettings settings = _backupManager.GetSettings();

            return new EasyLog.EasyLog(
                _logDirectory,
                settings.LogFormat);
        }

        private static List<int> ParseJobNumbers(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return new List<int>();
            }

            return command
                .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => int.TryParse(value, out int number) ? number : -1)
                .Where(number => number > 0)
                .Distinct()
                .ToList();
        }
    }
}