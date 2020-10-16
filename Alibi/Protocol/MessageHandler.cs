#nullable enable
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;
using System.Collections.Generic;
using System.Reflection;

namespace Alibi.Protocol
{
    internal static class MessageHandler
    {
        internal static readonly Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>();

        static MessageHandler()
        {
            AddHandlers();
        }

        public static void HandleMessage(IClient client, IAOPacket packet)
        {
            if (Handlers.ContainsKey(packet.Type))
            {
                var stateAttr = Handlers[packet.Type].Method.GetCustomAttribute<RequireStateAttribute>();

                if (stateAttr != null)
                    if (client.CurrentState != stateAttr.State)
                    {
                        client.Kick("Protocol violation.");
                        return;
                    }
                Handlers[packet.Type].Method.Invoke(Handlers[packet.Type].Target, new object[] { client, packet });
            }
            else
                Server.Logger.Log(LogSeverity.Warning, $" Unknown client message: '{packet.Type}'", true);
        }

        public static void RegisterMessageHandler(string messageName, MethodInfo handler, Plugin? target, bool overrideHandler = false)
        {
            if (!overrideHandler && Handlers.ContainsKey(messageName))
                return;

            Handlers[messageName] = new Handler(handler, target);
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
                            RegisterMessageHandler(attr.MessageName, method, null, true);
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
                        RegisterMessageHandler(attr.MessageName, method, plugin, true);
                }
            }
        }
    }
}