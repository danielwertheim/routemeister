using System;
using System.Linq;
using System.Threading.Tasks;

namespace Routemeister.Dispatchers
{
    public class AsyncDispatcher : IAsyncDispatcher
    {
        private readonly MessageHandlerCreator _messageHandlerCreator;
        private readonly MessageRoutes _messageRoutes;

        public Action<MessageEnvelope> OnBeforeRouting { private get; set; }
        public Action<MessageEnvelope> OnAfterRouted { private get; set; }

        public AsyncDispatcher(MessageHandlerCreator messageHandlerCreator, MessageRoutes messageRoutes)
        {
            if (messageHandlerCreator == null)
                throw new ArgumentNullException(nameof(messageHandlerCreator));

            if (messageRoutes == null)
                throw new ArgumentNullException(nameof(messageRoutes));

            _messageHandlerCreator = messageHandlerCreator;
            _messageRoutes = messageRoutes;
        }

        public async Task SendAsync(object message)
        {
            var route = _messageRoutes.GetRoute(message.GetType());
            if (route.Actions.Length > 1)
                throw new ArgumentException(
                    $"The message '{route.MessageType.FullName}' gave more than one route. This does not make sense in a Send scenario. Use Publish for that or inspect your registered handlers.",
                    nameof(message));

            var action = route.Actions[0];
            var envelope = new MessageEnvelope(message, route.MessageType);

            OnBeforeRouting?.Invoke(envelope);

            try
            {
                var handler = _messageHandlerCreator(action.HandlerType, envelope);
                if (handler == null)
                    throw new InvalidOperationException(
                        $"Message handler of type {action.HandlerType.FullName} created for message type {action.MessageType.FullName} was null.");
                var resultingTask = (Task)action.Invoke(handler, envelope.Message);

                await resultingTask.ConfigureAwait(false);
            }
            finally
            {
                OnAfterRouted?.Invoke(envelope);
            }
        }

        public async Task PublishAsync(object message)
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

        public async Task<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request)
        {
            var route = _messageRoutes.GetRoute(request.GetType());
            if (!route.Actions.Any())
                return default(TResponse);

            if (route.Actions.Length > 1)
                throw new ArgumentException(
                    $"The request '{route.MessageType.FullName}' gave more than one route. This does not make sense in a Request-Response scenario. Use Publish for that or inspect your registered handlers.",
                    nameof(request));

            var action = route.Actions[0];
            var envelope = new MessageEnvelope(request, route.MessageType);

            OnBeforeRouting?.Invoke(envelope);

            try
            {
                var handler = _messageHandlerCreator(action.HandlerType, envelope);
                if (handler == null)
                    throw new InvalidOperationException(
                        $"Message handler of type {action.HandlerType.FullName} created for message type {action.MessageType.FullName} was null.");
                var resultingTask = (Task<TResponse>)action.Invoke(handler, envelope.Message);

                return await resultingTask.ConfigureAwait(false);
            }
            finally
            {
                OnAfterRouted?.Invoke(envelope);
            }
        }
    }
}