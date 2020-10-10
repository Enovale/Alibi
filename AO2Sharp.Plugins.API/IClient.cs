using System.Net;

namespace AO2Sharp.Plugins.API
{
    public interface IClient
    {
        public IPAddress IpAddress { get; }
        public string HardwareId { get; }
        public IArea IArea { get; }

        public void Send(IAOPacket packet);
        public void SendOocMessage(string message);
    }
}
