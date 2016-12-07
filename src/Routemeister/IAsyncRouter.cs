using System.Threading.Tasks;

namespace Routemeister
{
    /// <summary>
    /// Defines an asynchronous router that just routes a message with no questions asked.
    /// For explicit messasge patterns as Send, Publish and Request-Response, see <see cref="IAsyncDispatcher"/>.
    /// </summary>
    public interface IAsyncRouter
    {
        /// <summary>
        /// Routes the sent message to any subscribed handler.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        Task RouteAsync<T>(T message);
    }
}