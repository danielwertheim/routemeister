using System;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class SequentialAsyncRouter : IAsyncRouter
    {
        private readonly MessageHandlerCreator _messageHandlerCreator;
        private readonly MessageRoutes _messageRoutes;

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

            foreach (var action in route.Actions)
                await action.Invoke(_messageHandlerCreator(action.HandlerType,envelope), envelope.Message).ConfigureAwait(false);
        }
    }
}