#nullable enable
using System;
using System.IO;

namespace AO2Sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists(Server.ConfigPath))
                new Configuration().SaveToFile(Server.ConfigPath);
            var server = new Server(Configuration.LoadFromFile(Server.ConfigPath));
            Server.ServerConfiguration.SaveToFile(Server.ConfigPath);
            Console.Title = "AO2Sharp - Running";
            server.Start();

            AppDomain.CurrentDomain.ProcessExit += DumpLogsAndExit;
            AppDomain.CurrentDomain.UnhandledException += DumpLogsAndExit;

            while (true) ;
        }

        static void DumpLogsAndExit(object? sender, EventArgs eventArgs)
        {
            Console.Title = "AO2Sharp - Crashed";
            if (eventArgs.GetType() == typeof(UnhandledExceptionEventArgs))
            {
                var error = (Exception)((UnhandledExceptionEventArgs)eventArgs).ExceptionObject;
                Server.Logger.Log(LogSeverity.Error, " " + error.Message + "\n" + error.StackTrace);
            }
            Server.Logger.Dump();
            Environment.Exit(0);
        }
    }
}
