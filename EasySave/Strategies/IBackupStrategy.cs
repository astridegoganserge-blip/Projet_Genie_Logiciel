using EasyLog;
using EasySave.Models;

namespace EasySave.Strategies
{
    public interface IBackupStrategy
    {
        void Execute(BackupJob job, EasyLog.EasyLog logger);
    }
}