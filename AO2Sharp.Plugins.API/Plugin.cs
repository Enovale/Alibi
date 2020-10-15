using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AO2Sharp.Plugins.API
{
    public abstract class Plugin
    {
        public abstract string ID { get; }
        public abstract string Name { get; }
        public IServer Server { get; set; }
        public IPluginManager PluginManager { get; set; }
        public Assembly Assembly { get; set; }
        public abstract void Initialize();

        public virtual void OnAllPluginsLoaded() { }
        public virtual void OnModCall(IClient caller, string reason) { }
        public virtual void OnBan(IClient banned, string reason, TimeSpan? expires = null) { }

        private Queue<string> _logBuffer = new Queue<string>(500);

        public void LogInfo(string message, bool verbose = false)
        {
            Log(0, $"[Info][{Name}] {message}", verbose);
        }

        public void LogSpecial(string message, bool verbose = false)
        {
            Log(1, $"[Special][{Name}] {message}", verbose);
        }

        public void LogWarning(string message, bool verbose = false)
        {
            Log(2, $"[Warning][{Name}] {message}", verbose);
        }

        public void LogError(string message, bool verbose = false)
        {
            Log(3, $"[Error][{Name}] {message}", verbose);
        }

        private void Log(int severity, string message, bool verbose = false)
        {
            if (verbose && !Server.VerboseLogs)
                return;
            if (severity < 0 || severity > 3)
                return;
            PluginManager.Log(severity, message, verbose);
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
