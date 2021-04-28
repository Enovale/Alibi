using System;
using System.IO;
using System.Net;
using Alibi.Helpers;
using Alibi.Plugins.API;
using Newtonsoft.Json;

namespace Alibi
{
    [Serializable]
    public class Configuration : IConfiguration
    {
#pragma warning disable CA2235 // Mark all non-serializable fields
        public string ServerName { get; internal set; } = "Test ServerRef";
        public string ServerDescription { get; internal set; } = "Example server description.";
        public string Motd { get; internal set; } =
            "Welcome to my test server! Type /help for a list of commands you can run.";
        public IPAddress BoundIpAddress { get; internal set; } = IPAddress.Parse("0.0.0.0");
        public int Port { get; internal set; } = 27016;
        public int WebsocketPort { get; internal set; } = 27017;
        public string MasterServerAddress { get; internal set; } = "master.aceattorneyonline.com";
        public int MasterServerPort { get; internal set; } = 27016;
        public int LogBufferSize { get; internal set; } = 500;
        public bool VerboseLogs { get; internal set; } = false;

        public bool Advertise { get; internal set; } = true;
        public bool AllowDoublePostsIfDifferentAnim { get; internal set; } = false;
        public int MaxPlayers { get; internal set; } = 100;
        public int MaxMultiClients { get; internal set; } = 4;
        public int MaxClientsOnOneNetwork { get; internal set; } = 8;
        public int TimeoutSeconds { get; internal set; } = 60;
        public int MaxMessageSize { get; internal set; } = 256;
        public int MaxShownameSize { get; internal set; } = 16;
        public int RateLimit { get; internal set; } = 50;
        public int RateLimitResetTime { get; internal set; } = 1;
        public TimeSpan RateLimitBanLength { get; internal set; } = new TimeSpan(0, 5, 0);

        public string[] FeatureList { get; internal set; } =
        {
            "yellowtext", "prezoom", "flipping", "customobjections",
            "deskmod", "evidence", "cccc_ic_support",
            "arup", "casing_alerts", "modcall_reason",
            "looping_sfx", "additive", "effects", "expanded_desk_mods"
        };

        public void SaveToFile(string path)
        {
            if (!Directory.Exists(Server.ConfigFolder))
                Directory.CreateDirectory(Server.ConfigFolder);
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new IpConverter());
            jsonSettings.Formatting = Formatting.Indented;
            File.WriteAllText(path, JsonConvert.SerializeObject(this, jsonSettings));
        }

        public static Configuration LoadFromFile(string path)
        {
            var jsonSettings = new JsonSerializerSettings {ContractResolver = new JsonResolver()};
            jsonSettings.Converters.Add(new IpConverter());
            var conf = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(path), jsonSettings);
            File.WriteAllText(path, JsonConvert.SerializeObject(conf, Formatting.Indented, jsonSettings));
            return conf;
        }
    }
}