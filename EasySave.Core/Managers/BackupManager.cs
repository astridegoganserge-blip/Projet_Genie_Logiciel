using System;
using System.Collections.Generic;
using System.Linq;
using EasyLog;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Core.Strategies;
using EasySave.Core.Services;

namespace EasySave.Core.Managers
{
    public class BackupManager
    {
        private readonly IJobRepository _jobRepository;
        private readonly ISettingsRepository _settingsRepository;

        public BackupManager(
            IJobRepository jobRepository,
            ISettingsRepository settingsRepository)
        {
            _jobRepository = jobRepository;
            _settingsRepository = settingsRepository;
        }

        public List<BackupJob> GetAllJobs()
        {
            return _jobRepository.GetAll();
        }

        public BackupJob? GetJob(Guid id)
        {
            return _jobRepository.GetById(id);
        }

        public bool AddJob(BackupJob job)
        {
            if (!job.ValidatePaths())
            {
                return false;
            }

            List<BackupJob> jobs = _jobRepository.GetAll();

            if (job.Id == Guid.Empty)
            {
                job.Id = Guid.NewGuid();
            }

            if (jobs.Any(existingJob => existingJob.Id == job.Id))
            {
                return false;
            }

            jobs.Add(job);
            _jobRepository.Save(jobs);

            return true;
        }

        public bool RemoveJob(Guid id)
        {
            BackupJob? job = _jobRepository.GetById(id);

            if (job == null)
            {
                return false;
            }

            _jobRepository.Delete(id);
            return true;
        }

        public bool UpdateJob(BackupJob updatedJob)
        {
            List<BackupJob> jobs = _jobRepository.GetAll();
            int index = jobs.FindIndex(job => job.Id == updatedJob.Id);

            if (index < 0)
            {
                return false;
            }

            if (!updatedJob.ValidatePaths())
            {
                return false;
            }

            jobs[index] = updatedJob;
            _jobRepository.Save(jobs);

            return true;
        }

        public bool ExecuteJob(Guid id, EasyLog.EasyLog logger)
        {
            BackupJob? job = _jobRepository.GetById(id);

            if (job == null)
            {
                return false;
            }

            AppSettings settings = _settingsRepository.Load();

            if (BusinessSoftwareWatcher.IsRunning(settings.BusinessSoftware))
            {
                logger.LogFileTransfer(
                    job.Name,
                    $"Business software detected: {settings.BusinessSoftware}",
                    string.Empty,
                    0,
                    -1,
                    0);

                return false;
            }

            IBackupStrategy strategy = SelectStrategy(job.Type);

            bool success = strategy.Execute(job, logger, settings);

            if (success)
            {
                job.LastExecutionTime = DateTime.Now;
                UpdateJob(job);
            }

            return success;
        }

        public bool ExecuteSequential(IEnumerable<Guid> ids, EasyLog.EasyLog logger)
        {
            bool allSucceeded = true;

            foreach (Guid id in ids)
            {
                if (!ExecuteJob(id, logger))
                {
                    allSucceeded = false;
                }
            }

            return allSucceeded;
        }

        public AppSettings GetSettings()
        {
            return _settingsRepository.Load();
        }

        public void SaveSettings(AppSettings settings)
        {
            _settingsRepository.Save(settings);
        }

        private static IBackupStrategy SelectStrategy(BackupType type)
        {
            return type switch
            {
                BackupType.Differential => new DifferentialBackupStrategy(),
                _ => new CompleteBackupStrategy()
            };
        }
    }
}