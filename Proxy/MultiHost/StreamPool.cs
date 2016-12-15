using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Proxy.MultiHost
{
    public class StreamPool : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Dictionary<Address, TcpClient> _repository = new Dictionary<Address, TcpClient>();

        public void Dispose()
        {
            foreach (var record in _repository)
            {
                using (record.Value)
                {
                }
            }

            GC.SuppressFinalize(this);
        }

        public Stream Get(Address address, Stream clientStream, CancellationToken token)
        {
            lock (_lock)
            {
                if (_repository.ContainsKey(address))
                {
                    var client = _repository[address];

                    Connect(address, client);

                    return client.GetStream();
                }
                else
                {
                    var client = new TcpClient();

                    _repository[address] = client;

                    Connect(address, client);

                    var stream = client.GetStream();

                    Proxy(stream, clientStream, token);

                    return stream;
                }
            }
        }

        private static void Connect(Address address, TcpClient client)
        {
            if (!client.Connected)
            {
                client.Connect(address.Hostname, address.Port);
            }
        }

        private static async void Proxy(Stream source, Stream destination, CancellationToken token)
        {
            const int bufferSize = 8196;

            var buffer = new byte[bufferSize];

            try
            {
                int bytes;
                do
                {
                    bytes = await source.ReadAsync(buffer, 0, bufferSize, token);
                    Console.Write(Encoding.ASCII.GetString(buffer, 0, bytes));
                    await destination.WriteAsync(buffer, 0, bytes, token);
                } while (bytes > 0 && !token.IsCancellationRequested);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}