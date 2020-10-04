using System;
using System.Collections.Generic;
using System.Text;

namespace AO2Sharp
{
    internal static class MessageHandler
    {
        private static Dictionary<string, Handler> _handlers = new Dictionary<string, Handler>();

        private delegate void Handler(Server server, string message);

        public static void HandleMessage(Server server, string message)
        {

        }
    }
}
