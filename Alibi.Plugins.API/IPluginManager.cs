namespace Alibi.Plugins.API
{
    /// <summary>
    /// Provides an interface to the internal plugin manager.
    /// Use this to interface with other plugins, or to get
    /// paths to configuration information, and log errors.
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// Check if a given plugin is loaded.
        /// </summary>
        /// <param name="id">The ID of the plugin to check (not necessarily your own)</param>
        /// <returns>Whether or not this plugin has been loaded yet or hasn't crashed.</returns>
        public bool IsPluginLoaded(string id);
        /// <summary>
        /// Fetches an instance of the plugin.
        /// </summary>
        /// <param name="id">The ID of the plugin to fetch</param>
        /// <returns>The plugin's instance that derives from Plugin</returns>
        /// <remarks>
        /// This is dynamic so it can be literally any class.
        /// Be careful what you cast, or you may burn.
        /// </remarks>
        public dynamic RequestPluginInstance(string id);
        /// <summary>
        /// Gets the absolute path to the configuration folder assigned to this plugin.
        /// ONLY use this folder as otherwise users will be confused and it creates confusing code.
        /// </summary>
        /// <param name="id">The ID of the plugin to fetch the config folder of (not necessarily your own)</param>
        /// <returns>An absolute path to that plugin's config folder</returns>
        public string GetConfigFolder(string id);
        /// <summary>
        /// Logs a message to the server console and log buffer, associated with the Plugin Manager.
        /// </summary>
        /// <param name="severity">How severe the log is (determines color and prefix)</param>
        /// <param name="message">The message to be logged</param>
        /// <param name="verbose">Should this log only be displayed if the server is Verbose</param>
        /// <example>
        /// [Date][Info][PluginManager] I am a Log!
        /// </example>
        public void Log(LogSeverity severity, string message, bool verbose);
    }
}