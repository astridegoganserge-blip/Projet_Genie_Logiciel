using EasySave.Core.Models;

namespace EasySave.Core.Repositories
{
    public interface ISettingsRepository
    {
        AppSettings Load();

        void Save(AppSettings settings);
    }
}