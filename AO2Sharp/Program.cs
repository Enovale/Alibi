using System;
using System.IO;

namespace AO2Sharp
{
    class Program
    {
        private static string configPath = "config.json";

        static void Main(string[] args)
        {
            if(!File.Exists(configPath))
                new Configuration().SaveToFile(configPath);
            var server = new Server(Configuration.LoadFromFile(configPath));
            Server.ServerConfiguration.SaveToFile(configPath);
            server.Start();

            while (true) ;
        }
    }
}
