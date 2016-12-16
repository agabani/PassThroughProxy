using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Proxy.Headers;
using Proxy.System;

namespace Proxy.ProxyHandlers
{
    public class ProxyAuthenticationHandler
    {
        public async Task<bool> Run(HttpHeader httpHeader, NetworkStream clientStream)
        {
            try
            {
                if (!IsProxyAuthorizationHeaderPresent(httpHeader))
                {
                    await SendProxyAuthenticationRequired(clientStream);
                    return false;
                }

                if (IsProxyAuthorizationCredentialsCorrect(httpHeader))
                {
                    return true;
                }

                await SendProxyAuthenticationInvalid(clientStream);
                return false;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private static bool IsProxyAuthorizationHeaderPresent(HttpHeader httpHeader)
        {
            return httpHeader.ArrayList.Any(@string => @string.StartsWith("Proxy-Authorization: Basic", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsProxyAuthorizationCredentialsCorrect(HttpHeader httpHeader)
        {
            const string key = "Proxy-Authorization: Basic";

            var value = httpHeader.ArrayList
                .First(@string => @string.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                .Substring(key.Length)
                .Trim();

            return Encoding.ASCII.GetString(Convert.FromBase64String(value)) == $"{Configuration.AuthenticationUsername}:{Configuration.AuthenticationPassword}";
        }

        private static async Task SendProxyAuthenticationRequired(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"Pass Through Proxy\"\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private static async Task SendProxyAuthenticationInvalid(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 407 Proxy Authentication Invalid\r\nProxy-Authenticate: Basic realm=\"Pass Through Proxy\"\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}