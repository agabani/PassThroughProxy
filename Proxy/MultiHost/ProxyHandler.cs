using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.MultiHost
{
    public class ProxyHandler
    {
        private const int BufferSize = 8196;

        public async Task Run(TcpClient client, CancellationToken token)
        {
            using (client)
            {
                await Task.WhenAll(Proxy(client.GetStream(), token));
            }
        }

        private static async Task Proxy(Stream client, CancellationToken token)
        {
            using (client)
            {
                var buffer = new byte[BufferSize];
                var stream = new HttpHeaderStream();

                using (var pool = new StreamPool())
                {
                    int bytes;
                    do
                    {
                        var header = await stream.GetHeader(client, token);

                        if (header == null)
                        {
                            return;
                        }

                        var host = pool.Get(header.Host, client, token);

                        bytes = await ForwardHeader(header, host, token);

                        if (header.ContentLength > 0)
                        {
                            bytes = await ForwardBody(client, host, header.ContentLength, buffer, BufferSize, token);
                        }
                    } while (bytes > 0 && !token.IsCancellationRequested);
                }
            }
        }

        private static async Task<int> ForwardHeader(HttpHeader httpHeader, Stream host, CancellationToken token)
        {
            await host.WriteAsync(httpHeader.Array, 0, httpHeader.Array.Length, token);
            Console.Write(Encoding.ASCII.GetString(httpHeader.Array, 0, httpHeader.Array.Length));
            return httpHeader.Array.Length;
        }

        private static async Task<int> ForwardBody(Stream client, Stream host, long contentLength, byte[] buffer, int bufferSize, CancellationToken token)
        {
            int bytesRead;

            do
            {
                bytesRead = await client.ReadAsync(buffer, 0, contentLength > bufferSize ? bufferSize : (int) contentLength, token);
                await host.WriteAsync(buffer, 0, bytesRead, token);
                Console.Write(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                contentLength -= bytesRead;
            } while (bytesRead > 0 && !token.IsCancellationRequested && contentLength > 0);

            return bytesRead;
        }
    }
}