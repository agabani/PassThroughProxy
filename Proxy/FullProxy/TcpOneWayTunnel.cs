using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.FullProxy
{
    public class TcpOneWayTunnel : IDisposable
    {
        private const int BufferSize = 8192;
        private CancellationTokenSource _cancellationTokenSource;
        private NetworkStream _host;

        public TcpOneWayTunnel(NetworkStream host)
        {
            _host = host;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Run(NetworkStream client)
        {
            await Task.WhenAny(
                Tunnel(_host, client, _cancellationTokenSource.Token));
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

                if (_host != null)
                {
                    _host.Dispose();
                    _host = null;
                }
            }
        }

        private static async Task Tunnel(NetworkStream source, NetworkStream destination, CancellationToken token)
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