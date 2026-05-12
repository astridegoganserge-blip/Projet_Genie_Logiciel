using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Core.Services;
using EasySave.Core.Strategies;



namespace EasySave.Core.Managers
{
    public class BackupManager
    {
        private readonly IJobRepository _jobRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ConcurrentDictionary<string, JobExecutionContext> _contexts = new();



        private volatile bool _editLock;
        private EasyLog.EasyLog? _sharedLogger;



        public BackupManager(
        IJobRepository jobRepository,
        ISettingsRepository settingsRepository)
        {
            _jobRepository = jobRepository;
            _settingsRepository = settingsRepository;
        }



        public bool IsEditLocked => _editLock;

        public void SetLogger(EasyLog.EasyLog logger)
        {
            _sharedLogger = logger;
        }

        public EasyLog.EasyLog? SharedLogger => _sharedLogger;

        public List<BackupJob> GetAllJobs()
        {
            return _jobRepository
            .GetAll()
            .OrderBy(job => job.Number)
            .ToList();
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



            if (job.Number <= 0)
            {
                job.Number = GetNextJobNumber(jobs);
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
            if (_editLock)
            {
                return false;
            }



            return UpdateJobInternal(updatedJob);
        }



        public bool ExecuteJob(Guid id, EasyLog.EasyLog logger)
        {
            if (_editLock)
            {
                return false;
            }



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



            using var context = new JobExecutionContext();
            _contexts.AddOrUpdate(job.Name, context, (_, oldContext) =>
            {
                oldContext.Dispose();
                return context;
            });



            try
            {
                IBackupStrategy strategy = SelectStrategy(job.Type);
                bool success = strategy.Execute(job, logger, settings, context);



                if (success)
                {
                    job.LastExecutionTime = DateTime.Now;
                    UpdateJobInternal(job);
                }



                return success;
            }
            catch (Exception ex)
            {
                StateTracker.MarkAsError(job.Name, $"Exception inattendue : {ex.Message}");
                return false;
            }
            finally
            {
                _contexts.TryRemove(job.Name, out _);
                PriorityFileFilter.ResetJob(job.Name);
            }

        }



        public bool ExecuteSequential(IEnumerable<Guid> ids, EasyLog.EasyLog logger)
        {
            if (_editLock)
            {
                return false;
            }



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



        public async Task<bool> ExecuteParallel(IEnumerable<Guid> ids, EasyLog.EasyLog logger)
        {
            if (_editLock)
            {
                return false;
            }



            Guid[] jobIds = ids.ToArray();



            if (jobIds.Length == 0)
            {
                return false;
            }



            Task<bool>[] tasks = jobIds
            .Select(id => Task.Run(() => ExecuteJob(id, logger)))
            .ToArray();



            bool[] results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }



        public void PauseJob(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                return;
            }



            if (_contexts.TryGetValue(jobName, out JobExecutionContext? context))
            {
                context.RequestPause();
                StateTracker.MarkAsPaused(jobName);
            }
        }



        public void ResumeJob(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                return;
            }



            if (_contexts.TryGetValue(jobName, out JobExecutionContext? context))
            {
                context.RequestResume();
                StateTracker.MarkAsResumed(jobName);
            }
        }



        public void StopJob(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                return;
            }



            if (_contexts.TryGetValue(jobName, out JobExecutionContext? context))
            {
                context.RequestStop();
            }
        }



        public void PauseAll()
        {
            foreach (string jobName in _contexts.Keys)
            {
                PauseJob(jobName);
            }
        }



        public void ResumeAll()
        {
            foreach (string jobName in _contexts.Keys)
            {
                ResumeJob(jobName);
            }
        }



        public void StopAll()
        {
            foreach (string jobName in _contexts.Keys)
            {
                StopJob(jobName);
            }
        }



        public void LockForEdit()
        {
            _editLock = true;
        }



        public void UnlockForEdit()
        {
            _editLock = false;
        }



        public AppSettings GetSettings()
        {
            return _settingsRepository.Load();
        }



        public void SaveSettings(AppSettings settings)
        {
            _settingsRepository.Save(settings);
        }



        private bool UpdateJobInternal(BackupJob updatedJob)
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



        private static int GetNextJobNumber(List<BackupJob> jobs)
        {
            return jobs.Count == 0
            ? 1
            : jobs.Max(job => job.Number) + 1;
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