using System;

namespace Alibi.Plugins.API.Exceptions
{
    /// <summary>
    /// Exception meaning a command failed to execute in the server.
    /// </summary>
    public class CommandException : Exception
    {
        public CommandException()
        {
        }

        public CommandException(string message)
            : base(message)
        {
        }

        public CommandException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}