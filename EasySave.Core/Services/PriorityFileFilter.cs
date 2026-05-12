using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;



namespace EasySave.Core.Services
{
    public static class PriorityFileFilter
    {
        private static readonly object Lock = new();
        private static readonly Dictionary<string, int> _pendingPerJob = new();



        public static void RegisterPriorityFiles(string jobName, int count)
        {
            if (count <= 0 || string.IsNullOrWhiteSpace(jobName))
            {
                return;
            }



            lock (Lock)
            {
                _pendingPerJob[jobName] = count;
            }
        }



        public static void NotifyPriorityFileCompleted(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                return;
            }



            lock (Lock)
            {
                if (_pendingPerJob.TryGetValue(jobName, out int current))
                {
                    int remaining = Math.Max(0, current - 1);

                    if (remaining == 0)
                    {
                        _pendingPerJob.Remove(jobName);
                    }
                    else
                    {
                        _pendingPerJob[jobName] = remaining;
                    }
                }
            }
        }



        public static void ResetJob(string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                return;
            }



            lock (Lock)
            {
                _pendingPerJob.Remove(jobName);
            }
        }



        public static bool CanProcess(
        string filePath,
        List<string> priorityExtensions,
        JobExecutionContext context)
        {
            if (context.StopToken.IsCancellationRequested)
            {
                return false;
            }



            if (IsPriorityFile(filePath, priorityExtensions))
            {
                return true;
            }



            while (HasPendingPriorityFiles())
            {
                if (context.StopToken.IsCancellationRequested)
                {
                    return false;
                }



                Thread.Sleep(100);
            }



            return !context.StopToken.IsCancellationRequested;
        }



        public static bool IsPriorityFile(string filePath, List<string> priorityExtensions)
        {
            if (priorityExtensions == null || priorityExtensions.Count == 0)
            {
                return false;
            }



            string fileExtension = Path.GetExtension(filePath);



            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                return false;
            }



            return priorityExtensions.Any(extension =>
            string.Equals(
            NormalizeExtension(extension),
            NormalizeExtension(fileExtension),
            StringComparison.OrdinalIgnoreCase));
        }



        private static bool HasPendingPriorityFiles()
        {
            lock (Lock)
            {
                return _pendingPerJob.Values.Any(count => count > 0);
            }
        }



        private static string NormalizeExtension(string extension)
        {
            string normalized = extension.Trim();



            if (!normalized.StartsWith("."))
            {
                normalized = "." + normalized;
            }



            return normalized;
        }
    }
}