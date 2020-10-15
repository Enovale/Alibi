using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AO2Sharp
{
    public class Logger
    {
        public const string LogsFolder = "Logs";

        private Server _server;
        private Queue<string> _logBuffer = new Queue<string>(Server.ServerConfiguration.LogBufferSize);
        private Queue<Tuple<LogSeverity, string>> _consoleLogQueue = new Queue<Tuple<LogSeverity, string>>();

        public Logger(Server server)
        {
            _server = server;
            Task.Run(PrintLogs);
        }

        private void PrintLogs()
        {
            while (true)
            {
                if (_consoleLogQueue.TryDequeue(out var log))
                {
                    Console.ForegroundColor = log.Item1 switch
                    {
                        LogSeverity.Info => ConsoleColor.White,
                        LogSeverity.Special => ConsoleColor.Cyan,
                        LogSeverity.Warning => ConsoleColor.Yellow,
                        LogSeverity.Error => ConsoleColor.Red,
                        _ => Console.ForegroundColor
                    };

                    Console.WriteLine(log.Item2);

                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Log a message of a specified severity (will change the console color)
        /// </summary>
        /// <param name="severity">Normal debug severities, though Special is for info that is
        /// more relevant to server owners.</param>
        /// <param name="message">The message to log</param>
        /// <param name="verbose">Should this only show when in verbose logs mode?</param>
        /// <param name="color">Manually override the log color</param>
        public void Log(LogSeverity severity, string message, bool verbose = false)
        {
            if (verbose && !Server.ServerConfiguration.VerboseLogs)
                return;
            string debug = verbose ? "[DEBUG]" : "";
            string log = $"{debug}[{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToShortTimeString()}][{severity}]{message}";
            AddLog(severity, log);
        }

        private void AddLog(LogSeverity severity, string log)
        {
            if (_logBuffer.Count >= Server.ServerConfiguration.LogBufferSize)
            {
                _logBuffer.Dequeue();
                _consoleLogQueue.Dequeue();
            }

            _logBuffer.Enqueue(log);
            _consoleLogQueue.Enqueue(new Tuple<LogSeverity, string>(severity, log));
        }

        public void IcMessageLog(string message, Area area, Client client)
        {
            Debug.Assert(client.Character != null, "client.Character == null during IC logging");
            Log(LogSeverity.Info, $"[IC][{area.Name}][{client.CharacterName}] {message}");
        }

        public void OocMessageLog(string message, Area area = null, string name = null)
        {
            string areaName = area == null ? "Global" : area.Name;
            string person = name ?? "Server";
            Log(LogSeverity.Info, $"[OC][{areaName}][{person}] {message}");
        }

        public bool Dump()
        {
            if (!Directory.Exists(LogsFolder))
                Directory.CreateDirectory(LogsFolder);

            if (_logBuffer.Count == 0)
                return false;

            var logDump = File.CreateText(Path.Combine(LogsFolder, $"server_{DateTime.Now:dd-M_HH-mm}.log"));
            while (_logBuffer.Count > 0)
                logDump.WriteLine(_logBuffer.Dequeue());
            logDump.Flush();
            logDump.Close();

            _server.DumpPluginLogs();
            return true;
        }
    }
}
