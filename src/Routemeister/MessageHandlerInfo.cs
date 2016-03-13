using System;

namespace Routemeister
{
    internal class MessageHandlerInfo
    {
        internal readonly Type MessageHandlerContainerType;
        internal readonly Type MessageType;
        internal readonly MessageHandlerInvoker MessageHandlerInvoker;

        internal MessageHandlerInfo(Type messageHandlerContainerType, Type messageType, MessageHandlerInvoker messageHandlerInvoker)
        {
            if (messageHandlerContainerType == null)
                throw new ArgumentNullException(nameof(messageHandlerContainerType));

            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            if (messageHandlerInvoker == null)
                throw new ArgumentNullException(nameof(messageHandlerInvoker));

            MessageHandlerContainerType = messageHandlerContainerType;
            MessageType = messageType;
            MessageHandlerInvoker = messageHandlerInvoker;
        }
    }
}