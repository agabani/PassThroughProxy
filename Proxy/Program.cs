using System;
using System.Threading.Tasks;
using Proxy.Configurations;

namespace Proxy
{
    internal class Program
    {
        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            using (new PassThroughProxy(Configuration.Settings.Server.Port, Configuration.Settings))
            {
                Console.WriteLine("Proxy is running." +
                                  $" Listening on port {Configuration.Settings.Server.Port}." +
                                  $" Authentication is {(Configuration.Settings.Authentication.Enabled ? "enabled" : "disabled")}." +
                                  $" Firewall is {(Configuration.Settings.Firewall.Enabled ? "enabled" : "disabled")}.");
                while (true)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}