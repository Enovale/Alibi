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
            return execPath.EndsWith("dotnet.exe") || execPath.EndsWith("dotnet")
                ? Environment.CurrentDirectory
                : Path.GetDirectoryName(execPath)!;
        }

        private static void ExitProgram(object? sender, EventArgs eventArgs)
        {
            Console.Title = "Alibi - Stopping";
            _server.Stop();
            switch (eventArgs)
            {
                case UnhandledExceptionEventArgs exceptionArgs:
                {
                    Console.Title = "Alibi - Crashed";
                    var genericError = (Exception) exceptionArgs.ExceptionObject;
                    Server.Logger.Log(LogSeverity.Error, $" {genericError.Message}\n{genericError.StackTrace}");
                    break;
                }
                case UnobservedTaskExceptionEventArgs taskExceptionArgs:
                    Console.Title = "Alibi - Crashed";
                    var unobservedTaskError = taskExceptionArgs.Exception;
                    Server.Logger.Log(LogSeverity.Error,
                        $" {unobservedTaskError!.Message}\n{unobservedTaskError.StackTrace}");
                    break;
                case ConsoleCancelEventArgs args:
                    args.Cancel = true;
                    Environment.Exit(0);
                    break;
            }
        }
    }
}