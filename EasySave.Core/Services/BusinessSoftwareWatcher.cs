using System;
using System.Diagnostics;
using System.Linq;

namespace EasySave.Core.Services
{
    public static class BusinessSoftwareWatcher
    {
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
                        string currentProcessName = process.ProcessName;

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