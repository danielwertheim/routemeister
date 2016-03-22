using System;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class SequentialAsyncMessageRouter : IAsyncMessageRouter
    {
        private readonly MessageHandlerCreator _messageHandlerCreator;
        private readonly MessageRoutes _messageRoutes;

        public SequentialAsyncMessageRouter(MessageHandlerCreator messageHandlerCreator, MessageRoutes messageRoutes)
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
            var messageType = message.GetType();
            var route = _messageRoutes.GetRoute(messageType);
            var envelope = new MessageEnvelope(message, messageType);

            foreach (var action in route.Actions)
                await action.Invoke(_messageHandlerCreator(action.HandlerType,envelope), envelope.Message).ConfigureAwait(false);
        }
    }
}