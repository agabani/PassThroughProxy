using System.Threading.Tasks;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class ProxyTypeHandler : IHandler
    {
        public Task<HandlerResult> Run(SessionContext context)
        {
            return Task.FromResult(context.Header.Verb == "CONNECT" ? HandlerResult.Https : HandlerResult.Http);
        }
    }
}