using System;
using System.Collections.Generic;
using System.IO;

namespace AO2Sharp.Plugins.API
{
    public abstract class Plugin
    {
        public abstract string ID { get; }
        public abstract string DebugName { get; }
        public IServer Server { get; set; }
        public abstract void Initialize(IPluginManager manager);

        public virtual void OnAllPluginsLoaded() { }
        public virtual void OnModCall(IClient caller, string reason) { }

        private Queue<string> _logBuffer = new Queue<string>(500);

        public void LogInfo(string message, bool verbose = false)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Log($"[Info][{DebugName}] {message}", verbose);
            Console.ResetColor();
        }

        public void LogWarning(string message, bool verbose = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log($"[Warning][{DebugName}] {message}", verbose);
            Console.ResetColor();
        }

        public void LogError(string message, bool verbose = false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log($"[Error][{DebugName}] {message}", verbose);
            Console.ResetColor();
        }

        public void LogSpecial(string message, bool verbose = false)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Log($"[Special][{DebugName}] {message}", verbose);
            Console.ResetColor();
        }

        private void Log(string message, bool verbose = false)
        {
            if (verbose && !Server.VerboseLogs)
                return;
            string debug = verbose ? "[DEBUG]" : "";
            string log = $"{debug}[{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToShortTimeString()}]{message}";
            Console.WriteLine(log);
            if (_logBuffer.Count >= 500)
                _logBuffer.Dequeue();
            _logBuffer.Enqueue(log);
        }

        public void DumpLogs(string path)
        {
            List<string> logDump = new List<string>(_logBuffer.Count);

            while (_logBuffer.Count > 0)
                logDump.Add(_logBuffer.Dequeue());

            File.WriteAllLines(path, logDump.ToArray());
        }
    }
}
