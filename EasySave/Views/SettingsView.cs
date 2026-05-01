using System;
using EasyLog;
using EasySave.Services;



namespace EasySave.Views
{
    public static class SettingsView
    {
        public static void ShowSettingsMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.T("ChooseLogFormat"));
            Console.WriteLine("1. JSON");
            Console.WriteLine("2. XML");
            Console.Write(">> ");
        }



        public static LogFormat AskLogFormat()
        {
            ShowSettingsMenu();



            string? choice = Console.ReadLine();



            return choice == "2"
            ? LogFormat.Xml
            : LogFormat.Json;
        }



        public static void ShowSettingsSaved()
        {
            Console.WriteLine(LanguageManager.T("SettingsSaved"));
        }
    }
}