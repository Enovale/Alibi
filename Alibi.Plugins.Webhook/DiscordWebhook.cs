using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;
using Alibi.Plugins.Webhook.Helpers;
using System;
using System.IO;
using System.Text.Json;

namespace Alibi.Plugins.Webhook
{
    public class DiscordWebhook : Plugin
    {
        public override string ID => "com.elijahzawesome.DiscordWebhook";
        public override string Name => "DiscordWebhook";

        public WebhookConfig Configuration;
        private bool _validConfig;
        private bool _enabled = true;

        private DWebHook _hook;

        public override void Initialize()
        {
            var configFile = Path.Combine(PluginManager.GetConfigFolder(ID), "config.json");

            if (!File.Exists(configFile) || string.IsNullOrWhiteSpace(File.ReadAllText(configFile)))
            {
                File.WriteAllText(configFile, JsonSerializer.Serialize(new WebhookConfig(), new JsonSerializerOptions { WriteIndented = true }));
                Log(LogSeverity.Error, "No config found. Check this mod's config JSON and add the needed values.");
                return;
            }

            Configuration = JsonSerializer.Deserialize<WebhookConfig>(File.ReadAllText(configFile));
            if (Configuration.WebhookUrl == null || Configuration.Username == null || Configuration.ModMessage == null)
            {
                Log(LogSeverity.Error, "Config file is empty, please fill in the webhook, username, and message in the JSON.");
                return;
            }

            _hook = new DWebHook(Configuration.WebhookUrl);
            _hook.Username = Configuration.Username;
            _hook.AvatarUrl = Configuration.AvatarURL;
            _validConfig = true;

            Log(LogSeverity.Info, "Discord Webhook loaded.");
        }

        [ModOnly]
        [CommandHandler("discord", "Enable or disable the discord webhook. (on/off)")]
        public void SetEnabledCommand(IClient client, string[] args)
        {
            if (args.Length <= 0)
            {
                client.SendOocMessage("Usage: /discord <on/off>");
                return;
            }

            _enabled = args[0].ToLower().StartsWith("on");
            client.SendOocMessage("Discord webhook successfully " + (_enabled ? "enabled." : "disabled."));
        }


        public override void OnModCall(IClient caller, string reason)
        {
            if (_validConfig && _enabled)
            {
                string decodedMessage = Configuration.ModMessage;
                decodedMessage = decodedMessage.Replace("%ch", caller.CharacterName);
                decodedMessage = decodedMessage.Replace("%a", caller.Area.Name);
                decodedMessage = decodedMessage.Replace("%r", reason);
                decodedMessage = decodedMessage.Replace("%ip", caller.IpAddress.ToString());
                decodedMessage = decodedMessage.Replace("%hwid", caller.HardwareId);
                decodedMessage = decodedMessage.Replace("%lsm", caller.LastSentMessage);
                _hook.SendMessage(decodedMessage);
            }
        }

        public override void OnBan(IClient banned, string reason, TimeSpan? expires = null)
        {
            if (_validConfig && _enabled)
            {
                string decodedMessage = Configuration.BanMessage;
                decodedMessage = decodedMessage.Replace("%ch", banned.CharacterName);
                decodedMessage = decodedMessage.Replace("%e",
                    expires != null ? expires.Value.LargestIntervalWithUnits() : "Never.");
                decodedMessage = decodedMessage.Replace("%r", reason);
                decodedMessage = decodedMessage.Replace("%ip", banned.IpAddress.ToString());
                decodedMessage = decodedMessage.Replace("%hwid", banned.HardwareId);
                decodedMessage = decodedMessage.Replace("%lsm", banned.LastSentMessage);
                _hook.SendMessage(decodedMessage);
            }
        }
    }
}
