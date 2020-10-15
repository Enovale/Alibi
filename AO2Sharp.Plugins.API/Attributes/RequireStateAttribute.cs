using System;
using System.Collections.Generic;
using System.Text;

namespace AO2Sharp.Plugins.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireStateAttribute : Attribute
    {
        public ClientState State { get; }

        public RequireStateAttribute(ClientState requiredState) => State = requiredState;
    }
}
