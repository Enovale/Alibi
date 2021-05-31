#nullable enable
using System;
using System.Net;
using Alibi.Plugins.API;
using Alibi.Protocol;

namespace Alibi.Plugins.Bot
{
    public class BotClient : Client, IClient
    {
        public BotClient(Server serverRef) : base(serverRef, new DummySession(), IPAddress.None)
        {
            LastAlive = DateTime.UtcNow;
            ((Server) ServerRef).OnPlayerJoined(this);
        }

        public void Receive(string message) => Receive(AOPacket.FromMessage(message));

        public void Receive(AOPacket packet)
        {
            LastAlive = DateTime.UtcNow;
            MessageHandler.HandleMessage(this, packet);
        }

        public override void BanHwid(string reason, TimeSpan? expireDate, IClient? banner)
        {
        }

        public override void BanIp(string reason, TimeSpan? expireDate, IClient? banner)
        {
        }

        public override void KickIfBanned()
        {
        }

        public override string GetBanReason() => string.Empty;
    }
}