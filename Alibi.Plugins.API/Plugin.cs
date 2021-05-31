#nullable enable
using System;

namespace Alibi.Plugins.API
{
    /// <summary>
    /// Represents a generic plugin for Alibi. Requires ID and Name to be overwritten.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// The ID of this plugin. Can be anything, but it is
        /// recommended to use the com.author.project format.
        /// </summary>
        /// <remarks>
        /// This is used to determine the config folder name,
        /// and for other plugins to find this plugin.
        /// </remarks>
        public abstract string ID { get; }

        /// <summary>
        /// The arbitrary name of this plugin.
        /// Doesn't matter if duplicated, and is only used for logs.
        /// </summary>
        public abstract string Name { get; }

        private readonly IServer _server;
        private readonly IPluginManager _pluginManager;

        /// <summary>
        /// Constructs a new Plugin, providing some internal objects to interface with.
        /// </summary>
        /// <param name="server">The internal server interface</param>
        /// <param name="pluginManager">
        /// The internal plugin manager interface, for
        /// configuration and interacting with other plugins
        /// </param>
        /// <remarks>
        /// Must be implemented in order to create a new plugin derived from this class.
        /// </remarks>
        protected Plugin(IServer server, IPluginManager pluginManager)
        {
            _server = server;
            _pluginManager = pluginManager;
        }

        /// <summary>
        /// Called once all plugins for the server have loaded.
        /// Put things like plugin dependency here.
        /// </summary>
        public virtual void OnAllPluginsLoaded()
        {
        }

        /// <summary>
        /// Called once the server is entirely initialized.
        /// All plugins will be loaded by this point.
        /// </summary>
        public virtual void OnServerInitialized()
        {
        }

        /// <summary>
        /// Called when a player joins the server, NOT when they are identified
        /// </summary>
        /// <param name="client">The client who just joined</param>
        /// <remarks>
        /// This player is not ready to be sent messages and should only
        /// be used to initialize starting data that is needed asap.
        /// </remarks>
        public virtual void OnPlayerJoined(IClient client)
        {
        }

        /// <summary>
        /// Called when a player fully identifies and joins the first area.
        /// </summary>
        /// <param name="client">The client who just joined</param>
        /// <remarks>
        /// You can assume this player is ready to play and be sent messages.
        /// </remarks>
        public virtual void OnPlayerConnected(IClient client)
        {
        }

        /// <summary>
        /// Called when any player sends an IC message in any area.
        /// </summary>
        /// <param name="client">The client who sent the message</param>
        /// <param name="message">The message content</param>
        /// <returns>Whether or not this message should continue being sent</returns>
        public virtual bool OnIcMessage(IClient client, ref string message)
        {
            return true;
        }

        /// <summary>
        /// Called when any player sends an OoC message in any area.
        /// </summary>
        /// <param name="client">The client who sent the message</param>
        /// <param name="message">The message content</param>
        /// <returns>Whether or not this message should continue being sent</returns>
        public virtual bool OnOocMessage(IClient client, ref string message)
        {
            return true;
        }

        /// <summary>
        /// Called when any player attempts to change the music in any area.
        /// Song names are not consistent across all servers and clients, try
        /// not to hardcode anything.
        /// </summary>
        /// <param name="client">The client that sent the music request</param>
        /// <param name="song">The song that was requested</param>
        /// <returns>Whether or not this music request should continue being processed</returns>
        public virtual bool OnMusicChange(IClient client, ref string song)
        {
            return true;
        }

        /// <summary>
        /// Called when any player calls for a moderator in any area.
        /// </summary>
        /// <param name="caller">The client that called</param>
        /// <param name="reason">The reasoning string that was provided (can be empty)</param>
        /// <returns>Whether or not this mod call should continue being processed</returns>
        public virtual bool OnModCall(IClient caller, string reason)
        {
            return true;
        }

        /// <summary>
        /// Called whenever a moderator bans a player.
        /// </summary>
        /// <param name="client">The client that was banned</param>
        /// <param name="banner">The client that banned them (Can be null, for server operations)</param>
        /// <param name="reason">Why they were banned (reference variable, can be modified)</param>
        /// <param name="expires">When this ban expires (Can be null)</param>
        /// <returns>Whether or not this ban should continue being processed.</returns>
        public virtual bool OnBan(IClient client, IClient? banner, ref string reason, TimeSpan? expires = null)
        {
            return true;
        }

        /// <summary>
        /// Use this to log anything your plugin needs to output.
        /// </summary>
        /// <param name="severity">Log severity (Appears in different colors with a severity prefix)</param>
        /// <param name="message">The message to be logged</param>
        /// <param name="verbose">Should this log only be output if the server is in verbose mode?</param>
        public void Log(LogSeverity severity, string message, bool verbose = false)
        {
            if (verbose && !_server.VerboseLogs)
                return;

            _pluginManager.Log(severity, $"[{Name}] {message}", verbose);
        }
    }
}