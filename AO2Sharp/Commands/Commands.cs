using System.Linq;
using System.Reflection;
using AO2Sharp.Protocol;

namespace AO2Sharp.Commands
{
    internal static class Commands
    {
        [CommandHandler("help", "Show's this text.")]
        internal static void Help(Client client, string[] args)
        {
            string finalResponse = "Commands: \n";
            foreach (var (command, description) in CommandHandler._handlers)
                finalResponse += $"/{command}: {description.Method.GetCustomAttributes<CommandHandlerAttribute>().First().ShortDesc}";

            client.SendOocMessage(finalResponse);
        }
    }
}
