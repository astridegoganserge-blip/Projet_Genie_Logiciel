using System;
using EasySave.Core_et_Model;
using EasySave.UI_et_LOG;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Load application settings
            AppSettings settings = ConfigurationManager.LoadSettings();

            // Load selected language (default: fr)
            string language = settings.Language ?? "fr";
            LanguageManager.LoadLanguage(language);

            // Start the console interface
            var ui = new ConsoleInterface();
            ui.Run(args);
        }
    }
}