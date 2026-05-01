using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasyLog
{
    public class EasyLog
    {
        private readonly string _logDirectory;
        private readonly LogFormat _format;

        public EasyLog(string logDirectory) : this(logDirectory, LogFormat.Json)
        {
        }

        public EasyLog(string logDirectory, LogFormat format)
        {
            _logDirectory = logDirectory;
            _format = format;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void LogFileTransfer(string backupName, string sourceFile, string targetFile, long fileSize, long transferTimeMs)
        {
            LogFileTransfer(
                backupName,
                sourceFile,
                targetFile,
                fileSize,
                transferTimeMs,
                0);
        }

        public void LogFileTransfer(
            string backupName,
            string sourceFile,
            string targetFile,
            long fileSize,
            long transferTimeMs,
            long encryptionTimeMs)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = backupName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs,
                EncryptionTimeMs = encryptionTimeMs
            };

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string extension = _format == LogFormat.Json ? "json" : "xml";
            string filePath = Path.Combine(_logDirectory, $"{date}.{extension}");

            List<LogEntry> entries = LoadExistingEntries(filePath);
            entries.Add(entry);

            SaveEntries(filePath, entries);
        }

        private List<LogEntry> LoadExistingEntries(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<LogEntry>();

            try
            {
                string content = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(content))
                    return new List<LogEntry>();

                if (_format == LogFormat.Xml)
                {
                    var serializer = new XmlSerializer(typeof(List<LogEntry>));
                    using var reader = new StringReader(content);
                    return serializer.Deserialize(reader) as List<LogEntry> ?? new List<LogEntry>();
                }
                else
                {
                    return JsonSerializer.Deserialize<List<LogEntry>>(content) ?? new List<LogEntry>();
                }
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        private void SaveEntries(string filePath, List<LogEntry> entries)
        {
            if (_format == LogFormat.Xml)
            {
                var serializer = new XmlSerializer(typeof(List<LogEntry>));
                using var writer = new StringWriter();
                serializer.Serialize(writer, entries);
                File.WriteAllText(filePath, writer.ToString());
            }
            else
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(entries, options);
                File.WriteAllText(filePath, json);
            }
        }
    }
}