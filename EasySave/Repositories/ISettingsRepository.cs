using EasySave.Models;
namespace EasySave.Repositories
{
    public interface ISettingsRepository
    {
        AppSettings Load();
        void Save(AppSettings settings);
    }
}