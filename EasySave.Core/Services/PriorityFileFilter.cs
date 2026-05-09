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
        private static int _pendingPriorityFiles;



        public static void RegisterPriorityFiles(int count)
        {
            if (count <= 0)
            {
                return;
            }



            lock (Lock)
            {
                _pendingPriorityFiles += count;
            }
        }



        public static void NotifyPriorityFileCompleted()
        {
            lock (Lock)
            {
                _pendingPriorityFiles = Math.Max(0, _pendingPriorityFiles - 1);
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
                return _pendingPriorityFiles > 0;
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