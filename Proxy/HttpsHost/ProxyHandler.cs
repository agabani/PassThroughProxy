using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.HttpsHost
{
    public class ProxyHandler
    {
        public async Task Run(TcpClient client)
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

                if (httpHeader.Verb != "CONNECT")
                {
                    throw new Exception();
                }

                using (var host = await CreateHost(httpHeader))
                using (var hostStream = host.GetStream())
                using (var tunnel = new TcpTunnel(clientStream, hostStream))
                {
                    var task = tunnel.Run();
                    await SendConnectionEstablised(clientStream);
                    await task;
                }
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