using AO2Sharp.Plugins.API;
using AO2Sharp.Plugins.API.Attributes;
using System.IO;
using System.Text.Json;

namespace AO2Sharp.Plugins.Webhook
{
    public class DiscordWebhook : Plugin
    {
        public override string ID => "com.elijahzawesome.DiscordWebhook";
        public override string Name => "DiscordWebhook";

        private string _configFile;
        private string _rawMessage;
        private bool _validConfig;
        private bool _enabled = true;

        private DWebHook _hook;

        public override void Initialize()
        {
            _configFile = Path.Combine(PluginManager.GetConfigFolder(ID), "config.json");

            if (!File.Exists(_configFile) || string.IsNullOrWhiteSpace(File.ReadAllText(_configFile)))
            {
                File.WriteAllText(_configFile, JsonSerializer.Serialize(new WebhookConfig(), new JsonSerializerOptions { WriteIndented = true }));
                LogError("No config found. Check this mod's config JSON and add the needed values.");
                return;
            }

            var config = JsonSerializer.Deserialize<WebhookConfig>(File.ReadAllText(_configFile));
            if (config.WebhookUrl == null || config.Username == null || config.Message == null)
            {
                LogError("Config file is empty, please fill in the webhook, username, and message in the JSON.");
                return;
            }

            _hook = new DWebHook(config.WebhookUrl);
            _hook.Username = config.Username;
            _hook.AvatarUrl = config.AvatarURL;
            _rawMessage = config.Message;
            _validConfig = true;

            LogInfo("Discord Webhook loaded.");
        }

        [ModOnly]
        [CustomCommandHandler("discord", "Enable or disable the discord webhook. (on/off)")]
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
                string decodedMessage = _rawMessage;
                decodedMessage = decodedMessage.Replace("%ch", caller.CharacterName);
                decodedMessage = decodedMessage.Replace("%a", caller.IArea.Name);
                decodedMessage = decodedMessage.Replace("%r", reason);
                decodedMessage = decodedMessage.Replace("%ip", caller.IpAddress.ToString());
                decodedMessage = decodedMessage.Replace("%hwid", caller.HardwareId);
                decodedMessage = decodedMessage.Replace("%lsm", caller.LastSentMessage);
                _hook.SendMessage(decodedMessage);
            }
        }
    }
}
