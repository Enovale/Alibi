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
        
        public BotPlugin(IServer server, IPluginManager pluginManager) : base(server, pluginManager)
        {
            if (pluginManager.IsPluginLoaded(ID))
                throw new PluginException($"{nameof(BotPlugin)} has already been loaded! Delete duplicate dlls.");
            
            RegisteredBots = new List<BotClient>(server.ServerConfiguration.MaxPlayers);
        }
    }
}