using System;
using System.Linq;

namespace Routemeister
{
    public class MessageRoute : IEquatable<MessageRoute>
    {
        public Type MessageType { get; }
        public IMessageHandlerAction[] Actions { get; }

        public static MessageRoute Empty(Type messageType)
        {
            return new MessageRoute(messageType);
        }

        private MessageRoute(Type messageType)
        {
            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            MessageType = messageType;
            Actions = new IMessageHandlerAction[0];
        }

        public MessageRoute(Type messageType, IMessageHandlerAction[] actions) : this(messageType)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            if (!actions.Any())
                throw new ArgumentException("A message route must have actions. No actions were passed.");

            Actions = actions;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MessageRoute);
        }

        public bool Equals(MessageRoute other)
        {
            return other != null
                && (ReferenceEquals(this, other) || MessageType == other.MessageType);
        }

        public override int GetHashCode()
        {
            return MessageType.GetHashCode();
        }
    }
}