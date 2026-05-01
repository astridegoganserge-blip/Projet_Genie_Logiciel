using System;



namespace EasySave.Views
{
    public static class LanguageView
    {
        public static string AskLanguage()
        {
            Console.WriteLine("Choose language / Choisissez la langue :");
            Console.WriteLine("1. Français");
            Console.WriteLine("2. English");
            Console.Write(">> ");



            string? choice = Console.ReadLine();



            return choice == "2" ? "en" : "fr";
        }



        public static void ShowLanguageSelected(string language)
        {
            string label = language == "en" ? "English" : "Français";
            Console.WriteLine($"Language selected / Langue sélectionnée : {label}");
        }
    }
}