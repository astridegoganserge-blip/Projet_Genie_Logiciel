namespace EasySave.Core.Models
{
    public class BackupJob
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string SourcePath { get; set; }
        public required string TargetPath { get; set; }
        public BackupType Type { get; set; }
        public DateTime? LastExecutionTime { get; set; }
    }
}
