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
            new Server(Configuration.LoadFromFile(configPath)).Start();

            while (true) ;
        }
    }
}
