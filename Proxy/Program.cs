using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proxy.Configurations;

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
            var commandlinePort = args.FirstOrDefault();

            var port = commandlinePort != null ? int.Parse(commandlinePort) : Configuration.Get().Server.Port;

            using (new PassThroughProxy(port))
            {
                Console.WriteLine($"Proxy is running. Listening on port {port}. Authentication is {(Configuration.Get().Authentication.Enabled ? "enabled" : "disabled")}.");
                while (true)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}