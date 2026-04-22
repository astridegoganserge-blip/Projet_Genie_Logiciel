namespace EasySave.Models
{
    public class BackupJob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public BackupType Type { get; set; }
        public DateTime LastExecutionTime { get; set; }

        public bool ValidatePaths()
        {
            return System.IO.Directory.Exists(SourcePath);
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} | {Type} | Source: {SourcePath} -> Target: {TargetPath}";
        }
    }
}