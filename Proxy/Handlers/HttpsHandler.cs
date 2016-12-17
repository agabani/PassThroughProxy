using System.IO;
using System.Text;
using System.Threading.Tasks;
using Proxy.Sessions;
using Proxy.Tunnels;

namespace Proxy.Handlers
{
    public class HttpsHandler : IHandler
    {
        private static readonly HttpsHandler Self = new HttpsHandler();

        private HttpsHandler()
        {
        }

        public async Task<HandlerResult> Run(SessionContext context)
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

        public static HttpsHandler Instance()
        {
            return Self;
        }

        private static async Task SendConnectionEstablised(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}