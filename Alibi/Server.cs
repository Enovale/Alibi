#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Alibi.Commands;
using Alibi.Database;
using Alibi.Helpers;
using Alibi.Plugins;
using Alibi.Plugins.API;
using Alibi.Protocol;
using Alibi.WebSocket;
using NetCoreServer;
using Newtonsoft.Json;
using AOPacket = Alibi.Helpers.AOPacket;

#pragma warning disable 8618

namespace Alibi
{
    public class Server : TcpServer, IServer
    {
        public static string PluginFolder = "Plugins";
        public static string PluginDepsFolder = "Dependencies";
        public static string ProcessPath = Process.GetCurrentProcess().MainModule!.FileName!;
        public static string ConfigFolder = "Config";
        public static string ConfigPath = Path.Combine(ConfigFolder, "config.json");
        public static string AreasPath = Path.Combine(ConfigFolder, "areas.json");
        public static string MusicPath = Path.Combine(ConfigFolder, "music.txt");
        public static string CharactersPath = Path.Combine(ConfigFolder, "characters.txt");

        public static IPAddress MasterServerIp { get; private set; }

        public static Logger Logger { get; private set; }
        public static Configuration ServerConfiguration { get; private set; }
        public static IDatabaseManager Database { get; private set; }
        public static string[] MusicList { get; private set; }
        public static string[] CharactersList { get; private set; }
        public static string Version { get; private set; }

        public List<IClient> ClientsConnected { get; }
        public int ConnectedPlayers { get; set; }
        public IArea[] Areas { get; }
        public string[] AreaNames { get; }
        public bool VerboseLogs => ServerConfiguration.VerboseLogs;

        private readonly Advertiser _advertiser;
        private readonly CancellationTokenSource _cancelTasksToken;
        private readonly PluginManager _pluginManager;
        private readonly WebSocketProxy _wsProxy;

        public Server(Configuration config) : base(config.BoundIpAddress, config.Port)
        {
            ServerConfiguration = config;
            Version asmVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            Version = $"{asmVersion.Major}.{asmVersion.Minor}.{asmVersion.Build}";
            Logger = new Logger(this);
            Logger.Log(LogSeverity.Special, $" Server starting up running version {Version}...");
            Database = new DatabaseManager();
            if (Database.CheckCredentials("admin", "ChangeThis"))
                Logger.Log(LogSeverity.Warning,
                    " Default admin login is 'admin', password is 'ChangeThis'. " +
                    "Please change this immediately by logging into this user and running /addadmin, " +
                    "then removing the dummy user with /removelogin.");

            ClientsConnected = new List<IClient>(ServerConfiguration.MaxPlayers);
            if (ServerConfiguration.Advertise)
            {
                MasterServerIp = Dns.GetHostAddresses(ServerConfiguration.MasterServerAddress).First();
                _advertiser = new Advertiser(MasterServerIp, ServerConfiguration.MasterServerPort);
            }

            ReloadConfig();

            var readAreas = JsonConvert.DeserializeObject<Area[]>(File.ReadAllText(AreasPath));
            if (readAreas == null || readAreas.Length == 0)
            {
                Logger.Log(LogSeverity.Warning,
                    "At least one area is required to start the server, writing default area...");
                readAreas = new[] {new Area()};
                File.WriteAllText(AreasPath, JsonConvert.SerializeObject(readAreas, Formatting.Indented));
            }
            Areas = readAreas;

            AreaNames = new string[Areas.Length];
            foreach (var area in Areas)
            {
                AreaNames[Array.IndexOf(Areas, area)] = area.Name;
                ((Area) area).Server = this;
                ((Area) area).TakenCharacters = new bool[CharactersList.Length];
            }

            if (ServerConfiguration.WebsocketPort > -1)
            {
                _wsProxy = new WebSocketProxy(IPAddress.Any, ServerConfiguration.WebsocketPort);
                _wsProxy.Start();
            }

            _pluginManager = new PluginManager(PluginFolder);
            _pluginManager.LoadPlugins(this);
            foreach (var plugin in _pluginManager.LoadedPlugins)
            {
                CommandHandler.AddCustomHandler(plugin);
                MessageHandler.AddCustomHandler(plugin);
            }

            Logger.Log(LogSeverity.Special, " Server started!");
            _cancelTasksToken = new CancellationTokenSource();
            CheckCorpses(_cancelTasksToken.Token);
            UnbanExpires(_cancelTasksToken.Token);
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

        public void Broadcast(IAOPacket message)
        {
            var clientQueue = new Queue<IClient>(ClientsConnected);
            while (clientQueue.Any())
            {
                var client = clientQueue.Dequeue();
                if (client.Connected)
                    client.Send(message);
            }
        }

        public void BroadcastOocMessage(string message)
        {
            AOPacket msgPacket = new AOPacket("CT", "ServerRef", message, "1");
            Broadcast(msgPacket);
            Logger.OocMessageLog(message, null, msgPacket.Objects[0]);
        }

        public IClient? FindUser(string str)
        {
            if (int.TryParse(str, out var id))
                return ClientsConnected.FirstOrDefault(c => c.Character == id) ?? null;
            //if (IPAddress.TryParse(str, out IPAddress ip))
            //    return ClientsConnected.FirstOrDefault(c => Equals(c.IpAddress, ip)) ?? null;
            var hwidSearch = ClientsConnected.FirstOrDefault(c => c.HardwareId == str) ?? null;
            if (hwidSearch != null)
                return hwidSearch;
            var oocSearch = ClientsConnected.FirstOrDefault(c => c.OocName == str) ?? null;
            if (oocSearch != null)
                return oocSearch;
            var charSearch = ClientsConnected.FirstOrDefault(c => c.CharacterName!.ToLower() == str.ToLower()) ??
                             null;
            if (charSearch != null)
                return charSearch;
            return null;
        }

        public void OnAllPluginsLoaded()
        {
            foreach (var p in _pluginManager.LoadedPlugins)
                try
                {
                    p.OnAllPluginsLoaded();
                }
                catch (Exception e)
                {
                    p.Log(LogSeverity.Error, $"Error occured during OnAllPluginsLoaded(), {e}");
                }
        }

        public void OnPlayerJoined(IClient client)
        {
            foreach (var p in _pluginManager.LoadedPlugins)
                try
                {
                    p.OnPlayerJoined(client);
                }
                catch (Exception e)
                {
                    p.Log(LogSeverity.Error, $"Error occured during OnPlayerJoined(), {e}");
                }
        }

        public bool OnIcMessage(IClient client, ref string message)
        {
            foreach (var p in _pluginManager.LoadedPlugins)
                try
                {
                    if (!p.OnIcMessage(client, ref message))
                        return false;
                }
                catch (Exception e)
                {
                    p.Log(LogSeverity.Error, $"Error occured during OnIcMessage(), {e}");
                }

            return true;
        }

        public bool OnOocMessage(IClient client, ref string message)
        {
            foreach (var p in _pluginManager.LoadedPlugins)
                try
                {
                    if (!p.OnOocMessage(client, ref message))
                        return false;
                }
                catch (Exception e)
                {
                    p.Log(LogSeverity.Error, $"Error occured during OnIcMessage(), {e}");
                }

            return true;
        }

        public bool OnMusicChange(IClient client, ref string song)
        {
            foreach (var p in _pluginManager.LoadedPlugins)
                try
                {
                    if (!p.OnMusicChange(client, ref song))
                        return false;
                }
                catch (Exception e)
                {
                    p.Log(LogSeverity.Error, $"Error occured during OnIcMessage(), {e}");
                }

            return true;
        }

        public bool OnModCall(IClient client, IAOPacket packet)
        {
            foreach (var p in _pluginManager.LoadedPlugins)
                try
                {
                    if (!p.OnModCall(client, packet.Objects[0]))
                        return false;
                }
                catch (Exception e)
                {
                    p.Log(LogSeverity.Error, $"Error occured during OnModCall(), {e}");
                }

            return true;
        }

        public bool OnBan(IClient client, ref string reason, TimeSpan? expires = null)
        {
            foreach (var p in _pluginManager.LoadedPlugins)
                try
                {
                    if (!p.OnBan(client, ref reason, expires))
                        return false;
                }
                catch (Exception e)
                {
                    p.Log(LogSeverity.Error, $"Error occured during OnBan(), {e}");
                }

            return true;
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

        private async void CheckCorpses(CancellationToken token)
        {
            var delayTask = Task.Delay(ServerConfiguration.TimeoutSeconds * 1000, token);
            Logger.Log(LogSeverity.Warning, " Checking for corpses and discarding...", true);
            var clientQueue = new Queue<Client>(ClientsConnected.Cast<Client>());
            while (clientQueue.Any())
            {
                var client = clientQueue.Dequeue();
                if (client.LastAlive.AddSeconds(ServerConfiguration.TimeoutSeconds) < DateTime.Now)
                {
                    Logger.Log(LogSeverity.Info, $"[{client.IpAddress}] Disconnected due to inactivity.", true);
                    // Forcibly kick.
                    client.Session.Disconnect();
                }
            }

            try
            {
                await delayTask;
            }
            catch (TaskCanceledException)
            {
                return;
            }

            CheckCorpses(token);
        }

        private async void UnbanExpires(CancellationToken token)
        {
            // Wait a minute each time because that's the minimum unit of measurement
            var delayTask = Task.Delay(60000, token);
            Logger.Log(LogSeverity.Warning, " Unbanning expired bans...", true);
            foreach (var bannedHwid in Database.GetBannedHwids())
                if (DateTime.Now.CompareTo(Database.GetBanExpiration(bannedHwid)) >= 0)
                    Database.UnbanHwid(bannedHwid);

            try
            {
                await delayTask;
            }
            catch (TaskCanceledException)
            {
                return;
            }

            UnbanExpires(token);
        }

        protected override TcpSession CreateSession()
        {
            return new ClientSession(this);
        }

        protected override void OnStopped()
        {
            Logger.Log(LogSeverity.Warning, "Stopping server...");
            _cancelTasksToken.Cancel();
            _advertiser.Stop();
            _wsProxy.Stop();
            Logger.Dump();
        }
    }
}