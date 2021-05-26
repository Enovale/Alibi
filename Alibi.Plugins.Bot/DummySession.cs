using Alibi.Plugins.API;

namespace Alibi.Plugins.Bot
{
    public class DummySession : ISession
    {
        private readonly IClient _client;

        public DummySession(IClient client) => _client = client;
        
        public bool Disconnect()
        {
            if (!_client.Connected)
                return false;
            
            _client.Kick(string.Empty);
            return true;
        }

        public long Send(string text) => 0;

        public long Send(byte[] buffer, long length, long size) => 0;

        public bool SendAsync(string text) => true;

        public bool SendAsync(byte[] buffer, long length, long size) => true;
    }
}