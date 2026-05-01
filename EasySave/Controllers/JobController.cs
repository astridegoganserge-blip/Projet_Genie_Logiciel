using System.Collections.Generic;
using EasyLog;
using EasySave.Models;
using EasySave.Repositories;
using EasySave.Services;
using EasySave.Strategies;



namespace EasySave.Controllers
{
    public class JobController
    {
        private readonly IJobRepository _jobRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly string _logDirectory;
        private const int MaxJobs = 5;



        public JobController(IJobRepository jobRepository, ISettingsRepository settingsRepository, string logDirectory)
        {
            _jobRepository = jobRepository;
            _settingsRepository = settingsRepository;
            _logDirectory = logDirectory;
        }



        public bool CreateJob(int id, string name, string source, string target, BackupType type)
        {
            List<BackupJob> jobs = _jobRepository.GetAll();



            if (jobs.Count >= MaxJobs)
            {
                return false;
            }



            if (id <= 0 || id > MaxJobs)
            {
                return false;
            }



            if (_jobRepository.GetById(id) != null)
            {
                return false;
            }



            var job = new BackupJob
            {
                Id = id,
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = type
            };



            if (!job.ValidatePaths())
            {
                return false;
            }



            jobs.Add(job);
            _jobRepository.Save(jobs);
            return true;
        }



        public bool DeleteJob(int id)
        {
            BackupJob? job = _jobRepository.GetById(id);



            if (job == null)
            {
                return false;
            }



            _jobRepository.Delete(id);
            return true;
        }



        public List<BackupJob> GetAllJobs()
        {
            return _jobRepository.GetAll();
        }



        public bool ExecuteJob(int id)
        {
            BackupJob? job = _jobRepository.GetById(id);



            if (job == null)
            {
                return false;
            }



            IBackupStrategy strategy = SelectStrategy(job.Type);
            EasyLog.EasyLog logger = CreateLogger();



            try
            {
                strategy.Execute(job, logger);
                job.LastExecutionTime = System.DateTime.Now;



                List<BackupJob> jobs = _jobRepository.GetAll();
                int index = jobs.FindIndex(currentJob => currentJob.Id == id);



                if (index >= 0)
                {
                    jobs[index] = job;
                    _jobRepository.Save(jobs);
                }



                return true;
            }
            catch
            {
                StateTracker.MarkAsError("Backup execution failed");
                return false;
            }
        }



        public bool ExecuteSequential(string command)
        {
            var parser = new CommandLineParser(new[] { command });
            List<int> ids = parser.ParseJobIds();



            if (ids.Count == 0)
            {
                return false;
            }



            bool allSuccess = true;



            foreach (int id in ids)
            {
                if (!ExecuteJob(id))
                {
                    allSuccess = false;
                }
            }



            return allSuccess;
        }


        private EasyLog.EasyLog CreateLogger()
        {
            AppSettings settings = _settingsRepository.Load();
            return new EasyLog.EasyLog(_logDirectory, settings.LogFormat);
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