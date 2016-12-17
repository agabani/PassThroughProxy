using System.IO;
using System.Text;
using System.Threading.Tasks;
using Proxy.Tunnels;

namespace Proxy.ProxyHandlerNext
{
    public class HttpsHandler : IHandler
    {
        public async Task<HandlerResult> Run(Context context)
        {
            if (context.CurrentHostAddress == null || !Equals(context.Header.Host, context.CurrentHostAddress))
            {
                return HandlerResult.NewHostRequired;
            }

            using (var tunnel = new TcpTwoWayTunnel(context.ClientStream, context.HostStream))
            {
                var task = tunnel.Run();
                await SendConnectionEstablised(context.ClientStream);
                await task;
            }

            return HandlerResult.Terminated;
        }

        private static async Task SendConnectionEstablised(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}