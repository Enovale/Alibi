using AO2Sharp.Commands;
using AO2Sharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AO2Sharp.Protocol
{
    internal static class Messages
    {
        [MessageHandler("HI")]
        internal static void HardwareId(Client client, AOPacket packet)
        {
            client.HardwareId = packet.Objects[0];
            client.Server.AddUser(client);
            client.KickIfBanned();

            // Check if this player is hardware banned and kick them if so
            // Also probably check if the max players is reached and kick them
            client.Send(new AOPacket("ID", "111111", "AO2Sharp", Server.Version));
        }

        [MessageHandler("ID")]
        internal static void SoftwareId(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("PN", client.Server.ConnectedPlayers.ToString(), Server.ServerConfiguration.MaxPlayers.ToString()));
            client.Send(new AOPacket("FL", new[]
            {
                "noencryption", "fastloading"
            }.Concat(Server.ServerConfiguration.FeatureList).ToArray()));
        }

        // Slow loading, ugh.
        [MessageHandler("askchar2")]
        internal static void RequestCharactersSlow(Client client, AOPacket packet)
        {
            RequestCharacterPageSlow(client, new AOPacket("AN", "0"));
        }

        [MessageHandler("AN")]
        internal static void RequestCharacterPageSlow(Client client, AOPacket packet)
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
        internal static void RequestEvidenceSlow(Client client, AOPacket packet)
        {
            if (int.TryParse(packet.Objects[0], out int index))
            {
                index = Math.Max(0, Math.Min(client.Area.EvidenceList.Count, index));

                client.Send(new AOPacket("EI", index.ToString(), client.Area.EvidenceList[index].ToPacket()));
            }
        }

        [MessageHandler("AM")]
        internal static void RequestMusicSlow(Client client, AOPacket packet)
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

                client.Send(new AOPacket("EM", new[] { index.ToString() }.Concatenate(categories[index].ToArray())));
            }
        }

        [MessageHandler("askchaa")]
        internal static void RequestResourceCounts(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("SI", Server.CharactersList.Length.ToString(), client.Server.Areas.First().EvidenceList.Count.ToString(), (client.Server.Areas.Length + Server.MusicList.Length).ToString()));
        }

        [MessageHandler("RC")]
        internal static void RequestCharacters(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("SC", Server.CharactersList));
        }

        [MessageHandler("RE")]
        internal static void RequestEvidence(Client client, AOPacket packet)
        {
            string[] evidenceList = new string[client.Area.EvidenceList.Count];
            for (var i = 0; i < client.Area.EvidenceList.Count; i++)
            {
                evidenceList[i] = client.Area.EvidenceList[i].ToPacket();
            }
            client.Area.Broadcast(new AOPacket("LE", evidenceList));
        }

        [MessageHandler("PE")]
        internal static void AddEvidence(Client client, AOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            client.Area.EvidenceList.Add(new Evidence(packet.Objects[0], packet.Objects[1], packet.Objects[2]));
            RequestEvidence(client, packet);
        }

        [MessageHandler("DE")]
        internal static void RemoveEvidence(Client client, AOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            if (int.TryParse(packet.Objects[0], out int id))
                client.Area.EvidenceList.RemoveAt(id);
            RequestEvidence(client, packet);
        }

        [MessageHandler("EE")]
        internal static void EditEvidence(Client client, AOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            if (int.TryParse(packet.Objects[0], out int id))
            {
                id = Math.Max(0, Math.Min(client.Area.EvidenceList.Count, id));

                client.Area.EvidenceList[id] = new Evidence(packet.Objects[1], packet.Objects[2], packet.Objects[3]);
            }
            RequestEvidence(client, packet);
        }

        [MessageHandler("RM")]
        internal static void RequestMusic(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("SM", client.Server.AreaNames.Concat(Server.MusicList).ToArray()));
        }

        [MessageHandler("RD")]
        internal static void Ready(Client client, AOPacket packet)
        {
            // This client didn't send us a hwid, need to kick
            if (string.IsNullOrWhiteSpace(client.HardwareId))
            {
                client.Session.Disconnect();
                return;
            }

            if (client.Connected)
                return;

            client.Area = client.Server.Areas.First();
            client.Connected = true;
            client.Server.ConnectedPlayers++;
            client.Area.PlayerCount++;
            client.Area.FullUpdate(client);
            // Tell all the other clients that someone has joined
            client.Area.AreaUpdate(AreaUpdateType.PlayerCount);

            client.Send(new AOPacket("HP", "1", client.Area.DefendantHp.ToString()));
            client.Send(new AOPacket("HP", "2", client.Area.ProsecutorHp.ToString()));
            client.Send(new AOPacket("FA", client.Server.AreaNames));
            client.Send(new AOPacket("BN", client.Area.Background));
            // TODO: Determine if this is needed because it's retarded
            // WebAO doesn't use it so im gonna assume its not
            //client.Send(new AOPacket("OPPASS", Server.ServerConfiguration.ModPassword));
            client.Send(new AOPacket("DONE"));
            client.SendOocMessage(Server.ServerConfiguration.MOTD);

            Server.Logger.Log(LogSeverity.Info, $"[{client.IpAddress}] Just joined the server.");
        }

        [MessageHandler("CH")]
        internal static void KeepAlive(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("CHECK"));
        }

        [MessageHandler("PW")]
        internal static void CharacterPassword(Client client, AOPacket packet)
        {
            client.Password = packet.Objects.First();
        }

        [MessageHandler("CC")]
        internal static void ChangeCharacter(Client client, AOPacket packet)
        {
            int charId;
            if (int.TryParse(packet.Objects[1], out charId))
            {
                if (client.Character != null)
                    client.Area.TakenCharacters[(int)client.Character] = false;

                if (charId > Server.CharactersList.Length)
                    return;
                if (charId < 0)
                    return;

                string charToTake = Server.CharactersList[charId];
                if (client.Area.TakenCharacters[charId] || charToTake == "")
                    return;

                client.Area.TakenCharacters[charId] = true;
                client.Character = charId;
                client.Area.UpdateTakenCharacters();

                client.Send(new AOPacket("PV", "111111", "CID", charId.ToString()));
            }
        }

        [MessageHandler("MC")]
        internal static void ChangeMusic(Client client, AOPacket packet)
        {
            string song = packet.Objects[0];

            foreach (var m in Server.MusicList)
            {
                if (song == m)
                {
                    client.Area.Broadcast(packet);
                    return;
                }
            }

            for (var i = 0; i < client.Server.AreaNames.Length; i++)
            {
                if (song == client.Server.AreaNames[i])
                    client.ChangeArea(i);
            }
        }

        [MessageHandler("MS")]
        internal static void IcMessage(Client client, AOPacket packet)
        {
            AOPacket validPacket = IcValidator.ValidateIcPacket(packet, client);
            if (validPacket.Type == "INVALID")
                return;

            client.Area.Broadcast(validPacket);
            Server.Logger.IcMessageLog(packet.Objects[4], client.Area, client);
        }

        [MessageHandler("CT")]
        internal static void OocMessage(Client client, AOPacket packet)
        {
            // TODO: Sanitization and cleaning (especially Zalgo)
            // maybe put this into anti-spam plugin
            string message = packet.Objects[1];
            client.OocName = packet.Objects[0];
            if (message.StartsWith("/"))
            {
                string command = message.Substring(1).Split(" ").First().Trim();
                List<string> arguments = new List<string>(message.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                arguments.RemoveAt(0);

                CommandHandler.HandleCommand(client, command, arguments.ToArray());
                return;
            }

            client.Area!.Broadcast(packet);
            Server.Logger.OocMessageLog(message, client.Area, packet.Objects[0]);
        }

        [MessageHandler("HP")]
        internal static void UpdateHealthBar(Client client, AOPacket packet)
        {
            if (!client.Position.ToLower().StartsWith("jud"))
                return;
            int hp;
            if (int.TryParse(packet.Objects[1], out hp))
            {
                if (packet.Objects[0] == "1")
                    client.Area.DefendantHp = Math.Max(0, Math.Min(hp, 10));
                else if (packet.Objects[0] == "2")
                    client.Area.ProsecutorHp = Math.Max(0, Math.Min(hp, 10));

                client.Area.Broadcast(packet);
            }
        }

        [MessageHandler("RT")]
        internal static void JudgeAnimation(Client client, AOPacket packet)
        {
            if ((client.Position ?? "").ToLower().StartsWith("jud"))
                client.Area.Broadcast(packet);
        }

        [MessageHandler("ZZ")]
        internal static void ModCall(Client client, AOPacket packet)
        {
            Server.Logger.Log(LogSeverity.Special, $"[{client.Area.Name}][{client.IpAddress}] " +
                                                   $"{client.CharacterName} called for mod with reasoning: {packet.Objects[0]}");
            var packetToSend = new AOPacket(packet.Type, "Someone has called mod in the " +
                                                         $"{client.Area.Name} area, with reasoning: {packet.Objects[0]}");

            foreach (var c in new Queue<Client>(client.Server.ClientsConnected))
            {
                if (c.Authed)
                    c.Send(packetToSend);
            }

            client.Server.OnModCall(client, packet);
        }

        [MessageHandler("WSIP")]
        internal static void UpdateWebsocketIp(Client client, AOPacket packet)
        {
            IPAddress ip = IPAddress.Parse(packet.Objects[0]);
            if (IPAddress.IsLoopback(client.IpAddress) && !IPAddress.IsLoopback(ip))
            {
                Server.Database.ChangeIp(client.HardwareId, client.IpAddress.ToString(), ip.ToString());
                client.IpAddress = ip;
                client.KickIfBanned();
            }
        }

        private static bool CanModifyEvidence(Client client)
        {
            if (client.Area.EvidenceModifications >= 0)
                return true;
            if (client.Area.EvidenceModifications == 1 && client.Area.IsClientCM(client))
                return true;
            client.SendOocMessage(
                $"{(client.Area.EvidenceModifications == 1 ? "Only CMs are" : "Noone is")} allowed to modify evidence in this area.");
            return false;
        }
    }
}
