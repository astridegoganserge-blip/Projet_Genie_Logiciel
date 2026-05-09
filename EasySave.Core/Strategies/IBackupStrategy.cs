using EasyLog;
using EasySave.Core.Models;
using EasySave.Core.Services;



namespace EasySave.Core.Strategies
{
    public interface IBackupStrategy
    {
        bool Execute(
        BackupJob job,
        EasyLog.EasyLog logger,
        AppSettings settings,
        JobExecutionContext context);
    }
}