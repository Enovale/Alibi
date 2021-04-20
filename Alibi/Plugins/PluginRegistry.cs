using System;
using System.Collections.Generic;
using System.Linq;
using Alibi.Plugins.API;

namespace Alibi.Plugins
{
    public class PluginRegistry
    {
        public List<Plugin> RegisteredPlugins { get; }

        private readonly PluginManager _owner;
        
        public PluginRegistry(PluginManager owner)
        {
            _owner = owner;
            RegisteredPlugins = new List<Plugin>();
        }

        public bool IsPluginRegistered(string id) => RegisteredPlugins.Exists(x => x.ID == id);

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