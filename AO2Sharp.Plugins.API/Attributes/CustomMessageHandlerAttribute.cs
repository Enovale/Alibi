using System;

namespace AO2Sharp.Plugins.API.Attributes
{

    [AttributeUsage(AttributeTargets.Method)]
    public class CustomMessageHandlerAttribute : Attribute
    {
        public string MessageName { get; }

        public CustomMessageHandlerAttribute(string messageName)
            => MessageName = messageName;
    }
}