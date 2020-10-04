using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace AO2Sharp
{
    [Serializable]
    internal class Configuration
    {
        public IPAddress BoundIpAddress { get; private set; } = IPAddress.Any;
        public int Port { get; private set; } = 27016;
        public int WebsocketPort { get; private set; } = 27017;

        public bool Advertise { get; private set; } = true;

        public void SaveToFile(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }

        public static Configuration LoadFromFile(string path)
        {
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(path));
        }
    }
}
