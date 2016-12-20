using System.Threading.Tasks;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class ProxyTypeHandler : IHandler
    {
        private static readonly ProxyTypeHandler Self = new ProxyTypeHandler();

        private ProxyTypeHandler()
        {
        }

        public Task<ExitReason> Run(SessionContext context)
        {
            return Task.FromResult(context.Header.Verb == "CONNECT" ? ExitReason.HttpsTunnelRequired : ExitReason.HttpProxyRequired);
        }

        public static ProxyTypeHandler Instance()
        {
            return Self;
        }
    }
}