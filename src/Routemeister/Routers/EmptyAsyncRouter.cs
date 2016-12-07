using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class EmptyAsyncRouter : IAsyncRouter
    {
        public Task RouteAsync<T>(T message)
        {
            return Task.FromResult(0);
        }
    }
}