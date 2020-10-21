using System;

namespace Alibi.Plugins.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireStateAttribute : Attribute
    {
        public ClientState State { get; }
        public bool Kick { get; }

        public RequireStateAttribute(ClientState requiredState, bool kickIfFalse = true)
        {
            State = requiredState;
            Kick = kickIfFalse;
        }
    }
}