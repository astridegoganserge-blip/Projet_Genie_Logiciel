using System;
using System.Collections.Generic;
using System.Linq;

namespace EasySave.Services
{
    public class CommandLineParser
    {
        private string _command;

        public CommandLineParser(string command)
        {
            _command = command;
        }

        public List<int> ParseJobIds()
        {
            if (string.IsNullOrWhiteSpace(_command))
                return new List<int>();

            if (_command.Contains("-"))
            {
                var parts = _command.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int start) &&
                    int.TryParse(parts[1], out int end))
                {
                    var result = new List<int>();
                    for (int i = start; i <= end; i++)
                        result.Add(i);
                    return result;
                }
            }
            else if (_command.Contains(";"))
            {
                return _command.Split(';')
                    .Select(s => int.TryParse(s.Trim(), out int id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList();
            }
            else if (int.TryParse(_command, out int singleId))
            {
                return new List<int> { singleId };
            }

            return new List<int>();
        }
    }
}