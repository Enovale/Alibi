using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Alibi.Plugins.API;

namespace Alibi.Plugins.Cerberus
{
    public class MainPlugin : Plugin
    {
        public override string ID => "com.elijahzawesome.Cerberus";
        public override string Name => "Cerberus";

        public static CerberusConfiguration Config;

        private string _configPath;

        private Dictionary<IClient, Tuple<DateTime, int>> _icTimeDict;

        public override void Initialize()
        {
            _configPath = Path.Combine(PluginManager.GetConfigFolder(ID), "config.json");
            if (!File.Exists(_configPath))
                File.WriteAllText(_configPath, JsonSerializer.Serialize(new CerberusConfiguration(),
                    new JsonSerializerOptions {WriteIndented = true}));

            Config = JsonSerializer.Deserialize<CerberusConfiguration>(File.ReadAllText(_configPath));

            _icTimeDict = new Dictionary<IClient, Tuple<DateTime, int>>(Server.ClientsConnected.Count);

            MutedClientsCheck();
        }

        // ReSharper disable once FunctionRecursiveOnAllPaths
        private async void MutedClientsCheck()
        {
            foreach (var client in _icTimeDict.Keys)
                if (client.Muted
                    && _icTimeDict[client].Item1.AddSeconds(Config.IcMuteLengthInSeconds).CompareTo(DateTime.Now) <
                    0)
                {
                    client.SendOocMessage("You have been un-muted.");
                    client.Muted = false;
                }

            await Task.Delay(1000);
            MutedClientsCheck();
        }

        public override bool OnIcMessage(IClient client, string message)
        {
            if (_icTimeDict.ContainsKey(client))
            {
                if (DateTime.Now.CompareTo(_icTimeDict[client].Item1) >= 0)
                {
                    _icTimeDict[client] = new Tuple<DateTime, int>(DateTime.Now.AddSeconds(1), 0);
                }
                else
                {
                    var tuple = _icTimeDict[client];
                    _icTimeDict[client] = new Tuple<DateTime, int>(tuple.Item1, tuple.Item2 + 1);
                }

                if (_icTimeDict[client].Item2 > Config.MaxIcMessagesPerSecond)
                {
                    client.Muted = true;
                    client.SendOocMessage($"You have been muted for {Config.IcMuteLengthInSeconds} seconds.");
                    _icTimeDict[client] = new Tuple<DateTime, int>(DateTime.Now, 0);
                    return false;
                }

                return true;
            }
            else
            {
                _icTimeDict.Add(client, new Tuple<DateTime, int>(DateTime.Now, 0));
            }

            return true;
        }
    }
}