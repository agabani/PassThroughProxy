using System.Threading;
using System.Threading.Tasks;
using Proxy.Headers;

namespace Proxy.ProxyHandlerNext
{
    public class FirstRequestHandler : IHandler
    {
        public async Task<HandlerResult> Run(Context context)
        {
            context.Header = await new HttpHeaderStream().GetHeader(context.ClientStream, CancellationToken.None);

            return context.Header != null ? HandlerResult.Initialized : HandlerResult.Terminated;
        }
    }
}