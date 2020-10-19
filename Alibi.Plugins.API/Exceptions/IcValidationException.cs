using System;

namespace Alibi.Plugins.API.Exceptions
{
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