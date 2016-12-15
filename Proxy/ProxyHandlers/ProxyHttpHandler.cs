using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Proxy.Headers;
using Proxy.Network;
using Proxy.Tunnels;

namespace Proxy.ProxyHandlers
{
    public class ProxyHttpHandler
    {
        private const int BufferSize = 8192;

        public async Task Run(HttpHeader header, NetworkStream clientStream)
        {
            Address currentAddress = null;
            TcpClient host = null;
            NetworkStream hostStream = null;
            TcpOneWayTunnel oneWayTunnel = null;

            int bytesRead;

            do
            {
                header = await GetHeader(header, clientStream);

                if (header == null)
                {
                    return;
                }

                if (IsNewHost(currentAddress, header.Host))
                {
                    TerminateHost(oneWayTunnel, hostStream, host);

                    host = await ConnectTo(header.Host);
                    hostStream = host.GetStream();
                    oneWayTunnel = Tunnel(hostStream, clientStream);

                    currentAddress = header.Host;
                }

                bytesRead = await ForwardHeader(header, hostStream);

                if (HasBody(header))
                {
                    bytesRead = await ForwardBody(clientStream, hostStream, header.ContentLength);
                }
                header = null;
            } while (bytesRead > 0);
        }

        private static async Task<HttpHeader> GetHeader(HttpHeader header, NetworkStream stream)
        {
            return header ?? await new HttpHeaderStream().GetHeader(stream, CancellationToken.None);
        }

        private static bool IsNewHost(Address currentAddress, Address targetAddress)
        {
            return currentAddress == null || !Equals(targetAddress, currentAddress);
        }

        private static void TerminateHost(params IDisposable[] objects)
        {
            foreach (var disposable in objects)
            {
                using (disposable)
                {
                }
            }
        }

        private static async Task<TcpClient> ConnectTo(Address address)
        {
            var host = new TcpClient();
            await host.ConnectAsync(address.Hostname, address.Port);
            return host;
        }

        private static TcpOneWayTunnel Tunnel(NetworkStream source, NetworkStream destination)
        {
            var tunnel = new TcpOneWayTunnel(source);
            tunnel.Run(destination).GetAwaiter();
            return tunnel;
        }

        private static async Task<int> ForwardHeader(HttpHeader httpHeader, NetworkStream host)
        {
            await host.WriteAsync(httpHeader.Array, 0, httpHeader.Array.Length);
            return httpHeader.Array.Length;
        }

        private static bool HasBody(HttpHeader header)
        {
            return header.ContentLength > 0;
        }

        private static async Task<int> ForwardBody(NetworkStream client, NetworkStream host, long contentLength)
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