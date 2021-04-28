using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Alibi.Plugins.API;

namespace Alibi
{
    public class Logger
    {
        public const string LogsFolder = "Logs";
        private readonly Queue<Tuple<LogSeverity, string>> _consoleLogQueue = new Queue<Tuple<LogSeverity, string>>();
        private readonly Queue<string> _logBuffer;

        private readonly Server _server;

        public Logger(Server server)
        {
            _server = server;
            _logBuffer = new Queue<string>(_server.ServerConfiguration.LogBufferSize);
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
        /// <param name="severity">
        /// Normal debug severities, though Special is for info that is
        /// more relevant to server owners.
        /// </param>
        /// <param name="message">The message to log</param>
        /// <param name="verbose">Should this only show when in verbose logs mode?</param>
        public void Log(LogSeverity severity, string message, bool verbose = false)
        {
            if (verbose && !_server.ServerConfiguration.VerboseLogs)
                return;
            var debug = verbose ? "[DEBUG]" : "";
            var log =
                $"{debug}[{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToShortTimeString()}][{severity}]{message}";
            AddLog(severity, log);
        }

        private void AddLog(LogSeverity severity, string log)
        {
            if (_logBuffer.Count >= _server.ServerConfiguration.LogBufferSize)
                _logBuffer.Dequeue();

            _logBuffer.Enqueue(log);
            _consoleLogQueue.Enqueue(new Tuple<LogSeverity, string>(severity, log));
        }

        public void IcMessageLog(string message, IArea area, IClient client)
        {
            Debug.Assert(client.Character != null, "client.Character == null during IC logging");
            Log(LogSeverity.Info, $"[IC][{area.Name}][{client.CharacterName}] {message}");
        }

        public void OocMessageLog(string message, IArea area = null, string name = null)
        {
            var areaName = area == null ? "Global" : area.Name;
            var person = name ?? "ServerRef";
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

            return true;
        }
    }
}