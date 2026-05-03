using System;
using System.Collections.Generic;
using EasySave.Core.Models;

namespace EasySave.Core.Repositories
{
    public interface IJobRepository
    {
        List<BackupJob> GetAll();

        BackupJob? GetById(Guid id);

        void Save(List<BackupJob> jobs);

        void Delete(Guid id);
    }
}