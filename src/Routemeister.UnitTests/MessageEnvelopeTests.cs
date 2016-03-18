using FluentAssertions;
using NUnit.Framework;

namespace Routemeister.UnitTests
{
    [TestFixture]
    public class MessageEnvelopeTests : UnitTestsOf<MessageEnvelope>
    {
        [Test]
        public void Should_be_able_to_carry_state()
        {
            UnitUnderTest = new MessageEnvelope(new FakeMessage(), typeof(FakeMessage));

            UnitUnderTest.SetState("testInt", 1);
            UnitUnderTest.GetState("testInt").Should().Be(1);

            UnitUnderTest.SetState("testString", "foo");
            UnitUnderTest.GetState("testString").Should().Be("foo");
        }

        private class FakeMessage { }
    }
}