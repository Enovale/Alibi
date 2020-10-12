using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AO2Sharp.Plugins.API;
using AO2Sharp.Plugins.API.Attributes;

namespace AO2Sharp.Commands
{
    internal static class CommandHandler
    {
        internal static readonly Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>();
        internal static readonly Dictionary<string, IPluginManager.CustomCommandHandler> CustomHandlers
            = new Dictionary<string, IPluginManager.CustomCommandHandler>();

        internal static readonly List<Tuple<string, string, bool>> HandlerInfo = new List<Tuple<string, string, bool>>();

        internal delegate void Handler(Client client, string[] args);

        static CommandHandler()
        {
            AddHandlers();
        }

        public static void HandleCommand(Client client, string command, string[] args)
        {
            if (Handlers.ContainsKey(command))
            {
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim('"');
                }
                var handler = Handlers[command];
                var modAttributes = handler.Method.GetCustomAttributes(typeof(ModOnlyAttribute));
                if (modAttributes.Any())
                {
                    if (client.Authed)
                        Handlers[command](client, args);
                    else
                        client.SendOocMessage(((ModOnlyAttribute)modAttributes.First()).ErrorMsg);
                }
                else
                    Handlers[command](client, args);
            }
            else if (CustomHandlers.ContainsKey(command))
            {
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim('"');
                }
                var handler = CustomHandlers[command];
                var customModAttributes =
                    handler.Method.GetCustomAttributes(typeof(Plugins.API.Attributes.ModOnlyAttribute));
                if (customModAttributes.Any())
                {
                    if (client.Authed)
                        CustomHandlers[command](client, args);
                    else
                        client.SendOocMessage(((Plugins.API.Attributes.ModOnlyAttribute)customModAttributes.First()).ErrorMsg);
                }
                else
                    CustomHandlers[command](client, args);
            }
            else
            {
                client.SendOocMessage("Unknown command. Type /help for a list of commands.");
            }
        }

        public static void RegisterCommandHandler(CommandHandlerAttribute attr, Handler handler)
        {
            if (!Handlers.ContainsKey(attr.Command))
            {
                Handlers[attr.Command] = handler;
                var isModOnly = handler.Method.GetCustomAttribute<ModOnlyAttribute>() != null;
                HandlerInfo.Add(new Tuple<string, string, bool>(attr.Command, attr.ShortDesc, isModOnly));
                SortInfo();
            }
        }

        public static void RegisterCustomCommandHandler(CustomCommandHandlerAttribute attr, IPluginManager.CustomCommandHandler handler, bool overrideHandler = false)
        {
            if (!CustomHandlers.ContainsKey(attr.Command))
            {
                CustomHandlers[attr.Command] = handler;
                var isModOnly = handler.Method.GetCustomAttribute<Plugins.API.Attributes.ModOnlyAttribute>() != null;
                HandlerInfo.Add(new Tuple<string, string, bool>(attr.Command, attr.ShortDesc, isModOnly));
                SortInfo();
            }

            if (overrideHandler && Handlers.ContainsKey(attr.Command))
            {
                Handlers.Remove(attr.Command);
                HandlerInfo.RemoveAll(t => t.Item1 == attr.Command);
                SortInfo();
            }
        }

        private static void SortInfo()
        {
            HandlerInfo.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        }

        public static void AddHandlers()
        {
            // Add vanilla handlers
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
                            RegisterCommandHandler(attr, (Handler) method.CreateDelegate(typeof(Handler)));
                    }
                }
            }
        }

        public static void AddCustomHandler(Plugin plugin)
        {
            // Add custom plugin handlers
            var types = plugin.Assembly.GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetRuntimeMethods();
                foreach (var method in methods)
                {
                    if (method.IsStatic)
                    {
                        var attr = method.GetCustomAttribute<CustomCommandHandlerAttribute>();

                        if (attr != null)
                        {
                            try
                            {
                                RegisterCustomCommandHandler(attr,
                                    (IPluginManager.CustomCommandHandler) method.CreateDelegate(
                                        typeof(IPluginManager.CustomCommandHandler)), true);
                            }
                            catch (ArgumentException)
                            {
                                plugin.LogError(
                                    $"Could not load handler {attr.Command} because it does not match the CustomCommandHandler signature.");
                            }
                        }
                    }
                }
            }
        }
    }
}
