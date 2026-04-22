using System;
using EasySave.Services;
using EasySave.UI;

namespace EasySave
{
    /// <summary>
    /// Point d'entrée de l'application EasySave.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Initialise la langue choisie par l'utilisateur, puis lance l'interface console.
        /// </summary>
        /// <param name="args">
        /// Arguments de ligne de commande optionnels.
        /// Si un argument est fourni, il est transmis à ConsoleInterface pour une exécution directe.
        /// </param>
        static void Main(string[] args)
        {
            // Demande à l'utilisateur de choisir la langue au démarrage
            Console.WriteLine("Choisissez la langue / Choose language:");
            Console.WriteLine("1. Français");
            Console.WriteLine("2. English");
            Console.Write("> ");

            string choice = Console.ReadLine();

            // Par défaut : français ; anglais si l'utilisateur tape "2"
            string lang = choice == "2" ? "en" : "fr";

            // Charge les traductions correspondant à la langue choisie
            LanguageManager.LoadLanguage(lang);

            // Lance l'interface console principale
            var ui = new ConsoleInterface();
            ui.Run(args);
        }
    }
}