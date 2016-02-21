using System;
using System.Linq;
using System.Threading.Tasks;

namespace Routemeister.Routers
{
    public class SimpleAsyncMessageRouter : IAsyncMessageRouter
    {
        protected MessageRoutes MessageRoutes { get; }

        public SimpleAsyncMessageRouter(MessageRoutes messageRoutes)
        {
            if (messageRoutes == null)
                throw new ArgumentNullException(nameof(messageRoutes));

            MessageRoutes = messageRoutes;
        }

        public async Task RouteAsync<T>(T msg)
        {
            if (msg == null)
                throw new ArgumentNullException(nameof(msg));

            var messageType = msg.GetType();
            var route = MessageRoutes.GetRoute(messageType);
            if(route == null)
                throw new ArgumentException($"Missing route for message type '{messageType.Name}'.", nameof(msg));

            await Task.WhenAll(route.Actions.Select(a => a(msg)));
        }
    }
}