using System;
using EasyLog;
using EasySave.Core.Models;
using EasySave.Services;

namespace EasySave.Views
{
    public static class SettingsView
    {
        public static void ShowSettingsMenu(AppSettings settings)
        {
            Console.Clear();

            Console.WriteLine(LanguageManager.T("SettingsTitle"));
            Console.WriteLine();

            Console.WriteLine("1. " + LanguageManager.T("SettingsLogFormat") + $" : {settings.LogFormat}");
            Console.WriteLine("2. " + LanguageManager.T("SettingsLogMode") + $" : {settings.LogMode}");
            Console.WriteLine("3. " + LanguageManager.T("SettingsDockerUrl") + $" : {settings.DockerLogServerUrl}");
            Console.WriteLine("4. " + LanguageManager.T("SettingsBusinessSoftware") + $" : {settings.BusinessSoftware}");
            Console.WriteLine("5. " + LanguageManager.T("SettingsMaxFileSize") + $" : {settings.MaxFileSizeKb} KB");
            Console.WriteLine("6. " + LanguageManager.T("SettingsDifferentialMirror") + $" : {FormatBoolean(settings.DeleteOrphanFilesInDifferential)}");

            Console.WriteLine();
            Console.WriteLine("--- " + LanguageManager.T("SettingsExtensionsSection") + " ---");
            Console.WriteLine(LanguageManager.T("SettingsExtensions") + " : " + FormatExtensions(settings.ExtensionsToEncrypt));
            Console.WriteLine(LanguageManager.T("SettingsPriorityExtensions") + " : " + FormatExtensions(settings.PriorityExtensions));

            Console.WriteLine();
            Console.WriteLine("7. " + LanguageManager.T("SettingsAddEncryptionExtension"));
            Console.WriteLine("8. " + LanguageManager.T("SettingsRemoveEncryptionExtension"));
            Console.WriteLine("9. " + LanguageManager.T("SettingsAddPriorityExtension"));
            Console.WriteLine("10. " + LanguageManager.T("SettingsRemovePriorityExtension"));

            Console.WriteLine();
            Console.WriteLine("0. " + LanguageManager.T("BackToMenu"));

            Console.WriteLine();
            Console.Write(LanguageManager.T("ChoicePrompt"));
        }

        public static LogFormat AskLogFormat()
        {
            Console.WriteLine(LanguageManager.T("ChooseLogFormat"));
            Console.WriteLine("1. JSON");
            Console.WriteLine("2. XML");
            Console.Write(">> ");

            string? choice = Console.ReadLine();

            return choice == "2"
                ? LogFormat.Xml
                : LogFormat.Json;
        }

        public static LogMode AskLogMode()
        {
            Console.WriteLine(LanguageManager.T("ChooseLogMode"));
            Console.WriteLine("1. Local");
            Console.WriteLine("2. Docker");
            Console.WriteLine("3. Both");
            Console.Write(">> ");

            string? choice = Console.ReadLine();

            return choice switch
            {
                "2" => LogMode.Docker,
                "3" => LogMode.Both,
                _ => LogMode.Local
            };
        }

        public static string AskDockerUrl()
        {
            Console.Write(LanguageManager.T("EnterDockerUrl"));
            return Console.ReadLine() ?? string.Empty;
        }

        public static string AskBusinessSoftware()
        {
            Console.Write(LanguageManager.T("EnterBusinessSoftware"));
            return Console.ReadLine() ?? string.Empty;
        }

        public static long AskMaxFileSizeKb()
        {
            Console.Write(LanguageManager.T("EnterMaxFileSizeKb"));

            if (long.TryParse(Console.ReadLine(), out long value) && value >= 0)
            {
                return value;
            }

            return 0;
        }

        public static bool AskDifferentialMirror()
        {
            Console.Write(LanguageManager.T("EnableDifferentialMirror"));

            string? choice = Console.ReadLine();

            return choice != null &&
                   (choice.Equals("y", StringComparison.OrdinalIgnoreCase)
                    || choice.Equals("yes", StringComparison.OrdinalIgnoreCase)
                    || choice.Equals("o", StringComparison.OrdinalIgnoreCase)
                    || choice.Equals("oui", StringComparison.OrdinalIgnoreCase)
                    || choice.Equals("1"));
        }

        public static string AskExtension()
        {
            Console.Write(LanguageManager.T("EnterExtension"));
            return Console.ReadLine() ?? string.Empty;
        }

        public static void ShowSettingsSaved()
        {
            Console.WriteLine(LanguageManager.T("SettingsSaved"));
        }

        public static void ShowInvalidChoice()
        {
            Console.WriteLine(LanguageManager.T("InvalidChoice"));
        }

        public static void ShowExtensionAlreadyExists()
        {
            Console.WriteLine(LanguageManager.T("ExtensionAlreadyExists"));
        }

        public static void ShowExtensionNotFound()
        {
            Console.WriteLine(LanguageManager.T("ExtensionNotFound"));
        }

        private static string FormatBoolean(bool value)
        {
            return value
                ? LanguageManager.T("Enabled")
                : LanguageManager.T("Disabled");
        }

        private static string FormatExtensions(System.Collections.Generic.List<string> extensions)
        {
            return extensions.Count == 0
                ? "-"
                : string.Join(", ", extensions);
        }
    }
}