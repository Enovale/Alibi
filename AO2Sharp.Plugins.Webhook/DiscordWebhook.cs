using AO2Sharp.Plugins.API;

namespace AO2Sharp.Plugins.Webhook
{
    public class DiscordWebhook : Plugin
    {
        public override string ID => "com.elijahzawesome.DiscordWebhook";
        public override string DebugName => "DiscordWebhook";

        public override void Initialize(IPluginManager manager)
        {
            LogInfo("Discord Webhook loaded.");
        }
    }
}
