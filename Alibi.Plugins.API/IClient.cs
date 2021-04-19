#nullable enable
using System;
using System.Net;

namespace Alibi.Plugins.API
{
    public interface IClient
    {
        public IServer ServerRef { get; }

        public bool Connected { get; }
        public int Auth { get; }
        public DateTime LastAlive { get; }
        public IPAddress IpAddress { get; }
        public string? HardwareId { get; }
        public IArea? Area { get; }
        public string? Position { get; set; }
        public ClientState CurrentState { get; set; }

        public string? Password { get; }
        public int? Character { get; set; }
        public string? CharacterName { get; }
        public string? OocName { get; }
        public string? LastSentMessage { get; set; }
        public bool Muted { get; set; }

        // Retarded pairing shit
        public int PairingWith { get; }
        public string? StoredEmote { get; }
        public int StoredOffset { get; }
        public bool StoredFlip { get; }

        public void ChangeArea(int index);
        public void KickIfBanned();
        public void Kick(string reason);
        public string GetBanReason();
        public void BanHwid(string reason, TimeSpan? expireDate, IClient banner);
        public void BanIp(string reason, TimeSpan? expireDate, IClient banner);

        public void Send(IAOPacket packet);
        public void SendOocMessage(string message, string? sender = null);
    }
}