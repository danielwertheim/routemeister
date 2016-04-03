using System.Threading.Tasks;

namespace Routemeister
{
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