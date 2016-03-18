using System;
using System.Collections.Generic;

namespace Routemeister
{
    public class MessageEnvelope
    {
        private readonly Dictionary<string, object> _state;

        public object Message { get; }
        public Type MessageType { get; }

        public MessageEnvelope(object message, Type messageType)
        {
            Message = message;
            MessageType = messageType;
            _state = new Dictionary<string, object>();
        }

        public object GetState(string key)
        {
            return _state[key];
        }

        public void SetState<T>(string key, T value)
        {
            _state[key] = value;
        }
    }
}