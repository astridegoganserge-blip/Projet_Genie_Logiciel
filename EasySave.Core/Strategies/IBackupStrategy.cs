using EasyLog;
using EasySave.Core.Models;

namespace EasySave.Core.Strategies
{
    public interface IBackupStrategy
    {
        bool Execute(
            BackupJob job,
            EasyLog.EasyLog logger,
            AppSettings settings);
    }
}