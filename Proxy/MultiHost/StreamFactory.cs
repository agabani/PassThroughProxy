using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Proxy.MultiHost
{
    public class StreamFactory : IDisposable
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

        public Stream GetStream(Address address, Action<Stream> handleStream)
        {
            lock (_lock)
            {
                if (_repository.ContainsKey(address))
                {
                    var client = _repository[address];

                    if (!client.Connected)
                    {
                        client.Connect(address.Hostname, address.Port);
                    }

                    return client.GetStream();
                }
                else
                {
                    var client = new TcpClient();

                    _repository[address] = client;

                    if (!client.Connected)
                    {
                        client.Connect(address.Hostname, address.Port);
                    }

                    var stream = client.GetStream();

                    handleStream(stream);

                    return stream;
                }
            }
        }

        public Stream GetStream(TcpClient client)
        {
            return client.GetStream();
        }
    }
}