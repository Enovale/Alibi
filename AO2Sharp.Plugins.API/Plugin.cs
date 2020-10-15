using System;
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

        public void Log(LogSeverity severity, string message, bool verbose = false)
        {
            if (verbose && !Server.VerboseLogs)
                return;

            PluginManager.Log(severity, $"[{Name}] {message}", verbose);
        }
    }
}
