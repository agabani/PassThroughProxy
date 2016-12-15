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

        public async Task Run(TcpClient tcpClient, Address address, CancellationToken token)
        {
            await Task.WhenAll(Proxy(tcpClient.GetStream(), token));
        }

        private static async Task Proxy(Stream client, CancellationToken token)
        {
            var buffer = new byte[BufferSize];
            var headerStream = new HttpHeaderStream();

            int bytes;

            var factory = new StreamFactory();

            do
            {
                Stream host;

                // Forward Header
                HttpHeader httpHeader;
                using (var header = await headerStream.GetStream(client, token))
                {
                    var array = header.ToArray();

                    httpHeader = new HttpHeader(array);

                    host = factory.GetStream(httpHeader.Host, client, token);

                    bytes = array.Length;
                    Console.Write(Encoding.ASCII.GetString(array, 0, bytes));
                    host.WriteAsync(array, 0, array.Length, token).Wait(token);
                }

                // Forward Body
                await ForwardBody(client, host, httpHeader.ContentLength, buffer, BufferSize, token);
            } while (bytes > 0 && !token.IsCancellationRequested);
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