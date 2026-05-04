using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Core.Models;


namespace EasySave.Core.Services
{
    public static class StateTracker
    {
        private static readonly Dictionary<string, JobState> States = new();
        private static readonly string StateFilePath = Path.Combine(AppContext.BaseDirectory, "state.json");

        public static void Initialize(BackupJob job, int totalFiles, long totalSize)
        {
            States[job.Name] = new JobState
            {
                BackupName = job.Name,
                LastActionTime = DateTime.Now,
                Status = JobStatus.Actif,
                TotalFiles = totalFiles,
                TotalSize = totalSize,
                RemainingFiles = totalFiles,
                RemainingSize = totalSize,
                Progression = 0,
                CurrentSourceFile = string.Empty,
                CurrentTargetFile = string.Empty
            };

            SaveState();
        }

        public static void UpdateProgress(
            string jobName,
            string sourceFile,
            string targetFile,
            long transferredSize)
        {
            if (!States.TryGetValue(jobName, out JobState? state))
            {
                return;
            }

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

            SaveState();
        }

        public static void MarkAsCompleted(string jobName)
        {
            if (!States.TryGetValue(jobName, out JobState? state))
            {
                return;
            }

            state.LastActionTime = DateTime.Now;
            state.Status = JobStatus.Terminé;
            state.RemainingFiles = 0;
            state.RemainingSize = 0;
            state.Progression = 100;
            state.CurrentSourceFile = string.Empty;
            state.CurrentTargetFile = string.Empty;

            SaveState();
        }

        public static void MarkAsError(string jobName)
        {
            if (!States.TryGetValue(jobName, out JobState? state))
            {
                return;
            }

            state.LastActionTime = DateTime.Now;
            state.Status = JobStatus.Erreur;

            SaveState();
        }

        public static void MarkAsInterrupted(string jobName)
        {
            if (!States.TryGetValue(jobName, out JobState? state))
            {
                return;
            }

            state.LastActionTime = DateTime.Now;
            state.Status = JobStatus.Interrompu;

            SaveState();
        }

        public static JobState? GetState(string jobName)
        {
            return States.TryGetValue(jobName, out JobState? state)
                ? state
                : null;
        }

        public static List<JobState> GetAllStates()
        {
            if (States.Count > 0)
            {
                return States.Values.ToList();
            }

            return LoadStatesFromFile();
        }

        private static void SaveState()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

            List<JobState> states = States.Values.ToList();
            string json = JsonSerializer.Serialize(states, options);
            File.WriteAllText(StateFilePath, json);
        }

        private static List<JobState> LoadStatesFromFile()
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

            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());

            return JsonSerializer.Deserialize<List<JobState>>(json, options) ?? new List<JobState>();
        }
    }
}