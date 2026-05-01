
using System;
using System.IO;
using System.Text.Json;
using EasySave.Models;



namespace EasySave.Repositories
{
    public class JsonSettingsRepository : ISettingsRepository
    {
        private readonly string _filePath;



        public JsonSettingsRepository()
        : this(Path.Combine(AppContext.BaseDirectory, "settings.json"))
        {
        }



        public JsonSettingsRepository(string filePath)
        {
            _filePath = filePath;
        }



        public AppSettings Load()
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings();
            }



            string json = File.ReadAllText(_filePath);



            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppSettings();
            }



            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }



        public void Save(AppSettings settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };



            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_filePath, json);
        }
    }
}

