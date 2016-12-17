using System.Threading.Tasks;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public interface IHandler
    {
        Task<HandlerResult> Run(SessionContext context);
    }
}