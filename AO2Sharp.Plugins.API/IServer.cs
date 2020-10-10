using System.Collections.Generic;

namespace AO2Sharp.Plugins.API
{
    public interface IServer
    {
        public List<IClient> Clients { get; }
        public bool VerboseLogs { get; }
    }
}
