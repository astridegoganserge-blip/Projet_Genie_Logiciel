namespace EasySave.Domain;

public interface IBackupStrategy
{
    BackupResult Execute(BackupJob job);
}