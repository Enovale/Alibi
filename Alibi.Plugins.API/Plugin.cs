using System;
using System.Reflection;

namespace Alibi.Plugins.API
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
        public virtual void OnPlayerJoined(IClient client) { }
        public virtual bool OnIcMessage(IClient client, ref string message) { return true; }
        public virtual bool OnOocMessage(IClient client, ref string message) { return true; }
        public virtual bool OnMusicChange(IClient client, ref string song) { return true; }
        public virtual bool OnModCall(IClient caller, string reason) { return true; }
        public virtual bool OnBan(IClient banned, ref string reason, TimeSpan? expires = null) { return true; }

        public void Log(LogSeverity severity, string message, bool verbose = false)
        {
            if (verbose && !Server.VerboseLogs)
                return;

            PluginManager.Log(severity, $"[{Name}] {message}", verbose);
        }
    }
}
