using System;
using System.Net.Sockets;
using System.Threading;
using Proxy.MultiHost;

namespace Proxy
{
    public class PassThroughProxy : IDisposable
    {
        private ProxyListener _proxyListener;

        public PassThroughProxy(int listenPort, Address address)
        {
            Action<TcpClient, CancellationToken> handleClient = async (client, token) => await new ProxyHandler().Run(client, address, token);

            _proxyListener = new ProxyListener(listenPort, handleClient);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_proxyListener != null)
                {
                    _proxyListener.Dispose();
                    _proxyListener = null;
                }
            }
        }
    }
}