using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Core.Services
{
    public static class CryptoService
    {
        public static long Encrypt(string filePath, List<string> extensionsToEncrypt)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return -1;
            }

            if (!File.Exists(filePath))
            {
                return -1;
            }

            if (!ShouldEncrypt(filePath, extensionsToEncrypt))
            {
                return 0;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // TODO V2: replace this simulation with the external CryptoSoft call.
                // Expected behavior:
                // - return encryption duration in milliseconds if successful;
                // - return a negative value if CryptoSoft fails.
                SimulateEncryption(filePath);

                stopwatch.Stop();

                return Math.Max(1, stopwatch.ElapsedMilliseconds);
            }
            catch
            {
                return -1;
            }
        }

        private static bool ShouldEncrypt(string filePath, List<string> extensionsToEncrypt)
        {
            if (extensionsToEncrypt == null || extensionsToEncrypt.Count == 0)
            {
                return false;
            }

            string fileExtension = Path.GetExtension(filePath);

            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                return false;
            }

            return extensionsToEncrypt.Any(extension =>
                string.Equals(
                    NormalizeExtension(extension),
                    NormalizeExtension(fileExtension),
                    StringComparison.OrdinalIgnoreCase));
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

        private static void SimulateEncryption(string filePath)
        {
            // Minimal non-destructive placeholder.
            // The file is not modified at this stage.
            _ = new FileInfo(filePath).Length;
        }
    }
}