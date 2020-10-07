using System.Linq;
using System.Reflection;

namespace AO2Sharp.Commands
{
    internal static class Commands
    {
        [CommandHandler("help", "Show's this text.")]
        internal static void Help(Client client, string[] args)
        {
            string finalResponse = "Commands: \n";
            foreach (var (command, description) in CommandHandler._handlers)
                finalResponse += $"/{command}: {description.Method.GetCustomAttributes<CommandHandlerAttribute>().First().ShortDesc}\n";

            client.SendOocMessage(finalResponse);
        }

        [CommandHandler("getlogs", "Retrieves the server logs and dumps them.")]
        internal static void GetLogs(Client client, string[] args)
        {
            if (Server.Logger.Dump().Result)
                client.SendOocMessage("Successfully dumped logs. Check the Logs folder");
            else
                client.SendOocMessage("No logs have been stored yet, can't dump.");
        }
    }
}
