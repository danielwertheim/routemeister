using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Routemeister.Routers;

namespace Routemeister.UnitTests.Routers
{
    [TestFixture]
    public class SequentialAsyncRouterTests : UnitTestsOf<SequentialAsyncRouter>
    {
        protected override void OnBeforeEachTest()
        {
            var factory = new MessageRouteFactory();
            var routes = new MessageRoutes
            {
                factory.Create(new[] {GetType().Assembly}, typeof (IHandle<>))
            };

            UnitUnderTest = new SequentialAsyncRouter((t, e) => Activator.CreateInstance(t), routes);
        }

        [Test]
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