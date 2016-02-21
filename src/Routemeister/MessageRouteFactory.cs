using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Routemeister
{
    public class MessageRouteFactory
    {
        protected Func<Type, object> MessageHandlerCreator { get; }

        /// <summary>
        /// Creates the factory. Use the <paramref name="messageHandlerCreator"/> to hook
        /// in a strategy for how the message handlers instances should be created, e.g. using your IoC.
        /// </summary>
        /// <param name="messageHandlerCreator"></param>
        public MessageRouteFactory(Func<Type, object> messageHandlerCreator)
        {
            if (messageHandlerCreator == null)
                throw new ArgumentNullException(nameof(messageHandlerCreator));

            MessageHandlerCreator = messageHandlerCreator;
        }

        /// <summary>
        /// Creates message routes.
        /// </summary>
        /// <param name="assemblies">At least one is required. Scanned for message handlers.</param>
        /// <param name="messageHandlerMarker">
        /// Ensure it is an generic interface containing one member only,
        /// which is a method accepting one argument.
        /// </param>
        /// <returns>Message routes</returns>
        public MessageRoute[] Create(Assembly[] assemblies, Type messageHandlerMarker)
        {
            EnsureValidAssemblies(assemblies);
            EnsureValidMessageHandlerMarker(messageHandlerMarker);

            var messageHandlerMethodName = ExtractMessageHandlerMethodName(messageHandlerMarker);
            var messageRoutes = new Dictionary<Type, MessageRoute>();
            var messageRouteActions = GetMessageRouteActions(assemblies, messageHandlerMarker);

            foreach (var kv in messageRouteActions)
            {
                var messageHandler = MessageHandlerCreator(kv.Key);
                foreach (var messageRouteAction in kv.Value)
                {
                    var actionMethod = GetMessageHandlerMethod(messageHandlerMethodName, messageRouteAction);
                    var actionDelegate = Delegate.CreateDelegate(messageRouteAction.ActionType, messageHandler, actionMethod);

                    var cfn = ActionConverter.MakeGenericMethodFor(messageRouteAction.MessageType);
                    var action = (Action<object>)cfn.Invoke(this, new object[] { actionDelegate });

                    if (!messageRoutes.ContainsKey(messageRouteAction.MessageType))
                        messageRoutes[messageRouteAction.MessageType] = new MessageRoute(messageRouteAction.MessageType);

                    messageRoutes[messageRouteAction.MessageType].Actions.Add(action);
                }
            }

            return messageRoutes.Values.ToArray();
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

        private static string ExtractMessageHandlerMethodName(Type messageHandlerMarker)
        {
            var methods = messageHandlerMarker.GetMethods();
            if (methods.Length != 1)
                throw new ArgumentException($"Sent message handler marker type '{messageHandlerMarker.Name}' needs to have one method only.");

            return methods.Single().Name;
        }

        private static IEnumerable<KeyValuePair<Type, MessageRouteAction[]>> GetMessageRouteActions(IEnumerable<Assembly> assemblies, Type messageHandlerMarker)
        {
            return assemblies
                .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
                .Select(messageHandlerType => new
                {
                    MessageHandlerType = messageHandlerType,
                    MessageRouteActions = messageHandlerType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == messageHandlerMarker)
                        .Select(i => new MessageRouteAction(messageHandlerType, i.GetGenericArguments()[0]))
                        .ToArray()
                })
                .Where(subscription => subscription.MessageRouteActions.Any())
                .Select(subscription => new KeyValuePair<Type, MessageRouteAction[]>(subscription.MessageHandlerType, subscription.MessageRouteActions));
        }

        private static MethodInfo GetMessageHandlerMethod(string methodHandlerMethodName, MessageRouteAction messageRouteAction)
        {
            return messageRouteAction.MessageHandlerType.GetMethod(methodHandlerMethodName, new[] { messageRouteAction.MessageType });
        }
    }
}