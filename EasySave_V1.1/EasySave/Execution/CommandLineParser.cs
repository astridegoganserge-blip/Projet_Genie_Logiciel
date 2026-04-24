using System;
using System.Collections.Generic;
using System.Linq;

namespace EasySave.Execution
{
    public sealed class CommandLineParser
    {
        private readonly string[] _args;

        public CommandLineParser(string[] args)
        {
            _args = args;
        }

        public List<int> ParseJobIds()
        {
            var ids = new List<int>();
            foreach (var arg in _args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                if (arg.Contains('-'))
                {
                    var parts = arg.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int start) &&
                        int.TryParse(parts[1], out int end) &&
                        start > 0 && end >= start)
                    {
                        for (int i = start; i <= end; i++)
                            ids.Add(i);

                        continue;
                    }
                }

                if (arg.Contains(','))
                {
                    var cleaned = arg.Replace("run=", "", StringComparison.OrdinalIgnoreCase);

                    var parts = cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var part in parts)
                    {
                        if (int.TryParse(part.Trim(), out int id) && id > 0)
                            ids.Add(id);
                    }

                    continue;
                }

                if (int.TryParse(arg, out int singleId) && singleId > 0)
                {
                    ids.Add(singleId);
                }
            }

            return ids;
        }
    }
}