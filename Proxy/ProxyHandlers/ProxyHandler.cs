using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Proxy.Headers;

namespace Proxy.ProxyHandlers
{
    public class ProxyHandler
    {
        public async Task Run(TcpClient client)
        {
            try
            {
                using (client)
                using (var clientStream = client.GetStream())
                {
                    var httpHeader = await new HttpHeaderStream()
                        .GetHeader(clientStream, CancellationToken.None);

                    if (httpHeader == null)
                    {
                        return;
                    }

                    if (httpHeader.Verb == "CONNECT")
                    {
                        await new ProxyTunnelHandler().Run(httpHeader, clientStream);
                    }
                    else
                    {
                        await new ProxyHttpHandler().Run(httpHeader, clientStream);
                    }
                }
            }
            catch (SocketException)
            {
            }
        }
    }
}