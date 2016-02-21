using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Routemeister.UnitTests
{
    [TestFixture]
    public abstract class UnitTestsOf<T> : UnitTests, IDisposable
    {
        protected T UnitUnderTest { get; set; }

        [DebuggerStepThrough]
        public void Dispose()
        {
            var asDisposable = UnitUnderTest as IDisposable;
            asDisposable?.Dispose();
        }
    }

    [TestFixture]
    public abstract class UnitTests
    {
        [SetUp]
        [DebuggerStepThrough]
        protected virtual void OnBeforeEachTest() { }

        [TearDown]
        [DebuggerStepThrough]
        protected virtual void OnAfterEachTest() { }

        [OneTimeSetUp]
        [DebuggerStepThrough]
        protected virtual void OnBeforeAllTests() { }

        [OneTimeTearDown]
        [DebuggerStepThrough]
        protected virtual void OnAfterAllTests() { }
    }
}