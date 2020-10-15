#nullable enable
using AO2Sharp.Helpers;
using AO2Sharp.Plugins.API;
using AO2Sharp.Plugins.API.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
#pragma warning disable IDE0060 // Remove unused parameter
// ReSharper disable UnusedParameter.Global

namespace AO2Sharp.Commands
{
    internal static class Commands
    {
        [CommandHandler("help", "Show's this text.")]
        internal static void Help(IClient client, string[] args)
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
        internal static void Motd(IClient client, string[] args)
        {
            client.SendOocMessage(Server.ServerConfiguration.MOTD);
        }

        [CommandHandler("online", "Shows the player count.")]
        internal static void PlayerCount(IClient client, string[] args)
        {
            client.SendOocMessage($"{client.Area!.PlayerCount} players in this Area.");
        }

        [CommandHandler("pos", "Set your position (def, wit, pro, jud, etc).")]
        internal static void SetPosition(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /pos <position>");

            client.Position = args[0];
            client.SendOocMessage($"You have changed your position to {args[0]}");
        }

        [CommandHandler("pm", "Send a private, un-logged message to the user.")]
        internal static void PrivateMessage(IClient client, string[] args)
        {
            if (args.Length < 2)
                throw new CommandException("Usage: /pm <id|oocname|characterName> <message>");

            IClient? userToPM = client.ServerRef.FindUser(args[0]);
            if (userToPM != null)
            {
                string message = string.Join(' ', args.Skip(1));
                userToPM.SendOocMessage(message, "(PM) " + client.OocName!);
                client.SendOocMessage($"Sent to {userToPM.CharacterName}.");
            }
            else
                throw new CommandException("That user cannot be found.");
        }

        [CommandHandler("bg", "Set the background for this area.")]
        internal static void SetBackground(IClient client, string[] args)
        {
            if (!client.Area!.BackgroundLocked)
            {
                ((Area)client.Area).Background = args[0];
                if (args.Length > 1)
                    client.Area.Broadcast(new AOPacket("BN", client.Area.Background, args[1]));
                else
                    client.Area.Broadcast(new AOPacket("BN", client.Area.Background));
            }

            client.SendOocMessage($"You have changed the background to {args[0]}");
        }

        [CommandHandler("areainfo", "Display area info, including players and name.")]
        internal static void AreaInfo(IClient client, string[] args)
        {
            string output = $"{client.Area!.Name}:";
            for (var i = 0; i < client.Area.TakenCharacters.Length; i++)
            {
                if (!client.Area.TakenCharacters[i])
                    continue;
                var tchar = Server.CharactersList[i];
                if (!client.Authed)
                    output += "\n" + tchar + ", ID: " + i;
                else
                    output +=
                        $"\n{client.ServerRef.ClientsConnected.Single(c => c.Character == i).IpAddress}: " +
                        $"{tchar}, ID: {client.Character}\n";
            }
            client.SendOocMessage(output);
        }

        [CommandHandler("arealock", "Set the lock on the current area. 0 = FREE, 1 = SPECTATABLE, 2 = LOCKED.")]
        internal static void LockArea(IClient client, string[] args)
        {
            if (!client.Area!.CanLock)
                throw new CommandException("This area cannot be locked/unlocked.");

            if (!client.Area.IsClientCM(client))
                throw new CommandException("Must be CM to lock areas.");

            if (args.Length <= 0)
                throw new CommandException("Usage: /arealock <0/1/2:FREE/SPECTATABLE/LOCKED>");

            if (int.TryParse(args[0], out int lockType))
            {
                client.Area.Locked = lockType switch
                {
                    0 => "FREE",
                    1 => "SPECTATABLE",
                    2 => "LOCKED",
                    _ => client.Area.Locked
                };
                client.SendOocMessage($"Area lock set to {client.Area.Locked}.");
                client.Area.AreaUpdate(AreaUpdateType.Locked);
            }
            else
                throw new CommandException("Invalid lock type provided.");
        }

        [CommandHandler("status", "Set the status on this area. Type /status help for types.")]
        internal static void AreaStatus(IClient client, string[] args)
        {
            if (!client.Area!.IsClientCM(client))
                throw new CommandException("Must be CM to change the area status.");

            if (args.Length <= 0)
                throw new CommandException($"{client.Area.Name}: {client.Area.Status}");

            string[] allowedStatuses = { "IDLE", "RP", "CASING", "LOOKING-FOR-PLAYERS", "LFP", "RECESS", "GAMING" };

            if (!allowedStatuses.Contains(args[0].ToUpper()))
                throw new CommandException($"Usage: /status <{string.Join('|', allowedStatuses)}>");

            client.Area.Status = args[0].ToUpper();
            client.SendOocMessage("Status changed successfully.");
            client.Area.AreaUpdate(AreaUpdateType.Status);
        }

        [CommandHandler("cm", "Add a CM to the area.")]
        internal static void AddCaseManager(IClient client, string[] args)
        {
            if (client.Character == null)
                throw new CommandException("You must not be a spectater to be a CM.");

            int characterToCm;
            if (args.Length <= 0)
                characterToCm = (int)client.Character;
            else if (!int.TryParse(args[0], out characterToCm))
            {
                if (Server.CharactersList.Contains(args[0]))
                    characterToCm = Array.IndexOf(Server.CharactersList, args[0]);
                else
                    throw new CommandException("Usage: /cm <character name/id>");
            }
            IClient clientToCm =
                client.ServerRef.ClientsConnected.Single(c => c.Character == characterToCm && c.Area == client.Area);
            IArea area = client.Area!;
            bool cmExists = area.CurrentCaseManagers.Count > 0;
            if (!cmExists)
            {
                area.CurrentCaseManagers.Add(clientToCm);
                area.AreaUpdate(AreaUpdateType.CourtManager);
                client.SendOocMessage($"{clientToCm.CharacterName} has become a CM.");
                return;
            }

            bool isAlreadyCM = area.IsClientCM(clientToCm);
            if (isAlreadyCM)
                throw new CommandException("They are already CM in this area.");
            else if (area.IsClientCM(client))
            {
                area.CurrentCaseManagers.Add(clientToCm);
                area.AreaUpdate(AreaUpdateType.CourtManager);
                client.SendOocMessage($"{clientToCm.CharacterName} has become a CM.");
            }
            else
                throw new CommandException("You must be added by the CM to do this.");
        }

        [CommandHandler("uncm", "Add a CM to the area.")]
        internal static void RemoveCaseManager(IClient client, string[] args)
        {
            int characterToDeCm = (int)(args.Length > 0 ? int.Parse(args[0]) : client.Character!);
            IClient clientToDeCm =
                client.ServerRef.ClientsConnected.Single(c => c.Character == characterToDeCm && c.Area == client.Area);
            IArea area = client.Area!;
            bool cmExists = area!.CurrentCaseManagers.Count > 0;
            if (!cmExists)
                throw new CommandException("There aren't any Case Managers in this area.");

            bool isTargetCM = area.IsClientCM(clientToDeCm);
            if (area.IsClientCM(client))
            {
                if (isTargetCM)
                {
                    area.CurrentCaseManagers.Remove(clientToDeCm);
                    area.AreaUpdate(AreaUpdateType.CourtManager);
                    client.SendOocMessage($"{clientToDeCm.CharacterName} is no longer a CM.");
                }
                else
                    throw new CommandException("They are not CM, so cannot de-CM them.");
            }
            else
                throw new CommandException("Must be CM to remove a CM.");
        }

        [CommandHandler("doc", "Set the document URL for the current Area's case.")]
        internal static void SetDocument(IClient client, string[] args)
        {
            if (args.Length <= 0)
                client.SendOocMessage(client.Area!.Document ?? "No document in this area.");

            if (!client.Area!.IsClientCM(client))
                throw new CommandException("Must be a CM to change the document.");

            client.Area.Document = args[0];
            client.SendOocMessage("Document changed.");
        }

        [CommandHandler("cleardoc", "Remove the currently set document.")]
        internal static void ClearDocument(IClient client, string[] args)
        {
            if (!client.Area!.IsClientCM(client))
                throw new CommandException("Must be a CM to change the document.");

            client.Area!.Document = null;
            client.SendOocMessage("Document removed.");
        }

        [CommandHandler("login", "Authenticates you to the server as a moderator.")]
        internal static void Login(IClient client, string[] args)
        {
            if (client.Authed)
                throw new CommandException("You are already logged in.");

            if (args.Length < 2)
                throw new CommandException("Usage: /login <username> <password>");

            if (!client.ServerRef.CheckLogin(args[0], args[1]))
                throw new CommandException("Incorrect credentials.");

            ((Client)client).Authed = true;
            client.SendOocMessage("You have been authenticated as " + args[0] + ".");
            Server.Logger.Log(LogSeverity.Info, $"[{client.IpAddress}] Logged in as {args[0]}.");
        }

        [CommandHandler("logout", "De-authenticates you if you are logged in as a moderator.")]
        internal static void Logout(IClient client, string[] args)
        {
            if (client.Authed)
            {
                ((Client)client).Authed = false;
                client.SendOocMessage("Logged out.");
            }
            else
                throw new CommandException("You are not logged in.");
        }

        [ModOnly]
        [CommandHandler("restart", "Restart's the server.")]
        internal static void Restart(IClient client, string[] args)
        {
            client.ServerRef.Stop();
            Server.Logger.Log(LogSeverity.Special, $"[{client.IpAddress}] Ran the restart command.");
            var env = Environment.GetCommandLineArgs();
            var process = Server.ProcessPath;
            Process.Start(process, string.Join(' ', env.Skip(1)));
            Environment.Exit(0);
        }

        [ModOnly]
        [CommandHandler("getlogs", "Retrieves the server logs and dumps them.")]
        internal static void GetLogs(IClient client, string[] args)
        {
            if (Server.Logger.Dump())
                client.SendOocMessage("Successfully dumped logs. Check the Logs folder.");
            else
                throw new CommandException("No logs have been stored yet, can't dump.");
        }

        [ModOnly]
        [CommandHandler("addlogin", "Adds a moderator user to the database.")]
        internal static void AddLogin(IClient client, string[] args)
        {
            if (args.Length < 2)
                throw new CommandException("Usage: /addlogin <username> <password>");

            args[0] = args[0].ToLower();
            if (client.ServerRef.AddLogin(args[0], args[1]))
                client.SendOocMessage($"User {args[0]} has been created.");
            else
                throw new CommandException($"User {args[0]} already exists or another error occured.");
        }

        [ModOnly]
        [CommandHandler("removelogin", "Removes a moderator user from the database")]
        internal static void RemoveLogin(IClient client, string[] args)
        {
            if (args.Length < 1)
                throw new CommandException("Usage: /removelogin <username>");

            if (client.ServerRef.RemoveLogin(args[0].ToLower()))
                client.SendOocMessage("Successfully removed user " + args[0] + ".");
            else
                throw new CommandException("Could not remove user " + args[0] + ". Does it exist?");
        }

        [ModOnly]
        [CommandHandler("ban", "Ban a user. You can specify a hardware ID or IP")]
        internal static void Ban(IClient client, string[] args)
        {
            if (args.Length < 1)
                throw new CommandException("Usage: /ban <hwid/ip> [reason]");

            string reason = args.Length > 1 ? args[1] : "No reason given.";
            string? expires = args.Length > 2 ? args[2] : null;

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
                foreach (var c in new Queue<IClient>(client.ServerRef.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])))
                {
                    c.BanIp(reason, expireDate);
                }
            }
            else
            {
                foreach (var c in new Queue<IClient>(client.ServerRef.ClientsConnected.Where(c => c.HardwareId == args[0])))
                {
                    c.BanHwid(reason, expireDate);
                }
            }

            client.SendOocMessage($"{args[0]} has been banned.");
        }

        [ModOnly]
        [CommandHandler("unban", "Unbans a user. You can specify a hardware ID or IP.")]
        internal static void Unban(IClient client, string[] args)
        {
            if (args.Length < 1)
                throw new CommandException("Usage: /unban <hwid/ip>");

            if (IPAddress.TryParse(args[0], out _))
                Server.Database.UnbanIp(args[0]);
            else
                Server.Database.UnbanHwid(args[0]);

            client.SendOocMessage($"{args[0]} has been unbanned.");
        }

        [ModOnly]
        [CommandHandler("kick", "Kick a user from the server. You can specify a hardware ID or IP")]
        internal static void Kick(IClient client, string[] args)
        {
            if (args.Length < 1)
                throw new CommandException("Usage: /kick <hwid/ip> [reason]");

            string reason = args.Length > 1 ? args[1] : "No reason given.";

            if (IPAddress.TryParse(args[0], out _))
            {
                // Gross
                foreach (var c in new Queue<IClient>(client.ServerRef.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])))
                {
                    c.Kick(reason);
                }
            }
            else
            {
                foreach (var c in new Queue<IClient>(client.ServerRef.ClientsConnected.Where(c => c.HardwareId == args[0])))
                {
                    c.Kick(reason);
                }
            }

            client.SendOocMessage($"{args[0]} has been kicked.");
        }

        [ModOnly]
        [CommandHandler("hwid", "Get the HWID of a user from IP or ID")]
        internal static void GetHwid(IClient client, string[] args)
        {
            if (args.Length <= 0 || (IPAddress.TryParse(args[0], out _) && !int.TryParse(args[0], out _)))
                throw new CommandException("Usage: /hwid <ip/charId>");

            IClient[] ipSearch = client.ServerRef.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0]).ToArray();
            if (ipSearch.Length > 0)
            {
                string output = "Hwids: ";
                foreach (var c in ipSearch)
                {
                    output += $"\n\"{c.HardwareId}\"";
                }
                client.SendOocMessage(output);
                return;
            }

            int searchedChar = int.Parse(args[0]);
            if (!client.Area!.TakenCharacters[searchedChar])
                throw new CommandException("Usage: /hwid <ip/charId>");
            IClient idSearch =
                    client.ServerRef.ClientsConnected.Single(c => c.Area == client.Area && c.Character == searchedChar);
            client.SendOocMessage($"Hwids: \n\"{idSearch.HardwareId}\"");
        }
    }
}
