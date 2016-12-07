using System.Threading.Tasks;

namespace Routemeister
{
    /// <summary>
    /// Defines an asynchronous dispatcher that supports
    /// different messaging patterns as Send, Publish and Request-Response.
    /// For pure routing, see <see cref="IAsyncRouter"/>.
    /// </summary>
    public interface IAsyncDispatcher
    {
        /// <summary>
        /// Send message to one single handler and do not care about any result.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <remarks>
        /// Supports one sinlge Handler of the command-message.
        /// If more than one is found, it will generate an exception.
        /// </remarks>
        Task SendAsync(object message);

        /// <summary>
        /// Publish message to zero or many handlers and do not care about any result.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PublishAsync(object message);

        /// <summary>
        /// Send request to one single handler and expect a response.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>
        /// Supports one sinlge handler of the request-message.
        /// If more than one is found, it will generate an exception.
        /// </remarks>
        Task<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request);
    }
}