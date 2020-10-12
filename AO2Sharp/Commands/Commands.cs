using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using AO2Sharp.Helpers;
using AO2Sharp.Plugins.API.Attributes;

namespace AO2Sharp.Commands
{
    internal static class Commands
    {
        [CommandHandler("help", "Show's this text.")]
        internal static void Help(Client client, string[] args)
        {
            string finalResponse = "Commands:";
            foreach (var (command, desc, modOnly) in CommandHandler.HandlerInfo)
            {
                if (client.Authed)
                    finalResponse +=
                        $"\n/{command}: {desc}";
                else if (!modOnly)
                    finalResponse += $"\n/{command}: {desc}";
            }

            client.SendOocMessage(finalResponse);
        }

        [CommandHandler("motd", "Displays the MOTD sent upon joining.")]
        internal static void Motd(Client client, string[] args)
        {
            client.SendOocMessage(Server.ServerConfiguration.MOTD);
        }

        [CommandHandler("online", "Shows the player count.")]
        internal static void PlayerCount(Client client, string[] args)
        {
            client.SendOocMessage($"{client.Area.PlayerCount} players in this Area.");
        }

        [CommandHandler("pos", "Set your position (def, wit, pro, jud, etc).")]
        internal static void SetPosition(Client client, string[] args)
        {
            client.Position = args[0];
            client.SendOocMessage($"You have changed your position to {args[0]}");
        }

        [CommandHandler("bg", "Set the background for this area.")]
        internal static void SetBackground(Client client, string[] args)
        {
            if (!client.Area.BackgroundLocked)
            {
                client.Area.Background = args[0];
                if(args.Length > 1)
                    client.Area.Broadcast(new AOPacket("BN", client.Area.Background, args[1]));
                else
                    client.Area.Broadcast(new AOPacket("BN", client.Area.Background));
            }

            client.SendOocMessage($"You have changed the background to {args[0]}");
        }

        [CommandHandler("areainfo", "Display area info, including players and name.")]
        internal static void AreaInfo(Client client, string[] args)
        {
            string output = $"{client.Area.Name}:";
            for (var i = 0; i < client.Area.TakenCharacters.Length; i++)
            {
                if (!client.Area.TakenCharacters[i])
                    continue;
                var tchar = Server.CharactersList[i];
                if (!client.Authed)
                    output += "\n" + tchar;
                else
                    output += $"{client.Server.ClientsConnected.Single(c => c.Character == i).IpAddress}: {tchar}\n";
            }
            client.SendOocMessage(output);
        }

        [CommandHandler("login", "Authenticates you to the server as a moderator.")]
        internal static void Login(Client client, string[] args)
        {
            if (client.Authed)
                client.SendOocMessage("You are already logged in.");

            if (args.Length < 2)
            {
                client.SendOocMessage("Usage: /login <username> <password>");
                return;
            }

            if (!client.Server.CheckLogin(args[0], args[1]))
            {
                client.SendOocMessage("Incorrect credentials.");
                return;
            }

            client.Authed = true;
            client.SendOocMessage("You have been authenticated as " + args[0] + ".");
            Server.Logger.Log(LogSeverity.Info, $"[{client.IpAddress}] Logged in as {args[0]}.");
        }

        [CommandHandler("logout", "De-authenticates you if you are logged in as a moderator.")]
        internal static void Logout(Client client, string[] args)
        {
            if (client.Authed)
            {
                client.Authed = false;
                client.SendOocMessage("Logged out.");
            }
            else
            {
                client.SendOocMessage("You are not logged in.");
            }
        }

        [ModOnly]
        [CommandHandler("restart", "Restart's the server.")]
        internal static void Restart(Client client, string[] args)
        {
            client.Server.Stop();
            Server.Logger.Log(LogSeverity.Special, $"[{client.IpAddress}] Ran the restart command.");
            var env = Environment.GetCommandLineArgs();
            var process = Server.ProcessPath;
            Process.Start(process, string.Join(' ', env.Skip(1)));
            Environment.Exit(0);
        }

        [ModOnly]
        [CommandHandler("getlogs", "Retrieves the server logs and dumps them.")]
        internal static void GetLogs(Client client, string[] args)
        {
            if (Server.Logger.Dump())
                client.SendOocMessage("Successfully dumped logs. Check the Logs folder.");
            else
                client.SendOocMessage("No logs have been stored yet, can't dump.");
        }

        [ModOnly]
        [CommandHandler("addlogin", "Adds a moderator user to the database.")]
        internal static void AddLogin(Client client, string[] args)
        {
            if (args.Length < 2)
            {
                client.SendOocMessage("Usage: /addlogin <username> <password>");
                return;
            }

            args[0] = args[0].ToLower();
            if (client.Server.AddLogin(args[0], args[1]))
                client.SendOocMessage($"User {args[0]} has been created.");
            else
                client.SendOocMessage($"Failed: User {args[0]} already exists or another error occured.");
        }

        [ModOnly]
        [CommandHandler("removelogin", "Removes a moderator user from the database")]
        internal static void RemoveLogin(Client client, string[] args)
        {
            if (args.Length < 1)
            {
                client.SendOocMessage("Usage: /removelogin <username>");
                return;
            }

            if (client.Server.RemoveLogin(args[0].ToLower()))
                client.SendOocMessage("Successfully removed user " + args[0] + ".");
            else
                client.SendOocMessage("Could not remove user " + args[0] + ". Does it exist?");
        }

        [ModOnly]
        [CommandHandler("ban", "Ban a user. You can specify a hardware ID or IP")]
        internal static void Ban(Client client, string[] args)
        {
            if (args.Length < 1)
            {
                client.SendOocMessage("Usage: /ban <hwid/ip> [reason]");
                return;
            }

            string reason = args.Length > 1 ? args[1] : "No reason given.";
            string expires = args.Length > 2 ? args[2] : null;

            TimeSpan? expireDate = null;
            if (expires != null)
            {
                string[] expireArgs = expires.Split(" ");
                if (expireArgs.Length >= 2)
                {
                    if (int.TryParse(expireArgs[0], out int value))
                    {
                        if (!expireArgs[1].EndsWith('s'))
                            expireArgs[1] += 's';
                        expireDate = expireArgs[1] switch
                        {
                            "minutes" => new TimeSpan(0, value, 0),
                            "hours" => new TimeSpan(value, 0, 0),
                            "day" => new TimeSpan(value, 0, 0, 0),
                            "week" => new TimeSpan(value * 7, 0, 0, 0),
                            // Shut up i don't care
                            "month" => new TimeSpan(value * 30, 0, 0, 0),
                            "perma" => null,
                            _ => null
                        };
                    }
                }
            }

            if (IPAddress.TryParse(args[0], out _))
            {
                // Gross
                foreach (var c in new Queue<Client>(client.Server.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])))
                {
                    c.BanIp(reason, expireDate);
                }
            }
            else
            {
                foreach (var c in new Queue<Client>(client.Server.ClientsConnected.Where(c => c.HardwareId == args[0])))
                {
                    c.BanHwid(reason, expireDate);
                }
            }

            client.SendOocMessage($"{args[0]} has been banned.");
        }

        [ModOnly]
        [CommandHandler("unban", "Unbans a user. You can specify a hardware ID or IP.")]
        internal static void Unban(Client client, string[] args)
        {
            if (args.Length < 1)
            {
                client.SendOocMessage("Usage: /unban <hwid/ip>");
                return;
            }

            if (IPAddress.TryParse(args[0], out _))
            {
                Server.Database.UnbanIp(args[0]);
            }
            else
            {
                Server.Database.UnbanHwid(args[0]);
            }

            client.SendOocMessage($"{args[0]} has been unbanned.");
        }

        [ModOnly]
        [CommandHandler("kick", "Kick a user from the server. You can specify a hardware ID or IP")]
        internal static void Kick(Client client, string[] args)
        {
            if (args.Length < 1)
            {
                client.SendOocMessage("Usage: /kick <hwid/ip> [reason]");
                return;
            }

            string reason = args.Length > 1 ? args[1] : "No reason given.";

            if (IPAddress.TryParse(args[0], out _))
            {
                // Gross
                foreach (var c in new Queue<Client>(client.Server.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])))
                {
                    c.Kick(reason);
                }
            }
            else
            {
                foreach (var c in new Queue<Client>(client.Server.ClientsConnected.Where(c => c.HardwareId == args[0])))
                {
                    c.Kick(reason);
                }
            }

            client.SendOocMessage($"{args[0]} has been kicked.");
        }
    }
}
