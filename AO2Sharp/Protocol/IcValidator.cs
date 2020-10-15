using AO2Sharp.Helpers;
using AO2Sharp.Plugins.API;
using System;
using System.Collections.Generic;

namespace AO2Sharp.Protocol
{
    public static class IcValidator
    {
        internal static AOPacket ValidateIcPacket(IAOPacket packet, IClient client)
        {
            AOPacket invalid = new AOPacket("INVALID");

            if (client.Character == null || !client.Connected)
                return invalid;

            List<string> validatedObjects = new List<string>(packet.Objects.Length);

            // Validate that `chat` is either 0, 1, or "chat"
            if (packet.Objects[0].ToIntOrZero() == 1)
                validatedObjects.Add("1");
            else
                if (packet.Objects[0].Contains("chat"))
                validatedObjects.Add("chat");
            else
                validatedObjects.Add("0");

            validatedObjects.Add(packet.Objects[1]);

            // Make sure character isn't ini-swapping if it isn't allowed
            if (!client.Area.IniSwappingAllowed && packet.Objects[2].ToLower() != Server.CharactersList[(int)client.Character].ToLower())
            {
                client.SendOocMessage("Ini-swapping isn't allowed in this area.");
                return invalid;
            }
            validatedObjects.Add(packet.Objects[2]);

            // Nothing should break if this isn't an existing emote
            ((Client)client).StoredEmote = packet.Objects[3];
            validatedObjects.Add(packet.Objects[3]);

            // Make sure message is sanitized(eventually) and prevent double messages
            // TODO: Sanitization and zalgo cleaning
            string sentMessage = packet.Objects[4].Trim();
            if (!Server.ServerConfiguration.AllowDoublePostsIfDifferentAnim && sentMessage == client.LastSentMessage)
                return invalid;
            client.LastSentMessage = sentMessage;
            validatedObjects.Add(sentMessage);

            // Validated client side anyway
            validatedObjects.Add(packet.Objects[5]);

            // Will just not play if it isn't found
            validatedObjects.Add(packet.Objects[6]);

            // Surprise, surprise AO Devs are useless, basically this number can never be 4,
            // and yet some clients will send 4.
            string allowedEmotes = "012356";

            if (packet.Objects[7] == "4")
                packet.Objects[7] = "6";
            if (!allowedEmotes.Contains(packet.Objects[7]))
                return invalid;
            validatedObjects.Add(packet.Objects[7]);

            // Make sure theyre telling us the player that they actually are
            if (packet.Objects[8].ToIntOrZero() != client.Character)
                return invalid;
            validatedObjects.Add(packet.Objects[8]);

            // Delay, limit this to 3 seconds to prevent spam
            validatedObjects.Add(Math.Min(packet.Objects[9].ToIntOrZero(), 3000).ToString());

            // Make sure objection mod is 1 2 3 or 4
            string allowedObjectionMods = "01234";
            // Have to check seperately because the text metadata could contain a 4
            if (allowedObjectionMods.Contains(packet.Objects[10][0]))
                validatedObjects.Add(packet.Objects[10]);
            else
                return invalid;

            // Make sure evidence exists
            int moddedEvidenceId = Math.Max(0,
                Math.Min(client.Area.EvidenceList.Count, packet.Objects[11].ToIntOrZero() - 1));
            if (client.Area.EvidenceList.Count == 0)
                validatedObjects.Add("0");
            else if (client.Area.EvidenceList[moddedEvidenceId] != null)
                validatedObjects.Add(moddedEvidenceId.ToString());
            else
                return invalid;

            // Make sure flip is 1 or 0
            int flip = packet.Objects[12].ToIntOrZero();
            if (flip != 0 && flip != 1)
                return invalid;
            ((Client)client).StoredFlip = flip == 1;
            validatedObjects.Add(flip.ToString());

            // Make sure realization is 1 or 0 
            int realization = packet.Objects[13].ToIntOrZero();
            if (realization == 0 || realization == 1)
                validatedObjects.Add(realization.ToString());
            else
                return invalid;

            // Make sure chat color is valid
            string allowedColors = "012345678";
            if (!allowedColors.Contains(packet.Objects[14]))
                return invalid;
            validatedObjects.Add(packet.Objects[14]);

            // 2.6+ Attributes
            if (packet.Objects.Length > 15)
            {
                // Showname
                validatedObjects.Add(packet.Objects[15]);

                // First object is the charID, second is whether or not they're in front
                string[] pair = packet.Objects[16].Split("^");
                ((Client)client).PairingWith = pair[0].ToIntOrZero();

                bool paired = false;
                // Other's name, emote, offset, and flip
                string[] otherData = { "0", "0", "0", "0" };
                foreach (var otherClient in client.ServerRef.ClientsConnected)
                {
                    if (otherClient.PairingWith == client.Character &&
                        otherClient.Character == pair[0].ToIntOrZero() &&
                        otherClient.Area == client.Area)
                    {
                        otherData[0] = Server.CharactersList[pair[0].ToIntOrZero()];
                        otherData[1] = otherClient.StoredEmote;
                        otherData[2] = otherClient.StoredOffset.ToString();
                        otherData[3] = otherClient.StoredFlip ? "1" : "0";
                        paired = true;
                    }
                }

                string pairStr;
                if (!paired)
                    pairStr = "-1";
                else
                    pairStr = packet.Objects[16];
                validatedObjects.Add(pairStr);
                validatedObjects.Add(otherData[0]);
                validatedObjects.Add(otherData[1]);

                // Self offset
                ((Client)client).StoredOffset = Math.Max(-100, Math.Min(100, packet.Objects[17].ToIntOrZero()));
                validatedObjects.Add(client.StoredOffset.ToString());
                validatedObjects.Add(otherData[2]);
                validatedObjects.Add(otherData[3]);

                // Non interrupting pre-animation
                int nip = packet.Objects[18].ToIntOrZero();
                if (nip != 0 && nip != 1)
                    return invalid;
                validatedObjects.Add(nip.ToString());
            }

            // 2.8+ Attributes
            if (packet.Objects.Length > 19)
            {
                // Make sure sfx looping is 1 or 0
                int sfxLoop = packet.Objects[19].ToIntOrZero();
                if (sfxLoop != 0 && sfxLoop != 1)
                    return invalid;
                validatedObjects.Add(sfxLoop.ToString());

                // Make sure screenshake is 1 or 0
                int screenshake = packet.Objects[20].ToIntOrZero();
                if (screenshake != 0 && screenshake != 1)
                    return invalid;
                validatedObjects.Add(screenshake.ToString());

                // Frames to shake on; spec not even available so we can't validate this
                validatedObjects.Add(packet.Objects[21]);

                // Frames to realization; spec not available as well
                validatedObjects.Add(packet.Objects[22]);

                // Frames to sfx; spec not available as well
                validatedObjects.Add(packet.Objects[23]);

                // Make sure additive is 1 or 0
                int additive = packet.Objects[24].ToIntOrZero();
                if (additive != 0 && additive != 1)
                    return invalid;
                validatedObjects.Add(additive.ToString());

                // Overlay effect; spec not available as well
                validatedObjects.Add(packet.Objects[25]);
            }

            return new AOPacket("MS", validatedObjects.ToArray());
        }
    }
}