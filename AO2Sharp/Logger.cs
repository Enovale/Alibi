using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AO2Sharp
{
    internal class Logger
    {
        public const string LogsFolder = "Logs";

        private Server _server;
        private Queue<string> _logBuffer = new Queue<string>(Server.ServerConfiguration.LogBufferSize);

        public Logger(Server server)
        {
            _server = server;
        }

        /// <summary>
        /// Log a message of a specified severity (will change the console color)
        /// </summary>
        /// <param name="severity">Normal debug severities, though Special is for info that is
        /// more relevant to server owners.</param>
        /// <param name="message">The message to log</param>
        public void Log(LogSeverity severity, string message, bool verbose = false)
        {
            if (verbose && !Server.ServerConfiguration.VerboseLogs)
                return;
            Console.ForegroundColor = severity switch
            {
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Special => ConsoleColor.Cyan,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };

            string log = $"[{DateTime.Now.DayOfWeek}, {DateTime.Now.ToShortTimeString()}][{severity}]{message}";
            Console.WriteLine(log);
            AddLog(log);

            Console.ResetColor();
        }

        private void AddLog(string log)
        {
            if (_logBuffer.Count >= Server.ServerConfiguration.LogBufferSize)
            {
                _logBuffer.Dequeue();
            }

            _logBuffer.Enqueue(log);
        }

        public void IcMessageLog(string message, Area area, Client client)
        {
            Debug.Assert(client.Character != null, "client.Character == null during IC logging");
            Log(LogSeverity.Info, $"[IC][{area.Name}][{Server.CharactersList[(int)client.Character]}] {message}");
        }

        public void OocMessageLog(string message, Area area = null, string name = null)
        {
            string areaName = area == null ? "Global" : area.Name;
            string person = name == null ? "Server" : name;
            Log(LogSeverity.Info, $"[OC][{areaName}][{person}] {message}");
        }

        public async Task<bool> Dump()
        {
            if (!Directory.Exists(LogsFolder))
                Directory.CreateDirectory(LogsFolder);

            if (_logBuffer.Count == 0)
                return false;

            var logDump = File.CreateText(Path.Combine(LogsFolder, $"server_{DateTime.Now:dd-M_HH-mm}.log"));
            while (_logBuffer.Count > 0)
                await logDump.WriteLineAsync(_logBuffer.Dequeue());
            await logDump.FlushAsync();
            logDump.Close();
            return true;
        }
    }
}
