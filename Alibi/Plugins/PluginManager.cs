using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Alibi.Plugins.API;

namespace Alibi.Plugins
{
    public class PluginManager : IPluginManager
    {
        public IReadOnlyList<Plugin> LoadedPlugins { get; }

        private readonly string _pluginFolder;
        private readonly PluginRegistry _registry;

        public PluginManager(string path)
        {
            _pluginFolder = path;
            Directory.CreateDirectory(_pluginFolder);
            Directory.CreateDirectory(Path.Combine(_pluginFolder, Server.PluginDepsFolder));
            _registry = new PluginRegistry(this);
            LoadedPlugins = _registry.RegisteredPlugins;
        }

        public bool IsPluginLoaded(string id) => _registry.IsPluginRegistered(id);

        public dynamic RequestPluginInstance(string id) => _registry.GetPluginInstance(id);

        public string GetConfigFolder(string id) =>
            Directory.CreateDirectory(Path.Combine(Server.ConfigFolder, id)).FullName;

        public void Log(LogSeverity severity, string message, bool verbose) =>
            Server.Logger.Log(severity, message, verbose);

        internal void LoadPlugins(IServer server)
        {
            var paths = Directory.GetFiles(_pluginFolder, "*.dll");

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var path in paths)
            {
                Server.Logger.Log(LogSeverity.Special,
                    $"[PluginLoader] Loading plugin: {Path.GetFileNameWithoutExtension(path)}");
                Assembly asm;
                try
                {
                    asm = Assembly.LoadFrom(path);
                }
                catch
                {
                    Server.Logger.Log(LogSeverity.Error,
                        $"[PluginLoader] Couldn't load {path}. Make sure it is a .NET assembly.");
                    continue;
                }

                Type pluginType;
                try
                {
                    pluginType = asm.GetTypes().Single(x => typeof(Plugin).IsAssignableFrom(x));
                }
                catch
                {
                    Server.Logger.Log(LogSeverity.Error,
                        $"[PluginLoader] Could not find a plugin type in {asm.GetName().Name}, " +
                        $"did you implement the Plugin base?");
                    continue;
                }

                Plugin instance;
                try
                {
                    instance = (Plugin) Activator.CreateInstance(pluginType);
                }
                catch (Exception e)
                {
                    Server.Logger.Log(LogSeverity.Error,
                        $"[PluginLoader] Could not create instance of {pluginType.Name}: {e}");
                    continue;
                }

                try
                {
                    _registry.RegisterPlugin(instance);
                }
                catch (Exception e)
                {
                    Server.Logger.Log(LogSeverity.Error, $"[PluginLoader] Unable to register {instance!.ID}: {e}");
                    continue;
                }

                try
                {
                    instance!.Server = server;
                    instance!.PluginManager = this;
                    instance!.Assembly = asm;
                    instance!.Initialize();
                }
                catch (Exception e)
                {
                    Server.Logger.Log(LogSeverity.Error,
                        $"[PluginLoader] Could not run initialization on {instance!.ID}: {e}" +
                        (server.VerboseLogs ? $"\n{e.StackTrace}" : ""));
                }
            }

            ((Server) server).OnAllPluginsLoaded();
            Server.Logger.Log(LogSeverity.Special, "[PluginLoader] Plugins loaded.");
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            var filename = args.Name.Split(',')[0] + ".dll".ToLower();

            var asmFile = Path.Combine(_pluginFolder, Server.PluginDepsFolder, filename);

            try
            {
                return Assembly.LoadFrom(asmFile);
            }
            catch (Exception e)
            {
                Server.Logger.Log(LogSeverity.Error, $"[PluginLoader] Error loading dependency, {e}");
                return null;
            }
        }
    }
}