namespace EasySave.Domain;

public interface IBackupStrategy
{
    void Execute(BackupJob job, StateTracker tracker, EasySave.Application.EasyLog logger);
}