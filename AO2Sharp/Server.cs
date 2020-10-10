using AO2Sharp.Database;
using AO2Sharp.Helpers;
using AO2Sharp.WebSocket;
using NetCoreServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AO2Sharp
{
    public class Server : TcpServer
    {
        public static string ProcessPath = Process.GetCurrentProcess().MainModule!.FileName;
        public static string ConfigFolder = "Config";
        public static string ConfigPath = Path.Combine(ConfigFolder, "config.json");
        public static string AreasPath = Path.Combine(ConfigFolder, "areas.json");
        public static string MusicPath = Path.Combine(ConfigFolder, "music.txt");
        public static string CharactersPath = Path.Combine(ConfigFolder, "characters.txt");

        public static Logger Logger;
        public static Configuration ServerConfiguration;
        public static DatabaseManager Database;
        public static string[] MusicList;
        public static string[] CharactersList;
        public static string Version;

        public readonly List<Client> ClientsConnected;
        public int ConnectedPlayers = 0;
        public readonly Area[] Areas;
        public readonly string[] AreaNames;
        public List<Evidence> EvidenceList = new List<Evidence>();

        private readonly Advertiser _advertiser;
        private readonly WebSocketProxy _wsProxy;

        public Server(Configuration config) : base(config.BoundIpAddress, config.Port)
        {
            ServerConfiguration = config;
            Logger = new Logger(this);
            Logger.Log(LogSeverity.Special, "Server starting up...");
            Database = new DatabaseManager();
            if (Database.CheckCredentials("admin", "ChangeThis"))
                Logger.Log(LogSeverity.Warning, " Default moderator login is 'admin', password is 'ChangeThis'. Please change this immediately.");
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version = fileVersionInfo.ProductVersion;

            ClientsConnected = new List<Client>(ServerConfiguration.MaxPlayers);
            if (ServerConfiguration.Advertise)
                _advertiser = new Advertiser(ServerConfiguration.MasterServerAddress, ServerConfiguration.MasterServerPort);
            ReloadConfig();

            Areas = JsonConvert.DeserializeObject<Area[]>(File.ReadAllText(AreasPath));
            if (Areas == null || Areas.Length == 0)
            {
                Logger.Log(LogSeverity.Warning, "At least one area is required to start the server, writing default area...");
                File.WriteAllText(AreasPath, JsonConvert.SerializeObject(new Area[] { Area.Default }, Formatting.Indented));
                Areas = JsonConvert.DeserializeObject<Area[]>(File.ReadAllText(AreasPath));
            }
            AreaNames = new string[Areas.Length];
            foreach (var area in Areas)
            {
                AreaNames[Array.IndexOf(Areas, area)] = area.Name;
                area.Server = this;
                area.TakenCharacters = new bool[CharactersList.Length];
            }

            if (ServerConfiguration.WebsocketPort > -1)
            {
                _wsProxy = new WebSocketProxy(IPAddress.Any, ServerConfiguration.WebsocketPort);
                _wsProxy.Start();
            }

            Logger.Log(LogSeverity.Special, "Server started!");
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
                Logger.Log(LogSeverity.Warning, " Checking for corpses and discarding...", true);
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
            AOPacket msgPacket = new AOPacket("CT", new[] { "Server", message, "1" });
            Broadcast(msgPacket);
            Logger.OocMessageLog(message, null, msgPacket.Objects[0]);
        }

        public bool AddUser(Client client)
        {
            return Database.AddUser(client.HardwareId, client.IpAddress.ToString());
        }

        public bool CheckLogin(string username, string password)
        {
            return Database.CheckCredentials(username, password);
        }

        public bool AddLogin(string username, string password)
        {
            return Database.AddLogin(username, password);
        }

        public bool RemoveLogin(string username)
        {
            return Database.RemoveLogin(username);
        }

        protected override TcpSession CreateSession()
        {
            return new ClientSession(this);
        }

        protected override void OnStopped()
        {
            Logger.Log(LogSeverity.Warning, "Stopping server...");
            _advertiser.Stop();
            _wsProxy.Stop();
        }
    }
}
