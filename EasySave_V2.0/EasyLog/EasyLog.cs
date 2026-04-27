using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasyLog
{
    public class EasyLog
    {
        private readonly string _logDirectory;
        private readonly LogFormat _format;

        public EasyLog(string logDirectory)
            : this(logDirectory, LogFormat.Json)
        {
        }

        public EasyLog(string logDirectory, LogFormat format)
        {
            _logDirectory = logDirectory;
            _format = format;

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        // v1.0 overload (retro‑compatibility)
        public void LogFileTransfer(
            string backupName,
            string sourceFile,
            string targetFile,
            long fileSize,
            long transferTimeMs)
        {
            LogFileTransfer(
                backupName,
                sourceFile,
                targetFile,
                fileSize,
                transferTimeMs,
                0); // no encryption
        }

        // v2.0 overload
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

            if (_format == LogFormat.Json)
                WriteJson(entry, filePath);
            else
                WriteXml(entry, filePath);
        }

        // JSON: one entry per line (append mode)
        private void WriteJson(LogEntry entry, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(entry, options);

            using var writer = new StreamWriter(filePath, append: true);
            writer.WriteLine(json);
        }

        // XML: full file rewrite (XML requires a full document)
        private void WriteXml(LogEntry entry, string filePath)
        {
            LogEntry[] entries;

            if (File.Exists(filePath))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(LogEntry[]));
                    using var reader = new StreamReader(filePath);
                    entries = serializer.Deserialize(reader) as LogEntry[] ?? Array.Empty<LogEntry>();
                }
                catch
                {
                    entries = Array.Empty<LogEntry>();
                }

                var list = new System.Collections.Generic.List<LogEntry>(entries)
                {
                    entry
                };
                entries = list.ToArray();
            }
            else
            {
                entries = new[] { entry };
            }

            var xmlSerializer = new XmlSerializer(typeof(LogEntry[]));
            using var writer = new StreamWriter(filePath, false);
            xmlSerializer.Serialize(writer, entries);
        }
    }
}