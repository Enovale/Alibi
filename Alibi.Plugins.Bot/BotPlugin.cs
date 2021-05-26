using Alibi.Plugins.API;

namespace Alibi.Plugins.Bot
{
    public class BotPlugin : Plugin
    {
        public override string ID => "com.elijahzawesome.bots";
        public override string Name => "Bot API";

        public BotPlugin(IServer server, IPluginManager pluginManager) : base(server, pluginManager)
        {
            var bot = new BotClient(server);
        }
    }
}