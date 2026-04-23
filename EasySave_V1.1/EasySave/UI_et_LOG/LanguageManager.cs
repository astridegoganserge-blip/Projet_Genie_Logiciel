using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class LanguageManager
{
    private static Dictionary<string, string> _translations = new();

    public static void LoadLanguage(string language)
    {
        try
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "languages", $"{language}.json");

            Console.WriteLine("Chargement du fichier : " + filePath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Fichier de langue introuvable : {filePath}");
                return;
            }

            string jsonContent = File.ReadAllText(filePath);

            Dictionary<string, string>? result =
                JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

            _translations = result ?? new Dictionary<string, string>();

            Console.WriteLine("Langue chargée avec succès.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur lors du chargement de la langue : " + ex.Message);
        }
    }

    public static string T(string key)
    {
        if (_translations.TryGetValue(key, out string? value))
            return value;

        return $"[{key}]";
    }
}