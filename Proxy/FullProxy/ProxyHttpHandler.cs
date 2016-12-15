using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.FullProxy
{
    public class ProxyHttpHandler
    {
        private const int BufferSize = 8192;

        public async Task Run(HttpHeader httpHeader, NetworkStream clientStream)
        {
            var address = httpHeader.Host;

            var host = new TcpClient();
            await host.ConnectAsync(address.Hostname, address.Port);

            var hostStream = host.GetStream();

            var oneWayTunnel = new TcpOneWayTunnel(hostStream);
            var oneWayTunnelTask = oneWayTunnel.Run(clientStream);

            await hostStream.WriteAsync(httpHeader.Array, 0, httpHeader.Array.Length);

            if (httpHeader.ContentLength > 0)
            {
                await ForwardBody(clientStream, hostStream, httpHeader.ContentLength);
            }

            var buffer = new byte[BufferSize];

            int bytesRead;

            do
            {
                var header = await new HttpHeaderStream().GetHeader(clientStream, CancellationToken.None);

                if (header == null)
                {
                    return;
                }

                if (header.Host.Hostname != address.Hostname || header.Host.Port != address.Port)
                {
                    using (oneWayTunnel)
                    using (hostStream)
                    using (host)
                    {
                    }

                    host = new TcpClient();
                    await host.ConnectAsync(header.Host.Hostname, header.Host.Port);
                    hostStream = host.GetStream();

                    oneWayTunnel = new TcpOneWayTunnel(hostStream);
                    oneWayTunnelTask = oneWayTunnel.Run(clientStream);

                    address = header.Host;
                }

                bytesRead = await ForwardHeader(header, hostStream);

                if (header.ContentLength > 0)
                {
                    bytesRead = await ForwardBody(clientStream, hostStream, header.ContentLength);
                }
            } while (bytesRead > 0);
        }

        private static async Task<int> ForwardHeader(HttpHeader httpHeader, NetworkStream host)
        {
            await host.WriteAsync(httpHeader.Array, 0, httpHeader.Array.Length);
            return httpHeader.Array.Length;
        }

        private async Task<int> ForwardBody(NetworkStream client, NetworkStream host, long contentLength)
        {
            var buffer = new byte[BufferSize];

            int bytesRead;

            do
            {
                bytesRead = await client.ReadAsync(buffer, 0, contentLength > BufferSize ? BufferSize : (int) contentLength);
                await host.WriteAsync(buffer, 0, bytesRead);
                contentLength -= bytesRead;
            } while (bytesRead > 0 && contentLength > 0);

            return bytesRead;
        }
    }
}