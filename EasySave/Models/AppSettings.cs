using EasyLog;

namespace EasySave.Core_et_Model
{

    public class AppSettings
    {
       public LogFormat LogFormat { get; set; } = LogFormat.Json;
       public string Language { get; set; } = "fr";
    }
}