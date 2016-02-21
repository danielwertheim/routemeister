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
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var messageType = message.GetType();
            var route = MessageRoutes.GetRoute(messageType);
            if(route == null)
                throw new ArgumentException($"Missing route for message type '{messageType.Name}'.", nameof(message));

            foreach (var action in route.Actions)
                await action(message).ConfigureAwait(false);
        }
    }
}