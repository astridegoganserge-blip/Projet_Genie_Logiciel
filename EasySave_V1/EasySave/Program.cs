
using System;
using EasySave.Services;
using EasySave.UI;

namespace EasySave;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("\t \t Bienvenue sur le logiciel EasySave 1.0\n");
        Console.WriteLine("Veuillez sélectionner la langue d'affichage / Please select your display language: ");
        Console.WriteLine("1. Français");
        Console.WriteLine("2. English");
        Console.Write("Saisissez votre choix (1/2) :  ");

        string? choice = Console.ReadLine();
        string lang = choice == "2" ? "en" : "fr";

        LanguageManager.LoadLanguage(lang);

        var ui = new ConsoleInterface();
        ui.Run(args);
    }
}