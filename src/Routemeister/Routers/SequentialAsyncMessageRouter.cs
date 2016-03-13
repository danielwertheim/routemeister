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

            foreach (var action in route.Actions)
                await action(message).ConfigureAwait(false);
        }
    }
}