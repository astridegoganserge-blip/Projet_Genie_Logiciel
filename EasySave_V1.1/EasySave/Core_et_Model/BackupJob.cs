namespace EasySave.Models
{
    public class BackupJob
    {
        public int Id { get; set; }
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
            return $"[{Id}] {Name} | {Type} | Source: {SourcePath} -> Target: {TargetPath}";
        }
    }
}