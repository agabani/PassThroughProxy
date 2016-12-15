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
            using (new PassThroughProxy(int.Parse(args.FirstOrDefault() ?? ConfigurationManager.AppSettings["port"])))
            {
                while (true)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}