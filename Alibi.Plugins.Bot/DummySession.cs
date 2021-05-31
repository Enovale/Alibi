using Alibi.Plugins.API;

namespace Alibi.Plugins.Bot
{
    public class DummySession : ISession
    {
        public bool Disconnect() => true;

        public long Send(string text) => 0;

        public long Send(byte[] buffer, long length, long size) => 0;

        public bool SendAsync(string text) => true;

        public bool SendAsync(byte[] buffer, long length, long size) => true;
    }
}