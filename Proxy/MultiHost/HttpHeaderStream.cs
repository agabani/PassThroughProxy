using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy.MultiHost
{
    public class HttpHeaderStream
    {
        private static readonly string[] Delimiter = {"\r", "\n", "\r", "\n"};

        public async Task<HttpHeader> GetHeader(Stream client, CancellationToken token)
        {
            using (var memoryStream = await GetStream(client, token))
            {
                var array = memoryStream.ToArray();

                return array.Length == 0 ? null : new HttpHeader(array);
            }
        }

        private static async Task<MemoryStream> GetStream(Stream client, CancellationToken token)
        {
            var memoryStream = new MemoryStream();
            var readBuffer = new byte[1];

            int bytesRead;
            var counter = 0;

            do
            {
                bytesRead = await client.ReadAsync(readBuffer, 0, 1, token);
                await memoryStream.WriteAsync(readBuffer, 0, bytesRead, token);

                counter = Encoding.ASCII.GetString(readBuffer) == Delimiter[counter] ? counter + 1 : 0;

                if (counter == Delimiter.Length)
                {
                    return memoryStream;
                }
            } while (bytesRead > 0 && !token.IsCancellationRequested);

            return memoryStream;
        }
    }
}