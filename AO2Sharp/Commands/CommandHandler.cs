using System.Collections.Generic;
using System.Reflection;

namespace AO2Sharp.Commands
{
    internal static class CommandHandler
    {
        internal static readonly Dictionary<string, Handler> _handlers = new Dictionary<string, Handler>();

        internal delegate void Handler(Client client, string[] args);

        static CommandHandler()
        {
            AddHandlers();
        }

        public static void HandleMessage(Client client, string command, string[] args)
        {
            if (_handlers.ContainsKey(command))
            {
                _handlers[command](client, args);
            }
            else
            {
                client.SendOocMessage("Unknown command. Type /help for a list of commands.");
            }
        }

        public static void RegisterMessageHandler(string messageName, Handler handler)
        {
            if (!_handlers.ContainsKey(messageName))
                _handlers.Add(messageName, handler);

            _handlers[messageName] = handler; ;
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
                        var attr = method.GetCustomAttribute<CommandHandlerAttribute>();

                        if (attr != null)
                            RegisterMessageHandler(attr.Command, (Handler)method.CreateDelegate(typeof(Handler)));
                    }
                }
            }
        }
    }
}
