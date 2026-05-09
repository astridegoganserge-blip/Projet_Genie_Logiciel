using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;



namespace EasySave.Core.Services
{
    public static class CryptoService
    {
        private const string EncryptionKey = "ProSoftEasySave";
        private const string MutexName = @"Global\CryptoSoft_ProSoft_Mutex";
        private static readonly TimeSpan MutexTimeout = TimeSpan.FromSeconds(30);



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



            string? cryptoSoftPath = FindCryptoSoftExecutable();



            if (string.IsNullOrWhiteSpace(cryptoSoftPath))
            {
                return -1;
            }



            if (!WaitForCryptoSoftAvailability())
            {
                return -2;
            }



            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = cryptoSoftPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                };



                processStartInfo.ArgumentList.Add(filePath);
                processStartInfo.ArgumentList.Add(EncryptionKey);



                using Process? process = Process.Start(processStartInfo);



                if (process == null)
                {
                    return -1;
                }



                process.WaitForExit();



                if (process.ExitCode < 0)
                {
                    return process.ExitCode;
                }



                return Math.Max(1, process.ExitCode);
            }
            catch
            {
                return -1;
            }
        }



        private static bool WaitForCryptoSoftAvailability()
        {
            using var mutex = new Mutex(false, MutexName);
            bool acquired = false;



            try
            {
                try
                {
                    acquired = mutex.WaitOne(MutexTimeout);
                }
                catch (AbandonedMutexException)
                {
                    acquired = true;
                }



                return acquired;
            }
            finally
            {
                if (acquired)
                {
                    mutex.ReleaseMutex();
                }
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



        private static string? FindCryptoSoftExecutable()
        {
            string baseDirectory = AppContext.BaseDirectory;



            string[] candidatePaths =
            {
 Path.Combine(baseDirectory, "CryptoSoft", "CryptoSoft.exe"),
 Path.Combine(baseDirectory, "CryptoSoft.exe"),



 Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "CryptoSoft", "bin", "Debug", "net10.0", "win-x64", "CryptoSoft.exe")),
 Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "CryptoSoft", "bin", "Release", "net10.0", "win-x64", "CryptoSoft.exe")),



 Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "CryptoSoft", "bin", "Debug", "net10.0", "CryptoSoft.exe")),
 Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "CryptoSoft", "bin", "Release", "net10.0", "CryptoSoft.exe"))
 };



            return candidatePaths.FirstOrDefault(File.Exists);
        }
    }
}