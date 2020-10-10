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
            server.Start();

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                Server.Logger.Dump();
                Environment.Exit(0);
            };
            
            while (true) ;
        }
    }
}
