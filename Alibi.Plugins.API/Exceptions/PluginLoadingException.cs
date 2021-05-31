using System;

namespace Alibi.Plugins.API.Exceptions
{
    public class PluginLoadingException : PluginException
    {
        public PluginLoadingException()
        {
        }

        public PluginLoadingException(string message) : base(message)
        {
        }

        public PluginLoadingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}