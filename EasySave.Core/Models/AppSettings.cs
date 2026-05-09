using System.Collections.Generic;
using EasyLog;



namespace EasySave.Core.Models
{
    public class AppSettings
    {
        public LogFormat LogFormat { get; set; } = LogFormat.Json;



        public string Language { get; set; } = "fr";



        public List<string> ExtensionsToEncrypt { get; set; } = new();



        public string BusinessSoftware { get; set; } = string.Empty;



        public List<string> PriorityExtensions { get; set; } = new();



        public long MaxFileSizeKb { get; set; }



        public LogMode LogMode { get; set; } = LogMode.Local;



        public string DockerLogServerUrl { get; set; } = string.Empty;
    }
}