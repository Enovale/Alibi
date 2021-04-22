using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Alibi.Plugins.API;

// ReSharper disable ClassNeverInstantiated.Global

namespace Alibi.Plugins.Fun
{
    public class FunPlugin : Plugin
    {
        public sealed override string ID => "com.elijahzawesome.Fun";
        public sealed override string Name => "Fun";

        public static readonly Dictionary<IClient, bool> Disemvoweled = new Dictionary<IClient, bool>();
        public static readonly Dictionary<IClient, bool> Shaken = new Dictionary<IClient, bool>();
        public static string EightBallConfigPath;

        public FunPlugin(IServer server, IPluginManager pluginManager) : base(server, pluginManager)
        {
            EightBallConfigPath = Path.Combine(pluginManager.GetConfigFolder(ID), "8ball.json");
        }

        public override void OnPlayerJoined(IClient client)
        {
            Disemvoweled.Add(client, false);
            Shaken.Add(client, false);
        }

        public override bool OnIcMessage(IClient client, ref string message)
        {
            if (Disemvoweled[client])
            {
                string[] vowels = { "A", "E", "I", "O", "U", "Y" };
                foreach (var vowel in vowels)
                    message = message.Replace(vowel, "", true, CultureInfo.InvariantCulture);
            }

            if (Shaken[client])
            {
                var words = message.Split(' ');
                message = string.Join(' ', words.OrderBy(w => Commands.Rand.Next()));
            }

            return true;
        }
    }
}