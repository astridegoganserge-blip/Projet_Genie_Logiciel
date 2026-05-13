using EasyLog;
using EasySave.Core.Models;
using EasySave.Core.Repositories;
using EasySave.Services;

namespace EasySave.Controllers
{
    public class SettingsController
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsController()
            : this(new JsonSettingsRepository())
        {
        }

        public SettingsController(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public AppSettings GetSettings()
        {
            return _settingsRepository.Load();
        }

        public void SaveSettings(AppSettings settings)
        {
            _settingsRepository.Save(settings);
        }

        public void UpdateLogFormat(LogFormat format)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.LogFormat = format;
            _settingsRepository.Save(settings);
        }

        public void UpdateLogMode(LogMode logMode)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.LogMode = logMode;
            _settingsRepository.Save(settings);
        }

        public void UpdateDockerLogServerUrl(string dockerUrl)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.DockerLogServerUrl = dockerUrl.Trim();
            _settingsRepository.Save(settings);
        }

        public void UpdateBusinessSoftware(string businessSoftware)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.BusinessSoftware = businessSoftware.Trim();
            _settingsRepository.Save(settings);
        }

        public void UpdateMaxFileSizeKb(long maxFileSizeKb)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.MaxFileSizeKb = maxFileSizeKb < 0 ? 0 : maxFileSizeKb;
            _settingsRepository.Save(settings);
        }

        public void UpdateDeleteOrphanFilesInDifferential(bool enabled)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.DeleteOrphanFilesInDifferential = enabled;
            _settingsRepository.Save(settings);
        }

        public bool AddEncryptionExtension(string extension)
        {
            AppSettings settings = _settingsRepository.Load();
            string normalized = NormalizeExtension(extension);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            if (settings.ExtensionsToEncrypt.Contains(normalized))
            {
                return false;
            }

            settings.ExtensionsToEncrypt.Add(normalized);
            _settingsRepository.Save(settings);

            return true;
        }

        public bool RemoveEncryptionExtension(string extension)
        {
            AppSettings settings = _settingsRepository.Load();
            string normalized = NormalizeExtension(extension);

            bool removed = settings.ExtensionsToEncrypt.Remove(normalized);

            if (removed)
            {
                _settingsRepository.Save(settings);
            }

            return removed;
        }

        public bool AddPriorityExtension(string extension)
        {
            AppSettings settings = _settingsRepository.Load();
            string normalized = NormalizeExtension(extension);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            if (settings.PriorityExtensions.Contains(normalized))
            {
                return false;
            }

            settings.PriorityExtensions.Add(normalized);
            _settingsRepository.Save(settings);

            return true;
        }

        public bool RemovePriorityExtension(string extension)
        {
            AppSettings settings = _settingsRepository.Load();
            string normalized = NormalizeExtension(extension);

            bool removed = settings.PriorityExtensions.Remove(normalized);

            if (removed)
            {
                _settingsRepository.Save(settings);
            }

            return removed;
        }

        public void UpdateLanguage(string language)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.Language = language;
            _settingsRepository.Save(settings);

            LanguageManager.LoadLanguage(language);
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return string.Empty;
            }

            string normalized = extension.Trim();

            if (!normalized.StartsWith("."))
            {
                normalized = "." + normalized;
            }

            return normalized.ToLowerInvariant();
        }
    }
}