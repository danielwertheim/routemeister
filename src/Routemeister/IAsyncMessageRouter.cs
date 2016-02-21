using System.Threading.Tasks;

namespace Routemeister
{
    public interface IAsyncMessageRouter
    {
        Task RouteAsync<T>(T message);
    }
}