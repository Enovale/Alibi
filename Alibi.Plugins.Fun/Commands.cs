using System;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;
using Alibi.Plugins.API.Exceptions;

namespace Alibi.Plugins.Fun
{
    public class Commands
    {
        public static Random Rand = new Random();

        private static string GetRoll(int max, int times)
        {
            string rollResult = "Rolled:";
            for (int i = 0; i < times; i++)
            {
                rollResult += $"\n{Rand.Next(max)}";
            }

            return rollResult;
        }
        
        [CommandHandler("roll", "Roll the dice and get a number.")]
        public static void Roll(IClient client, string[] args)
        {
            int maximum = 6;
            int rolls = 1;
            if (args.Length == 1)
                if (int.TryParse(args[0], out int maxTest))
                    maximum = maxTest;
            if(args.Length >= 2)
                if (int.TryParse(args[1], out int rollTest))
                    rolls = Math.Min(10, rollTest);
            
            client.Area!.BroadcastOocMessage(GetRoll(maximum, rolls));
        }
        
        [CommandHandler("rollp", "Roll the dice and get a number, which is sent privately.")]
        public static void RollPrivately(IClient client, string[] args)
        {
            int maximum = 6;
            int rolls = 1;
            if (args.Length < 2)
                if (int.TryParse(args[1], out int maxTest))
                    maximum = maxTest;
            if(args.Length >= 2)
                if (int.TryParse(args[0], out int rollTest))
                    rolls = Math.Min(10, rollTest);
            
            client.SendOocMessage(GetRoll(maximum, rolls));
        }

        [CommandHandler("coinflip", "Flip a coin. The entire area can see the answer.")]
        public static void CoinFlip(IClient client, string[] args)
        {
            client.Area!.BroadcastOocMessage("A coin was flipped: " + (Rand.Next(0, 1) == 1 ? "Heads" : "Tails"));
        }

        [CommandHandler("8ball", "Ask the 8ball a question, and it shall be answered...")]
        public static void EightBall(IClient client, string[] args)
        {
            if(args.Length <= 0)
                throw new CommandException("Usage: /8ball <question>");
            string question = string.Join(' ', args);
            client.Area!.BroadcastOocMessage(
                $"{client.OocName} has asked the 8ball: \"{question}\"\n" +
                $"The 8ball says: {Fun.EightBall.Responses[Rand.Next(Fun.EightBall.Responses.Length - 1)]}");
        }

        [ModOnly]
        [CommandHandler("disemvowel", "Remove all vowels from this user's IC messages.")]
        public static void Disemvowel(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /disemvowel <charid/charname/oocname>");
            var givenClient = client.ServerRef.FindUser(args[0]);
            if(givenClient != null)
                FunPlugin.Disemvoweled[givenClient] = true;
            else
                throw new CommandException("User not found.");
            
            client.Area!.BroadcastOocMessage($"{givenClient.CharacterName}'s tongue has been tied.");
        }

        [ModOnly]
        [CommandHandler("undisemvowel", "Untie a user's tongue.")]
        public static void UnDisemvowel(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /undisemvowel <charid/charname/oocname>");
            var givenClient = client.ServerRef.FindUser(args[0]);
            if(givenClient != null)
                FunPlugin.Disemvoweled[givenClient] = false;
            else
                throw new CommandException("User not found.");
            
            client.Area!.BroadcastOocMessage($"{givenClient.CharacterName}'s tongue has been freed.");
        }

        [ModOnly]
        [CommandHandler("shake", "Shake around this user's IC messages.")]
        public static void Shake(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /shake <charid/charname/oocname>");
            var givenClient = client.ServerRef.FindUser(args[0]);
            if(givenClient != null)
                FunPlugin.Shaken[givenClient] = true;
            else
                throw new CommandException("User not found.");
            
            client.Area!.BroadcastOocMessage($"{givenClient.CharacterName} is shook up.");
        }

        [ModOnly]
        [CommandHandler("unshake", "Stabilize this user's IC messages.")]
        public static void UnShake(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /unshake <charid/charname/oocname>");
            var givenClient = client.ServerRef.FindUser(args[0]);
            if(givenClient != null)
                FunPlugin.Shaken[givenClient] = false;
            else
                throw new CommandException("User not found.");
            
            client.Area!.BroadcastOocMessage($"{givenClient.CharacterName} caught his balance.");
        }
    }
}