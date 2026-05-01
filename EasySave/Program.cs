using System;
using EasySave.Models;
using EasySave.Services;
using EasySave.UI;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Choose language / Choisissez la langue :");
            Console.WriteLine("1. Français");
            Console.WriteLine("2. English");
            Console.Write(">> ");

            string? choice = Console.ReadLine();
            string language = choice == "2" ? "en" : "fr";

            AppSettings settings = ConfigurationManager.LoadSettings();
            settings.Language = language;
            ConfigurationManager.SaveSettings(settings);

            LanguageManager.LoadLanguage(language);

            var ui = new ConsoleInterface();
            ui.Run(args);
        }
    }
}