namespace Proxy.Handlers
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