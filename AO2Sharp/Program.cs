#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AO2Sharp
{
    class Program
    {
#pragma warning disable 8618
        private static Server _server;
#pragma warning restore 8618

        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!;
            if (!File.Exists(Server.ConfigPath) 
                || new FileInfo(Server.ConfigPath).Length <= 0)
                new Configuration().SaveToFile(Server.ConfigPath);
            _server = new Server(Configuration.LoadFromFile(Server.ConfigPath));
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
