using System;
using System.Net;

namespace Alibi.Plugins.API
{
    /// <summary>
    /// Represents the server configuration on disk.
    /// </summary>
    public interface IConfiguration
    {
        string ServerName { get; }
        string ServerDescription { get; }

        /// <summary>
        /// Message of the Day that is sent to clients in the Out of Context chat when they join.
        /// </summary>
        string Motd { get; }

        /// <summary>
        /// Which IP Address to bind this server to internally.
        /// </summary>
        IPAddress BoundIpAddress { get; }

        /// <summary>
        /// The port that all native clients will connect to. (Cannot be the same as the Websocket Port)
        /// </summary>
        int Port { get; }

        /// <summary>
        /// The port that all websocket clients (WebAO) clients will connect to
        /// (Cannot be the same as the Native Port)
        /// </summary>
        int WebsocketPort { get; }

        /// <summary>
        /// The address of the AO2 masterserver that this server advertises to.
        /// </summary>
        string MasterServerAddress { get; }

        /// <summary>
        /// The AO2 masterserver port to advertise on
        /// </summary>
        int MasterServerPort { get; }

        /// <summary>
        /// How many logs to store for when the server crashes and dumps them.
        /// </summary>
        /// <remarks>This is 500 by default.</remarks>
        int LogBufferSize { get; }

        /// <summary>
        /// Should the server process logs that are marked verbose?
        /// </summary>
        bool VerboseLogs { get; }

        /// <summary>
        /// Should this server tell the AO2 masterserver of its existence?
        /// </summary>
        /// <remarks>
        /// Not doing this will mean you have to add a manual favorite
        /// for the local server, and name/description information will
        /// not be visible. Unless debugging, this should always be true.
        /// </remarks>
        bool Advertise { get; }

        /// <summary>
        /// Whether or not players should be able to post the same message twice in
        /// In-Character chat even if they change the emote used
        /// </summary>
        bool AllowDoublePostsIfDifferentAnim { get; }

        /// <summary>
        /// The maximum amount of clients allowed to join the server.
        /// </summary>
        int MaxPlayers { get; }

        /// <summary>
        /// How many clients one machine is allowed to connect to this server.
        /// </summary>
        int MaxMultiClients { get; }

        /// <summary>
        /// How long until an idle (not sending keepalives) client is kicked from the server.
        /// </summary>
        int TimeoutSeconds { get; }

        /// <summary>
        /// How long an Ic or OoC message can be. Longer messages will return an error to the player.
        /// </summary>
        /// <remarks>
        /// Diacritics, emojis, and other weird unicode shit may count as multiple
        /// characters, so don't expect that 1 character always equals 1 size.
        /// </remarks>
        int MaxMessageSize { get; }

        /// <summary>
        /// How long a showname can be.
        /// </summary>
        /// <remarks>
        /// This should be pretty short because there is not much space allocated for this on the client.
        /// </remarks>
        int MaxShownameSize { get; }

        /// <summary>
        /// How many packets the client can send within the RateLimitResetTime before they are rate limited.
        /// </summary>
        int RateLimit { get; }

        /// <summary>
        /// How long (in seconds) until the rate limit timer is reset.
        /// </summary>
        int RateLimitResetTime { get; }

        /// <summary>
        /// How long until a rate limit is lifted.
        /// </summary>
        TimeSpan RateLimitBanLength { get; }

        /// <summary>
        /// What AO2 protocol features this server supports
        /// </summary>
        /// <remarks>
        /// Non-default features should not be added to this list. The option is only here
        /// to allow users to disable certain features they don't want/need.
        /// </remarks>
        string[] FeatureList { get; }
    }
}