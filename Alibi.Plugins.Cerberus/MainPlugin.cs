#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;

#pragma warning disable 8618

namespace Alibi.Plugins.Cerberus
{
    public class MainPlugin : Plugin
    {
        public sealed override string ID => "com.elijahzawesome.Cerberus";
        public sealed override string Name => "Cerberus";

        public readonly CerberusConfiguration Config;

        private readonly Dictionary<IClient, MuteInfo> _clientDict;
        private readonly Dictionary<IClient, string?> _lastOocMsgDict;
        private readonly Dictionary<IArea, bool> _silencedAreas;

        public MainPlugin(IServer server, IPluginManager pluginManager) : base(server, pluginManager)
        {
            var configPath = Path.Combine(pluginManager.GetConfigFolder(ID), "config.json");
            if (!File.Exists(configPath) || new FileInfo(configPath).Length <= 0)
                File.WriteAllText(configPath, JsonSerializer.Serialize(new CerberusConfiguration(),
                    new JsonSerializerOptions {WriteIndented = true}));

            Config = JsonSerializer.Deserialize<CerberusConfiguration>(File.ReadAllText(configPath))!;

            _clientDict = new Dictionary<IClient, MuteInfo>();
            foreach (var client in server.ClientsConnected)
                _clientDict.Add(client, new MuteInfo());
            _lastOocMsgDict = new Dictionary<IClient, string?>();

            _silencedAreas = new Dictionary<IArea, bool>(server.Areas.Length);
            foreach (var area in server.Areas)
                _silencedAreas.Add(area, false);

            MutedClientsCheck();
        }

        private async void MutedClientsCheck()
        {
            while (true)
            {
                var queue = new Queue<IClient>(_clientDict.Keys);
                while (queue.Count > 0)
                {
                    var client = queue.Dequeue();
                    if (_clientDict[client].IcMuted
                        && _clientDict[client].IcTimer.AddSeconds(Config.IcMuteLengthInSeconds)
                            .CompareTo(DateTime.Now) < 0)
                    {
                        client.SendOocMessage("You have been un-muted in IC.");
                        _clientDict[client].IcMuted = false;
                    }

                    if (_clientDict[client].OocMuted
                        && _clientDict[client].OocTimer.AddSeconds(Config.OocMuteLengthInSeconds)
                            .CompareTo(DateTime.Now) < 0)
                    {
                        client.SendOocMessage("You have been un-muted in OOC.");
                        _clientDict[client].OocMuted = false;
                    }

                    if (_clientDict[client].MusicMuted
                        && _clientDict[client].MusicTimer.AddSeconds(Config.MusicMuteLengthInSeconds)
                            .CompareTo(DateTime.Now) < 0)
                    {
                        client.SendOocMessage("You have been un-muted from changing Music.");
                        _clientDict[client].MusicMuted = false;
                    }
                }

                await Task.Delay(1000);
            }
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