#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Alibi.Helpers;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;
using Alibi.Plugins.API.Exceptions;
using Alibi.Protocol;
using AOPacket = Alibi.Helpers.AOPacket;

#pragma warning disable IDE0060 // Remove unused parameter
// ReSharper disable UnusedParameter.Global

namespace Alibi.Commands
{
    internal static class Commands
    {
        private static readonly Random _random = new Random();

        [CommandHandler("help", "Show's this text.")]
        internal static void Help(IClient client, string[] args)
        {
            string finalResponse = "Commands:";
            var commandsAvailable = CommandHandler.HandlerInfo
                .Where(t => client.Auth >= AuthType.MODERATOR || !t.Item3).ToList();
            var pageSize = 5;
            var page = 0;
            if (args.Length >= 1)
                if (int.TryParse(args[0], out var givenPage))
                    page = Math.Max(0, Math.Min(givenPage - 1, (commandsAvailable.Count - 1) / pageSize));
            var totalPages = (int) Math.Max(0, Math.Floor((float) commandsAvailable.Count / pageSize));
            foreach (var (command, desc, modOnly) in commandsAvailable.Skip(page * pageSize).Take(pageSize))
                finalResponse += $"\n/{command}: {desc}";
            finalResponse += $"\n====PAGE {page + 1} of {totalPages + 1}====";

            client.SendOocMessage(finalResponse);
        }

        [CommandHandler("randomchar", "Changes you to a random character.")]
        internal static void RandomChar(IClient client, string[] args)
        {
            var charsLeft = Server.CharactersList.Length;
            while (charsLeft > 0)
            {
                var character = _random.Next(Server.CharactersList.Length - 1);
                if (!client.Area!.TakenCharacters[character])
                {
                    Messages.ChangeCharacter(client, new AOPacket("CC", "0", character.ToString(), ""));
                    return;
                }

                charsLeft--;
            }

            throw new CommandException("All characters are taken.");
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

            var userToPm = client.ServerRef.FindUser(args[0]);
            if (userToPm != null)
            {
                string message = string.Join(' ', args.Skip(1));
                userToPm.SendOocMessage(message, "(PM) " + client.OocName!);
                client.SendOocMessage($"Sent to {userToPm.CharacterName}.");
            }
            else
            {
                throw new CommandException("That user cannot be found.");
            }
        }

        [CommandHandler("bg", "Set the background for this area.")]
        internal static void SetBackground(IClient client, string[] args)
        {
            if (!client.Area!.BackgroundLocked)
            {
                ((Area) client.Area).Background = args[0];
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
                if (client.Auth < AuthType.MODERATOR)
                    output += "\n" + tchar + ", ID: " + i;
                else
                    output +=
                        $"\n{client.ServerRef.FindUser(tchar).IpAddress}: " +
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

            if (int.TryParse(args[0], out var lockType))
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
            {
                throw new CommandException("Invalid lock type provided.");
            }
        }

        [CommandHandler("status", "Set the status on this area. Type /status help for types.")]
        internal static void AreaStatus(IClient client, string[] args)
        {
            if (!client.Area!.IsClientCM(client))
                throw new CommandException("Must be CM to change the area status.");

            if (args.Length <= 0)
                throw new CommandException($"{client.Area.Name}: {client.Area.Status}");

            string[] allowedStatuses = {"IDLE", "RP", "CASING", "LOOKING-FOR-PLAYERS", "LFP", "RECESS", "GAMING"};

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
            {
                characterToCm = (int) client.Character;
            }
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
            var cmExists = area.CurrentCaseManagers.Count > 0;
            if (!cmExists)
            {
                area.CurrentCaseManagers.Add(clientToCm);
                area.AreaUpdate(AreaUpdateType.CourtManager);
                client.SendOocMessage($"{clientToCm.CharacterName} has become a CM.");
                return;
            }

            var isAlreadyCm = area.IsClientCM(clientToCm);
            if (isAlreadyCm)
            {
                throw new CommandException("They are already CM in this area.");
            }

            if (area.IsClientCM(client))
            {
                area.CurrentCaseManagers.Add(clientToCm);
                area.AreaUpdate(AreaUpdateType.CourtManager);
                client.SendOocMessage($"{clientToCm.CharacterName} has become a CM.");
            }
            else
            {
                throw new CommandException("You must be added by the CM to do this.");
            }
        }

        [CommandHandler("uncm", "Add a CM to the area.")]
        internal static void RemoveCaseManager(IClient client, string[] args)
        {
            var characterToDeCm = (int) (args.Length > 0 ? int.Parse(args[0]) : client.Character!);
            IClient clientToDeCm =
                client.ServerRef.ClientsConnected.Single(c => c.Character == characterToDeCm && c.Area == client.Area);
            IArea area = client.Area!;
            var cmExists = area!.CurrentCaseManagers.Count > 0;
            if (!cmExists)
                throw new CommandException("There aren't any Case Managers in this area.");

            var isTargetCM = area.IsClientCM(clientToDeCm);
            if (area.IsClientCM(client))
            {
                if (isTargetCM)
                {
                    area.CurrentCaseManagers.Remove(clientToDeCm);
                    area.AreaUpdate(AreaUpdateType.CourtManager);
                    client.SendOocMessage($"{clientToDeCm.CharacterName} is no longer a CM.");
                }
                else
                {
                    throw new CommandException("They are not CM, so cannot de-CM them.");
                }
            }
            else
            {
                throw new CommandException("Must be CM to remove a CM.");
            }
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
            if (client.Auth >= AuthType.MODERATOR)
                throw new CommandException("You are already logged in.");

            if (args.Length < 2)
                throw new CommandException("Usage: /login <username> <password>");

            if (!Server.Database.CheckCredentials(args[0], args[1]))
                throw new CommandException("Incorrect credentials.");

            ((Client) client).Auth = Server.Database.GetPermissionLevel(args[0]);
            client.SendOocMessage("You have been authenticated as " + args[0] + ".");
            Server.Logger.Log(LogSeverity.Info, $"[{client.IpAddress}] Logged in as {args[0]}.");
        }

        [CommandHandler("logout", "De-authenticates you if you are logged in as a moderator.")]
        internal static void Logout(IClient client, string[] args)
        {
            if (client.Auth >= AuthType.MODERATOR)
            {
                ((Client) client).Auth = AuthType.USER;
                client.SendOocMessage("Logged out.");
            }
            else
            {
                throw new CommandException("You are not logged in.");
            }
        }

        [ModOnly]
        [CommandHandler("announce", "Send a message to the entire server.")]
        internal static void Announce(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /announce <message>");
            client.ServerRef.BroadcastOocMessage(string.Join(' ', args));
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

        [AdminOnly]
        [CommandHandler("addmod", "Adds a moderator user to the database.")]
        internal static void AddModerator(IClient client, string[] args)
        {
            if (args.Length < 2)
                throw new CommandException("Usage: /addmod <username> <password>");

            args[0] = args[0].ToLower();
            if (Server.Database.AddLogin(args[0], args[1], AuthType.MODERATOR))
                client.SendOocMessage($"User {args[0]} has been created.");
            else
                throw new CommandException($"User {args[0]} already exists or another error occured.");
        }

        [AdminOnly]
        [CommandHandler("addadmin", "Adds an administrator user to the database.")]
        internal static void AddAdministrator(IClient client, string[] args)
        {
            if (args.Length < 2)
                throw new CommandException("Usage: /addadmin <username> <password>");

            args[0] = args[0].ToLower();
            if (Server.Database.AddLogin(args[0], args[1], AuthType.ADMINISTRATOR))
                client.SendOocMessage($"User {args[0]} has been created.");
            else
                throw new CommandException($"User {args[0]} already exists or another error occured.");
        }

        [AdminOnly]
        [CommandHandler("removelogin", "Removes a moderator user from the database.")]
        internal static void RemoveLogin(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /removelogin <username>");

            if (Server.Database.RemoveLogin(args[0].ToLower()))
                client.SendOocMessage($"Successfully removed user {args[0]}.");
            else
                throw new CommandException($"Could not remove user {args[0]}. " +
                                           $"Make sure this isn't the only admin and that " +
                                           $"this username exists in the database.");
        }

        [AdminOnly]
        [CommandHandler("promote", "Upgrades a moderator's permissions level to administrator.")]
        internal static void PromoteLogin(IClient client, string[] args)
        {
            if(args.Length <= 0)
                throw new CommandException("Usage: /promote <username>");
            
            if(Server.Database.ChangeLoginPermissions(args[0], AuthType.ADMINISTRATOR))
                client.SendOocMessage($"Successfully promoted {args[0]} to Admin.");
            else
                throw new CommandException("Couldn't find that username.");
        }

        [AdminOnly]
        [CommandHandler("demote", "Downgrades an admin's permissions level to a moderator.")]
        internal static void DemoteLogin(IClient client, string[] args)
        {
            if(args.Length <= 0)
                throw new CommandException("Usage: /demote <username>");
            
            if(Server.Database.ChangeLoginPermissions(args[0], AuthType.MODERATOR))
                client.SendOocMessage($"Successfully demoted {args[0]} to Moderator.");
            else
                throw new CommandException("Couldn't find that username.");
        }

        [ModOnly]
        [CommandHandler("mute", "Prevent a player from sending messages to IC or OOC")]
        internal static void Mute(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /mute <charid/name/oocname/hwid>");

            client.ServerRef.FindUser(args[0])!.Muted = true;
        }

        [ModOnly]
        [CommandHandler("unmute", "Allow a player to send messages again")]
        internal static void UnMute(IClient client, string[] args)
        {
            if (args.Length <= 0)
                throw new CommandException("Usage: /unmute <charid/name/oocname/hwid>");

            client.ServerRef.FindUser(args[0])!.Muted = false;
        }

        [ModOnly]
        [CommandHandler("ban", "Ban a user. You can specify a hardware ID or IP")]
        internal static void Ban(IClient client, string[] args)
        {
            if (args.Length < 1)
                throw new CommandException("Usage: /ban <hwid/ip> [reason]");

            string reason = args.Length > 1 ? args[1] : "No reason given.";
            var expires = args.Length > 2 ? args[2] : null;

            var expireDate = TimeSpan.Zero;
            if (expires != null)
            {
                string[] expireArgs = expires.Split(" ");
                if (expireArgs.Length >= 2)
                    if (int.TryParse(expireArgs[0], out var value))
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
                            _ => TimeSpan.Zero
                        };
                    }
            }

            if (IPAddress.TryParse(args[0], out _))
                // Gross
                foreach (var c in new Queue<IClient>(
                    client.ServerRef.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])))
                    c.BanIp(reason, expireDate);
            else
                foreach (var c in new Queue<IClient>(
                    client.ServerRef.ClientsConnected.Where(c => c.HardwareId == args[0])))
                    c.BanHwid(reason, expireDate);

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
                // Gross
                foreach (var c in new Queue<IClient>(
                    client.ServerRef.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])))
                    c.Kick(reason);
            else
                foreach (var c in new Queue<IClient>(
                    client.ServerRef.ClientsConnected.Where(c => c.HardwareId == args[0])))
                    c.Kick(reason);

            client.SendOocMessage($"{args[0]} has been kicked.");
        }

        [ModOnly]
        [CommandHandler("hwid", "Get the HWID of a user from IP or ID")]
        internal static void GetHwid(IClient client, string[] args)
        {
            if (args.Length <= 0 || IPAddress.TryParse(args[0], out _) && !int.TryParse(args[0], out _))
                throw new CommandException("Usage: /hwid <ip/charId>");

            IClient[] ipSearch = client.ServerRef.ClientsConnected.Where(c => c.IpAddress.ToString() == args[0])
                .ToArray();
            if (ipSearch.Length > 0)
            {
                string output = "Hwids: ";
                foreach (var c in ipSearch) output += $"\n\"{c.HardwareId}\"";
                client.SendOocMessage(output);
                return;
            }

            var searchedChar = int.Parse(args[0]);
            if (!client.Area!.TakenCharacters[searchedChar])
                throw new CommandException("Usage: /hwid <ip/charId>");
            IClient idSearch =
                client.ServerRef.ClientsConnected.Single(c => c.Area == client.Area && c.Character == searchedChar);
            client.SendOocMessage($"Hwids: \n\"{idSearch.HardwareId}\"");
        }
    }
}