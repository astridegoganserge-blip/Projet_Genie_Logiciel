using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{
    public static class LanguageManager
    {
        private static Dictionary<string, string> _currentTranslations = new Dictionary<string, string>();
        private static string _currentLanguage = "fr";

        public static void LoadLanguage(string languageCode)
        {
            _currentLanguage = languageCode;
            string fileName = languageCode == "en" ? "en.json" : "fr.json";
            string filePath = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _currentTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? new Dictionary<string, string>();
            }
        }

        public static string GetString(string key)
        {
            if (_currentTranslations.ContainsKey(key))
                return _currentTranslations[key];

            return $"[{key}]"; // Retourne la clé si traduction manquante
        }

        public static string CurrentLanguage => _currentLanguage;
    }
}