namespace Proxy
{
    public class Address
    {
        public Address(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public string Hostname { get; private set; }
        public int Port { get; private set; }
    }
}