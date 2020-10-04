using System;

namespace AO2Sharp.Protocol
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageHandlerAttribute : Attribute
    {
        public string MessageName { get; }

        public MessageHandlerAttribute(string messageName)
            => MessageName = messageName;
    }
}
