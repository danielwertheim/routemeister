using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Routemeister.UnitTests
{
    [TestFixture]
    public class MessageRouteFactoryTests : UnitTestsOf<MessageRouteFactory>
    {
        protected override void OnBeforeEachTest()
        {
            UnitUnderTest = new MessageRouteFactory(Activator.CreateInstance);
        }

        [Test]
        public void Should_create_two_routes_with_two_actions_each_When_two_concrete_message_handlers_in_two_classes_exists()
        {
            var routes = UnitUnderTest.Create(new[] { GetType().Assembly }, typeof(IHandleForCaseA<>));

            routes.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(ConcreteMessageA)).Actions.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(ConcreteMessageB)).Actions.Should().HaveCount(2);
        }

        [Test]
        public void Should_create_two_routes_with_two_actions_each_When_two_interface_message_handlers_in_two_classes_exists()
        {
            var routes = UnitUnderTest.Create(new[] { GetType().Assembly }, typeof(IHandleForCaseB<>));

            routes.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(INonConcreteMessageA)).Actions.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(INonConcreteMessageB)).Actions.Should().HaveCount(2);
        }

        public interface IHandleForCaseA<in T>
        {
            Task HandleAsync(T message);
        }

        public interface IHandleForCaseB<in T>
        {
            Task HandleAsync(T message);
        }

        public class ConcreteMessageA { }

        public class ConcreteMessageB { }

        public interface INonConcreteMessageA { }
        public interface INonConcreteMessageB { }

        public class HandlerA :
            IHandleForCaseA<ConcreteMessageA>,
            IHandleForCaseA<ConcreteMessageB>,
            IHandleForCaseB<INonConcreteMessageA>,
            IHandleForCaseB<INonConcreteMessageB>
        {
            public Task HandleAsync(ConcreteMessageA message)
            {
                return Task.FromResult(0);
            }

            public Task HandleAsync(ConcreteMessageB message)
            {
                return Task.FromResult(0);
            }

            public Task HandleAsync(INonConcreteMessageA message)
            {
                return Task.FromResult(0);
            }

            public Task HandleAsync(INonConcreteMessageB message)
            {
                return Task.FromResult(0);
            }
        }

        public class HandlerB :
            IHandleForCaseA<ConcreteMessageA>,
            IHandleForCaseA<ConcreteMessageB>,
            IHandleForCaseB<INonConcreteMessageA>,
            IHandleForCaseB<INonConcreteMessageB>
        {
            public Task HandleAsync(ConcreteMessageA message)
            {
                return Task.FromResult(0);
            }

            public Task HandleAsync(ConcreteMessageB message)
            {
                return Task.FromResult(0);
            }

            public Task HandleAsync(INonConcreteMessageA message)
            {
                return Task.FromResult(0);
            }

            public Task HandleAsync(INonConcreteMessageB message)
            {
                return Task.FromResult(0);
            }
        }
    }
}