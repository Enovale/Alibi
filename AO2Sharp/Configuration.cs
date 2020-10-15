using AO2Sharp.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace AO2Sharp
{
    [Serializable]
    public class Configuration
    {
#pragma warning disable CA2235 // Mark all non-serializable fields
        public string ServerName = "Test Server";
        public string ServerDescription = "Example server description.";
        public string MOTD = "Welcome to my test server! Type /help for a list of commands you can run.";

        public IPAddress BoundIpAddress = IPAddress.Parse("0.0.0.0");
        public int Port = 27016;
        public int WebsocketPort = 27017;
        public string MasterServerAddress = "master.aceattorneyonline.com";
        public int MasterServerPort = 27016;
        public int LogBufferSize = 500;
        public bool VerboseLogs = false;

        public bool Advertise = true;
        public bool AllowDoublePostsIfDifferentAnim = false;
        public int MaxPlayers = 100;
        public int TimeoutSeconds = 60;

        public string[] FeatureList =
        {
            "yellowtext", "prezoom", "flipping", "customobjections",
            "deskmod", "evidence", "cccc_ic_support",
            "arup", "casing_alerts", "modcall_reason",
            "looping_sfx", "additive", "effects"
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
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new IpConverter());
            var conf = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(path), jsonSettings);
            File.WriteAllText(path, JsonConvert.SerializeObject(conf, Formatting.Indented, jsonSettings));
            return conf;
        }
    }
}
