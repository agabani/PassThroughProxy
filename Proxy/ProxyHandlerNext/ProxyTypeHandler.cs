using System.Threading.Tasks;

namespace Proxy.ProxyHandlerNext
{
    public class ProxyTypeHandler : IHandler
    {
        public Task<HandlerResult> Run(Context context)
        {
            return Task.FromResult(context.Header.Verb == "CONNECT" ? HandlerResult.Https : HandlerResult.Http);
        }
    }
}