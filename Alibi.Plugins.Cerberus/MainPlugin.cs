#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;

#pragma warning disable 8618

namespace Alibi.Plugins.Cerberus
{
    public class MainPlugin : Plugin
    {
        public override string ID => "com.elijahzawesome.Cerberus";
        public override string Name => "Cerberus";

        public static CerberusConfiguration Config;

        private Dictionary<IClient, MuteInfo> _clientDict;

        private string _configPath;
        private Dictionary<IClient, string?> _lastOocMsgDict;
        private Dictionary<IArea, bool> _silencedAreas;

        public override void Initialize()
        {
            _configPath = Path.Combine(PluginManager.GetConfigFolder(ID), "config.json");
            if (!File.Exists(_configPath) || new FileInfo(_configPath).Length <= 0)
                File.WriteAllText(_configPath, JsonSerializer.Serialize(new CerberusConfiguration(),
                    new JsonSerializerOptions {WriteIndented = true}));

            Config = JsonSerializer.Deserialize<CerberusConfiguration>(File.ReadAllText(_configPath))!;

            _clientDict = new Dictionary<IClient, MuteInfo>();
            foreach (var client in Server.ClientsConnected)
                _clientDict.Add(client, new MuteInfo());
            _lastOocMsgDict = new Dictionary<IClient, string?>();
            
            _silencedAreas = new Dictionary<IArea, bool>(Server.Areas.Length);
            for (var i = 0; i < Server.Areas.Length; i++)
            {
                _silencedAreas.Add(Server.Areas[i], false);
            }

            MutedClientsCheck();
        }

        // ReSharper disable once FunctionRecursiveOnAllPaths
        private async void MutedClientsCheck()
        {
            var queue = new Queue<IClient>(_clientDict.Keys);
            while (queue.Count > 0)
            {
                var client = queue.Dequeue();
                if (_clientDict[client].IcMuted
                    && _clientDict[client].IcTimer.AddSeconds(Config.IcMuteLengthInSeconds).CompareTo(DateTime.Now) <
                    0)
                {
                    client.SendOocMessage("You have been un-muted in IC.");
                    _clientDict[client].IcMuted = false;
                }

                if (_clientDict[client].OocMuted
                    && _clientDict[client].OocTimer.AddSeconds(Config.OocMuteLengthInSeconds).CompareTo(DateTime.Now) <
                    0)
                {
                    client.SendOocMessage("You have been un-muted in OOC.");
                    _clientDict[client].OocMuted = false;
                }

                if (_clientDict[client].MusicMuted
                    && _clientDict[client].MusicTimer.AddSeconds(Config.MusicMuteLengthInSeconds)
                        .CompareTo(DateTime.Now) <
                    0)
                {
                    client.SendOocMessage("You have been un-muted from changing Music.");
                    _clientDict[client].MusicMuted = false;
                }
            }

            await Task.Delay(1000);
            MutedClientsCheck();
        }

        public override void OnPlayerJoined(IClient client)
        {
            _clientDict[client] = new MuteInfo();
            _lastOocMsgDict[client] = null;
        }

        private string StripZalgo(string message)
        {
            if (!Config.StripZalgo)
                return message;
            StringBuilder sb = new StringBuilder();
            foreach (var c in message.Normalize(NormalizationForm.FormC))
                if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);

            return sb.ToString();
        }

        public override bool OnIcMessage(IClient client, ref string message)
        {
            if (_silencedAreas[client.Area!] && client.Auth < AuthType.MODERATOR)
            {
                client.SendOocMessage("Thy room has been silenced. Hush, mortal, whilst the gods speaketh.");
                return false;
            }
            
            message = StripZalgo(message);
            if (Config.IcMuteLengthInSeconds < 0 || Config.MaxIcMessagesPerSecond < 0)
                return true;
            if (_clientDict[client].IcMuted)
                return false;
            if (DateTime.Now.CompareTo(_clientDict[client].IcTimer) >= 0)
            {
                _clientDict[client].IcTimer = DateTime.Now.AddSeconds(1);
                _clientDict[client].IcMessages = 0;
            }
            else
            {
                _clientDict[client].IcMessages++;
            }

            if (_clientDict[client].IcMessages > Config.MaxIcMessagesPerSecond)
            {
                client.SendOocMessage($"You have been IC muted for {Config.IcMuteLengthInSeconds} seconds.");
                _clientDict[client].IcTimer = DateTime.Now;
                _clientDict[client].IcMessages = 0;
                _clientDict[client].IcMuted = true;
                return false;
            }

            return true;
        }

        public override bool OnOocMessage(IClient client, ref string message)
        {
            if (_lastOocMsgDict[client] != null && _lastOocMsgDict[client] == message.Trim())
            {
                client.SendOocMessage("Cannot double-post in OOC.");
                return false;
            }

            message = StripZalgo(message);
            _lastOocMsgDict[client] = message.Trim();
            if (Config.OocMuteLengthInSeconds < 0 || Config.MaxOocMessagesPerSecond < 0)
                return true;
            if (_clientDict[client].OocMuted)
                return false;
            if (DateTime.Now.CompareTo(_clientDict[client].OocTimer) >= 0)
            {
                _clientDict[client].OocTimer = DateTime.Now.AddSeconds(1);
                _clientDict[client].OocMessages = 0;
            }
            else
            {
                _clientDict[client].OocMessages++;
            }

            if (_clientDict[client].OocMessages > Config.MaxOocMessagesPerSecond)
            {
                client.SendOocMessage($"You have been OOC muted for {Config.OocMuteLengthInSeconds} seconds.");
                _clientDict[client].OocTimer = DateTime.Now;
                _clientDict[client].OocMessages = 0;
                _clientDict[client].OocMuted = true;
                return false;
            }

            return true;
        }

        public override bool OnMusicChange(IClient client, ref string song)
        {
            if (Config.MusicMuteLengthInSeconds < 0 || Config.MaxMusicMessagesPerSecond < 0)
                return true;
            if (_clientDict[client].MusicMuted)
                return false;
            if (DateTime.Now.CompareTo(_clientDict[client].MusicTimer) >= 0)
            {
                _clientDict[client].MusicTimer = DateTime.Now.AddSeconds(1);
                _clientDict[client].MusicMessages = 0;
            }
            else
            {
                _clientDict[client].MusicMessages++;
            }

            if (_clientDict[client].MusicMessages > Config.MaxMusicMessagesPerSecond)
            {
                client.SendOocMessage($"You have been Music muted for {Config.MusicMuteLengthInSeconds} seconds.");
                _clientDict[client].MusicTimer = DateTime.Now;
                _clientDict[client].MusicMessages = 0;
                _clientDict[client].MusicMuted = true;
                return false;
            }

            return true;
        }

        [ModOnly]
        [CommandHandler("silence", "Mute everyone in the area except for moderators.")]
        public void Silence(IClient client, string[] args)
        {
            _silencedAreas[client.Area!] = true;
            client.SendOocMessage("The room has been struck with fear and unable to speak.");
        }

        [ModOnly]
        [CommandHandler("unsilence", "Allows the room to speak once more.")]
        public void UnSilence(IClient client, string[] args)
        {
            _silencedAreas[client.Area!] = false;
            client.SendOocMessage("The tension has lifted and they now may speak freely.");
        }
    }
}