using System;
using EasySave.Core_et_Model;
using EasySave.UI_et_LOG;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Ask user to choose language at startup
            Console.WriteLine("Choose language / Choisissez la langue :");
            Console.WriteLine("1. Français");
            Console.WriteLine("2. English");
            Console.Write(">> ");

            string? choice = Console.ReadLine();
            string language = choice == "2" ? "en" : "fr";

            // Save the chosen language in settings
            AppSettings settings = ConfigurationManager.LoadSettings();
            settings.Language = language;
            ConfigurationManager.SaveSettings(settings);

            // Load selected language
            LanguageManager.LoadLanguage(language);

            // Start the console interface
            var ui = new ConsoleInterface();
            ui.Run(args);
        }
    }
}