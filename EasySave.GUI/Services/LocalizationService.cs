using System;
using System.Linq;
using System.Windows;

namespace EasySave.GUI.Services
{
    public static class LocalizationService
    {
        public static string CurrentLanguage { get; private set; } = "fr";

        public static void ApplyLanguage(string? language)
        {
            string normalizedLanguage = NormalizeLanguage(language);

            string resourcePath = $"pack://application:,,,/Resources/Strings.{normalizedLanguage}.xaml";

            var newDictionary = new ResourceDictionary
            {
                Source = new Uri(resourcePath, UriKind.Absolute)
            };

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var oldDictionaries = dictionaries
                .Where(dictionary =>
                    dictionary.Source != null &&
                    dictionary.Source.OriginalString.Contains("Strings.", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (ResourceDictionary dictionary in oldDictionaries)
            {
                dictionaries.Remove(dictionary);
            }

            dictionaries.Add(newDictionary);

            CurrentLanguage = normalizedLanguage;
        }

        private static string NormalizeLanguage(string? language)
        {
            return string.Equals(language, "en", StringComparison.OrdinalIgnoreCase)
                ? "en"
                : "fr";
        }
    }
}