using System;

namespace UnitTests
{
    public abstract class UnitTestsOf<T> : UnitTests
    {
        protected T UnitUnderTest { get; set; }

        protected override void OnDisposing()
            => (UnitUnderTest as IDisposable)?.Dispose();
    }

    public abstract class UnitTests : IDisposable
    {
        protected UnitTests()
        {
            OnBeforeEachTest();
        }

        public void Dispose()
        {
            OnAfterEachTest();
            OnDisposing();
        }

        protected virtual void OnBeforeEachTest() { }
        protected virtual void OnAfterEachTest() { }
        protected virtual void OnDisposing() { }
    }
}