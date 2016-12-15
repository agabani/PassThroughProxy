using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Proxy.Headers;
using Proxy.Tunnels;

namespace Proxy.ProxyHandlers
{
    public class ProxyTunnelHandler
    {
        public async Task Run(HttpHeader httpHeader, NetworkStream clientStream)
        {
            using (var host = await CreateHost(httpHeader))
            using (var hostStream = host.GetStream())
            using (var tunnel = new TcpTwoWayTunnel(clientStream, hostStream))
            {
                var task = tunnel.Run();
                await SendConnectionEstablised(clientStream);
                await task;
            }
        }

        private static async Task<TcpClient> CreateHost(HttpHeader httpHeader)
        {
            var host = new TcpClient();
            await host.ConnectAsync(httpHeader.Host.Hostname, httpHeader.Host.Port);
            return host;
        }

        private static async Task SendConnectionEstablised(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}