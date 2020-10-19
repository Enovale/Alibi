using System;
using System.Collections.Generic;
using System.IO;
using Alibi.Plugins.API;

namespace Alibi.Plugins.Fun
{
    public class FunPlugin : Plugin
    {
        public override string ID => "com.elijahzawesome.Fun";
        public override string Name => "Fun";

        public static Dictionary<IClient, bool> Disemvoweled = new Dictionary<IClient, bool>();
        public static string EightBallConfigPath;

        public override void Initialize()
        {
            EightBallConfigPath = Path.Combine(PluginManager.GetConfigFolder(ID), "8ball.json");
            var _ = EightBall.Responses;
        }

        public override void OnPlayerJoined(IClient client)
        {
            Disemvoweled.Add(client, false);
        }

        public override bool OnIcMessage(IClient client, ref string message)
        {
            if (Disemvoweled[client])
            {
                string[] vowels = new[]
                {
                    "A", "E", "I", "O", "U", "Y",
                    "a", "e", "e", "o", "u", "y"
                };
                foreach (var vowel in vowels)
                {
                    message = message.Replace(vowel, "");
                }
            }

            return true;
        }
    }
}