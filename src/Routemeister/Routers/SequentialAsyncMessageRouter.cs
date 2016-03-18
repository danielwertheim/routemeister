using System;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class SequentialAsyncMessageRouter : IAsyncMessageRouter
    {
        protected MessageRoutes MessageRoutes { get; }

        public SequentialAsyncMessageRouter(MessageRoutes messageRoutes)
        {
            if (messageRoutes == null)
                throw new ArgumentNullException(nameof(messageRoutes));

            MessageRoutes = messageRoutes;
        }

        public async Task RouteAsync<T>(T message)
        {
            var messageType = message.GetType();
            var route = MessageRoutes.GetRoute(messageType);
            var envelope = new MessageEnvelope(message, messageType);

            foreach (var action in route.Actions)
                await action(envelope).ConfigureAwait(false);
        }
    }
}