using System;

namespace Routemeister
{
    public interface IMessageHandlerAction
    {
        Type HandlerType { get; }
        Type MessageType { get; }

        object Invoke(object handler, object message);
    }
}