using EasyLog;
using EasySave.Models;
using EasySave.Repositories;
using EasySave.Services;



namespace EasySave.Controllers
{
    public class SettingsController
    {
        private readonly ISettingsRepository _settingsRepository;



        public SettingsController(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }



        public AppSettings GetSettings()
        {
            return _settingsRepository.Load();
        }



        public void UpdateLogFormat(LogFormat format)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.LogFormat = format;
            _settingsRepository.Save(settings);
        }



        public void UpdateLanguage(string language)
        {
            AppSettings settings = _settingsRepository.Load();
            settings.Language = language;
            _settingsRepository.Save(settings);



            LanguageManager.LoadLanguage(language);
        }
    }
}