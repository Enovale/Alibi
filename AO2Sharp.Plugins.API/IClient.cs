namespace AO2Sharp.Plugins.API
{
    public interface IClient
    {
        public void Send(IAOPacket packet);
        public void SendOocMessage(string message);
    }
}
