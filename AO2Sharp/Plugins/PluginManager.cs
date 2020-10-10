using AO2Sharp.Plugins.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AO2Sharp.Plugins
{
    public class PluginManager : IPluginManager
    {
        private string PluginFolder { get; }
        private PluginRegistry Registry { get; }

        public PluginManager(string path)
        {
            PluginFolder = path;
            Registry = new PluginRegistry(this);
        }

        public bool IsPluginLoaded(string id)
            => Registry.IsPluginRegistered(id);

        public dynamic RequestPluginInstance(string id)
            => Registry.GetPluginInstance(id);

        public List<Plugin> GetAllPlugins()
            => Registry.RegisteredPlugins;

        internal void LoadPlugins(IServer server)
        {
            var paths = Directory.GetFiles(PluginFolder, "*.dll");

            foreach (var path in paths)
            {
                Server.Logger.Log(LogSeverity.Special, $"[PluginLoader] Loading plugin: {Path.GetDirectoryName(path)}");
                Assembly asm;
                try
                {
                    asm = Assembly.LoadFrom(path);
                }
                catch
                {
                    Server.Logger.Log(LogSeverity.Error, $"[PluginLoader] Couldn't load {path}. Make sure it is a .NET assembly.");
                    continue;
                }

                Type pluginType;
                try
                {
                    pluginType = asm.GetTypes().Single(x => typeof(Plugin).IsAssignableFrom(x));
                }
                catch
                {
                    Server.Logger.Log(LogSeverity.Error, $"[PluginLoader] Could not find a plugin type in {asm.GetName().Name}, did you implement the Plugin base?");
                    continue;
                }

                Plugin instance;
                try
                {
                    instance = (Plugin)Activator.CreateInstance(pluginType);
                }
                catch (Exception e)
                {
                    Server.Logger.Log(LogSeverity.Error, $"[PluginLoader] Could not create instance of {pluginType.Name}: {e.Message}");
                    continue;
                }

                try
                {
                    Registry.RegisterPlugin(instance);
                }
                catch (Exception e)
                {
                    Server.Logger.Log(LogSeverity.Error, $"[PluginLoader] Unable to register {instance!.ID}: {e.Message}");
                    continue;
                }

                instance!.Server = server;
                instance!.Initialize(this);
            }

            Server.Logger.Log(LogSeverity.Special, "[PluginLoader] Plugins loaded.");
        }
    }
}
