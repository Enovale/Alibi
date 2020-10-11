#nullable enable
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

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
            //AppDomain.CurrentDomain.FirstChanceException += DumpLogsAndExit;
            TaskScheduler.UnobservedTaskException += DumpLogsAndExit;

            while (true) ;
        }

        static void DumpLogsAndExit(object? sender, EventArgs eventArgs)
        {
            Console.Title = "AO2Sharp - Stopping";
            if (eventArgs.GetType() == typeof(UnhandledExceptionEventArgs))
            {
                Console.Title = "AO2Sharp - Crashed";
                var error = (Exception)((UnhandledExceptionEventArgs)eventArgs).ExceptionObject;
                Server.Logger.Log(LogSeverity.Error, " " + error.Message + "\n" + error.StackTrace);
            }
            if (eventArgs.GetType() == typeof(UnobservedTaskExceptionEventArgs))
            {
                Console.Title = "AO2Sharp - Crashed";
                var error = ((UnobservedTaskExceptionEventArgs)eventArgs).Exception;
                Server.Logger.Log(LogSeverity.Error, " " + error.Message + "\n" + error.StackTrace);
            }
            if (eventArgs.GetType() == typeof(FirstChanceExceptionEventArgs))
            {
                Console.Title = "AO2Sharp - Crashed";
                var error = ((FirstChanceExceptionEventArgs)eventArgs).Exception;
                Server.Logger.Log(LogSeverity.Error, " " + error.Message + "\n" + error.StackTrace);
            }
            Server.Logger.Dump();
            Environment.Exit(0);
        }
    }
}
