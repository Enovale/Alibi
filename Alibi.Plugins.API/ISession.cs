namespace Alibi.Plugins.API
{
    public interface ISession
    {
        public bool Disconnect();
        public long Send(string text);
        public long Send(byte[] buffer, long length, long size);
        public bool SendAsync(string text);
        public bool SendAsync(byte[] buffer, long length, long size);
    }
}