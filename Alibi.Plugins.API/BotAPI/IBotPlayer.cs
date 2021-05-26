namespace Alibi.Plugins.API.BotAPI
{
    public interface IBotPlayer : IClient
    {
        public int BotId { get; }
        
        public void Receive(AOPacket packet);

        public void Speak(string message);

        public void SpeakOoc(string message, string name);
    }
}