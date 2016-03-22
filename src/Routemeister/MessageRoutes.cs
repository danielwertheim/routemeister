using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Routemeister
{
    public class MessageRoutes : IEnumerable<MessageRoute>
    {
        private readonly ConcurrentDictionary<Type, MessageRoute> _state = new ConcurrentDictionary<Type, MessageRoute>();

        public bool IsEmpty => _state.Any() == false;

        public IEnumerable<Type> KnownMessageTypes => _state.Keys;

        public MessageRoute this[Type messageType] => _state[messageType];

        public bool HasRoute(Type messageType)
        {
            if (messageType == null)
                throw new ArgumentNullException(nameof(messageType));

            return _state.ContainsKey(messageType);
        }

        public MessageRoutes Add(IEnumerable<MessageRoute> routes)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            foreach (var route in routes)
                Add(route);

            return this;
        }

        public MessageRoutes Add(MessageRoute newRoute)
        {
            if (newRoute == null)
                throw new ArgumentNullException(nameof(newRoute));

            MessageRoute existingRoute;
            if (_state.TryGetValue(newRoute.MessageType, out existingRoute))
            {
                if (ReferenceEquals(existingRoute, newRoute))
                    return this;

                throw new InvalidOperationException($"Route for message type '{newRoute.MessageType.Name}' already exists.");
            }

            if (_state.TryAdd(newRoute.MessageType, newRoute))
                return this;

            throw new Exception($"Could not add new route for message type '{newRoute.MessageType.Name}'. Do not know why.");
        }

        public MessageRoute GetRoute(Type messageType)
        {
            MessageRoute route;

            return _state.TryGetValue(messageType, out route)
                ? route
                : MessageRoute.Empty(messageType);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<MessageRoute> GetEnumerator()
        {
            return _state.Values.GetEnumerator();
        }
    }
}