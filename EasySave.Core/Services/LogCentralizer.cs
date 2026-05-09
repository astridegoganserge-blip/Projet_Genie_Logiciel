using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyLog;



namespace EasySave.Core.Services
{
    public static class LogCentralizer
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };



        private static string _serverUrl = string.Empty;



        public static void Configure(string serverUrl)
        {
            _serverUrl = string.IsNullOrWhiteSpace(serverUrl)
            ? string.Empty
            : serverUrl.Trim().TrimEnd('/');
        }



        public static async Task SendAsync(LogEntry entry, string machineName)
        {
            if (string.IsNullOrWhiteSpace(_serverUrl))
            {
                return;
            }



            try
            {
                var payload = new
                {
                    MachineName = string.IsNullOrWhiteSpace(machineName)
                ? Environment.MachineName
                : machineName,
                    Log = entry
                };



                string json = JsonSerializer.Serialize(payload);



                using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");



                using HttpResponseMessage response = await Client.PostAsync(
                $"{_serverUrl}/log",
                content);



                _ = response.IsSuccessStatusCode;
            }
            catch
            {
                // Network errors must never block backup execution.
            }
        }
    }
}