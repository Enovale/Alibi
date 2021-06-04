#nullable enable
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alibi.Plugins.API;
using Alibi.Protocol;

namespace Alibi.Plugins.Bot
{
    public class BotClient : Client
    {
        private CancellationTokenSource _tokenSource;
        
        public BotClient(Server serverRef) : base(serverRef, new DummySession(), IPAddress.None)
        {
            LastAlive = DateTime.UtcNow;
            ((Server) ServerRef).OnPlayerJoined(this);

            _tokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (true)
                {
                    if (_tokenSource.IsCancellationRequested)
                        return;

                    Receive(new AOPacket("CH"));
                    await Task.Delay(Math.Min(1, ServerRef.ServerConfiguration.TimeoutSeconds - 10) * 1000);
                }
            });
        }

        public override void OnSessionDisconnected()
        {
            _tokenSource.Cancel();
            base.OnSessionDisconnected();
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