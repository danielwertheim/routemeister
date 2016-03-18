using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class MiddlewareEnabledSequentialAsyncMessageRouter : IAsyncMessageRouter
    {
        private readonly Stack<Func<Func<MessageEnvelope, Task>, Func<MessageEnvelope, Task>>> _q = new Stack<Func<Func<MessageEnvelope, Task>, Func<MessageEnvelope, Task>>>();

        protected MessageRoutes MessageRoutes { get; }

        public MiddlewareEnabledSequentialAsyncMessageRouter(MessageRoutes messageRoutes)
        {
            if (messageRoutes == null)
                throw new ArgumentNullException(nameof(messageRoutes));

            MessageRoutes = messageRoutes;
        }

        public void Use(Func<Func<MessageEnvelope, Task>, Func<MessageEnvelope, Task>> middleware)
        {
            _q.Push(middleware);
        }

        public async Task RouteAsync<T>(T message)
        {
            var messageType = message.GetType();
            var route = MessageRoutes.GetRoute(messageType);
            var envelope = new MessageEnvelope(message, messageType);

            if (!_q.Any())
                foreach (var action in route.Actions)
                    await action(envelope).ConfigureAwait(false);
            else
                foreach (var action in route.Actions)
                    await ProcessAsync(
                        envelope,
                        async e => await action(e).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task ProcessAsync(MessageEnvelope envelope, Func<MessageEnvelope, Task> root)
        {
            if (!_q.Any())
                return;

            Func<MessageEnvelope, Task> prev;

            using (var e = _q.GetEnumerator())
            {
                if (!e.MoveNext())
                    return;

                prev = e.Current.Invoke(root);
                while (e.MoveNext())
                    prev = e.Current(prev);
            }

            if (prev != null)
                await prev(envelope).ConfigureAwait(false);
        }
    }
}