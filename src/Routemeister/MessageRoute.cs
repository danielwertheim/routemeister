using System;
using System.Collections.Generic;

namespace Routemeister
{
    public class MessageRoute : IEquatable<MessageRoute>
    {
        public Type MessageType { get; }
        public IList<Action<object>> Actions { get; private set; }

        public MessageRoute(Type messageType)
        {
            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            MessageType = messageType;
            Actions = new List<Action<object>>();
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