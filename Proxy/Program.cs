using System;
using System.Threading.Tasks;
using Proxy.Configurations;

namespace Proxy
{
    internal class Program
    {
        private static void Main()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            using (new PassThroughProxy(Configuration.Get().Server.Port))
            {
                Console.WriteLine("Proxy is running." +
                                  $" Listening on port {Configuration.Get().Server.Port}." +
                                  $" Authentication is {(Configuration.Get().Authentication.Enabled ? "enabled" : "disabled")}." +
                                  $" Firewall is {(Configuration.Get().Firewall.Enabled ? "enabled" : "disabled")}.");
                while (true)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}