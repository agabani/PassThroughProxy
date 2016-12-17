namespace Proxy.ProxyHandlerNext
{
    public enum HandlerResult
    {
        Uninitialized,
        Initialized,
        NewHostRequired,
        Connected,
        Http,
        Https,
        Terminated,
    }
}