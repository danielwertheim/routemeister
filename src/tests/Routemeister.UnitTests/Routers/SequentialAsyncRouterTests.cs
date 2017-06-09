using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Routemeister.Routers;
using Xunit;

namespace Routemeister.UnitTests.Routers
{
    public class SequentialAsyncRouterTests : UnitTestsOf<SequentialAsyncRouter>
    {
        protected override void OnBeforeEachTest()
        {
            var factory = new MessageRouteFactory();
            var routes = new MessageRoutes
            {
                factory.Create(new[] {GetType().GetTypeInfo().Assembly}, typeof (IHandle<>))
            };

            UnitUnderTest = new SequentialAsyncRouter((t, e) => Activator.CreateInstance(t), routes);
        }

        [Fact]
        public async Task Should_route()
        {
            var concreteMessageA = new ConcreteMessageA();
            var concreteMessageB = new ConcreteMessageB();

            await UnitUnderTest.RouteAsync(concreteMessageA);
            await UnitUnderTest.RouteAsync(concreteMessageB);

            concreteMessageA.Data.Should().Contain(new[]
            {
                "HandlerA.HandleAsync<ConcreteMessageA>",
                "HandlerB.HandleAsync<ConcreteMessageA>"
            });
            concreteMessageB.Data.Should().Contain(new[]
            {
                "HandlerA.HandleAsync<ConcreteMessageB>",
                "HandlerB.HandleAsync<ConcreteMessageB>"
            });
        }

        [Fact]
        public async Task Should_invoke_OnBeforeRouting_and_OnAfterRouter_and_pass_state_When_specified()
        {
            var theState = Guid.NewGuid();
            var interceptedMatchingState = false;
            UnitUnderTest.OnBeforeRouting = envelope => envelope.SetState("TheState", theState);
            UnitUnderTest.OnAfterRouted = envelope => interceptedMatchingState = Equals(envelope.GetState("TheState"), theState);

            var concreteMessageA = new ConcreteMessageA();
            await UnitUnderTest.RouteAsync(concreteMessageA);

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

        public class HandlerA :
           IHandle<ConcreteMessageA>,
           IHandle<ConcreteMessageB>
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
        }

        public class HandlerB :
           IHandle<ConcreteMessageA>,
           IHandle<ConcreteMessageB>
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
        }
    }
}