using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

namespace AO2Sharp.Commands
{
    internal static class Commands
    {
        [CommandHandler("help", "Show's this text.")]
        internal static void Help(Client client, string[] args)
        {
            string finalResponse = "Commands: \n";
            foreach (var (command, handler) in CommandHandler._handlers)
            {
                if (client.Authed)
                    finalResponse +=
                        $"/{command}: {handler.Method.GetCustomAttributes<CommandHandlerAttribute>().First().ShortDesc}\n";
                else if (!handler.Method.GetCustomAttributes(typeof(ModOnlyAttribute)).Any())
                    finalResponse += $"/{command}: {handler.Method.GetCustomAttributes<CommandHandlerAttribute>().First().ShortDesc}\n";
            }

            client.SendOocMessage(finalResponse);
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
                client.Authed = false;
            else
                client.SendOocMessage("You are not logged in.");
        }

        [CommandHandler("pc", "Shows the player count.")]
        internal static void PlayerCount(Client client, string[] args)
        {
            client.SendOocMessage($"{client.Area.PlayerCount} players in this Area.");
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
                client.SendOocMessage("Usage: /ban <hdid/ip> [reason]");
                return;
            }

            string reason = args.Length > 1 ? args[1] : "No reason given.";

            if (IPAddress.TryParse(args[0], out _))
            {
                // Gross
                foreach (var c in new Queue<Client>(client.Server.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])))
                {
                    c.BanIp(reason);
                }
            }
            else
            {
                foreach (var c in new Queue<Client>(client.Server.ClientsConnected.Where(c => c.HardwareId == args[0])))
                {
                    c.BanHdid(reason);
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
                client.SendOocMessage("Usage: /unban <hdid/ip>");
                return;
            }

            if (IPAddress.TryParse(args[0], out _))
            {
                Server.Database.UnbanIp(args[0]);
            }
            else
            {
                Server.Database.UnbanHdid(args[0]);
            }

            client.SendOocMessage($"{args[0]} has been unbanned.");
        }
    }
}
