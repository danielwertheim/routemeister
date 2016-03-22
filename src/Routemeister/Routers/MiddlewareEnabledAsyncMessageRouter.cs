using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class MiddlewareEnabledAsyncMessageRouter : IAsyncMessageRouter
    {
        private readonly MessageHandlerCreator _messageHandlerCreator;
        private readonly MessageRoutes _messageRoutes;
        private readonly Stack<Func<Func<MessageEnvelope, Task>, Func<MessageEnvelope, Task>>> _middlewares;

        public MiddlewareEnabledAsyncMessageRouter(MessageHandlerCreator messageHandlerCreator, MessageRoutes messageRoutes)
        {
            if (messageHandlerCreator == null)
                throw new ArgumentNullException(nameof(messageHandlerCreator));

            if (messageRoutes == null)
                throw new ArgumentNullException(nameof(messageRoutes));

            _messageHandlerCreator = messageHandlerCreator;
            _messageRoutes = messageRoutes;
            _middlewares = new Stack<Func<Func<MessageEnvelope, Task>, Func<MessageEnvelope, Task>>>();
        }

        public void Use(Func<Func<MessageEnvelope, Task>, Func<MessageEnvelope, Task>> middleware)
        {
            _middlewares.Push(middleware);
        }

        public async Task RouteAsync<T>(T message)
        {
            var messageType = message.GetType();
            var route = _messageRoutes.GetRoute(messageType);
            var envelope = new MessageEnvelope(message, messageType);

            if (!_middlewares.Any())
                foreach (var action in route.Actions)
                    await action.Invoke(_messageHandlerCreator(action.HandlerType, envelope), envelope.Message).ConfigureAwait(false);
            else
                foreach (var action in route.Actions)
                    await ProcessAsync(
                        envelope,
                        async e => await action.Invoke(_messageHandlerCreator(action.HandlerType, envelope), envelope.Message).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task ProcessAsync(MessageEnvelope envelope, Func<MessageEnvelope, Task> root)
        {
            if (!_middlewares.Any())
                return;

            Func<MessageEnvelope, Task> prev;

            using (var e = _middlewares.GetEnumerator())
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