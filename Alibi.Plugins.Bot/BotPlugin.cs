using System;
using System.Collections.Generic;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Exceptions;

namespace Alibi.Plugins.Bot
{
    public sealed class BotPlugin : Plugin
    {
        public override string ID => "com.elijahzawesome.bots";
        public override string Name => "BotPlugin";

        public readonly List<BotClient> RegisteredBots;

        private readonly Server _server;
        
        public BotPlugin(IServer server, IPluginManager pluginManager) : base(server, pluginManager)
        {
            _server = server as Server;
            RegisteredBots = new List<BotClient>(server.ServerConfiguration.MaxPlayers);
        }

        public override void OnServerInitialized()
        {
            var testBot = CreateBot();
            var testBot2 = CreateBot();
            var testBot3 = CreateBot();
        }

        public BotClient CreateBot()
        {
            var newBot = new BotClient(_server);
            newBot.Receive(new AOPacket("HI", Guid.NewGuid().ToString().Replace("-", "")));
            newBot.Receive(new AOPacket("ID", "Alibi", "Bot"));
            newBot.Receive(new AOPacket("RD"));
            RegisteredBots.Add(newBot);
            return newBot;
        }
    }
}