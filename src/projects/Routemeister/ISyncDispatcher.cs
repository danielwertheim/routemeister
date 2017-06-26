namespace Routemeister
{
    /// <summary>
    /// Defines an synchronous dispatcher that supports
    /// different messaging patterns as Send, Publish and Request-Response.
    /// </summary>
    public interface ISyncDispatcher
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
        void Send(object message);

        /// <summary>
        /// Publish message to zero or many handlers and do not care about any result.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void Publish(object message);

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
        TResponse Request<TResponse>(IRequest<TResponse> request);
    }
}