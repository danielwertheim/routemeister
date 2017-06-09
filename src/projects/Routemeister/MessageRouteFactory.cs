using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Routemeister
{
    public class MessageRouteFactory
    {
        /// <summary>
        /// Creates message routes.
        /// </summary>
        /// <param name="assembly">Assembly to be scanned for message handlers</param>
        /// <param name="messageHandlerMarker">
        /// Ensure it is an generic interface containing one member only,
        /// which is a method accepting one argument.
        /// </param>
        /// <returns>Message routes</returns>
        public MessageRoutes Create(Assembly assembly, Type messageHandlerMarker)
        {
            return Create(new[] { assembly }, messageHandlerMarker);
        }

        /// <summary>
        /// Creates message routes.
        /// </summary>
        /// <param name="assemblies">Assemblies to be scanned for message handlers.</param>
        /// <param name="messageHandlerMarker">
        /// Ensure it is an generic interface containing one member only,
        /// which is a method accepting one argument.
        /// </param>
        /// <returns>Message routes</returns>
        public MessageRoutes Create(Assembly[] assemblies, Type messageHandlerMarker)
        {
            EnsureValidAssemblies(assemblies);
            EnsureValidMessageHandlerMarker(messageHandlerMarker);

            var messageHandlerActions = GetMessageHandlerActions(assemblies, messageHandlerMarker);
            var routes = messageHandlerActions
                .GroupBy(action => action.MessageType)
                .Select(a => new MessageRoute(a.Key, a.ToArray()));

            return new MessageRoutes().Add(routes);
        }

        private static void EnsureValidAssemblies(Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            if (assemblies.Length == 0)
                throw new ArgumentException("You need to pass at least one assembly to be scanned for message handlers.", nameof(assemblies));
        }

        private static void EnsureValidMessageHandlerMarker(Type messageHandlerMarker)
        {
            if (messageHandlerMarker == null)
                throw new ArgumentNullException(nameof(messageHandlerMarker));

            var typeInfo = messageHandlerMarker.GetTypeInfo();

            if (!typeInfo.IsInterface)
                throw new ArgumentException(
                    "The message handler marker interface needs to be an interface.",
                    nameof(messageHandlerMarker));

            if (!typeInfo.IsGenericType)
                throw new ArgumentException(
                    "The message handler marker interface needs to be a generic interface.",
                    nameof(messageHandlerMarker));

            if (messageHandlerMarker.GetMethods().Length != 1)
                throw new ArgumentException(
                    "The message handler marker interface needs to have exactly one method.",
                    nameof(messageHandlerMarker));

            var method = messageHandlerMarker.GetMethods().Single();
            if (method.GetParameters().Length != 1)
                throw new ArgumentException(
                    "The message handler marker interface needs to have exactly one method accepting only one argument (the message being routed).",
                    nameof(messageHandlerMarker));

            if (messageHandlerMarker.GetMembers().Any(m => !m.Equals(method)))
                throw new ArgumentException(
                    "The message handler marker interface needs to have exactly one member being the method handling the message being routed.",
                    nameof(messageHandlerMarker));
        }

        private static IEnumerable<IMessageHandlerAction> GetMessageHandlerActions(IEnumerable<Assembly> assemblies, Type messageHandlerMarker)
        {
            var messageHandlerMethodName = ExtractMessageHandlerMethodName(messageHandlerMarker);

            return assemblies
                .SelectMany(a => a
                    .GetTypes()
                    .Select(t => new { Type = t, Info = t.GetTypeInfo() })
                    .Where(ht => ht.Info.IsClass && !ht.Info.IsAbstract))
                .SelectMany(ht => ht.Type
                    .GetInterfaces()
                    .Select(it => new { Type = it, Info = it.GetTypeInfo() })
                    .Where(it => it.Info.IsGenericType && it.Type.GetGenericTypeDefinition() == messageHandlerMarker)
                    .Select(hti => new MessageHandlerAction(
                        ht.Type,
                        hti.Type.GetGenericArguments()[0],
                        GetMessageHandlerInvoker(ht.Type, hti.Type.GetGenericArguments()[0], messageHandlerMethodName))));
        }

        private static string ExtractMessageHandlerMethodName(Type messageHandlerMarker)
        {
            var methods = messageHandlerMarker.GetMethods();
            if (methods.Length != 1)
                throw new ArgumentException($"Sent message handler marker type '{messageHandlerMarker.Name}' needs to have one method only with one argument only.");

            return methods.Single().Name;
        }

        private static MessageHandlerInvoker GetMessageHandlerInvoker(Type messageHandlerContainerType, Type messageType, string messageHandlerMethodName)
        {
            var method = messageHandlerContainerType.GetMethod(messageHandlerMethodName, new[] { messageType });

            return IlMessageHandlerInvokerFactory.GetMethodInvoker(method);
        }
    }
}