using Alibi.Commands;
using Alibi.Exceptions;
using Alibi.Helpers;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedParameter.Global
#pragma warning disable IDE0060 // Remove unused parameter

namespace Alibi.Protocol
{
    internal static class Messages
    {
        [MessageHandler("HI")]
        [RequireState(ClientState.NewClient)]
        internal static void HardwareId(IClient client, IAOPacket packet)
        {
            if (packet.Objects.Length <= 0)
                return;

            if (client.HardwareId != null)
                return;

            ((Client) client).HardwareId = packet.Objects[0];
            Server.Database.AddUser(client.HardwareId, client.IpAddress.ToString());
            client.KickIfBanned();
            client.CurrentState = ClientState.Handshook;

            // Check if this player is hardware banned and kick them if so
            // Also probably check if the max players is reached and kick them
            client.Send(new AOPacket("ID", "111111", "Alibi", Server.Version));
        }

        [MessageHandler("ID")]
        [RequireState(ClientState.Handshook)]
        internal static void SoftwareId(IClient client, IAOPacket packet)
        {
            client.CurrentState = ClientState.Identified;
            client.Send(new AOPacket("PN", client.ServerRef.ConnectedPlayers.ToString(),
                Server.ServerConfiguration.MaxPlayers.ToString()));
            client.Send(new AOPacket("FL", new[]
            {
                "noencryption", "fastloading"
            }.Concat(Server.ServerConfiguration.FeatureList).ToArray()));
        }

        // Slow loading, ugh.
        [MessageHandler("askchar2")]
        [RequireState(ClientState.Identified)]
        internal static void RequestCharactersSlow(IClient client, IAOPacket packet)
        {
            RequestCharacterPageSlow(client, new AOPacket("AN", "0"));
        }

        [MessageHandler("AN")]
        [RequireState(ClientState.Identified)]
        internal static void RequestCharacterPageSlow(IClient client, IAOPacket packet)
        {
            // According to current client
            int page = int.Parse(packet.Objects[0]);
            int pageSize = 10 * 9;
            int pageOffset = pageSize * page;

            if (pageOffset >= Server.CharactersList.Length)
            {
                client.Send(new AOPacket("EI", page.ToString(), ""));
                return;
            }

            var finalPacket = new AOPacket("CI",
                Server.CharactersList.Skip(pageOffset).TakeWhile((c, i) => i < Server.CharactersList.Length).ToArray());
            client.Send(finalPacket);
        }

        [MessageHandler("AE")]
        [RequireState(ClientState.Identified)]
        internal static void RequestEvidenceSlow(IClient client, IAOPacket packet)
        {
            if (int.TryParse(packet.Objects[0], out int index))
            {
                index = Math.Max(0, Math.Min(client.Area.EvidenceList.Count, index));

                client.Send(new AOPacket("EI", index.ToString(), client.Area.EvidenceList[index].ToPacket()));
            }
        }

        [MessageHandler("AM")]
        [RequireState(ClientState.Identified)]
        internal static void RequestMusicSlow(IClient client, IAOPacket packet)
        {
            if (int.TryParse(packet.Objects[0], out int index))
            {
                index = Math.Max(0, index);

                var musicList = Server.MusicList.ToList();
                var categories = musicList.Split(m => m.Contains(".")).ToArray();

                if (index >= categories.Length)
                {
                    Ready(client, new AOPacket("RD"));
                    return;
                }

                client.Send(new AOPacket("EM", new[] {index.ToString()}.Concatenate(categories[index].ToArray())));
            }
        }

        [MessageHandler("askchaa")]
        [RequireState(ClientState.Identified)]
        internal static void RequestResourceCounts(IClient client, IAOPacket packet)
        {
            client.Send(new AOPacket("SI", Server.CharactersList.Length.ToString(),
                client.ServerRef.Areas.First().EvidenceList.Count.ToString(),
                (client.ServerRef.Areas.Length + Server.MusicList.Length).ToString()));
        }

        [MessageHandler("RC")]
        [RequireState(ClientState.Identified)]
        internal static void RequestCharacters(IClient client, IAOPacket packet)
        {
            client.Send(new AOPacket("SC", Server.CharactersList));
        }

        [MessageHandler("RE")]
        [RequireState(ClientState.InArea, false)]
        internal static void RequestEvidence(IClient client, IAOPacket packet)
        {
            string[] evidenceList = new string[client.Area!.EvidenceList.Count];
            for (var i = 0; i < client.Area.EvidenceList.Count; i++)
            {
                evidenceList[i] = client.Area.EvidenceList[i].ToPacket();
            }

            client.Area.Broadcast(new AOPacket("LE", evidenceList));
        }

        [MessageHandler("RM")]
        [RequireState(ClientState.Identified)]
        internal static void RequestMusic(IClient client, IAOPacket packet)
        {
            client.Send(new AOPacket("SM", client.ServerRef.AreaNames.Concat(Server.MusicList).ToArray()));
        }

        [MessageHandler("RD")]
        [RequireState(ClientState.Identified)]
        internal static void Ready(IClient client, IAOPacket packet)
        {
            // This client didn't send us a hwid, need to kick
            if (string.IsNullOrWhiteSpace(client.HardwareId))
            {
                ((Client) client).Session.Disconnect();
                return;
            }

            if (client.Connected)
                return;

            ((Client) client).Area = client.ServerRef.Areas.First(a => a.Locked == "FREE");
            ((Client) client).Connected = true;
            client.CurrentState = ClientState.InArea;
            client.ServerRef.ConnectedPlayers++;
            ((Area) client.Area)!.PlayerCount++;
            client.Area.FullUpdate(client);
            // Tell all the other clients that someone has joined
            client.Area.AreaUpdate(AreaUpdateType.PlayerCount);

            client.Send(new AOPacket("HP", "1", client.Area.DefendantHp.ToString()));
            client.Send(new AOPacket("HP", "2", client.Area!.ProsecutorHp.ToString()));
            client.Send(new AOPacket("FA", client.ServerRef.AreaNames));
            client.Send(new AOPacket("BN", client.Area.Background));
            // TODO: Determine if this is needed because it's retarded
            // WebAO doesn't use it so im gonna assume its not
            //client.Send(new AOPacket("OPPASS", ServerRef.ServerConfiguration.ModPassword));
            client.Send(new AOPacket("DONE"));
            client.SendOocMessage(Server.ServerConfiguration.MOTD);

            Server.Logger.Log(LogSeverity.Info, $"[{client.IpAddress}] Just joined the server.");
        }

        [MessageHandler("CH")]
        internal static void KeepAlive(IClient client, IAOPacket packet)
        {
            client.Send(new AOPacket("CHECK"));
        }

        [MessageHandler("PE")]
        [RequireState(ClientState.InArea)]
        internal static void AddEvidence(IClient client, IAOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            client.Area!.EvidenceList.Add(new Evidence(packet.Objects[0], packet.Objects[1], packet.Objects[2]));
            RequestEvidence(client, packet);
        }

        [MessageHandler("DE")]
        [RequireState(ClientState.InArea)]
        internal static void RemoveEvidence(IClient client, IAOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            if (int.TryParse(packet.Objects[0], out int id))
                client.Area.EvidenceList.RemoveAt(id);
            RequestEvidence(client, packet);
        }

        [MessageHandler("EE")]
        [RequireState(ClientState.InArea)]
        internal static void EditEvidence(IClient client, IAOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            if (int.TryParse(packet.Objects[0], out int id))
            {
                id = Math.Max(0, Math.Min(client.Area!.EvidenceList.Count, id));

                client.Area.EvidenceList[id] = new Evidence(packet.Objects[1], packet.Objects[2], packet.Objects[3]);
            }

            RequestEvidence(client, packet);
        }

        [MessageHandler("PW")]
        [RequireState(ClientState.InArea)]
        internal static void CharacterPassword(IClient client, IAOPacket packet)
        {
            ((Client) client).Password = packet.Objects.First();
        }

        [MessageHandler("CC")]
        [RequireState(ClientState.InArea)]
        internal static void ChangeCharacter(IClient client, IAOPacket packet)
        {
            if (int.TryParse(packet.Objects[1], out var charId))
            {
                if (client.Character != null)
                    client.Area.TakenCharacters[(int) client.Character] = false;

                if (charId > Server.CharactersList.Length)
                    return;
                if (charId < 0)
                    return;

                string charToTake = Server.CharactersList[charId];
                if (client.Area!.TakenCharacters[charId] || string.IsNullOrWhiteSpace(charToTake))
                    return;

                client.Area.TakenCharacters[charId] = true;
                client.Character = charId;
                client.Area.UpdateTakenCharacters();

                client.Send(new AOPacket("PV", "111111", "CID", charId.ToString()));
            }
        }

        [MessageHandler("MC")]
        [RequireState(ClientState.InArea)]
        internal static void ChangeMusic(IClient client, IAOPacket packet)
        {
            if (packet.Objects.Length <= 0)
                return;
            string song = packet.Objects[0];
            if (!client.ServerRef.OnMusicChange(client, song))
                return;

            foreach (var m in Server.MusicList)
            {
                if (song == m)
                {
                    client.Area.Broadcast(packet);
                    return;
                }
            }

            for (var i = 0; i < client.ServerRef.AreaNames.Length; i++)
            {
                if (song == client.ServerRef.AreaNames[i])
                    client.ChangeArea(i);
            }
        }

        [MessageHandler("MS")]
        [RequireState(ClientState.InArea)]
        internal static void IcMessage(IClient client, IAOPacket packet)
        {
            if (client.Muted)
                return;
            try
            {
                AOPacket validPacket = IcValidator.ValidateIcPacket(packet, client);
                
                if (!client.ServerRef.OnIcMessage(client, validPacket.Objects[4])) // 4 is the message
                    return;

                client.Area!.Broadcast(validPacket);
                Server.Logger.IcMessageLog(packet.Objects[4], client.Area, client);
            }
            catch (IcValidationException e)
            {
                client.SendOocMessage(e.Message);
                //Server.Logger.Log(LogSeverity.Error, e.Message, true);
            }
        }

        [MessageHandler("CT")]
        [RequireState(ClientState.InArea)]
        internal static void OocMessage(IClient client, IAOPacket packet)
        {
            if (client.Muted)
                return;
            if (packet.Objects.Length < 2)
                return;

            // TODO: Sanitization and cleaning (especially Zalgo)
            // maybe put this into anti-spam plugin
            string message = packet.Objects[1];
            if (message.Length > Server.ServerConfiguration.MaxMessageSize)
            {
                client.SendOocMessage("Message was too long.");
                return;
            }

            ((Client) client).OocName = packet.Objects[0];
            if (message.StartsWith("/"))
            {
                string command = message.Substring(1).Split(" ").First().Trim();
                List<string> arguments = new List<string>(message.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                arguments.RemoveAt(0);

                CommandHandler.HandleCommand(client, command, arguments.ToArray());
                return;
            }

            if (!client.ServerRef.OnOocMessage(client, message))
                return;
            
            client.Area!.Broadcast(packet);
            Server.Logger.OocMessageLog(message, client.Area, packet.Objects[0]);
        }

        [MessageHandler("HP")]
        [RequireState(ClientState.InArea)]
        internal static void UpdateHealthBar(IClient client, IAOPacket packet)
        {
            if (!client.Position.ToLower().StartsWith("jud"))
                return;
            if (int.TryParse(packet.Objects[1], out var hp))
            {
                if (packet.Objects[0] == "1")
                    client.Area!.DefendantHp = Math.Max(0, Math.Min(hp, 10));
                else if (packet.Objects[0] == "2")
                    client.Area!.ProsecutorHp = Math.Max(0, Math.Min(hp, 10));

                client.Area!.Broadcast(packet);
            }
        }

        [MessageHandler("RT")]
        [RequireState(ClientState.InArea)]
        internal static void JudgeAnimation(IClient client, IAOPacket packet)
        {
            if ((client.Position ?? "").ToLower().StartsWith("jud"))
                client.Area.Broadcast(packet);
        }

        [MessageHandler("ZZ")]
        [RequireState(ClientState.InArea)]
        internal static void ModCall(IClient client, IAOPacket packet)
        {
            if (!client.ServerRef.OnModCall(client, packet))
                return;

            Server.Logger.Log(LogSeverity.Special, $"[{client.Area.Name}][{client.IpAddress}] " +
                                                   $"{client.CharacterName} called for mod with reasoning: {packet.Objects[0]}");
            var packetToSend = new AOPacket(packet.Type, "Someone has called mod in the " +
                                                         $"{client.Area.Name} area, with reasoning: {packet.Objects[0]}");

            foreach (var c in new Queue<IClient>(client.ServerRef.ClientsConnected))
            {
                if (c.Authed)
                    c.Send(packetToSend);
            }
        }

        [MessageHandler("WSIP")]
        [RequireState(ClientState.Identified)]
        internal static void UpdateWebsocketIp(IClient client, IAOPacket packet)
        {
            IPAddress ip = IPAddress.Parse(packet.Objects[0]);
            if (IPAddress.IsLoopback(client.IpAddress) && !IPAddress.IsLoopback(ip))
            {
                Server.Database.ChangeIp(client.HardwareId, client.IpAddress.ToString(), ip.ToString());
                ((Client) client).IpAddress = ip;
                client.KickIfBanned();
            }

            if (((Server) client.ServerRef).ClientsConnected.Count(c => ip.ToString() == c.IpAddress.ToString())
                >= Alibi.Server.ServerConfiguration.MaxMultiClients)
            {
                client.Kick($"Cannot have more than {Server.ServerConfiguration.MaxMultiClients} clients at the same");
            }
        }

        private static bool CanModifyEvidence(IClient client)
        {
            if (((Area) client.Area).EvidenceModifications <= 0)
                return true;
            if (((Area) client.Area).EvidenceModifications == 1 && client.Area.IsClientCM(client))
                return true;
            client.SendOocMessage(
                $"{(((Area) client.Area).EvidenceModifications == 1 ? "Only CMs are" : "Noone is")} allowed to modify evidence in this area.");
            return false;
        }
    }
}