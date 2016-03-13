using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Routemeister.UnitTests
{
    [TestFixture]
    public class MessageRoutesTests : UnitTestsOf<MessageRoutes>
    {
        protected override void OnBeforeEachTest()
        {
            UnitUnderTest = new MessageRoutes();
        }

        [Test]
        public void Add_of_enumerable_Should_add_two_When_two_routes_are_passed()
        {
            var routes = new[]
            {
                CreateMessageRoute<ConcreteMessageA>(),
                CreateMessageRoute<ConcreteMessageB>()
            };

            UnitUnderTest.Add(routes);

            UnitUnderTest.Should().Contain(routes);
        }

        [Test]
        public void Add_of_single_Should_add()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();

            UnitUnderTest.Add(route);

            UnitUnderTest.Should().Contain(route);
        }

        [Test]
        public void Add_of_single_Should_not_add_When_same_route_is_added_twice()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();

            UnitUnderTest.Add(route);
            UnitUnderTest.Add(route);

            UnitUnderTest.Should().HaveCount(1);
        }

        [Test]
        public void Add_of_single_Should_add_When_two_routes_with_different_message_type_are_added()
        {
            var routeA = CreateMessageRoute<ConcreteMessageA>();
            var routeB = CreateMessageRoute<ConcreteMessageB>();

            UnitUnderTest.Add(routeA);
            UnitUnderTest.Add(routeB);

            UnitUnderTest.Should().HaveCount(2);
        }

        [Test]
        public void Add_of_single_Should_not_add_When_two_routes_with_same_message_type_are_added()
        {
            var routeA1 = CreateMessageRoute<ConcreteMessageA>();
            var routeA2 = CreateMessageRoute<ConcreteMessageA>();

            UnitUnderTest.Add(routeA1);

            Action action = () => UnitUnderTest.Add(routeA2);
            action
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Route for message type 'ConcreteMessageA' already exists.");
        }

        [Test]
        public void IsEmpty_Should_return_false_When_no_routes_has_been_added()
        {
            UnitUnderTest.IsEmpty.Should().BeTrue();
        }

        [Test]
        public void IsEmpty_Should_return_true_When_routes_has_been_added()
        {
            UnitUnderTest.Add(CreateMessageRoute<ConcreteMessageA>());

            UnitUnderTest.IsEmpty.Should().BeFalse();
        }

        [Test]
        public void KnownMessageTypes_Should_yield_no_types_When_no_routes_has_been_added()
        {
            UnitUnderTest.KnownMessageTypes.Should().BeEmpty();
        }

        [Test]
        public void KnownMessageTypes_Should_yield_message_types_When_routes_has_been_added()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();
            UnitUnderTest.Add(route);

            UnitUnderTest.KnownMessageTypes.Should().Contain(route.MessageType);
        }

        [Test]
        public void Indexer_Should_yield_route_by_message_type_When_route_for_it_exists()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();
            UnitUnderTest.Add(route);

            UnitUnderTest[route.MessageType].Should().Be(route);
        }

        [Test]
        public void HasRoute_Should_return_false_When_no_route_exist_for_message_type()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();
            UnitUnderTest.Add(route);

            UnitUnderTest.HasRoute(typeof(ConcreteMessageB)).Should().BeFalse();
        }

        [Test]
        public void HasRoute_Should_return_true_When_route_exist_for_message_type()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();
            UnitUnderTest.Add(route);

            UnitUnderTest.HasRoute(route.MessageType).Should().BeTrue();
        }

        [Test]
        public void GetRoute_Should_return_empty_route_When_no_route_exist_for_message_type()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();
            UnitUnderTest.Add(route);

            var retrievedRoute = UnitUnderTest.GetRoute(typeof(ConcreteMessageB));

            retrievedRoute.Should().NotBeNull();
            retrievedRoute.Actions.Should().BeEmpty();
        }

        [Test]
        public void GetRoute_Should_return_route_When_route_exist_for_message_type()
        {
            var route = CreateMessageRoute<ConcreteMessageA>();
            UnitUnderTest.Add(route);

            UnitUnderTest.GetRoute(route.MessageType).Should().Be(route);
        }

        private static MessageRoute CreateMessageRoute<T>()
        {
            Func<object, Task> fakeAction = message => Task.FromResult(0);

            return new MessageRoute(typeof(T), new[] { fakeAction });
        }

        public class ConcreteMessageA { }

        public class ConcreteMessageB { }
    }
}