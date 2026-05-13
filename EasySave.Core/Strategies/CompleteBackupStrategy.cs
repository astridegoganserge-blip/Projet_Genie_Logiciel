using System.Collections.Generic;
using System.IO;
using EasySave.Core.Models;



namespace EasySave.Core.Strategies
{
 
    public sealed class CompleteBackupStrategy : BackupStrategyBase
    {
        protected override IEnumerable<string> SelectSourceFiles(
            BackupJob job,
            AppSettings settings)
        {
            return Directory.GetFiles(
                job.SourcePath,
                "*",
                SearchOption.AllDirectories);
        }

    }
}