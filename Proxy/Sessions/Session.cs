using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Proxy.Handlers;

namespace Proxy.Sessions
{
    public class Session
    {
        private static readonly Dictionary<HandlerResult, IHandler> Handlers = new Dictionary<HandlerResult, IHandler>
        {
            {HandlerResult.Uninitialized, FirstRequestHandler.Instance()},
            {HandlerResult.Initialized, AuthenticationHandler.Instance()},
            {HandlerResult.Authenticated, ProxyTypeHandler.Instance()},
            {HandlerResult.AuthenticationNotRequired, ProxyTypeHandler.Instance()},
            {HandlerResult.Http, HttpHandler.Instance()},
            {HandlerResult.Https, HttpsHandler.Instance()},
            {HandlerResult.NewHostRequired, FirewallHandler.Instance()},
            {HandlerResult.NewHostConnectionRequired, NewHostHandler.Instance()},
            {HandlerResult.Connected, ProxyTypeHandler.Instance()}
        };

        public async Task Run(TcpClient client)
        {
            var result = HandlerResult.Uninitialized;

            using (var context = new SessionContext(client))
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