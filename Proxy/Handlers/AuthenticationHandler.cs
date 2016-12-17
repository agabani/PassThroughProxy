using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proxy.Configurations;
using Proxy.Headers;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class AuthenticationHandler : IHandler
    {
        private static readonly AuthenticationHandler Self = new AuthenticationHandler();

        private AuthenticationHandler()
        {
        }

        public async Task<HandlerResult> Run(SessionContext context)
        {
            if (!IsAuthenticationRequired())
            {
                return HandlerResult.AuthenticationNotRequired;
            }

            if (!IsProxyAuthorizationHeaderPresent(context.Header))
            {
                await SendProxyAuthenticationRequired(context.ClientStream);
                return HandlerResult.Terminated;
            }

            if (IsProxyAuthorizationCredentialsCorrect(context.Header))
            {
                return HandlerResult.Authenticated;
            }

            await SendProxyAuthenticationInvalid(context.ClientStream);
            return HandlerResult.Terminated;
        }

        public static AuthenticationHandler Instance()
        {
            return Self;
        }

        private static bool IsAuthenticationRequired()
        {
            return Configuration.Get().Authentication.Enabled;
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

            return Encoding.ASCII.GetString(Convert.FromBase64String(value)) == $"{Configuration.Get().Authentication.Username}:{Configuration.Get().Authentication.Password}";
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