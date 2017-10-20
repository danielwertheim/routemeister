using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class MiddlewareEnabledAsyncRouter : IAsyncRouter
    {
        private readonly MessageHandlerCreator _messageHandlerCreator;
        private readonly MessageRoutes _messageRoutes;
        private readonly Stack<Func<Func<MessageEnvelope, Task>, Func<MessageEnvelope, Task>>> _middlewares;

        public MiddlewareEnabledAsyncRouter(MessageHandlerCreator messageHandlerCreator, MessageRoutes messageRoutes)
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
            var route = _messageRoutes.GetRoute(message.GetType());
            var envelope = new MessageEnvelope(message, route.MessageType);

            var routeActions = route.Actions.Select(a => Tuple.Create(a, _messageHandlerCreator(a.HandlerType, envelope))).ToList();
            foreach (var routeAction in routeActions)
            {
                var action = routeAction.Item1;
                var handler = routeAction.Item2;
                if (handler == null)
                    throw new InvalidOperationException(
                        $"Message handler of type {action.HandlerType.FullName} created for message type {action.MessageType.FullName} was null.");
            }
            if (!_middlewares.Any())
            {
                foreach (var routeAction in routeActions)
                {
                    var action = routeAction.Item1;
                    var handler = routeAction.Item2;
                    var resultingTask = (Task) action.Invoke(handler, envelope.Message);
                    await resultingTask.ConfigureAwait(false);
                }
            }
            else
            {
                foreach (var routeAction in routeActions)
                {
                    var action = routeAction.Item1;
                    var handler = routeAction.Item2;
                    await ProcessAsync(
                        envelope,
                        async e =>
                        {
                            var resultingTask = (Task) action.Invoke(handler, envelope.Message);
                            await resultingTask.ConfigureAwait(false);
                        }
                    ).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessAsync(MessageEnvelope envelope, Func<MessageEnvelope, Task> root)
        {
            Func<MessageEnvelope, Task> prev;

            using (var e = _middlewares.GetEnumerator())
            {
                if (!e.MoveNext())
                    return;

                prev = e.Current.Invoke(root);
                while (e.MoveNext())
                    prev = e.Current.Invoke(prev);
            }

            if (prev != null)
                await prev.Invoke(envelope).ConfigureAwait(false);
        }
    }
}