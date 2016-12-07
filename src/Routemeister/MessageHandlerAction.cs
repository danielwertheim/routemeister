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

            if (messageType.IsValueType)
                throw new ArgumentException(
                    $"The message type {messageType.FullName} is a value type. In order to get away from boxing and unboxing, please do not use value types.",
                    nameof(messageType));

            HandlerType = handlerType;
            MessageType = messageType;
            Invoker = invoker;
        }

        public Task Invoke(object handler, object message) => Invoker(handler, message);
    }
}