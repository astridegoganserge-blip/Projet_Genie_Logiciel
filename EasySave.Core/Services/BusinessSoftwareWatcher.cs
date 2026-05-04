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
                return Process
                    .GetProcessesByName(normalizedProcessName)
                    .Any();
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