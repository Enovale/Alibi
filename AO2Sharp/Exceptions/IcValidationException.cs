using System;

namespace AO2Sharp.Exceptions
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