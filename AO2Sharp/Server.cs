using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AO2Sharp.Helpers;
using NetCoreServer;
using Newtonsoft.Json;

namespace AO2Sharp
{
    internal class Server : TcpServer
    {
        public static string ConfigFolder = "Config";
        public static string ConfigPath = Path.Combine(ConfigFolder, "config.json");
        public static string AreasPath = Path.Combine(ConfigFolder, "areas.json");
        public static string MusicPath = Path.Combine(ConfigFolder, "music.txt");
        public static string CharactersPath = Path.Combine(ConfigFolder, "characters.txt");

        public static Configuration ServerConfiguration;
        public static string[] MusicList;
        public static string[] CharactersList;
        public static string Version;

        public readonly List<Client> ClientsConnected;
        public int ConnectedPlayers = 0;
        public readonly Area[] Areas;
        public readonly string[] AreaNames;
        public List<Evidence> EvidenceList = new List<Evidence>();

        private Advertiser _advertiser;

        public Server(Configuration config) : base(config.BoundIpAddress, config.Port)
        {
            ServerConfiguration = config;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fileVersionInfo.ProductVersion;

            ClientsConnected = new List<Client>(ServerConfiguration.MaxPlayers);
            if(ServerConfiguration.Advertise)
                _advertiser = new Advertiser(ServerConfiguration.MasterServerAddress, ServerConfiguration.MasterServerPort);
            ReloadConfig();
            
            Areas = JsonConvert.DeserializeObject<Area[]>(File.ReadAllText(AreasPath));
            if (Areas == null || Areas.Length == 0)
            {
                Console.WriteLine("At least one area is required to start the server, writing default area...");
                File.WriteAllText(AreasPath, JsonConvert.SerializeObject(new Area[] {Area.Default}, Formatting.Indented));
                Areas = JsonConvert.DeserializeObject<Area[]>(File.ReadAllText(AreasPath));
            }
            AreaNames = new string[Areas.Length];
            foreach (var area in Areas)
            {
                AreaNames[Array.IndexOf(Areas, area)] = area.Name;
                area.Server = this;
                area.TakenCharacters = new bool[CharactersList.Length];
            }

            CheckCorpses();
        }

        public void ReloadConfig()
        {
            EnsureConfigFiles();
            MusicList = File.ReadAllLines(MusicPath);
            if (MusicList[0].Contains("."))
            {
                var tmp = new List<string>(MusicList);
                tmp.Insert(0, "==Music==");
                MusicList = tmp.ToArray();
            }
            CharactersList = File.ReadAllLines(CharactersPath);
        }

        private void EnsureConfigFiles()
        {
            if (!File.Exists(MusicPath) || string.IsNullOrWhiteSpace(File.ReadAllText(MusicPath)))
                File.WriteAllText(MusicPath, "==Music==\nAnnounce The Truth (AA).opus");
            if (!File.Exists(CharactersPath) || string.IsNullOrWhiteSpace(File.ReadAllText(CharactersPath)))
                File.WriteAllText(CharactersPath, "Apollo");
            if (!File.Exists(AreasPath))
                File.Create(AreasPath).Close();
        }

        private async void CheckCorpses()
        {
            while (true)
            {
                var delayTask = Task.Delay(ServerConfiguration.TimeoutSeconds * 1000);
                Console.WriteLine("!DEBUG: Checking for corpses and disconnecting them.");
                var clientQueue = new Queue<Client>(ClientsConnected);
                while (clientQueue.Any())
                {
                    var client = clientQueue.Dequeue();
                    if (client.LastAlive.AddSeconds(ServerConfiguration.TimeoutSeconds) < DateTime.Now)
                        // Forcibly kick.
                        client.Session.Disconnect();
                }
                await delayTask; // wait until at least 10s elapsed since delayTask created
            }
        }

        public void Broadcast(AOPacket message)
        {
            var clientQueue = new Queue<Client>(ClientsConnected);
            while (clientQueue.Any())
            {
                var client = clientQueue.Dequeue();
                if (client.Connected)
                    client.Send(message);
            }
        }

        public void BroadcastOocMessage(string message)
        {
            Broadcast(new AOPacket("CT", new []{"Server", message, "1"}));
        }

        protected override TcpSession CreateSession()
        {
            return new ClientSession(this);
        }
    }
}
