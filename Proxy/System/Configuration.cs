using System.Configuration;

namespace Proxy.System
{
    public static class Configuration
    {
        public static int ProxyPort => int.Parse(ConfigurationManager.AppSettings["proxy.port"]);
        public static bool AuthenticationEnabled => bool.Parse(ConfigurationManager.AppSettings["authentication.enabled"]);
        public static string AuthenticationUsername => ConfigurationManager.AppSettings["authentication.username"];
        public static string AuthenticationPassword => ConfigurationManager.AppSettings["authentication.password"];
    }
}