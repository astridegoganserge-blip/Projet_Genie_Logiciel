using System;
using System.IO;



namespace EasySave.Core.Models
{
    public class BackupJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();



        public int Number { get; set; }



        public required string Name { get; set; }



        public required string SourcePath { get; set; }



        public required string TargetPath { get; set; }



        public BackupType Type { get; set; }



        public DateTime? LastExecutionTime { get; set; }



        public bool ValidatePaths()
        {
            return Directory.Exists(SourcePath);
        }



        public override string ToString()
        {
            return $"[{Number:00}] {Name} | {Type} | {SourcePath} -> {TargetPath}";
        }
    }
}