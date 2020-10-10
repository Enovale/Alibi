using AO2Sharp.Plugins.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AO2Sharp.Plugins
{
    public class PluginRegistry
    {
        public List<Plugin> RegisteredPlugins { get; }

        public PluginManager Owner { get; }

        public PluginRegistry(PluginManager owner)
        {
            Owner = owner;
            RegisteredPlugins = new List<Plugin>();
        }

        public bool IsPluginRegistered(string id)
            => RegisteredPlugins.Exists(x => x.ID == id);

        public Plugin GetPluginInstance(string id)
        {
            if (!IsPluginRegistered(id))
                throw new InvalidOperationException($"[PluginLoader] Plugin '{id}' was never registered.");

            return RegisteredPlugins.Single(x => x.ID == id);
        }

        public void RegisterPlugin(Plugin plugin)
        {
            if (IsPluginRegistered(plugin.ID))
                throw new InvalidOperationException($"[PluginLoader] Cannot register two of the same ID: {plugin.ID}");

            RegisteredPlugins.Add(plugin);
        }
    }

}
