using System;

namespace Routemeister
{
    /// <summary>
    /// Factory method that creates instances of the class
    /// that holds the member (function) that a message
    /// will be routed to.
    /// </summary>
    /// <param name="messageHandlerType">
    /// The declaring type of the class holding the member being invoked.
    /// </param>
    /// <param name="envelope">
    /// An message envelope carrying the message and optional meta data.
    /// </param>
    /// <returns></returns>
    public delegate object MessageHandlerCreator(Type messageHandlerType, MessageEnvelope envelope);
}