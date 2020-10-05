using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using AO2Sharp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AO2Sharp
{
    [Serializable]
    internal class Configuration
    {
        public string ServerName = "Test Server";
        public string ServerDescription = "Example server description.";
        public string ModPassword = "ChangeThis";

        public IPAddress BoundIpAddress = IPAddress.Parse("0.0.0.0");
        public int Port = 27016;
        public int WebsocketPort = 27017;
        public string MasterServerAddress = "master.aceattorneyonline.com";
        public int MasterServerPort = 27016;

        public bool Advertise = true;
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
            jsonSettings.Converters.Add(new IPConverter());
            jsonSettings.Formatting = Formatting.Indented;
            File.WriteAllText(path, JsonConvert.SerializeObject(this, jsonSettings));
        }

        public static Configuration LoadFromFile(string path)
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new IPConverter());
            var conf = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(path), jsonSettings);
            File.WriteAllText(path, JsonConvert.SerializeObject(conf, jsonSettings));
            return conf;
        }
    }
}
