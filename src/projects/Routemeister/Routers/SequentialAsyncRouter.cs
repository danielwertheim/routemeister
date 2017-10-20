using System;
using System.Linq;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class SequentialAsyncRouter : IAsyncRouter
    {
        private readonly MessageHandlerCreator _messageHandlerCreator;
        private readonly MessageRoutes _messageRoutes;

        public Action<MessageEnvelope> OnBeforeRouting { private get; set; }
        public Action<MessageEnvelope> OnAfterRouted { private get; set; }

        public SequentialAsyncRouter(MessageHandlerCreator messageHandlerCreator, MessageRoutes messageRoutes)
        {
            if (messageHandlerCreator == null)
                throw new ArgumentNullException(nameof(messageHandlerCreator));

            if (messageRoutes == null)
                throw new ArgumentNullException(nameof(messageRoutes));

            _messageHandlerCreator = messageHandlerCreator;
            _messageRoutes = messageRoutes;
        }

        public async Task RouteAsync<T>(T message)
        {
            var route = _messageRoutes.GetRoute(message.GetType());
            var envelope = new MessageEnvelope(message, route.MessageType);

            OnBeforeRouting?.Invoke(envelope);

            try
            {
                var routeActions = route.Actions.Select(a => Tuple.Create(a, _messageHandlerCreator(a.HandlerType, envelope))).ToList();
                foreach (var routeAction in routeActions)
                {
                    var action = routeAction.Item1;
                    var handler = routeAction.Item2;
                    if (handler == null)
                        throw new InvalidOperationException(
                            $"Message handler of type {action.HandlerType.FullName} created for message type {action.MessageType.FullName} was null.");
                }
                foreach (var routeAction in routeActions)
                {
                    var action = routeAction.Item1;
                    var handler = routeAction.Item2;
                    var resultingTask = (Task)action.Invoke(handler, envelope.Message);

                    await resultingTask.ConfigureAwait(false);
                }
            }
            finally
            {
                OnAfterRouted?.Invoke(envelope);
            }
        }
    }
}