using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Routemeister.UnitTests
{
    [TestFixture]
    public abstract class UnitTestsOf<T> : UnitTests, IDisposable
    {
        protected T UnitUnderTest { get; set; }

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
        protected virtual void OnBeforeEachTest() { }

        [TearDown]
        protected virtual void OnAfterEachTest() { }

        [OneTimeSetUp]
        protected virtual void OnBeforeAllTests() { }

        [OneTimeTearDown]
        protected virtual void OnAfterAllTests() { }
    }
}