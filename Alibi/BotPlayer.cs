using System;
using System.Net;
using Alibi.Plugins.API;
using Alibi.Plugins.API.BotAPI;
using Alibi.Protocol;

namespace Alibi
{
    public class BotPlayer : Client, IBotPlayer
    {
        public int BotId { get; }
        
        public BotPlayer(Server serverRef) : base(serverRef, session, ip)
        {
        }

        public void Receive(AOPacket packet)
        {
            MessageHandler.HandleMessage(this, packet);
        }

        public void Speak(string message)
        {
            throw new NotImplementedException();
        }

        public void SpeakOoc(string message, string name)
        {
            throw new NotImplementedException();
        }
    }
}