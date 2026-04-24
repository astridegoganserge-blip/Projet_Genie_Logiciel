using EasyLog;
using EasySave.Core_et_Model;

namespace EasySave.Execution
{
    public interface IBackupStrategy
    {
        void Execute(BackupJob job, EasyLog.EasyLog logger);
    }
}