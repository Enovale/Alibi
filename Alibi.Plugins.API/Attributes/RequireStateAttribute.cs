using System;

namespace Alibi.Plugins.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireStateAttribute : Attribute
    {
        public ClientState State { get; }

        public RequireStateAttribute(ClientState requiredState) => State = requiredState;
    }
}
