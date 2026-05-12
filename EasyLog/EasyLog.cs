using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Xml.Serialization;

namespace EasyLog
{
    public class EasyLog : IDisposable
    {
        private readonly string _logDirectory;
        private readonly LogFormat _format;
        private readonly BlockingCollection<(LogEntry Entry, string FilePath)> _queue;
        private readonly Thread _writerThread;
        private bool _disposed;

        public EasyLog(string logDirectory) : this(logDirectory, LogFormat.Json)
        {
        }

        public EasyLog(string logDirectory, LogFormat format)
        {
            _logDirectory = logDirectory;
            _format = format;
            _queue = new BlockingCollection<(LogEntry, string)>();

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            _writerThread = new Thread(ProcessQueue)
            {
                IsBackground = true,
                Name = "EasyLog-Writer"
            };

            _writerThread.Start();
        }

        public void LogFileTransfer(
            string backupName,
            string sourceFile,
            string targetFile,
            long fileSize,
            long transferTimeMs)
        {
            LogFileTransfer(backupName, sourceFile, targetFile, fileSize, transferTimeMs, 0);
        }

        public void LogFileTransfer(
            string backupName,
            string sourceFile,
            string targetFile,
            long fileSize,
            long transferTimeMs,
            long encryptionTimeMs)
        {
            if (_disposed)
            {
                return;
            }

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

            string date = entry.Timestamp.ToString("yyyy-MM-dd");
            string extension = _format == LogFormat.Json ? "json" : "xml";
            string filePath = Path.Combine(_logDirectory, $"{date}.{extension}");

            _queue.TryAdd((entry, filePath));
        }

        public void Flush()
        {
            _queue.CompleteAdding();
            _writerThread.Join();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (!_queue.IsAddingCompleted)
            {
                _queue.CompleteAdding();
            }

            _writerThread.Join(TimeSpan.FromSeconds(5));
            _queue.Dispose();
        }

        private void ProcessQueue()
        {
            foreach ((LogEntry entry, string filePath) in _queue.GetConsumingEnumerable())
            {
                try
                {
                    WriteEntry(entry, filePath);
                }
                catch
                {
                    // Ne jamais bloquer le thread de log
                }
            }
        }

        private void WriteEntry(LogEntry entry, string filePath)
        {
            if (_format == LogFormat.Json)
            {
                string line = JsonSerializer.Serialize(entry) + Environment.NewLine;
                File.AppendAllText(filePath, line);
            }
            else
            {
                List<LogEntry> entries = LoadXmlEntries(filePath);
                entries.Add(entry);
                SaveXmlEntries(filePath, entries);
            }
        }

        private static List<LogEntry> LoadXmlEntries(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<LogEntry>();
            }

            try
            {
                string content = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(content))
                {
                    return new List<LogEntry>();
                }

                var serializer = new XmlSerializer(typeof(List<LogEntry>));
                using var reader = new StringReader(content);
                return serializer.Deserialize(reader) as List<LogEntry> ?? new List<LogEntry>();
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        private static void SaveXmlEntries(string filePath, List<LogEntry> entries)
        {
            var serializer = new XmlSerializer(typeof(List<LogEntry>));
            using var writer = new StringWriter();
            serializer.Serialize(writer, entries);
            File.WriteAllText(filePath, writer.ToString());
        }
    }
}