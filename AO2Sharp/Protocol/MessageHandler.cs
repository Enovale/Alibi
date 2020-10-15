using System;
using AO2Sharp.Helpers;
using System.Collections.Generic;
using System.Reflection;
using AO2Sharp.Plugins.API;
using AO2Sharp.Plugins.API.Attributes;

namespace AO2Sharp.Protocol
{
    internal static class MessageHandler
    {
        private static readonly Dictionary<string, Handler> _handlers = new Dictionary<string, Handler>();
        private static readonly Dictionary<string, Action<IClient, IAOPacket>> _customHandlers =
            new Dictionary<string, Action<IClient, IAOPacket>>();

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
            else if (_customHandlers.ContainsKey(packet.Type))
            {
                _customHandlers[packet.Type](client, packet);
            }
            else
            {
                Server.Logger.Log(LogSeverity.Warning, $"Unknown client message: '{packet.Type}'", true);
            }
        }

        public static void RegisterMessageHandler(string messageName, Handler handler) => _handlers[messageName] = handler;

        public static void RegisterCustomMessageHandler(string messageName, Action<IClient, IAOPacket> handler, bool overrideHandler = false)
        {
            if (overrideHandler && _handlers.ContainsKey(messageName))
                _handlers.Remove(messageName);
            else if (!overrideHandler && _handlers.ContainsKey(messageName))
                return;

            if (_customHandlers.ContainsKey(messageName))
            {
                Server.Logger.Log(LogSeverity.Warning,
                    $"[PluginLoader] Tried to add two of the same message handler: {messageName}.");
                return;
            }

            _customHandlers[messageName] = handler;
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

        public static void AddCustomHandler(Plugin plugin)
        {
            var types = plugin.Assembly.GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetRuntimeMethods();
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<CustomMessageHandlerAttribute>();

                    if (attr != null)
                        RegisterCustomMessageHandler(attr.MessageName, (c, p) =>
                        {
                            method.Invoke(plugin, new object[] { c, p });
                        }, true);
                }
            }
        }
    }
}