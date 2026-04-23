using EasyLog;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies;

public interface IBackupStrategy
{
    void Execute(BackupJob job, EasyLog.EasyLog logger);
}