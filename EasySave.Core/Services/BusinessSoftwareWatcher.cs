using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;



namespace EasySave.Core.Services
{
    public static class BusinessSoftwareWatcher
    {
        private static readonly TimeSpan WatchInterval = TimeSpan.FromMilliseconds(500);



        public static bool IsRunning(string? processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return false;
            }



            string normalizedProcessName = NormalizeProcessName(processName);



            try
            {
                Process[] processes = Process.GetProcesses();



                return processes.Any(process =>
                {
                    try
                    {
                        string currentProcessName = NormalizeProcessName(process.ProcessName);



                        return string.Equals(
                        currentProcessName,
                        normalizedProcessName,
                        StringComparison.OrdinalIgnoreCase)
                        || currentProcessName.Contains(
                        normalizedProcessName,
                        StringComparison.OrdinalIgnoreCase)
                        || normalizedProcessName.Contains(
                        currentProcessName,
                        StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return false;
            }
        }



        public static void StartContinuousWatch(
        string processName,
        Action onDetected,
        Action onCleared,
        CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return;
            }



            bool wasRunning = false;



            while (!token.IsCancellationRequested)
            {
                bool isRunning = IsRunning(processName);



                if (isRunning && !wasRunning)
                {
                    wasRunning = true;
                    SafeInvoke(onDetected);
                }
                else if (!isRunning && wasRunning)
                {
                    wasRunning = false;
                    SafeInvoke(onCleared);
                }



                if (token.WaitHandle.WaitOne(WatchInterval))
                {
                    break;
                }
            }
        }



        private static void SafeInvoke(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // Business software monitoring must never stop the application.
            }
        }



        private static string NormalizeProcessName(string processName)
        {
            string normalized = processName.Trim();



            if (normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized[..^4];
            }



            return normalized;
        }
    }
}