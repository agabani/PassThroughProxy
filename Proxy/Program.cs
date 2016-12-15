using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Proxy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(IEnumerable<string> args)
        {
            var port = int.Parse(args.FirstOrDefault() ?? ConfigurationManager.AppSettings["port"]);

            using (new PassThroughProxy(port))
            {
                Console.WriteLine($"Listening on port {port}...");
                while (true)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}