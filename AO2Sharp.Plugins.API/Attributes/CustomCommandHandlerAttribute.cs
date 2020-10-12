using System;
using System.Collections.Generic;
using System.Text;

namespace AO2Sharp.Plugins.API.Attributes
{
    
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomCommandHandlerAttribute : Attribute
    {
        public string Command { get; }
        public string ShortDesc { get; }

        public CustomCommandHandlerAttribute(string commandName, string desc = "")
        {
            Command = commandName;
            ShortDesc = desc;
        }
    }
}
