#nullable enable
using System;
using System.Net;

namespace AO2Sharp.Plugins.API
{
    public interface IClient
    {
        public IServer IServer { get; }

        public bool Connected { get; }
        public bool Authed { get; }
        public DateTime LastAlive { get; }
        public IPAddress IpAddress { get; }
        public string? HardwareId { get; }
        public IArea IArea { get; }
        public string? Position { get; set; }

        public string? Password { get; }
        public int? Character { get; set; }
        public string? CharacterName { get; }
        public string? OocName { get; }
        public string? LastSentMessage { get; set; }

        // Retarded pairing shit
        public int PairingWith { get; }
        public string? StoredEmote { get; }
        public int StoredOffset { get; }
        public bool StoredFlip { get; }

        public void ChangeArea(int index);
        public void Kick(string reason);
        public string GetBanReason();
        public void BanHwid(string reason, TimeSpan? expireDate);
        public void BanIp(string reason, TimeSpan? expireDate);

        public void Send(IAOPacket packet);
        public void SendOocMessage(string message, string? sender = null);
    }
}
