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

        private static void Parse(HttpHeader self, byte[] array)
        {
            var strings = Encoding.ASCII.GetString(array).Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            self.Host = Address(strings);
        }

        private static Address Address(IEnumerable<string> strings)
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
    }
}