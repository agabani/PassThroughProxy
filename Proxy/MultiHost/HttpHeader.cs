using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy.MultiHost
{
    public class HttpHeader
    {
        public HttpHeader(byte[] array)
        {
            Array = array;
            Parse(this, array);
        }

        public byte[] Array { get; private set; }
        public Address Host { get; private set; }
        public long ContentLength { get; private set; }

        private static void Parse(HttpHeader self, byte[] array)
        {
            var strings = Encoding.ASCII.GetString(array).Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            self.Host = GetAddress(strings);
            self.ContentLength = GetContentLength(strings);
        }

        private static Address GetAddress(IEnumerable<string> strings)
        {
            const string key = "host:";

            var split = strings
                .Single(s => s.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                .Substring(key.Length)
                .TrimStart()
                .Split(':');

            switch (split.Length)
            {
                case 1:
                    return new Address(split[0], 80);
                case 2:
                    return new Address(split[0], int.Parse(split[1]));
                default:
                    throw new FormatException(string.Join(":", split));
            }
        }

        private static long GetContentLength(IEnumerable<string> strings)
        {
            const string key = "content-length:";

            return Convert.ToInt64(strings
                .SingleOrDefault(s => s.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                ?.Substring(key.Length)
                .TrimStart());
        }
    }
}