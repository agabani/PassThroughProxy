using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Proxy.Headers;
using Proxy.Tunnels;

namespace Proxy.ProxyHandlerNext
{
    public class HttpHandler : IHandler
    {
        private const int BufferSize = 8192;

        public async Task<HandlerResult> Run(Context context)
        {
            if (context.CurrentHostAddress == null || !Equals(context.Header.Host, context.CurrentHostAddress))
            {
                return HandlerResult.NewHostRequired;
            }

            var buffer = new byte[BufferSize];

            var oneWayTunnel = Tunnel(context.HostStream, context.ClientStream);

            try
            {
                int bytesRead;

                do
                {
                    context.Header = await GetHeader(context.Header, context.ClientStream);

                    if (context.Header == null)
                    {
                        return HandlerResult.Terminated;
                    }

                    if (context.CurrentHostAddress == null || !Equals(context.Header.Host, context.CurrentHostAddress))
                    {
                        TerminateHost(oneWayTunnel);

                        return HandlerResult.NewHostRequired;
                    }

                    bytesRead = await ForwardHeader(context.Header, context.HostStream);

                    if (HasBody(context.Header))
                    {
                        bytesRead = await ForwardBody(context.ClientStream, context.HostStream, context.Header.ContentLength, buffer);
                    }

                    context.Header = null;
                } while (bytesRead > 0);
            }
            finally
            {
                TerminateHost(oneWayTunnel);
            }

            return HandlerResult.Terminated;
        }

        private static async Task<HttpHeader> GetHeader(HttpHeader header, NetworkStream stream)
        {
            return header ?? await new HttpHeaderStream().GetHeader(stream, CancellationToken.None);
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

        private static TcpOneWayTunnel Tunnel(NetworkStream source, NetworkStream destination)
        {
            var tunnel = new TcpOneWayTunnel();
            tunnel.Run(destination, source).GetAwaiter();
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

        private static async Task<int> ForwardBody(Stream client, Stream host, long contentLength, byte[] buffer)
        {
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