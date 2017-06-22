using System.Threading.Tasks;

namespace Routemeister
{
    /// <summary>
    /// A premade definition of an async request handler.
    /// Please note, you can define your own interface for this.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IAsyncRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}