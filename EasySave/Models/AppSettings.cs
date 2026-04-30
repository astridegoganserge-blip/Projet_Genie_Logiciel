using EasyLog;

namespace EasySave.Models
{

    public class AppSettings
    {
       public LogFormat LogFormat { get; set; } = LogFormat.Json;
       public string Language { get; set; } = "fr";
    }
}