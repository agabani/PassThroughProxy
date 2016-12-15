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
            var streamFactory = new StreamFactory();

            var client = streamFactory.GetStream(tcpClient);
            var host = streamFactory.GetStream(address, client, token);

            await Task.WhenAll(Proxy(client, host, token));
        }

        private static async Task Proxy(Stream source, Stream destination, CancellationToken token)
        {
            var buffer = new byte[BufferSize];

            int bytes;

            do
            {
                bytes = await source.ReadAsync(buffer, 0, BufferSize, token);
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytes));
                await destination.WriteAsync(buffer, 0, bytes, token);
            } while (bytes > 0 && !token.IsCancellationRequested);
        }
    }
}