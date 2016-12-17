namespace Proxy.Configurations
{
    public class Server
    {
        public Server(int port)
        {
            Port = port;
        }

        public int Port { get; private set; }
    }
}