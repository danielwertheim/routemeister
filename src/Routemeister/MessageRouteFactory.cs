using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Routemeister
{
    public class MessageRouteFactory
    {
        protected MessageHandlerCreator MessageHandlerCreator { get; }

        /// <summary>
        /// Creates the factory. Use the <paramref name="messageHandlerCreator"/> to hook
        /// in a strategy for how the message handlers instances should be created, e.g. using your IoC.
        /// </summary>
        /// <param name="messageHandlerCreator"></param>
        public MessageRouteFactory(MessageHandlerCreator messageHandlerCreator)
        {
            if (messageHandlerCreator == null)
                throw new ArgumentNullException(nameof(messageHandlerCreator));

            MessageHandlerCreator = messageHandlerCreator;
        }

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

            var messageRoutes = new Dictionary<Type, List<Func<MessageEnvelope, Task>>>();

            foreach (var groupedMessageHandlerInfos in GetMessageHandlerInfos(assemblies, messageHandlerMarker).GroupBy(i => i.Key))
            {
                foreach (var messageHandlerInfo in groupedMessageHandlerInfos.SelectMany(i => i.Value))
                {
                    var messageHandler = CreateMessageHandler(messageHandlerInfo);

                    List<Func<MessageEnvelope, Task>> messageHandlers;
                    if (!messageRoutes.TryGetValue(messageHandlerInfo.MessageType, out messageHandlers))
                        messageRoutes[messageHandlerInfo.MessageType] = new List<Func<MessageEnvelope, Task>>();

                    messageRoutes[messageHandlerInfo.MessageType].Add(messageHandler);
                }
            }

            return new MessageRoutes
            {
                messageRoutes.Select(mr => new MessageRoute(mr.Key, mr.Value.ToArray()))
            };
        }

        private Func<MessageEnvelope, Task> CreateMessageHandler(MessageHandlerInfo messageHandlerInfo)
        {
            return envelope => messageHandlerInfo.MessageHandlerInvoker.Invoke(
                MessageHandlerCreator(messageHandlerInfo.MessageHandlerContainerType, envelope),
                envelope.Message);
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

            if (!messageHandlerMarker.IsInterface)
                throw new ArgumentException(
                    "The message handler marker interface needs to be an interface.",
                    nameof(messageHandlerMarker));

            if (!messageHandlerMarker.IsGenericType)
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

            if (messageHandlerMarker.GetMembers().Any(m => m != method))
                throw new ArgumentException(
                    "The message handler marker interface needs to have exactly one member being the method handling the message being routed.",
                    nameof(messageHandlerMarker));
        }

        private static IEnumerable<KeyValuePair<Type, MessageHandlerInfo[]>> GetMessageHandlerInfos(IEnumerable<Assembly> assemblies, Type messageHandlerMarker)
        {
            var messageHandlerMethodName = ExtractMessageHandlerMethodName(messageHandlerMarker);

            return assemblies
                .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
                .Select(messageHandlerContainerType => new
                {
                    MessageHandlerContainerType = messageHandlerContainerType,
                    MessageHandlers = messageHandlerContainerType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == messageHandlerMarker)
                        .Select(i => new MessageHandlerInfo(
                            messageHandlerContainerType,
                            i.GetGenericArguments()[0],
                            GetMessageHandlerInvoker(messageHandlerContainerType, i.GetGenericArguments()[0], messageHandlerMethodName)))
                        .ToArray()
                })
                .Where(subscription => subscription.MessageHandlers.Any())
                .Select(subscription => new KeyValuePair<Type, MessageHandlerInfo[]>(subscription.MessageHandlerContainerType, subscription.MessageHandlers));
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