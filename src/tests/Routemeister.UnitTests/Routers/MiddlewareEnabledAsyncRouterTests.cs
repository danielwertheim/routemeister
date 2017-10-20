using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Routemeister.Routers;
using Xunit;

namespace Routemeister.UnitTests.Routers
{
    public class MiddlewareEnabledAsyncRouterTests : UnitTestsOf<MiddlewareEnabledAsyncRouter>
    {
        protected override void OnBeforeEachTest()
        {
            var factory = new MessageRouteFactory();
            var routes = new MessageRoutes
            {
                factory.Create(new[] {GetType().GetTypeInfo().Assembly}, typeof (IHandle<>))
            };

            UnitUnderTest = new MiddlewareEnabledAsyncRouter((t, e) => Activator.CreateInstance(t), routes);
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
        public async Task Should_invoke_middlewares_When_only_one_middleware_exists()
        {
            var interceptionsFromMiddlewares = new List<string>();

            UnitUnderTest.Use(next => async envelope =>
            {
                interceptionsFromMiddlewares.Add($"MW Begin {envelope.MessageType.Name}");
                await next(envelope).ConfigureAwait(false);
                interceptionsFromMiddlewares.Add($"MW End {envelope.MessageType.Name}");
            });

            await UnitUnderTest.RouteAsync(new ConcreteMessageA());
            await UnitUnderTest.RouteAsync(new ConcreteMessageB());

            interceptionsFromMiddlewares.Should().ContainInOrder(
                "MW Begin ConcreteMessageA",
                "MW End ConcreteMessageA",
                "MW Begin ConcreteMessageB",
                "MW End ConcreteMessageB");
        }

        [Fact]
        public async Task Should_invoke_middlewares_When_more_then_one_middleware_exists()
        {
            var interceptionsFromMiddlewares = new List<string>();

            UnitUnderTest.Use(next => async envelope =>
            {
                interceptionsFromMiddlewares.Add($"MW1 Begin {envelope.MessageType.Name}");
                await next(envelope).ConfigureAwait(false);
                interceptionsFromMiddlewares.Add($"MW1 End {envelope.MessageType.Name}");
            });

            UnitUnderTest.Use(next => async envelope =>
            {
                interceptionsFromMiddlewares.Add($"MW2 Begin {envelope.MessageType.Name}");
                await next(envelope).ConfigureAwait(false);
                interceptionsFromMiddlewares.Add($"MW2 End {envelope.MessageType.Name}");
            });

            await UnitUnderTest.RouteAsync(new ConcreteMessageA());
            await UnitUnderTest.RouteAsync(new ConcreteMessageB());

            interceptionsFromMiddlewares.Should().ContainInOrder(
                "MW1 Begin ConcreteMessageA",
                "MW2 Begin ConcreteMessageA",
                "MW2 End ConcreteMessageA",
                "MW1 End ConcreteMessageA",
                "MW1 Begin ConcreteMessageB",
                "MW2 Begin ConcreteMessageB",
                "MW2 End ConcreteMessageB",
                "MW1 End ConcreteMessageB");
        }

        [Fact]
        public async Task Should_not_invoke_third_middleware_When_second_terminates_chain()
        {
            var interceptionsFromMiddlewares = new List<string>();

            UnitUnderTest.Use(next => async envelope =>
            {
                interceptionsFromMiddlewares.Add($"MW1 Begin {envelope.MessageType.Name}");
                await next(envelope).ConfigureAwait(false);
                interceptionsFromMiddlewares.Add($"MW1 End {envelope.MessageType.Name}");
            });

            UnitUnderTest.Use(next => envelope =>
            {
                interceptionsFromMiddlewares.Add($"MW2 Terminating {envelope.MessageType.Name}");
                return Task.FromResult(0);
            });

            UnitUnderTest.Use(next => async envelope =>
            {
                interceptionsFromMiddlewares.Add($"MW3 Begin {envelope.MessageType.Name}");
                await next(envelope).ConfigureAwait(false);
                interceptionsFromMiddlewares.Add($"MW3 End {envelope.MessageType.Name}");
            });

            await UnitUnderTest.RouteAsync(new ConcreteMessageA());
            await UnitUnderTest.RouteAsync(new ConcreteMessageB());

            interceptionsFromMiddlewares.Should().NotContain(e => e.StartsWith("MW3"));
            interceptionsFromMiddlewares.Should().ContainInOrder(
                "MW1 Begin ConcreteMessageA",
                "MW2 Terminating ConcreteMessageA",
                "MW1 End ConcreteMessageA",
                "MW1 Begin ConcreteMessageB",
                "MW2 Terminating ConcreteMessageB",
                "MW1 End ConcreteMessageB");
        }

        [Fact]
        public async Task Should_be_able_to_pass_envelope_state()
        {
            var interceptionsFromMiddlewares = new List<string>();

            UnitUnderTest.Use(next => async envelope =>
            {
                envelope.SetState("MW1", $"MW1 of {envelope.MessageType.Name}");
                await next(envelope).ConfigureAwait(false);
                interceptionsFromMiddlewares.Add($"MW2 in MW1 is '{envelope.GetState("MW2")}'");
            });
            UnitUnderTest.Use(next => async envelope =>
            {
                interceptionsFromMiddlewares.Add($"MW1 in MW2 is '{envelope.GetState("MW1")}'");
                envelope.SetState("MW2", $"MW2 of {envelope.MessageType.Name}");
                await next(envelope).ConfigureAwait(false);
            });

            await UnitUnderTest.RouteAsync(new ConcreteMessageA());
            await UnitUnderTest.RouteAsync(new ConcreteMessageB());

            interceptionsFromMiddlewares.Should().ContainInOrder(
                "MW1 in MW2 is 'MW1 of ConcreteMessageA'",
                "MW2 in MW1 is 'MW2 of ConcreteMessageA'",
                "MW1 in MW2 is 'MW1 of ConcreteMessageB'",
                "MW2 in MW1 is 'MW2 of ConcreteMessageB'");
        }

        [Fact]
        public async Task Should_throw_if_created_handler_is_null()
        {
            var factory = new MessageRouteFactory();
            var routes = new MessageRoutes
            {
                factory.Create(new[] {GetType().GetTypeInfo().Assembly}, typeof (IHandle<>))
            };
            UnitUnderTest = new MiddlewareEnabledAsyncRouter((t, e) => null, routes);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => UnitUnderTest.RouteAsync(new ConcreteMessageA()));

            exception.Message.Should().Be($"Message handler of type {typeof(HandlerA).FullName} created for message type {typeof(ConcreteMessageA).FullName} was null.");
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