using AO2Sharp.Plugins.API;
using AO2Sharp.Plugins.API.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AO2Sharp.Protocol
{
    internal static class MessageHandler
    {
        private static readonly Dictionary<string, Action<IClient, IAOPacket>> _handlers = new Dictionary<string, Action<IClient, IAOPacket>>();

        static MessageHandler()
        {
            AddHandlers();
        }

        public static void HandleMessage(IClient client, IAOPacket packet)
        {
            if (_handlers.ContainsKey(packet.Type))
            {
                _handlers[packet.Type](client, packet);
            }
            else
            {
                Server.Logger.Log(LogSeverity.Warning, $"Unknown client message: '{packet.Type}'", true);
            }
        }

        public static void RegisterMessageHandler(string messageName, Action<IClient, IAOPacket> handler, bool overrideHandler = false)
        {
            if (!overrideHandler && _handlers.ContainsKey(messageName))
                return;

            _handlers[messageName] = handler;
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
                            RegisterMessageHandler(attr.MessageName,
                                (c, p) => { method.Invoke(null, new object[] { c, p }); });
                    }
                }
            }
        }

        public static void AddCustomHandler(Plugin plugin)
        {
            var types = plugin.Assembly.GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetRuntimeMethods();
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<MessageHandlerAttribute>();

                    if (attr != null)
                        RegisterMessageHandler(attr.MessageName, (c, p) =>
                        {
                            method.Invoke(plugin, new object[] { c, p });
                        }, true);
                }
            }
        }
    }
}