using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Services;



namespace EasySave.Views
{
    public static class JobView
    {
        public static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.T("AppTitle"));
            Console.WriteLine("1. " + LanguageManager.T("MenuCreate"));
            Console.WriteLine("2. " + LanguageManager.T("MenuExecute"));
            Console.WriteLine("3. " + LanguageManager.T("MenuSequential"));
            Console.WriteLine("4. " + LanguageManager.T("MenuShow"));
            Console.WriteLine("5. " + LanguageManager.T("MenuDelete"));
            Console.WriteLine("6. " + LanguageManager.T("MenuSettings"));
            Console.WriteLine("0. " + LanguageManager.T("MenuExit"));
            Console.Write(LanguageManager.T("ChoicePrompt"));
        }



        public static void ShowJobList(List<BackupJob> jobs)
        {
            Console.WriteLine();
            Console.WriteLine("--- " + LanguageManager.T("AppTitle") + " ---");



            if (jobs.Count == 0)
            {
                Console.WriteLine(LanguageManager.T("JobNotFound"));
            }
            else
            {
                foreach (BackupJob job in jobs)
                {
                    Console.WriteLine(job);
                }
            }



            Console.WriteLine("---");
        }



        public static void ShowJobCreated()
        {
            Console.WriteLine(LanguageManager.T("JobCreated"));
        }



        public static void ShowJobDeleted()
        {
            Console.WriteLine(LanguageManager.T("JobDeleted"));
        }



        public static void ShowJobNotFound()
        {
            Console.WriteLine(LanguageManager.T("JobNotFound"));
        }



        public static void ShowMaxJobsReached()
        {
            Console.WriteLine(LanguageManager.T("MaxJobsReached"));
        }



        public static void ShowBackupCompleted()
        {
            Console.WriteLine(LanguageManager.T("BackupCompleted"));
        }



        public static void ShowBackupFailed()
        {
            Console.WriteLine(LanguageManager.T("BackupFailed"));
        }



        public static void ShowError(string message)
        {
            Console.WriteLine(message);
        }



        public static int ReadJobId()
        {
            Console.Write(LanguageManager.T("EnterJobId"));



            if (int.TryParse(Console.ReadLine(), out int id))
            {
                return id;
            }



            return -1;
        }



        public static string ReadSequentialCommand()
        {
            Console.Write(LanguageManager.T("MenuSequential") + " ");
            return Console.ReadLine() ?? string.Empty;
        }



        public static (int id, string name, string source, string target, BackupType type) ReadCreateJobForm()
        {
            Console.Write(LanguageManager.T("EnterJobId"));
            int.TryParse(Console.ReadLine(), out int id);



            Console.Write(LanguageManager.T("EnterName"));
            string name = Console.ReadLine() ?? string.Empty;



            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Unnamed";
            }



            Console.Write(LanguageManager.T("EnterSource"));
            string source = Console.ReadLine() ?? string.Empty;



            Console.Write(LanguageManager.T("EnterTarget"));
            string target = Console.ReadLine() ?? string.Empty;



            Console.Write(LanguageManager.T("EnterType"));
            string typeChoice = Console.ReadLine() ?? "1";



            BackupType type = typeChoice == "2"
            ? BackupType.Differential
            : BackupType.Complete;



            return (id, name, source, target, type);
        }



        public static void Pause()
        {
            Console.WriteLine(LanguageManager.T("PressAnyKey"));
            Console.ReadKey();
        }
    }
}