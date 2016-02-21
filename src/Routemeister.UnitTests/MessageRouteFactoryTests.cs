using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Routemeister.UnitTests
{
    [TestFixture]
    public class MessageRouteFactoryTests
    {
        private readonly MessageRouteFactory _factory;

        public MessageRouteFactoryTests()
        {
            _factory = new MessageRouteFactory(Activator.CreateInstance);
        }

        [Test]
        public void Should_create_two_routes_with_two_actions_each_When_two_concrete_message_handlers_in_two_classes_exists()
        {
            var routes = _factory.Create(new[] { GetType().Assembly }, typeof(IHandleForCaseA<>));

            routes.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(ConcreteMessageA)).Actions.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(ConcreteMessageB)).Actions.Should().HaveCount(2);
        }

        [Test]
        public void Should_create_two_routes_with_two_actions_each_When_two_interface_message_handlers_in_two_classes_exists()
        {
            var routes = _factory.Create(new[] { GetType().Assembly }, typeof(IHandleForCaseB<>));

            routes.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(INonConcreteMessageA)).Actions.Should().HaveCount(2);
            routes.Single(r => r.MessageType == typeof(INonConcreteMessageB)).Actions.Should().HaveCount(2);
        }
    }

    internal interface IHandleForCaseA<in T>
    {
        void Handle(T message);
    }

    internal interface IHandleForCaseB<in T>
    {
        void Handle(T message);
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
        public void Handle(ConcreteMessageA message)
        {
        }

        public void Handle(ConcreteMessageB message)
        {
        }

        public void Handle(INonConcreteMessageA message)
        {
        }

        public void Handle(INonConcreteMessageB message)
        {
        }
    }

    public class HandlerB :
        IHandleForCaseA<ConcreteMessageA>,
        IHandleForCaseA<ConcreteMessageB>,
        IHandleForCaseB<INonConcreteMessageA>,
        IHandleForCaseB<INonConcreteMessageB>
    {
        public void Handle(ConcreteMessageA message)
        {
        }

        public void Handle(ConcreteMessageB message)
        {
        }

        public void Handle(INonConcreteMessageA message)
        {
        }

        public void Handle(INonConcreteMessageB message)
        {
        }
    }
}