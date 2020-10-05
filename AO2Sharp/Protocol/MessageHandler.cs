using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AO2Sharp.Helpers;

namespace AO2Sharp.Protocol
{
    internal static class MessageHandler
    {
        private static readonly Dictionary<string, Handler> _handlers = new Dictionary<string, Handler>();

        internal delegate void Handler(Client client, AOPacket packet);

        static MessageHandler()
        {
            AddHandlers();
        }

        public static void HandleMessage(Client client, AOPacket packet)
        {
            if (_handlers.ContainsKey(packet.Type))
            {
                _handlers[packet.Type](client, packet);
            }
            else
            {
                Console.WriteLine($"Dispatcher: Unknown client message '{packet.Type}'");
            }
        }

        public static void RegisterMessageHandler(string messageName, Handler handler)
        {
            if (!_handlers.ContainsKey(messageName))
                _handlers.Add(messageName, handler);

            _handlers[messageName] = handler;;
        }

        public static void AddHandlers()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetRuntimeMethods();
                foreach (var method in methods)
                {
                    if (method.IsStatic)
                    {
                        var attr = method.GetCustomAttribute<MessageHandlerAttribute>();

                        if (attr != null)
                            RegisterMessageHandler(attr.MessageName, (Handler)method.CreateDelegate(typeof(Handler)));
                    }
                }
            }
        }
    }
}
