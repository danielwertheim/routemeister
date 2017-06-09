using System.Threading.Tasks;

namespace Routemeister
{
    /// <summary>
    /// Invokes the action that is to recieve the message in the message handler.
    /// </summary>
    /// <param name="messageHandler">The class that holds the member to be invoked.</param>
    /// <param name="message">The message to be received by the member being invoked.</param>
    /// <returns></returns>
    internal delegate Task MessageHandlerInvoker(object messageHandler, object message);
}