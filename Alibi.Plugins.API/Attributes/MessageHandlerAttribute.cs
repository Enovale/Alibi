using System;

namespace Alibi.Plugins.API.Attributes
{
    /// <summary>
    /// Specifies a packet handler to register in the server
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageHandlerAttribute : Attribute
    {
        public string MessageName { get; }
        public bool Override { get; }

        /// <param name="messageName">The packet ID to register</param>
        /// <param name="overrideHandler">Should this handler overwrite a default server packet handler?</param>
        public MessageHandlerAttribute(string messageName, bool overrideHandler = false)
        {
            MessageName = messageName;
            Override = overrideHandler;
        }
    }
}