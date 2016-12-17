using System.Linq;
using System.Threading.Tasks;
using Proxy.Configurations;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class FirewallHandler : IHandler
    {
        public Task<HandlerResult> Run(SessionContext context)
        {
            if (!Configuration.Get().Firewall.Enabled)
            {
                return Task.FromResult(HandlerResult.NewHostConnectionRequired);
            }

            var rule = Configuration.Get()
                .Firewall.Rules
                .SingleOrDefault(r => r.Hostname == context.Header.Host.Hostname);

            return rule == null || rule.Action == ActionEnum.Allow
                ? Task.FromResult(HandlerResult.NewHostConnectionRequired)
                : Task.FromResult(HandlerResult.Terminated);
        }
    }
}