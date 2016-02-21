using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Routemeister
{
    public class MessageRoutes : IEnumerable<MessageRoute>
    {
        protected ConcurrentDictionary<Type, MessageRoute> Items { get; }

        public bool IsEmpty => Items.Any() == false;

        public IEnumerable<Type> KnownMessageTypes => Items.Keys;

        public MessageRoute this[Type messageType] => Items[messageType];

        public MessageRoutes()
        {
            Items = new ConcurrentDictionary<Type, MessageRoute>();
        }

        public bool Contains(MessageRoute route)
        {
            return Items.ContainsKey(route.MessageType);
        }

        public MessageRoutes Add(IEnumerable<MessageRoute> routes)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            foreach (var route in routes)
                Add(route);

            return this;
        }

        public MessageRoutes Add(MessageRoute route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (!Contains(route))
            {
                Items.TryAdd(route.MessageType, route);
                return this;
            }

            var existingRoute = Items[route.MessageType];
            if(ReferenceEquals(existingRoute, route))
                return this;

            foreach (var action in route.Actions)
                existingRoute.Actions.Add(action);

            return this;
        }

        public MessageRoute GetRoute(Type messageType)
        {
            return Items.ContainsKey(messageType) ? Items[messageType] : null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<MessageRoute> GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }
    }
}