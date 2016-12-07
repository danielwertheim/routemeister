using System.Threading.Tasks;

namespace Routemeister
{
    public interface IAsyncRequestHandlerOf<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}