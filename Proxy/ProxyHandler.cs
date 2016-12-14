using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy
{
    public class ProxyHandler
    {
        private const int BufferSize = 8196;

        public async Task Run(TcpClient client, string hostname, int port, CancellationToken token)
        {
            using (var target = new TcpClient())
            {
                await target.ConnectAsync(hostname, port);

                await Task.WhenAll(
                    Proxy(target, client, token),
                    Proxy(client, target, token));
            }
        }

        private static async Task Proxy(TcpClient source, TcpClient destination, CancellationToken token)
        {
            var buffer = new byte[BufferSize];

            using (var sourceStream = source.GetStream())
            using (var destinationStream = destination.GetStream())
            {
                int bytes;

                do
                {
                    bytes = await sourceStream.ReadAsync(buffer, 0, BufferSize, token);
                    Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytes));
                    await destinationStream.WriteAsync(buffer, 0, bytes, token);
                } while (bytes != 0 && !token.IsCancellationRequested);
            }
        }
    }
}