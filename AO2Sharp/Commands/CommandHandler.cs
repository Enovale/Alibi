#nullable enable
using AO2Sharp.Plugins.API;
using AO2Sharp.Plugins.API.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AO2Sharp.Commands
{
    internal static class CommandHandler
    {
        internal static readonly Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>();

        internal static readonly List<Tuple<string, string, bool>> HandlerInfo = new List<Tuple<string, string, bool>>();

        static CommandHandler()
        {
            AddHandlers();
        }

        public static void HandleCommand(IClient client, string command, string[] args)
        {
            if (Handlers.ContainsKey(command))
            {
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim('"');
                }
                var handler = Handlers[command];
                var modAttributes = handler.Method.GetCustomAttributes(typeof(ModOnlyAttribute));
                try
                {
                    if (modAttributes.Any())
                    {
                        if (client.Authed)
                            Handlers[command].Method.Invoke(Handlers[command].Target, new object[] { client, args });
                        else
                            client.SendOocMessage(((ModOnlyAttribute)modAttributes.First()).ErrorMsg);
                    }
                    else
                        Handlers[command].Method.Invoke(Handlers[command].Target, new object[] { client, args });
                }
                catch (CommandException e)
                {
                    client.SendOocMessage("Error: " + e.Message);
                }
            }
            else
            {
                client.SendOocMessage("Unknown command. Type /help for a list of commands.");
            }
        }

        public static void RegisterCommandHandler(CommandHandlerAttribute attr, 
            MethodInfo handler, Plugin? target = null, bool overrideHandler = false)
        {
            if (!overrideHandler && Handlers.ContainsKey(attr.Command))
                return;

            Handlers[attr.Command] = new Handler(handler, target);
            var isModOnly = handler.GetCustomAttribute<ModOnlyAttribute>() != null;
            HandlerInfo.Add(new Tuple<string, string, bool>(attr.Command, attr.ShortDesc, isModOnly));
            SortInfo();
        }

        private static void SortInfo() => HandlerInfo.Sort((x, y) => x.Item1.CompareTo(y.Item1));

        public static void AddHandlers()
        {
            // Add vanilla handlers
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<CommandHandlerAttribute>();

                    if (attr != null)
                        RegisterCommandHandler(attr, method, null, true);
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
                    var attr = method.GetCustomAttribute<CommandHandlerAttribute>();

                    if (attr != null)
                    {
                        try
                        {
                            RegisterCommandHandler(attr, method, plugin, true);
                        }
                        catch (ArgumentException)
                        {
                            plugin.Log(LogSeverity.Error,
                                $"Could not load handler {attr.Command} because it does not match the CustomCommandHandler signature.");
                        }
                    }
                }
            }
        }
    }
}
