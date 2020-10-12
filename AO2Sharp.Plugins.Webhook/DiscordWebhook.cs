using System;
using System.IO;
using System.Text.Json;
using AO2Sharp.Plugins.API;
using AO2Sharp.Plugins.API.Attributes;

namespace AO2Sharp.Plugins.Webhook
{
    public class DiscordWebhook : Plugin
    {
        public override string ID => "com.elijahzawesome.DiscordWebhook";
        public override string Name => "DiscordWebhook";

        private string _configFile;
        private bool _validConfig;

        private DWebHook _hook;

        public override void Initialize(IPluginManager manager)
        {
            _configFile = Path.Combine(manager.GetConfigFolder(ID), "config.json");

            if (!File.Exists(_configFile) || string.IsNullOrWhiteSpace(File.ReadAllText(_configFile)))
            {
                File.WriteAllText(_configFile, JsonSerializer.Serialize(new WebhookConfig(), new JsonSerializerOptions { WriteIndented = true }));
                LogError("No config found. Check this mod's config JSON and add the needed values.");
                return;
            }

            var config = JsonSerializer.Deserialize<WebhookConfig>(File.ReadAllText(_configFile));
            if (config.WebhookUrl == null || config.Username == null)
            {
                LogError("Config file is empty, please add the needed values to the JSON.");
                return;
            }

            _hook = new DWebHook(config.WebhookUrl);
            _hook.Username = config.Username;
            _hook.AvatarUrl = config.AvatarURL;
            _validConfig = true;

            LogInfo("Discord Webhook loaded.");
        }

        [CustomCommandHandler("customtest", "A csutom command test!")]
        public static void Custom(IClient client, string[] args)
        {
            Console.WriteLine("Test!");
        }

        public override void OnModCall(IClient caller, string reason)
        {
            if(_validConfig)
                _hook.SendMessage($"{caller.IpAddress} has called for a moderator in {caller.IArea.Name}. Reasoning: {reason}");
        }
    }
}
