using System;

namespace Alibi.Plugins.API.Attributes
{
    /// <summary>
    /// Specifies a command to register in the server
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHandlerAttribute : Attribute
    {
        public string Command { get; }
        public string ShortDesc { get; }
        public bool Override { get; }

        /// <param name="commandName">The name of the command, the user will type a / followed by this name.</param>
        /// <param name="desc">Optional description of the command to be shown in /help</param>
        /// <param name="overrideHandler">Should this command overwrite a registered server command?</param>
        public CommandHandlerAttribute(string commandName, string desc = "", bool overrideHandler = false)
        {
            Command = commandName;
            ShortDesc = desc;
            Override = overrideHandler;
        }
    }
}