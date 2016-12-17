namespace Proxy.Handlers
{
    public enum HandlerResult
    {
        Uninitialized,
        Initialized,
        NewHostRequired,
        NewHostConnectionRequired,
        Connected,
        Authenticated,
        AuthenticationNotRequired,
        Http,
        Https,
        Terminated
    }
}