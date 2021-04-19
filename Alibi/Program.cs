#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Alibi.Plugins.API;

namespace Alibi
{
    internal static class Program
    {
#pragma warning disable 8618
        private static Server _server;
#pragma warning restore 8618

        private static void Main(string[] args)
        {
            Environment.CurrentDirectory = GetRealProcessDirectory();
            if (!File.Exists(Server.ConfigPath)
                || new FileInfo(Server.ConfigPath).Length <= 0)
                new Configuration().SaveToFile(Server.ConfigPath);
            _server = new Server(Configuration.LoadFromFile(Server.ConfigPath));
            Console.Title = "Alibi - Running";
            _server.Start();

            AppDomain.CurrentDomain.ProcessExit += ExitProgram;
            Console.CancelKeyPress += ExitProgram;
            AppDomain.CurrentDomain.UnhandledException += ExitProgram;
            TaskScheduler.UnobservedTaskException += ExitProgram;

            while (true) ;
        }

        private static string GetRealProcessDirectory()
        {
            var execPath = Process.GetCurrentProcess().MainModule!.FileName!;
            return (execPath.EndsWith("dotnet.exe") ? Environment.CurrentDirectory : Path.GetDirectoryName(execPath))!;
        }

        private static void ExitProgram(object? sender, EventArgs eventArgs)
        {
            Console.Title = "Alibi - Stopping";
            if (eventArgs is UnhandledExceptionEventArgs exceptionArgs)
            {
                Console.Title = "Alibi - Crashed";
                var error = (Exception) exceptionArgs.ExceptionObject;
                Server.Logger.Log(LogSeverity.Error, $" {error.Message}\n{error.StackTrace}");
            }

            if (eventArgs is UnobservedTaskExceptionEventArgs taskExceptionArgs)
            {
                Console.Title = "Alibi - Crashed";
                var error = taskExceptionArgs.Exception;
                Server.Logger.Log(LogSeverity.Error, $" {error!.Message}\n{error.StackTrace}");
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