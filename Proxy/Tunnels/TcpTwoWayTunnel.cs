using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.Tunnels
{
    public class TcpTwoWayTunnel : IDisposable
    {
        private const int BufferSize = 8192;
        private CancellationTokenSource _cancellationTokenSource;
        private NetworkStream _client;
        private NetworkStream _host;

        public TcpTwoWayTunnel(NetworkStream client, NetworkStream host)
        {
            _client = client;
            _host = host;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Run()
        {
            await Task.WhenAny(
                Tunnel(_client, _host, _cancellationTokenSource.Token),
                Tunnel(_host, _client, _cancellationTokenSource.Token));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }

                if (_host != null)
                {
                    _host.Dispose();
                    _host = null;
                }
            }
        }

        private static async Task Tunnel(Stream source, Stream destination, CancellationToken token)
        {
            var buffer = new byte[BufferSize];

            try
            {
                int bytesRead;
                do
                {
                    bytesRead = await source.ReadAsync(buffer, 0, BufferSize, token);
                    await destination.WriteAsync(buffer, 0, bytesRead, token);
                } while (bytesRead > 0 && !token.IsCancellationRequested);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}