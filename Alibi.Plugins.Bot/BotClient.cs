using System.Net;

namespace Alibi.Plugins.Bot
{
    public class BotClient : Client
    {
        public BotClient(Server serverRef) : base(serverRef, new DummySession(), IPAddress.None)
        {
        }
    }
}