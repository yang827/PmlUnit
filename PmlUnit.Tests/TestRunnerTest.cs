﻿using System;
using System.Collections;
using Moq;
using NUnit.Framework;

namespace PmlUnit.Tests
{
    [TestFixture]
    [TestOf(typeof(TestRunner))]
    public class TestRunnerTest
    {
        [Test]
        public void Constructor_ShouldCheckForNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new TestRunner((ObjectProxy)null));
            Assert.Throws<ArgumentNullException>(() => new TestRunner((Clock)null));
            Assert.Throws<ArgumentNullException>(() => new TestRunner(Mock.Of<ObjectProxy>(), null));
            Assert.Throws<ArgumentNullException>(() => new TestRunner(null, Mock.Of<Clock>()));
            Assert.Throws<ArgumentNullException>(() => new TestRunner(null, null));
        }

        [Test]
        public void Dispose_ShouldDisposeTheProxy()
        {
            var proxy = new Mock<ObjectProxy>();
            var runner = new TestRunner(proxy.Object);
            runner.Dispose();
            proxy.Verify(p => p.Dispose());
        }

        [Test]
        public void Dispose_ShouldOnlyDisposeTheProxyOnce()
        {
            var proxy = new Mock<ObjectProxy>();
            var runner = new TestRunner(proxy.Object);
            runner.Dispose();
            runner.Dispose();
            proxy.Verify(p => p.Dispose(), Times.Once());
        }

        [Test]
        public void Run_ShouldFailAfterBeingDisposed()
        {
            var runner = new TestRunner(Mock.Of<ObjectProxy>(), Mock.Of<Clock>());
            runner.Dispose();
            var testCase = new TestCaseBuilder("Test").AddTest("one").Build();
            Assert.Throws<ObjectDisposedException>(() => runner.Run(testCase));
            Assert.Throws<ObjectDisposedException>(() => runner.Run(testCase.Tests[0]));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Run_ShouldInvokeTheProxysRunMethod(bool hasSetUp, bool hasTearDown)
        {
            var proxy = new Mock<ObjectProxy>();
            var runner = new TestRunner(proxy.Object);
            var builder = new TestCaseBuilder("Test").AddTest("one");
            builder.HasSetUp = hasSetUp;
            builder.HasTearDown = hasTearDown;
            runner.Run(builder.Build().Tests[0]);
            proxy.Verify(p => p.Invoke("run", "Test", "one", hasSetUp, hasTearDown), Times.Exactly(1));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Run_ShouldInvokeTheProxyForAllTestsInTheTestCase(bool hasSetUp, bool hasTearDown)
        {
            var proxy = new Mock<ObjectProxy>();
            var runner = new TestRunner(proxy.Object);
            var builder = new TestCaseBuilder("Test").AddTest("one").AddTest("two").AddTest("three");
            builder.HasSetUp = hasSetUp;
            builder.HasTearDown = hasTearDown;
            runner.Run(builder.Build());
            proxy.Verify(p => p.Invoke("run", "Test", "one", hasSetUp, hasTearDown), Times.Exactly(1));
            proxy.Verify(p => p.Invoke("run", "Test", "two", hasSetUp, hasTearDown), Times.Exactly(1));
            proxy.Verify(p => p.Invoke("run", "Test", "three", hasSetUp, hasTearDown), Times.Exactly(1));
        }

        [Test]
        public void Run_ShouldQueryTheClock()
        {
            var clock = new Mock<Clock>();
            var runner = new TestRunner(Mock.Of<ObjectProxy>(), clock.Object);
            runner.Run(new TestCaseBuilder("TestCase").AddTest("test").Build().Tests[0]);
            clock.VerifyGet(mock => mock.CurrentInstant, Times.Exactly(2));
        }

        [TestCase(20)]
        [TestCase(12)]
        [TestCase(0)]
        [TestCase(-5)]
        [TestCase(3600)]
        [TestCase(86401)]
        public void Run_ShouldReturnTestResultWithElapsedTime(int duration)
        {
            int seconds = 0;
            var clock = new Mock<Clock>();
            clock.SetupGet(mock => mock.CurrentInstant)
                .Returns(() => Instant.FromSeconds(seconds))
                .Callback(() => seconds = duration);
            var runner = new TestRunner(Mock.Of<ObjectProxy>(), clock.Object);
            var result = runner.Run(new TestCaseBuilder("Test").AddTest("method").Build().Tests[0]);
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromSeconds(duration)));
        }

        [TestCase(13)]
        [TestCase(123)]
        [TestCase(128937489)]
        public void Run_ShouldReturnTestResultWithElapsedTimeWhenTestFails(int duration)
        {
            int seconds = 0;
            var proxy = new Mock<ObjectProxy>();
            proxy.Setup(mock => mock.Invoke(It.IsAny<string>(), It.IsAny<object[]>())).Throws<Exception>();
            var clock = new Mock<Clock>();
            clock.SetupGet(mock => mock.CurrentInstant)
                .Returns(() => Instant.FromSeconds(seconds))
                .Callback(() => seconds = duration);
            var runner = new TestRunner(proxy.Object, clock.Object);
            var result = runner.Run(new TestCaseBuilder("Test").AddTest("method").Build().Tests[0]);
            Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromSeconds(duration)));
        }

        [Test]
        public void Run_ShouldReturnSuccessfulResultWhenTestSucceeds()
        {
            var runner = new TestRunner(Mock.Of<ObjectProxy>());
            var result = runner.Run(new TestCaseBuilder("Test").AddTest("method").Build().Tests[0]);
            Assert.That(result.Success);
        }

        [Test]
        public void Run_ShouldReturnFailedResultWhenTestFails()
        {
            var error = new Exception();
            var proxy = new Mock<ObjectProxy>();
            proxy.Setup(mock => mock.Invoke(It.IsAny<string>(), It.IsAny<object[]>())).Throws(error);
            var runner = new TestRunner(proxy.Object);
            var result = runner.Run(new TestCaseBuilder("Test").AddTest("test").Build().Tests[0]);
            Assert.That(!result.Success);
            Assert.That(result.Error, Is.SameAs(error));
        }

        [Test]
        public void Run_ShouldReturnFailedResultWhenTestReturnsError()
        {
            var error = new Hashtable();
            error[1.0] = "This is an error message...";
            error[2.0] = "... which spans two lines";
            var proxy = new Mock<ObjectProxy>();
            proxy.Setup(mock => mock.Invoke(It.IsAny<string>(), It.IsAny<object[]>())).Returns(error);
            var runner = new TestRunner(proxy.Object);
            var result = runner.Run(new TestCaseBuilder("Test").AddTest("test").Build().Tests[0]);
            Assert.That(!result.Success);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Message, Is.EqualTo("This is an error message...\n... which spans two lines\n"));
        }

        [Test]
        public void Run_ShouldDisposeTestReturnValue()
        {
            var result = new Mock<IDisposable>();
            var proxy = new Mock<ObjectProxy>();
            proxy.Setup(mock => mock.Invoke(It.IsAny<string>(), It.IsAny<object[]>())).Returns(result.Object);
            var runner = new TestRunner(proxy.Object);
            runner.Run(new TestCaseBuilder("Test").AddTest("test").Build().Tests[0]);
            result.Verify(mock => mock.Dispose(), Times.Exactly(1));
        }
    }
}