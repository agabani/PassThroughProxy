using System.Threading;
using System.Threading.Tasks;
using Proxy.Headers;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class FirstRequestHandler : IHandler
    {
        public async Task<HandlerResult> Run(SessionContext context)
        {
            context.Header = await new HttpHeaderStream().GetHeader(context.ClientStream, CancellationToken.None);

            return context.Header != null ? HandlerResult.Initialized : HandlerResult.Terminated;
        }
    }
}