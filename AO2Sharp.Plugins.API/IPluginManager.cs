namespace AO2Sharp.Plugins.API
{
    public interface IPluginManager
    {
        public delegate void CustomCommandHandler(IClient client, string[] args);
        public delegate void CustomMessageHandler(IClient client, IAOPacket packet);

        public bool IsPluginLoaded(string id);
        public dynamic RequestPluginInstance(string id);
        public string GetConfigFolder(string id);
        public void Log(int severity, string message, bool verbose);
    }
}
