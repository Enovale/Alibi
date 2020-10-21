using System;

namespace Alibi.Plugins.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageHandlerAttribute : Attribute
    {
        public string MessageName { get; }
        public bool Override { get; }

        public MessageHandlerAttribute(string messageName, bool overrideHandler = false)
        {
            MessageName = messageName;
            Override = overrideHandler;
        }
    }
}