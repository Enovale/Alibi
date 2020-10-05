using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AO2Sharp.Helpers;

namespace AO2Sharp.Protocol
{
    internal static class Messages
    {
        [MessageHandler("HI")]
        internal static void HardwareId(Client client, AOPacket packet)
        {
            Console.WriteLine("Recieved hardware ID: " + packet.Objects[0]);
            client.HardwareId = packet.Objects[0];

            // Check if this player is hardware banned and kick them if so
            // Also probably check if the max players is reached and kick them
            client.Send(new AOPacket("ID", new [] {"111111", "AO2Sharp", Server.Version}));
        }

        [MessageHandler("ID")]
        internal static void SoftwareId(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("PN", new []
                {
                    client.Server.ConnectedPlayers.ToString(),
                    Server.ServerConfiguration.MaxPlayers.ToString()
                }
            ));
            client.Send(new AOPacket("FL", new []
            {
                "noencryption", "fastloading"
            }.Concat(Server.ServerConfiguration.FeatureList).ToArray()));
        }

        [MessageHandler("askchaa")]
        internal static void RequestResourceCounts(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("SI", new []
            {
                Server.CharactersList.Length.ToString(),
                client.Server.EvidenceList.Count.ToString(),
                Server.MusicList.Length.ToString()
            }));
        }

        [MessageHandler("RC")]
        internal static void RequestCharacters(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("SC", Server.CharactersList));
        }

        [MessageHandler("RE")]
        internal static void RequestEvidence(Client client, AOPacket packet)
        {
            string evidenceList = "";
            client.Server.EvidenceList.ForEach(e =>
            {
                evidenceList += e.ToPacket();
            });
            client.Send(new AOPacket("LE", evidenceList));
        }

        [MessageHandler("RM")]
        internal static void RequestMusic(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("SM", Server.MusicList));
        }

        [MessageHandler("RD")]
        internal static void Ready(Client client, AOPacket packet)
        {
            client.Send(new AOPacket("DONE"));
        }
    }
}
