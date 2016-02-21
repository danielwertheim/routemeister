using System;

namespace Routemeister
{
    internal class MessageRouteAction
    {
        internal readonly Type MessageHandlerType;
        internal readonly Type MessageType;
        internal readonly Type ActionType;

        internal MessageRouteAction(Type messageHandlerType, Type messageType)
        {
            if (messageHandlerType == null)
                throw new ArgumentNullException(nameof(messageHandlerType));

            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            MessageHandlerType = messageHandlerType;
            MessageType = messageType;
            ActionType = typeof(Action<>).MakeGenericType(messageType);
        }
    }
}