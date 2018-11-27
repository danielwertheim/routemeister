using System;
using System.Collections.Concurrent;
using System.Reflection;
using FluentAssertions;
using Routemeister;
using Routemeister.Dispatchers;
using Xunit;

namespace UnitTests.Dispatchers
{
    public class SyncDispatcherTests : UnitTestsOf<SyncDispatcher>
    {
        protected override void OnBeforeEachTest()
        {
            var factory = new MessageRouteFactory();
            var routes = new MessageRoutes
            {
                factory.Create(new[] {GetType().GetTypeInfo().Assembly}, typeof (IMessageHandler<>)),
                factory.Create(new[] {GetType().GetTypeInfo().Assembly}, typeof (IRequestHandler<,>))
            };

            UnitUnderTest = new SyncDispatcher((t, e) => Activator.CreateInstance(t), routes);
        }

        [Fact]
        public void Send_Should_send_to_single_receiver()
        {
            var concreteMessageA = new ConcreteMessageA();

            UnitUnderTest.Send(concreteMessageA);

            concreteMessageA.Data.Should().Contain(new[]
            {
                "HandlerA.Handle<ConcreteMessageA>"
            });
        }

        [Fact]
        public void Publish_Should_publish_to_multiple_receivers()
        {
            var concreteMessageB = new ConcreteMessageB();

            UnitUnderTest.Publish(concreteMessageB);

            concreteMessageB.Data.Should().Contain(new[]
            {
                "HandlerA.Handle<ConcreteMessageB>",
                "HandlerB.Handle<ConcreteMessageB>"
            });
        }

        [Fact]
        public void Request_Should_send_to_single_receiver()
        {
            var requestMessage = new RequestMessage();

            var data = UnitUnderTest.Request(requestMessage);

            data.Should().Contain(new[]
            {
                "HandlerA.Handle<RequestMessage>"
            });
        }

        [Fact]
        public void Send_Should_invoke_OnBeforeRouting_and_OnAfterRouter_and_pass_state_When_specified()
        {
            var theState = Guid.NewGuid();
            var interceptedMatchingState = false;
            UnitUnderTest.OnBeforeRouting = envelope => envelope.SetState("TheState", theState);
            UnitUnderTest.OnAfterRouted = envelope => interceptedMatchingState = Equals(envelope.GetState("TheState"), theState);

            var concreteMessageA = new ConcreteMessageA();
            UnitUnderTest.Send(concreteMessageA);

            interceptedMatchingState.Should().BeTrue();
        }

        [Fact]
        public void Publish_Should_invoke_OnBeforeRouting_and_OnAfterRouter_and_pass_state_When_specified()
        {
            var theState = Guid.NewGuid();
            var interceptedMatchingState = false;
            UnitUnderTest.OnBeforeRouting = envelope => envelope.SetState("TheState", theState);
            UnitUnderTest.OnAfterRouted = envelope => interceptedMatchingState = Equals(envelope.GetState("TheState"), theState);

            var concreteMessageB = new ConcreteMessageB();
            UnitUnderTest.Publish(concreteMessageB);

            interceptedMatchingState.Should().BeTrue();
        }

        [Fact]
        public void Request_Should_invoke_OnBeforeRouting_and_OnAfterRouter_and_pass_state_When_specified()
        {
            var theState = Guid.NewGuid();
            var interceptedMatchingState = false;
            UnitUnderTest.OnBeforeRouting = envelope => envelope.SetState("TheState", theState);
            UnitUnderTest.OnAfterRouted = envelope => interceptedMatchingState = Equals(envelope.GetState("TheState"), theState);

            var requestMessage = new RequestMessage();
            UnitUnderTest.Request(requestMessage);

            interceptedMatchingState.Should().BeTrue();
        }

        public class ConcreteMessageA
        {
            public ConcurrentBag<string> Data { get; } = new ConcurrentBag<string>();
        }

        public class ConcreteMessageB
        {
            public ConcurrentBag<string> Data { get; } = new ConcurrentBag<string>();
        }

        public class RequestMessage : IRequest<ConcurrentBag<string>> { }

        public class HandlerA :
            IMessageHandler<ConcreteMessageA>,
            IMessageHandler<ConcreteMessageB>,
            IRequestHandler<RequestMessage, ConcurrentBag<string>>
        {
            public void Handle(ConcreteMessageA message)
            {
                message.Data.Add($"{GetType().Name}.{nameof(Handle)}<{message.GetType().Name}>");
            }

            public void Handle(ConcreteMessageB message)
            {
                message.Data.Add($"{GetType().Name}.{nameof(Handle)}<{message.GetType().Name}>");
            }

            public ConcurrentBag<string> Handle(RequestMessage request)
            {
                return new ConcurrentBag<string>
                {
                    $"{GetType().Name}.{nameof(Handle)}<{request.GetType().Name}>",
                };
            }
        }

        public class HandlerB :
            IMessageHandler<ConcreteMessageB>
        {
            public void Handle(ConcreteMessageB message)
            {
                message.Data.Add($"{GetType().Name}.{nameof(Handle)}<{message.GetType().Name}>");
            }
        }
    }
}
