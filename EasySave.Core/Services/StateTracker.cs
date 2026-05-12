using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using EasySave.Core.Models;



namespace EasySave.Core.Services
{
    public static class StateTracker
    {
        private static readonly ConcurrentDictionary<string, JobState> States = new();
        private static readonly ReaderWriterLockSlim FileLock = new();
        private static readonly string StateFilePath = Path.Combine(AppContext.BaseDirectory, "state.json");



        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };



        static StateTracker()
        {
            JsonOptions.Converters.Add(new JsonStringEnumConverter());
        }



        public static void Initialize(BackupJob job, int totalFiles, long totalSize)
        {
            var state = new JobState
            {
                BackupName = job.Name,
                LastActionTime = DateTime.Now,
                Status = JobStatus.Actif,
                TotalFiles = totalFiles,
                TotalSize = totalSize,
                RemainingFiles = totalFiles,
                RemainingSize = totalSize,
                Progression = totalFiles == 0 ? 100 : 0,
                CurrentSourceFile = string.Empty,
                CurrentTargetFile = string.Empty,
                IsPaused = false
            };



            States.AddOrUpdate(job.Name, state, (_, _) => state);
            SaveState();
        }



        public static void UpdateProgress(
        string jobName,
        string sourceFile,
        string targetFile,
        long transferredSize)
        {
            UpdateState(jobName, state =>
            {
                state.LastActionTime = DateTime.Now;
                state.CurrentSourceFile = sourceFile;
                state.CurrentTargetFile = targetFile;
                state.RemainingFiles = Math.Max(0, state.RemainingFiles - 1);
                state.RemainingSize = Math.Max(0, state.RemainingSize - transferredSize);



                if (state.TotalFiles > 0)
                {
                    int completedFiles = state.TotalFiles - state.RemainingFiles;
                    state.Progression = Math.Round((double)completedFiles / state.TotalFiles * 100, 2);
                }
                else
                {
                    state.Progression = 100;
                }



                return state;
            });
        }



        public static void MarkAsCompleted(string jobName)
        {
            UpdateState(jobName, state =>
            {
                state.LastActionTime = DateTime.Now;
                state.Status = JobStatus.Terminé;
                state.RemainingFiles = 0;
                state.RemainingSize = 0;
                state.Progression = 100;
                state.CurrentSourceFile = string.Empty;
                state.CurrentTargetFile = string.Empty;
                state.IsPaused = false;



                return state;
            });
        }



        public static void MarkAsError(string jobName, string errorMessage = "")
        {
            UpdateState(jobName, state =>
            {
                state.LastActionTime = DateTime.Now;
                state.Status = JobStatus.Erreur;
                state.IsPaused = false;
                state.ErrorMessage = errorMessage;



                return state;
            });
        }



        public static void MarkAsInterrupted(string jobName)
        {
            UpdateState(jobName, state =>
            {
                state.LastActionTime = DateTime.Now;
                state.Status = JobStatus.Interrompu;
                state.IsPaused = false;
                state.CurrentSourceFile = string.Empty;
                state.CurrentTargetFile = string.Empty;



                return state;
            });
        }



        public static void MarkAsPaused(string jobName)
        {
            UpdateState(jobName, state =>
            {
                state.LastActionTime = DateTime.Now;
                state.Status = JobStatus.EnPause;
                state.IsPaused = true;



                return state;
            });
        }



        public static void MarkAsResumed(string jobName)
        {
            UpdateState(jobName, state =>
            {
                state.LastActionTime = DateTime.Now;
                state.Status = JobStatus.Actif;
                state.IsPaused = false;



                return state;
            });
        }



        public static JobState? GetState(string jobName)
        {
            EnsureStatesLoaded();



            return States.TryGetValue(jobName, out JobState? state)
            ? CloneState(state)
            : null;
        }



        public static List<JobState> GetAllStates()
        {
            EnsureStatesLoaded();



            return States.Values
            .Select(CloneState)
            .OrderBy(state => state.BackupName)
            .ToList();
        }



        private static void UpdateState(string jobName, Func<JobState, JobState> updateAction)
        {
            if (!States.TryGetValue(jobName, out JobState? currentState))
            {
                EnsureStatesLoaded();



                if (!States.TryGetValue(jobName, out currentState))
                {
                    return;
                }
            }



            JobState updatedState = updateAction(CloneState(currentState));
            States.AddOrUpdate(jobName, updatedState, (_, _) => updatedState);



            SaveState();
        }



        private static void SaveState()
        {
            FileLock.EnterWriteLock();



            try
            {
                List<JobState> states = States.Values
                .Select(CloneState)
                .OrderBy(state => state.BackupName)
                .ToList();



                string json = JsonSerializer.Serialize(states, JsonOptions);

                string tempFilePath = StateFilePath + ".tmp";
                File.WriteAllText(tempFilePath, json);

                if (File.Exists(StateFilePath))
                {
                    File.Replace(tempFilePath, StateFilePath, null);
                }
                else
                {
                    File.Move(tempFilePath, StateFilePath);
                }
            }
            finally
            {
                FileLock.ExitWriteLock();
            }
        }



        private static void EnsureStatesLoaded()
        {
            if (!States.IsEmpty)
            {
                return;
            }



            List<JobState> loadedStates = LoadStatesFromFile();



            foreach (JobState state in loadedStates)
            {
                States.TryAdd(state.BackupName, state);
            }
        }



        private static List<JobState> LoadStatesFromFile()
        {
            FileLock.EnterReadLock();



            try
            {
                if (!File.Exists(StateFilePath))
                {
                    return new List<JobState>();
                }



                string json = File.ReadAllText(StateFilePath);



                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<JobState>();
                }



                try
                {
                    return JsonSerializer.Deserialize<List<JobState>>(json, JsonOptions)
                    ?? new List<JobState>();
                }
                catch (JsonException)
                {

                    try
                    {
                        string corruptedPath = StateFilePath + ".corrupted";
                        File.Copy(StateFilePath, corruptedPath, true);
                    }
                    catch
                    {
                        // Best-effort quarantine; ignore secondary I/O errors.
                    }

                    return new List<JobState>();
                }
            }
            finally
            {
                FileLock.ExitReadLock();
            }
        }



        private static JobState CloneState(JobState state)
        {
            return new JobState
            {
                BackupName = state.BackupName,
                LastActionTime = state.LastActionTime,
                Status = state.Status,
                TotalFiles = state.TotalFiles,
                TotalSize = state.TotalSize,
                RemainingFiles = state.RemainingFiles,
                RemainingSize = state.RemainingSize,
                Progression = state.Progression,
                CurrentSourceFile = state.CurrentSourceFile,
                CurrentTargetFile = state.CurrentTargetFile,
                IsPaused = state.IsPaused,
                ErrorMessage = state.ErrorMessage

            };
        }
    }
}