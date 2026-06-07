using System.Net;

namespace Alibi
{
    internal interface IAdvertiser
    {
        public void Start(string url);
        public void Stop();
    }
}