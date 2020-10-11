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
            Directory.CreateDirectory(PluginFolder);
            Directory.CreateDirectory(Path.Combine(PluginFolder, Server.PluginDepsFolder));
            Registry = new PluginRegistry(this);
        }

        public bool IsPluginLoaded(string id)
            => Registry.IsPluginRegistered(id);

        public dynamic RequestPluginInstance(string id)
            => Registry.GetPluginInstance(id);

        public string GetConfigFolder(string id)
            => Directory.CreateDirectory(Path.Combine(Server.ConfigFolder, id)).FullName;

        public List<Plugin> GetAllPlugins()
            => Registry.RegisteredPlugins;

        internal void LoadPlugins(IServer server)
        {
            var paths = Directory.GetFiles(PluginFolder, "*.dll");

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var path in paths)
            {
                Server.Logger.Log(LogSeverity.Special, $"[PluginLoader] Loading plugin: {Path.GetFileNameWithoutExtension(path)}");
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

                try
                {
                    instance!.Server = server;
                    instance!.Initialize(this);
                }
                catch (Exception e)
                {
                    Server.Logger.Log(LogSeverity.Error, 
                        $"[PluginLoader] Could not run initialization on {instance!.ID}: {e.Message}" + (server.VerboseLogs ? $"\n{e.StackTrace}" : ""));
                    continue;
                }
            }

            ((Server)server).OnAllPluginsLoaded();
            Server.Logger.Log(LogSeverity.Special, "[PluginLoader] Plugins loaded.");
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string filename = args.Name.Split(',')[0] + ".dll".ToLower();

            string asmFile = Path.Combine(PluginFolder, Server.PluginDepsFolder, filename);
    
            try
            {
                return Assembly.LoadFrom(asmFile);
            }
            catch (Exception e)
            {
                Server.Logger.Log(LogSeverity.Error, $"[PluginLoader] Error loading dependency, {e.Message}");
                return null;
            }
        }
    }
}
