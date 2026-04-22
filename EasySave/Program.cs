using System;
using EasySave.Services;
using EasySave.UI;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Choisissez la langue / Choose language:");
            Console.WriteLine("1. Français");
            Console.WriteLine("2. English");
            Console.Write("> ");

            string choice = Console.ReadLine();
            string lang = choice == "2" ? "en" : "fr";

            LanguageManager.LoadLanguage(lang);

            var ui = new ConsoleInterface();
            ui.Run(args);
        }
    }
}