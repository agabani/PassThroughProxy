using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Proxy.ProxyHandlerNext
{
    public class Session
    {
        private static readonly Dictionary<HandlerResult, IHandler> Handlers = new Dictionary<HandlerResult, IHandler>
        {
            {HandlerResult.Uninitialized, new FirstRequestHandler()},
            {HandlerResult.Initialized, new AuthenticationHandler()},
            {HandlerResult.Authenticated, new ProxyTypeHandler()},
            {HandlerResult.AuthenticationNotRequired, new ProxyTypeHandler()},
            {HandlerResult.Http, new HttpHandler()},
            {HandlerResult.Https, new HttpsHandler()},
            {HandlerResult.NewHostRequired, new NewHostHandler()},
            {HandlerResult.Connected, new ProxyTypeHandler()}
        };

        public async Task Run(TcpClient client)
        {
            var result = HandlerResult.Uninitialized;

            using (var context = new Context(client))
            {
                do
                {
                    try
                    {
                        result = await Handlers[result].Run(context);
                    }
                    catch (SocketException)
                    {
                        result = HandlerResult.Terminated;
                    }
                } while (result != HandlerResult.Terminated);
            }
        }
    }
}