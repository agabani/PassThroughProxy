namespace Proxy.ProxyHandlerNext
{
    public enum HandlerResult
    {
        Uninitialized,
        Initialized,
        NewHostRequired,
        Connected,
        Authenticated,
        AuthenticationNotRequired,
        Http,
        Https,
        Terminated
    }
}