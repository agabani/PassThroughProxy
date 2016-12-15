using System.Threading.Tasks;

namespace Proxy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            using (var passThroughProxy = new PassThroughProxy(8889))
            {
                while (true)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}