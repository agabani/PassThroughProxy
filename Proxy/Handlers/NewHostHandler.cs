using System.Net.Sockets;
using System.Threading.Tasks;
using Proxy.Headers;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class NewHostHandler : IHandler
    {
        public async Task<HandlerResult> Run(SessionContext context)
        {
            context.RemoveHost();

            context.AddHost(await Connect(context.Header));

            context.CurrentHostAddress = context.Header.Host;

            return HandlerResult.Connected;
        }

        private static async Task<TcpClient> Connect(HttpHeader httpHeader)
        {
            var host = new TcpClient();
            await host.ConnectAsync(httpHeader.Host.Hostname, httpHeader.Host.Port);
            return host;
        }
    }
}