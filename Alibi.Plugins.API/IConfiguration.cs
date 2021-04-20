using System;
using System.Net;

namespace Alibi.Plugins.API
{
    public interface IConfiguration
    {
        string ServerName { get; }
        string ServerDescription { get; }
        string Motd { get; }
        IPAddress BoundIpAddress { get; }
        int Port { get; }
        int WebsocketPort { get; }
        string MasterServerAddress { get; }
        int MasterServerPort { get; }
        int LogBufferSize { get; }
        bool VerboseLogs { get; }
        bool Advertise { get; }
        bool AllowDoublePostsIfDifferentAnim { get; }
        int MaxPlayers { get; }
        int MaxMultiClients { get; }
        int TimeoutSeconds { get; }
        int MaxMessageSize { get; }
        int MaxShownameSize { get; }
        int RateLimit { get; }
        int RateLimitResetTime { get; }
        TimeSpan RateLimitBanLength { get; }
        string[] FeatureList { get; }
    }
}