using System.Collections.Generic;
using Alibi.Plugins.API;

namespace Alibi.Plugins.Bot
{
    public class BotPlugin : Plugin
    {
        public override string ID => "com.elijahzawesome.bots";
        public override string Name => "Bot API";

        public List<BotClient> RegisteredBots;

        private readonly IServer _server;

        public BotPlugin(IServer server, IPluginManager pluginManager) : base(server, pluginManager)
        {
            _server = server;
        }

        public override void OnServerStarted()
        {
            var bot = new BotClient(_server);
            bot.SpeakOoc("Yummy");
        }
    }
}