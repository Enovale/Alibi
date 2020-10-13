﻿#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace AO2Sharp
{
    class Program
    {
        private static Server _server;

        static void Main(string[] args)
        {
            if (!File.Exists(Server.ConfigPath))
                new Configuration().SaveToFile(Server.ConfigPath);
            _server = new Server(Configuration.LoadFromFile(Server.ConfigPath));
            Server.ServerConfiguration.SaveToFile(Server.ConfigPath);
            Console.Title = "AO2Sharp - Running";
            _server.Start();

            AppDomain.CurrentDomain.ProcessExit += ExitProgram;
            Console.CancelKeyPress += ExitProgram;
            AppDomain.CurrentDomain.UnhandledException += ExitProgram;
            TaskScheduler.UnobservedTaskException += ExitProgram;

            while (true) ;
        }

        static void ExitProgram(object? sender, EventArgs eventArgs)
        {
            Console.Title = "AO2Sharp - Stopping";
            if (eventArgs is UnhandledExceptionEventArgs exceptionArgs)
            {
                Console.Title = "AO2Sharp - Crashed";
                var error = (Exception)exceptionArgs.ExceptionObject;
                Server.Logger.Log(LogSeverity.Error, " " + error.Message + "\n" + error.StackTrace);
            }
            if (eventArgs is UnobservedTaskExceptionEventArgs taskExceptionArgs)
            {
                Console.Title = "AO2Sharp - Crashed";
                var error = taskExceptionArgs.Exception;
                Server.Logger.Log(LogSeverity.Error, " " + error!.Message + "\n" + error.StackTrace);
            }
            _server.Stop();
            if (eventArgs is ConsoleCancelEventArgs args)
            {
                args.Cancel = true;
                Environment.Exit(0);
            }
        }
    }
}
