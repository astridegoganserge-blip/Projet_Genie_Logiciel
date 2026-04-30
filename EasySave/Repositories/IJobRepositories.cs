using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.Repositories
{
    public interface IJobRepository
    {
        List<BackupJob> GetAll();

        BackupJob? GetById(int id);

        void Save(List<BackupJob> jobs);

        void Delete(int id);
    }
}