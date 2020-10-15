using System;

namespace AO2Sharp.Plugins.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHandlerAttribute : Attribute
    {
        public string Command { get; }
        public string ShortDesc { get; }
        public bool Override { get; }

        public CommandHandlerAttribute(string commandName, string desc = "", bool overrideHandler = false)
        {
            Command = commandName;
            ShortDesc = desc;
            Override = overrideHandler;
        }
    }
}
