#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alibi.Plugins.API;
using Alibi.Plugins.API.Attributes;
using Alibi.Plugins.API.Exceptions;

namespace Alibi.Commands
{
    internal static class CommandHandler
    {
        internal static readonly Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>();

        internal static readonly List<Tuple<string, string, int>>
            HandlerInfo = new List<Tuple<string, string, int>>();

        static CommandHandler()
        {
            AddHandlers();
        }

        public static void HandleCommand(IClient client, string command, string[] args)
        {
            if (Handlers.ContainsKey(command))
            {
                for (var i = 0; i < args.Length; i++)
                    args[i] = args[i].Trim('"');
                var handler = Handlers[command];
                var modAttr = handler.Method.GetCustomAttributes(typeof(ModOnlyAttribute));
                var adminAttr = handler.Method.GetCustomAttributes(typeof(AdminOnlyAttribute));
                try
                {
                    if (modAttr.Any())
                    {
                        if (client.Auth >= AuthType.MODERATOR)
                            Handlers[command].Method.Invoke(Handlers[command].Target, new object[] {client, args});
                        else
                            client.SendOocMessage(((ModOnlyAttribute) modAttr.First()).ErrorMsg);
                    }
                    else if (adminAttr.Any())
                    {
                        if (client.Auth >= AuthType.ADMINISTRATOR)
                            Handlers[command].Method.Invoke(Handlers[command].Target, new object[] {client, args});
                        else
                            client.SendOocMessage(((AdminOnlyAttribute) adminAttr.First()).ErrorMsg);
                    }
                    else
                    {
                        Handlers[command].Method.Invoke(Handlers[command].Target, new object[] {client, args});
                    }
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException?.GetType() == typeof(CommandException))
                        client.SendOocMessage("Error: " + ((CommandException) e.InnerException!).Message);
                    else
                        Server.Logger.Log(LogSeverity.Error, $" {e.InnerException}");
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
            var permissionNeeded = AuthType.USER;
            if (handler.GetCustomAttribute<ModOnlyAttribute>() != null)
                permissionNeeded = AuthType.MODERATOR;
            if (handler.GetCustomAttribute<AdminOnlyAttribute>() != null)
                permissionNeeded = AuthType.ADMINISTRATOR;
            HandlerInfo.Add(new Tuple<string, string, int>(attr.Command, attr.ShortDesc, permissionNeeded));
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
            var types = plugin.GetType().Assembly.GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetRuntimeMethods();
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<CommandHandlerAttribute>();

                    if (attr != null)
                        RegisterCommandHandler(attr, method, plugin, true);
                }
            }
        }
    }
}