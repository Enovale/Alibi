using System;

namespace AO2Sharp.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHandlerAttribute : Attribute
    {
        public string Command { get; }
        public string ShortDesc { get; }

        public CommandHandlerAttribute(string commandName, string desc = "")
        {
            Command = commandName;
            ShortDesc = desc;
        }
    }
}
