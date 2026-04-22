using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{
    /// <summary>
    /// Service statique de gestion des traductions de l'application.
    /// Charge un fichier JSON de langue et fournit les chaînes localisées via une clé.
    /// </summary>
    public static class LanguageManager
    {
        // Dictionnaire contenant les paires clé/valeur de la langue actuellement chargée
        private static Dictionary<string, string> _currentTranslations = new Dictionary<string, string>();

        // Code de la langue active ("fr" par défaut)
        private static string _currentLanguage = "fr";

        /// <summary>
        /// Charge le fichier de traduction correspondant au code de langue donné.
        /// Les fichiers sont attendus dans le dossier "Resources" du répertoire d'exécution.
        /// </summary>
        /// <param name="languageCode">Code de langue : "fr" ou "en".</param>
        public static void LoadLanguage(string languageCode)
        {
            _currentLanguage = languageCode;

            // Détermine le nom du fichier JSON à charger selon la langue
            string fileName = languageCode == "en" ? "en.json" : "fr.json";
            string filePath = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);

            // Charge et désérialise le fichier uniquement s'il existe
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _currentTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Retourne la chaîne localisée associée à la clé donnée.
        /// Si la clé est absente du dictionnaire, retourne la clé entourée de crochets.
        /// </summary>
        /// <param name="key">Clé de traduction.</param>
        /// <returns>Texte traduit, ou "[key]" si la traduction est manquante.</returns>
        public static string GetString(string key)
        {
            if (_currentTranslations.ContainsKey(key))
                return _currentTranslations[key];

            return $"[{key}]"; // Retourne la clé si traduction manquante
        }

        /// <summary>
        /// Code de la langue actuellement chargée ("fr" ou "en").
        /// </summary>
        public static string CurrentLanguage => _currentLanguage;
    }
}