using System;
using System.Windows;



namespace EasySave.Core.Models
{

    public class JobState
    {
        public string BackupName { get; set; } = string.Empty;

        public string DisplayStatus
        {
            get
            {
                string key = $"Status{Status}";
                object? resource = Application.Current?.TryFindResource(key);
                return resource?.ToString() ?? Status.ToString();
            }
        }



        public DateTime LastActionTime { get; set; }



        public JobStatus Status { get; set; } = JobStatus.Terminé;



        public int TotalFiles { get; set; }



        public long TotalSize { get; set; }



        public int RemainingFiles { get; set; }



        public long RemainingSize { get; set; }



        public double Progression { get; set; }



        public string CurrentSourceFile { get; set; } = string.Empty;



        public string CurrentTargetFile { get; set; } = string.Empty;



        public bool IsPaused { get; set; }


        public string ErrorMessage { get; set; } = string.Empty;
    }
}