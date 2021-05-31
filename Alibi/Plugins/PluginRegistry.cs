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
        public bool IsPluginRegistered(Type type) => RegisteredPlugins.Exists(x => x.GetType() == type);

        public bool IsPluginRegistered<T>() where T : Plugin => RegisteredPlugins.Exists(x => x is T);

        public Plugin GetPluginInstance(string id)
        {
            if (!IsPluginRegistered(id))
                throw new InvalidOperationException($"[PluginLoader] Plugin '{id}' was never registered.");

            return RegisteredPlugins.Single(x => x.ID == id);
        }

        public Plugin GetPluginInstance(Type type)
        {
            if (!IsPluginRegistered(type))
                throw new InvalidOperationException($"[PluginLoader] Plugin '{nameof(type)}' was never registered.");

            return RegisteredPlugins.Single(x => x.GetType() == type);
        }

        public T GetPluginInstance<T>() where T : Plugin
        {
            if (!IsPluginRegistered<T>())
                throw new InvalidOperationException($"[PluginLoader] Plugin '{nameof(T)}' was never registered.");

            return RegisteredPlugins.Single(x => x is T) as T;
        }

        public void RegisterPlugin(Plugin plugin)
        {
            if (IsPluginRegistered(plugin.ID))
                throw new InvalidOperationException($"[PluginLoader] Cannot register two of the same ID: {plugin.ID}");

            RegisteredPlugins.Add(plugin);
        }
    }
}