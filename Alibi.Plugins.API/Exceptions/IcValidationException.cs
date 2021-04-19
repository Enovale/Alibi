using System;

namespace Alibi.Plugins.API.Exceptions
{
    /// <summary>
    /// Exception meaning an Ic message failed to be verified. This usually means
    /// improper data was sent, or the message contains something disallowed.
    /// </summary>
    public class IcValidationException : Exception
    {
        public IcValidationException()
        {
        }

        public IcValidationException(string message)
            : base(message)
        {
        }

        public IcValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}