using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alibi.Commands;
using Alibi.Helpers;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;
using Alibi.Plugins.API.Exceptions;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedParameter.Global
#pragma warning disable IDE0060 // Remove unused parameter

namespace Alibi.Protocol
{
    internal static class Messages
    {
        [MessageHandler("HI")]
        [RequireState(ClientState.NewClient)]
        internal static void HardwareId(IClient client, AOPacket packet)
        {
            if (packet.Objects.Length <= 0)
                return;

            if (client.HardwareId != null)
                return;

            ((Client) client).HardwareId = packet.Objects[0];
            client.ServerRef.Database.AddUser(client.HardwareId, client.IpAddress.ToString());
            client.KickIfBanned();

            if (!IPAddress.IsLoopback(client.IpAddress) &&
                client.ServerRef.ClientsConnected.Count(c => client.HardwareId == c.HardwareId)
                > client.ServerRef.ServerConfiguration.MaxMultiClients)
            {
                client.Send(new AOPacket("BD",
                    "Not a real ban: Can't have more than " +
                    $"{client.ServerRef.ServerConfiguration.MaxMultiClients} clients on a single computer at a time."));
                Task.Delay(500);
                client.Session.Disconnect();
                return;
            }

            client.CurrentState = ClientState.PostHandshake;

            client.Send(new AOPacket("ID", "111111", "Alibi", client.ServerRef.Version));
        }

        [MessageHandler("ID")]
        [RequireState(ClientState.PostHandshake)]
        internal static void SoftwareId(IClient client, AOPacket packet)
        {
            client.CurrentState = ClientState.Identified;
            client.Send(new AOPacket("PN", client.ServerRef.ConnectedPlayers.ToString(),
                client.ServerRef.ServerConfiguration.MaxPlayers.ToString()));
            client.Send(new AOPacket("FL", new[]
            {
                "noencryption", "fastloading"
            }.Concat(client.ServerRef.ServerConfiguration.FeatureList).ToArray()));
        }

        // Slow loading, ugh.
        [MessageHandler("askchaa")]
        [RequireState(ClientState.Identified)]
        internal static void RequestResourceCounts(IClient client, AOPacket packet)
        {
            client.Send(new AOPacket("SI", client.ServerRef.CharactersList.Length.ToString(),
                client.ServerRef.Areas.First().EvidenceList.Count.ToString(),
                (client.ServerRef.Areas.Length + client.ServerRef.MusicList.Length).ToString()));
        }

        [MessageHandler("askchar2")]
        [RequireState(ClientState.Identified)]
        internal static void RequestCharactersSlow(IClient client, AOPacket packet)
        {
            RequestCharacterPageSlow(client, new AOPacket("AN", "0"));
        }

        [MessageHandler("AN")]
        [RequireState(ClientState.Identified)]
        internal static void RequestCharacterPageSlow(IClient client, AOPacket packet)
        {
            if (int.TryParse(packet.Objects[0], out var page))
            {
                // According to current client
                var pageSize = 10 * 9;
                var pageOffset = pageSize * page;

                if (pageOffset >= client.ServerRef.CharactersList.Length)
                {
                    RequestEvidenceSlow(client, new AOPacket("EI", "1"));
                    return;
                }

                var chars = new[] {page.ToString()}
                    .Concat(client.ServerRef.CharactersList
                        .Skip(pageOffset)
                        .Take(pageSize));

                var finalPacket = new AOPacket("CI", chars.ToArray());
                client.Send(finalPacket);
            }
        }

        [MessageHandler("AE")]
        [RequireState(ClientState.Identified)]
        internal static void RequestEvidenceSlow(IClient client, AOPacket packet)
        {
            if (int.TryParse(packet.Objects[0], out var page))
            {
                // According to current client
                var pageSize = 6 * 2;
                var pageOffset = pageSize * page;
                var area = client.ServerRef.Areas.First();

                if (pageOffset >= area.EvidenceList.Count)
                {
                    RequestMusicSlow(client, new AOPacket("EM", "0"));
                    return;
                }

                var evidence = new[] {page.ToString()}.Concat(area.EvidenceList.Skip(pageOffset)
                    .Take(pageSize).Select(e => e.ToPacket()));

                var finalPacket = new AOPacket("EI", evidence.ToArray());
                client.Send(finalPacket);
            }
        }

        [MessageHandler("EM")]
        [RequireState(ClientState.Identified)]
        internal static void RequestMusicSlow(IClient client, AOPacket packet)
        {
            if (int.TryParse(packet.Objects[0], out var index))
            {
                index = Math.Max(0, index);

                var musicList = client.ServerRef.MusicList.ToList();
                var categories = musicList.Split(m => m.Contains(".")).ToArray();

                if (index >= categories.Length)
                {
                    Ready(client, new AOPacket("RD"));
                    return;
                }

                var music = new[] {index.ToString()}.Concat(categories[index]);

                client.Send(new AOPacket("EM", music.ToArray()));
            }
        }

        [MessageHandler("RC")]
        [RequireState(ClientState.Identified)]
        internal static void RequestCharacters(IClient client, AOPacket packet)
        {
            client.Send(new AOPacket("SC", client.ServerRef.CharactersList));
        }

        [MessageHandler("RE")]
        [RequireState(ClientState.InArea)]
        internal static void RequestEvidence(IClient client, AOPacket packet)
        {
            var evidenceList = new string[client.Area!.EvidenceList.Count];
            for (var i = 0; i < client.Area.EvidenceList.Count; i++)
                evidenceList[i] = client.Area.EvidenceList[i].ToPacket();

            client.Send(new AOPacket("LE", evidenceList));
        }

        [MessageHandler("RM")]
        [RequireState(ClientState.Identified)]
        internal static void RequestMusic(IClient client, AOPacket packet)
        {
            client.Send(new AOPacket("SM", client.ServerRef.AreaNames.Concat(client.ServerRef.MusicList).ToArray()));
        }

        [MessageHandler("RD")]
        [RequireState(ClientState.Identified)]
        internal static void Ready(IClient client, AOPacket packet)
        {
            // This client didn't send us a hwid, need to kick
            if (string.IsNullOrWhiteSpace(client.HardwareId))
            {
                ((Client) client).Session.Disconnect();
                return;
            }

            if (client.Connected)
                return;

            ((Client) client).Connected = true;
            client.CurrentState = ClientState.InArea;
            client.ServerRef.ConnectedPlayers++;
            ((Server) client.ServerRef).OnPlayerConnected(client);
            client.ChangeArea(Array.FindIndex(client.ServerRef.Areas, a => a.Locked == "FREE"));
            client.SendOocMessage(client.ServerRef.ServerConfiguration.Motd);

            Server.Logger.Log(LogSeverity.Info, $"[{client.IpAddress}] Just joined the Server.");
        }

        [MessageHandler("CH")]
        internal static void KeepAlive(IClient client, AOPacket packet)
        {
            client.Send(new AOPacket("CHECK"));
        }

        [MessageHandler("PE")]
        [RequireState(ClientState.InArea)]
        internal static void AddEvidence(IClient client, AOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            client.Area!.EvidenceList.Add(new Evidence(packet.Objects[0], packet.Objects[1], packet.Objects[2]));
            RequestEvidence(client, packet);
        }

        [MessageHandler("DE")]
        [RequireState(ClientState.InArea)]
        internal static void RemoveEvidence(IClient client, AOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            if (int.TryParse(packet.Objects[0], out var id))
                client.Area!.EvidenceList.RemoveAt(id);
            RequestEvidence(client, packet);
        }

        [MessageHandler("EE")]
        [RequireState(ClientState.InArea)]
        internal static void EditEvidence(IClient client, AOPacket packet)
        {
            if (!CanModifyEvidence(client))
                return;
            if (int.TryParse(packet.Objects[0], out var id))
            {
                id = Math.Max(0, Math.Min(client.Area!.EvidenceList.Count, id));

                client.Area.EvidenceList[id] = new Evidence(packet.Objects[1], packet.Objects[2], packet.Objects[3]);
            }

            RequestEvidence(client, packet);
        }

        [MessageHandler("PW")]
        [RequireState(ClientState.InArea)]
        internal static void CharacterPassword(IClient client, AOPacket packet)
        {
            ((Client) client).Password = packet.Objects.First();
        }

        [MessageHandler("CC")]
        [RequireState(ClientState.InArea)]
        internal static void ChangeCharacter(IClient client, AOPacket packet)
        {
            if (int.TryParse(packet.Objects[1], out var charId))
            {
                if (client.Character != null)
                    client.Area!.TakenCharacters[(int) client.Character] = false;

                if (charId > client.ServerRef.CharactersList.Length)
                    return;
                if (charId < 0)
                    return;

                var charToTake = client.ServerRef.CharactersList[charId];
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
        internal static void ChangeMusic(IClient client, AOPacket packet)
        {
            if (packet.Objects.Length <= 0)
                return;
            var song = packet.Objects[0];

            for (var i = 0; i < client.ServerRef.AreaNames.Length; i++)
            {
                if (song == client.ServerRef.AreaNames[i])
                {
                    client.ChangeArea(i);
                    return;
                }
            }

            if (!((Server) client.ServerRef).OnMusicChange(client, ref song))
                return;

            foreach (var m in client.ServerRef.MusicList)
                if (song == m)
                    client.Area!.Broadcast(packet);
        }

        [MessageHandler("MS")]
        [RequireState(ClientState.InArea)]
        internal static void IcMessage(IClient client, AOPacket packet)
        {
            if (client.Muted)
                return;
            try
            {
                var validPacket = IcValidator.ValidateIcPacket(packet, client);

                if (!((Server) client.ServerRef).OnIcMessage(client, ref validPacket.Objects[4])) // 4 is the message
                    return;

                if (validPacket.Objects[4].Length > client.ServerRef.ServerConfiguration.MaxMessageSize)
                    throw new IcValidationException("Message was too long.");

                client.Area!.Broadcast(validPacket);
                Server.Logger.IcMessageLog(packet.Objects[4], client.Area, client);
            }
            catch (IcValidationException e)
            {
                client.SendOocMessage(e.Message);
            }
        }

        [MessageHandler("CT")]
        [RequireState(ClientState.InArea)]
        internal static void OocMessage(IClient client, AOPacket packet)
        {
            if (client.Muted)
                return;
            if (packet.Objects.Length < 2)
                return;

            var message = packet.Objects[1];

            ((Client) client).OocName = packet.Objects[0];
            if (message.StartsWith("/"))
            {
                var command = message.Substring(1).Split(" ").First().Trim();
                var arguments = new List<string>(message.Split(" ", StringSplitOptions.RemoveEmptyEntries).Skip(1));

                CommandHandler.HandleCommand(client, command, arguments.ToArray());
                return;
            }

            if (!((Server) client.ServerRef).OnOocMessage(client, ref message))
                return;
            packet.Objects[1] = message;

            if (message.Length > client.ServerRef.ServerConfiguration.MaxMessageSize)
            {
                client.SendOocMessage("Message was too long.");
                return;
            }

            client.Area!.Broadcast(packet);
            Server.Logger.OocMessageLog(message, client.Area, packet.Objects[0]);
        }

        [MessageHandler("SETCASE")]
        [RequireState(ClientState.InArea)]
        internal static void SetCasePreferences(IClient client, AOPacket packet)
        {
            if (packet.Objects.Length < 7)
                return;

            client.CasingPreferences = CasingFlags.None;
            if (packet.Objects[1] == "1") // Skip caselist because who cares
                client.CasingPreferences |= CasingFlags.CaseManager;
            if (packet.Objects[2] == "1")
                client.CasingPreferences |= CasingFlags.Defense;
            if (packet.Objects[3] == "1")
                client.CasingPreferences |= CasingFlags.Prosecutor;
            if (packet.Objects[4] == "1")
                client.CasingPreferences |= CasingFlags.Judge;
            if (packet.Objects[5] == "1")
                client.CasingPreferences |= CasingFlags.Jury;
            if (packet.Objects[6] == "1")
                client.CasingPreferences |= CasingFlags.Stenographer;
        }

        [MessageHandler("CASEA")]
        [RequireState(ClientState.InArea)]
        internal static void CaseAlert(IClient client, AOPacket packet)
        {
            if (packet.Objects.Length < 6)
                return;

            packet.Objects[0] =
                AOPacket.EncodeToAOPacket(
                    $"A case called \"{packet.Objects[0]}\" needs a role you want to fill in {client.Area!.Name}.");

            var flags = CasingFlags.None;
            if (packet.Objects[1] == "1") // Skip caselist because who cares, also CM isn't included ig
                flags |= CasingFlags.Defense;
            if (packet.Objects[2] == "1")
                flags |= CasingFlags.Prosecutor;
            if (packet.Objects[3] == "1")
                flags |= CasingFlags.Judge;
            if (packet.Objects[4] == "1")
                flags |= CasingFlags.Jury;
            if (packet.Objects[5] == "1")
                flags |= CasingFlags.Stenographer;

            foreach (var neededPlayer in client.ServerRef.ClientsConnected.Where(
                c => c.CasingPreferences.HasFlag(flags)))
                neededPlayer.Send(packet);
        }

        [MessageHandler("HP")]
        [RequireState(ClientState.InArea)]
        internal static void UpdateHealthBar(IClient client, AOPacket packet)
        {
            if (!client.Position!.ToLower().StartsWith("jud"))
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
        internal static void JudgeAnimation(IClient client, AOPacket packet)
        {
            if ((client.Position ?? "").ToLower().StartsWith("jud"))
                client.Area!.Broadcast(packet);
        }

        [MessageHandler("ZZ")]
        [RequireState(ClientState.InArea)]
        internal static void ModCall(IClient client, AOPacket packet)
        {
            if (!((Server) client.ServerRef).OnModCall(client, packet))
                return;

            Server.Logger.Log(LogSeverity.Special, $"[{client.Area!.Name}][{client.IpAddress}] " +
                                                   $"{client.CharacterName} called for mod with reasoning: {packet.Objects[0]}");
            var packetToSend = new AOPacket(packet.Type, "Someone has called mod in the " +
                                                         $"{client.Area.Name} area, with reasoning: {packet.Objects[0]}");

            foreach (var c in new Queue<IClient>(client.ServerRef.ClientsConnected))
                if (c.Auth >= AuthType.MODERATOR)
                    c.Send(packetToSend);
        }

        private static bool CanModifyEvidence(IClient client)
        {
            if (((Area) client.Area)!.EvidenceModifications <= 0)
                return true;
            if (((Area) client.Area).EvidenceModifications == 1 && client.Area.IsClientCM(client))
                return true;
            client.SendOocMessage(
                $"{(((Area) client.Area).EvidenceModifications == 1 ? "Only CMs are" : "Noone is")} allowed to modify evidence in this area.");
            return false;
        }
    }
}