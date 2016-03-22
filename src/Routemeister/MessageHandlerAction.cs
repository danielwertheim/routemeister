using System;
using System.Threading.Tasks;

namespace Routemeister
{
    internal class MessageHandlerAction : IMessageHandlerAction
    {
        internal readonly MessageHandlerInvoker Invoker;

        public Type HandlerType { get; }
        public Type MessageType { get; }

        internal MessageHandlerAction(Type handlerType, Type messageType, MessageHandlerInvoker invoker)
        {
            if (handlerType == null)
                throw new ArgumentNullException(nameof(handlerType));

            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            if (invoker == null)
                throw new ArgumentNullException(nameof(invoker));

            HandlerType = handlerType;
            MessageType = messageType;
            Invoker = invoker;
        }

        public Task Invoke(object handler, object message) => Invoker(handler, message);
    }
}