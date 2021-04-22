#nullable enable
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Alibi.Plugins.API;

namespace Alibi
{
    internal static class Program
    {
        internal static readonly ManualResetEvent ResetEvent;
        
        private static readonly Server _server;

        static Program()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            ResetEvent = new ManualResetEvent(false);
            Environment.CurrentDirectory = GetRealProcessDirectory();
            if (!File.Exists(Server.ConfigPath) || new FileInfo(Server.ConfigPath).Length <= 0)
                new Configuration().SaveToFile(Server.ConfigPath);
            _server = new Server(Configuration.LoadFromFile(Server.ConfigPath));
            Console.Title = "Alibi - Running";
            _server.Start();
        }

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += ExitProgram;
            Console.CancelKeyPress += ExitProgram;
            AppDomain.CurrentDomain.UnhandledException += ExitProgram;
            TaskScheduler.UnobservedTaskException += ExitProgram;

            ResetEvent.WaitOne();
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