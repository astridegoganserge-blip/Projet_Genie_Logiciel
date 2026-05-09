using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using EasySave.Core.Models;



namespace EasySave.Core.Repositories
{
    public class JsonSettingsRepository : ISettingsRepository
    {
        private static readonly ReaderWriterLockSlim Lock = new();



        private readonly string _filePath;



        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };



        public JsonSettingsRepository()
        : this(Path.Combine(AppContext.BaseDirectory, "settings.json"))
        {
        }



        public JsonSettingsRepository(string filePath)
        {
            _filePath = filePath;
        }



        public AppSettings Load()
        {
            Lock.EnterReadLock();



            try
            {
                return NormalizeSettings(LoadSettingsUnsafe());
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }



        public void Save(AppSettings settings)
        {
            Lock.EnterWriteLock();



            try
            {
                SaveSettingsUnsafe(NormalizeSettings(settings));
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }



        private AppSettings LoadSettingsUnsafe()
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings();
            }



            string json = File.ReadAllText(_filePath);



            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppSettings();
            }



            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }



        private void SaveSettingsUnsafe(AppSettings settings)
        {
            string? directory = Path.GetDirectoryName(_filePath);



            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }



            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_filePath, json);
        }



        private static AppSettings NormalizeSettings(AppSettings settings)
        {
            settings.Language = string.IsNullOrWhiteSpace(settings.Language)
            ? "fr"
            : settings.Language;



            settings.BusinessSoftware ??= string.Empty;
            settings.ExtensionsToEncrypt ??= new();
            settings.PriorityExtensions ??= new();
            settings.DockerLogServerUrl ??= string.Empty;



            if (settings.MaxFileSizeKb < 0)
            {
                settings.MaxFileSizeKb = 0;
            }



            return settings;
        }
    }
}