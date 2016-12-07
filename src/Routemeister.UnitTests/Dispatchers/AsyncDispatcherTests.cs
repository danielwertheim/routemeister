using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Routemeister.Dispatchers;

namespace Routemeister.UnitTests.Dispatchers
{
    [TestFixture]
    public class AsyncDispatcherTests : UnitTestsOf<AsyncDispatcher>
    {
        protected override void OnBeforeEachTest()
        {
            var factory = new MessageRouteFactory();
            var routes = new MessageRoutes
            {
                factory.Create(new[] {GetType().Assembly}, typeof (IHandle<>)),
                factory.Create(new[] {GetType().Assembly}, typeof (IAsyncRequestHandlerOf<,>))
            };

            UnitUnderTest = new AsyncDispatcher((t, e) => Activator.CreateInstance(t), routes);
        }

        [Test]
        public async Task SendAsync_Should_send_to_single_receiver()
        {
            var concreteMessageA = new ConcreteMessageA();

            await UnitUnderTest.SendAsync(concreteMessageA);

            concreteMessageA.Data.Should().Contain(new[]
            {
                "HandlerA.HandleAsync<ConcreteMessageA>"
            });
        }

        [Test]
        public async Task PublishAsync_Should_publish_to_multiple_receivers()
        {
            var concreteMessageB = new ConcreteMessageB();

            await UnitUnderTest.PublishAsync(concreteMessageB);

            concreteMessageB.Data.Should().Contain(new[]
            {
                "HandlerA.HandleAsync<ConcreteMessageB>",
                "HandlerB.HandleAsync<ConcreteMessageB>"
            });
        }

        [Test]
        public async Task RequestAsync_Should_send_to_single_receiver()
        {
            var requestMessage = new RequestMessage();

            var data = await UnitUnderTest.RequestAsync(requestMessage);

            data.Should().Contain(new[]
            {
                "HandlerA.HandleAsync<RequestMessage>"
            });
        }

        [Test]
        public async Task SendAsync_Should_invoke_OnBeforeRouting_and_OnAfterRouter_and_pass_state_When_specified()
        {
            var theState = Guid.NewGuid();
            var interceptedMatchingState = false;
            UnitUnderTest.OnBeforeRouting = envelope => envelope.SetState("TheState", theState);
            UnitUnderTest.OnAfterRouted = envelope => interceptedMatchingState = Equals(envelope.GetState("TheState"), theState);

            var concreteMessageA = new ConcreteMessageA();
            await UnitUnderTest.SendAsync(concreteMessageA);

            interceptedMatchingState.Should().BeTrue();
        }

        [Test]
        public async Task PublishAsync_Should_invoke_OnBeforeRouting_and_OnAfterRouter_and_pass_state_When_specified()
        {
            var theState = Guid.NewGuid();
            var interceptedMatchingState = false;
            UnitUnderTest.OnBeforeRouting = envelope => envelope.SetState("TheState", theState);
            UnitUnderTest.OnAfterRouted = envelope => interceptedMatchingState = Equals(envelope.GetState("TheState"), theState);

            var concreteMessageB = new ConcreteMessageB();
            await UnitUnderTest.PublishAsync(concreteMessageB);

            interceptedMatchingState.Should().BeTrue();
        }

        [Test]
        public async Task RequestAsync_Should_invoke_OnBeforeRouting_and_OnAfterRouter_and_pass_state_When_specified()
        {
            var theState = Guid.NewGuid();
            var interceptedMatchingState = false;
            UnitUnderTest.OnBeforeRouting = envelope => envelope.SetState("TheState", theState);
            UnitUnderTest.OnAfterRouted = envelope => interceptedMatchingState = Equals(envelope.GetState("TheState"), theState);

            var requestMessage = new RequestMessage();
            await UnitUnderTest.RequestAsync(requestMessage);

            interceptedMatchingState.Should().BeTrue();
        }

        public interface IHandle<in T>
        {
            Task HandleAsync(T message);
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
            IHandle<ConcreteMessageA>,
            IHandle<ConcreteMessageB>,
            IAsyncRequestHandlerOf<RequestMessage, ConcurrentBag<string>>
        {
            public Task HandleAsync(ConcreteMessageA message)
            {
                message.Data.Add($"{GetType().Name}.HandleAsync<{message.GetType().Name}>");

                return Task.FromResult(0);
            }

            public Task HandleAsync(ConcreteMessageB message)
            {
                message.Data.Add($"{GetType().Name}.HandleAsync<{message.GetType().Name}>");

                return Task.FromResult(0);
            }

            public Task<ConcurrentBag<string>> HandleAsync(RequestMessage request)
            {
                return Task.FromResult(new ConcurrentBag<string>
                {
                    $"{GetType().Name}.HandleAsync<{request.GetType().Name}>"
                });
            }
        }

        public class HandlerB :
            IHandle<ConcreteMessageB>
        {
            public Task HandleAsync(ConcreteMessageB message)
            {
                message.Data.Add($"{GetType().Name}.HandleAsync<{message.GetType().Name}>");

                return Task.FromResult(0);
            }
        }
    }
}