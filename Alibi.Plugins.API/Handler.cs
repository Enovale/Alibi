#nullable enable
using System.Reflection;

namespace Alibi.Plugins.API
{
    public class Handler
    {
        public MethodInfo Method { get; }
        public Plugin? Target { get; }

        public Handler(MethodInfo method, Plugin? target)
        {
            Method = method;
            Target = target;
        }
    }
}