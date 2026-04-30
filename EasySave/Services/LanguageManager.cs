using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{ 
    public static class LanguageManager
    {
        private static Dictionary<string, string> _translations = new();

        public static void LoadLanguage(string language)
        {
            try
            {
                string filePath = Path.Combine(AppContext.BaseDirectory, "languages", $"{language}.json");

                if (!File.Exists(filePath))
                {
                    _translations = new Dictionary<string, string>();
                    return;
                }

                string jsonContent = File.ReadAllText(filePath);

                Dictionary<string, string>? result =
                JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                _translations = result ?? new Dictionary<string, string>();
            }
                catch
                {
                    _translations = new Dictionary<string, string>();
                }   
        }

            public static string T(string key)
            {
                if (_translations.TryGetValue(key, out string? value))
                return value;

                    return $"[{key}]";
            }
    }
}