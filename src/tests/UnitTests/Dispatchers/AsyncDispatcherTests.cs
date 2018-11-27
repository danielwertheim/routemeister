using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Routemeister;
using Routemeister.Dispatchers;
using Xunit;

namespace UnitTests.Dispatchers
{
    public class AsyncDispatcherTests : UnitTestsOf<AsyncDispatcher>
    {
        protected override void OnBeforeEachTest()
        {
            var factory = new MessageRouteFactory();
            var routes = new MessageRoutes
            {
                factory.Create(new[] {GetType().GetTypeInfo().Assembly}, typeof (IAsyncMessageHandler<>)),
                factory.Create(new[] {GetType().GetTypeInfo().Assembly}, typeof (IAsyncRequestHandler<,>))
            };

            UnitUnderTest = new AsyncDispatcher((t, e) => Activator.CreateInstance(t), routes);
        }

        [Fact]
        public async Task SendAsync_Should_send_to_single_receiver()
        {
            var concreteMessageA = new ConcreteMessageA();

            await UnitUnderTest.SendAsync(concreteMessageA);

            concreteMessageA.Data.Should().Contain(new[]
            {
                "HandlerA.HandleAsync<ConcreteMessageA>"
            });
        }

        [Fact]
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

        [Fact]
        public async Task RequestAsync_Should_send_to_single_receiver()
        {
            var requestMessage = new RequestMessage();

            var data = await UnitUnderTest.RequestAsync(requestMessage);

            data.Should().Contain(new[]
            {
                "HandlerA.HandleAsync<RequestMessage>"
            });
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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
            IAsyncMessageHandler<ConcreteMessageA>,
            IAsyncMessageHandler<ConcreteMessageB>,
            IAsyncRequestHandler<RequestMessage, ConcurrentBag<string>>
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
            IAsyncMessageHandler<ConcreteMessageB>
        {
            public Task HandleAsync(ConcreteMessageB message)
            {
                message.Data.Add($"{GetType().Name}.HandleAsync<{message.GetType().Name}>");

                return Task.FromResult(0);
            }
        }
    }
}