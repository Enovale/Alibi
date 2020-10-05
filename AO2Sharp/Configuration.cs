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
        public string ServerName { get; private set; } = "Test Server";
        public string ServerDescription { get; private set; } = "Example server description.";

        public IPAddress BoundIpAddress { get; private set; } = IPAddress.Parse("0.0.0.0");
        public int Port { get; private set; } = 27016;
        public int WebsocketPort { get; private set; } = 27017;
        public string MasterServerAddress { get; private set; } = "master.aceattorneyonline.com";
        public int MasterServerPort { get; private set; } = 27016;

        public bool Advertise { get; private set; } = true;
        public int MaxPlayers { get; private set; } = 100;
        public int TimeoutSeconds { get; private set; } = 60;

        public string[] FeatureList { get; private set; } =
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
            var json = JObject.Parse(File.ReadAllText(path));
            json.Merge(new Configuration(), new JsonMergeSettings()
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });
            return JsonConvert.DeserializeObject<Configuration>(json.ToString(), jsonSettings);
        }
    }
}
