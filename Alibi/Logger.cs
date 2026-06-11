using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Alibi.Plugins.API;

namespace Alibi
{
    public class Logger : IDisposable
    {
        public const string LogsFolder = "Logs";
        private readonly Queue<string> _logBuffer;
        private readonly CancellationTokenSource _fileWriterToken = new();

        private readonly Server _server;

        public Logger(Server server)
        {
            _server = server;
            _logBuffer = new Queue<string>(_server.ServerConfiguration.LogBufferSize);
            
            _ = WriteFileLogLoop(_fileWriterToken.Token);
        }

        private async Task WriteFileLogLoop(CancellationToken token)
        {
            await using var writer = File.CreateText(Path.Combine(LogsFolder, $"server_{DateTime.Now:dd-M_HH-mm}.log"));
            while (!token.IsCancellationRequested)
            {
                await WriteFile(writer, token);
                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    await WriteFile(writer, token);
                }
            }
        }

        private async Task WriteFile(StreamWriter writer, CancellationToken token)
        {
            while (_logBuffer.Count > 0)
                await writer.WriteLineAsync(_logBuffer.Dequeue());
            await writer.FlushAsync(token);
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
            Console.ForegroundColor = severity switch
            {
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Special => ConsoleColor.Cyan,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };
            Console.WriteLine(log);
            Console.ResetColor();
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

        public void Dispose()
        {
            Log(LogSeverity.Info, "Stopping logger...", true);
            _fileWriterToken?.Cancel();
            _fileWriterToken?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}