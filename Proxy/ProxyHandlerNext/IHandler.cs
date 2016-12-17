using System.Threading.Tasks;

namespace Proxy.ProxyHandlerNext
{
    public interface IHandler
    {
        Task<HandlerResult> Run(Context context);
    }
}