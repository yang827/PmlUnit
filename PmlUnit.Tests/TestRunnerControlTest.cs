﻿// Copyright (c) 2019 Florian Zimmermann.
// Licensed under the MIT License: https://opensource.org/licenses/MIT
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using Moq;

using NUnit.Framework;

namespace PmlUnit.Tests
{
    [TestFixture]
    [TestOf(typeof(PmlTestRunner))]
    [Apartment(ApartmentState.STA)]
    public class TestRunnerControlTest
    {
        [Test]
        public void Constructor_ShouldCheckForNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new TestRunnerControl(null, null, null));

            Assert.Throws<ArgumentNullException>(() => new TestRunnerControl(Mock.Of<TestCaseProvider>(), null, null));
            Assert.Throws<ArgumentNullException>(() => new TestRunnerControl(null, Mock.Of<AsyncTestRunner>(), null));
            Assert.Throws<ArgumentNullException>(() => new TestRunnerControl(null, null, Mock.Of<SettingsProvider>()));

            Assert.Throws<ArgumentNullException>(() => new TestRunnerControl(Mock.Of<TestCaseProvider>(), Mock.Of<AsyncTestRunner>(), null));
            Assert.Throws<ArgumentNullException>(() => new TestRunnerControl(Mock.Of<TestCaseProvider>(), null, Mock.Of<SettingsProvider>()));
            Assert.Throws<ArgumentNullException>(() => new TestRunnerControl(null, Mock.Of<AsyncTestRunner>(), Mock.Of<SettingsProvider>()));
        }

        [Test]
        public void Dispose_DisposesTestRunner()
        {
            var runnerMock = new Mock<AsyncTestRunner>();
            var control = new TestRunnerControl(Mock.Of<TestCaseProvider>(), runnerMock.Object, Mock.Of<SettingsProvider>());
            // Act
            control.Dispose();
            // Assert
            runnerMock.Verify(runner => runner.Dispose());
        }
    }

    [TestFixture]
    [TestOf(typeof(PmlTestRunner))]
    [Apartment(ApartmentState.STA)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TestRunnerControlProviderTest
    {
        private List<TestCase> TestCases;
        private Mock<TestCaseProvider> ProviderMock;
        private TestRunnerControl RunnerControl;
        private TestListView TestList;

        [SetUp]
        public void Setup()
        {
            TestCases = new List<TestCase>();
            var first = new TestCase("Foo", "foo.pmlobj");
            first.Tests.Add("one");
            first.Tests.Add("two");
            TestCases.Add(first);
            var second = new TestCase("Bar", "bar.pmlobj");
            second.Tests.Add("three");
            second.Tests.Add("four");
            second.Tests.Add("five");
            TestCases.Add(second);

            ProviderMock = new Mock<TestCaseProvider>();
            ProviderMock.Setup(provider => provider.GetTestCases()).Returns(TestCases);

            RunnerControl = new TestRunnerControl(ProviderMock.Object, Mock.Of<AsyncTestRunner>(), Mock.Of<SettingsProvider>());
            TestList = RunnerControl.FindControl<TestListView>("TestList");
        }

        [TearDown]
        public void TearDown()
        {
            RunnerControl.Dispose();
        }

        [Test]
        public void LoadTests_LoadsTestsFromTheTestCaseProvider()
        {
            // Act
            RunnerControl.LoadTests();
            // Assert
            ProviderMock.Verify(provider => provider.GetTestCases());
        }

        [Test]
        public void LoadTests_AddsTestsToList()
        {
            // Act
            RunnerControl.LoadTests();
            // Assert
            var allTests = TestList.AllTests;
            Assert.That(allTests, Is.EquivalentTo(TestCases[0].Tests.Concat(TestCases[1].Tests)));
        }
    }

    [TestFixture]
    [TestOf(typeof(PmlTestRunner))]
    [Apartment(ApartmentState.STA)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TestRunnerControlRunTest
    {
        private TestCase TestCase;
        private Mock<AsyncTestRunner> RunnerMock;
        private TestRunnerControl RunnerControl;
        private TestListView TestList;
        private TestListViewModel Model;
        private TestSummaryView TestSummary;
        private IEnumerable<Test> Tests;

        [SetUp]
        public void Setup()
        {
            TestCase = new TestCase("TestCase", "testcase.pmlobj");
            TestCase.Tests.Add("one").Result = new TestResult(TimeSpan.FromSeconds(1));
            TestCase.Tests.Add("two").Result = new TestResult(TimeSpan.FromSeconds(1), new PmlError("error"));
            TestCase.Tests.Add("three").Result = new TestResult(TimeSpan.FromSeconds(1));
            TestCase.Tests.Add("four");

            RunnerMock = new Mock<AsyncTestRunner>();
            RunnerMock
                .Setup(runner => runner.RunAsync(It.IsAny<IEnumerable<Test>>()))
                .Callback((IEnumerable<Test> tests) => Tests = tests);

            RunnerControl = new TestRunnerControl(Mock.Of<TestCaseProvider>(), RunnerMock.Object, Mock.Of<SettingsProvider>());
            TestSummary = RunnerControl.FindControl<TestSummaryView>("TestSummary");
            TestList = RunnerControl.FindControl<TestListView>("TestList");
            TestList.TestCases.Add(TestCase);

            Model = TestList.GetModel();

            foreach (var entry in Model.Entries)
            {
                var testEntry = entry as TestListTestEntry;
                if (testEntry != null)
                    testEntry.IsSelected = testEntry.Test.Name == "two" || testEntry.Test.Name == "four";
            }
        }

        [TearDown]
        public void TearDown()
        {
            RunnerControl.Dispose();
        }

        [Test]
        public void RunAllLinkClick_RunsAllTests()
        {
            // Act
            RunEventHandler("OnRunAllLinkClick");
            // Assert
            RunnerMock.Verify(runner => runner.RunAsync(It.IsAny<IEnumerable<Test>>()), Times.Once());
            Assert.That(Tests, Is.EquivalentTo(TestCase.Tests));
        }

        [Test]
        public void RunPassedLinkClick_OnlyRunsPassedTests()
        {
            // Act
            RunEventHandler("OnRunPassedTestsMenuItemClick");
            // Assert
            RunnerMock.Verify(runner => runner.RunAsync(It.IsAny<IEnumerable<Test>>()), Times.Once());
            Assert.That(Tests, Is.EquivalentTo(new List<Test>() { TestCase.Tests["one"], TestCase.Tests["three"] }));
        }

        [Test]
        public void RunFailedLinkClick_OnlyRunsFailedTests()
        {
            // Act
            RunEventHandler("OnRunFailedTestsMenuItemClick");
            // Assert
            RunnerMock.Verify(runner => runner.RunAsync(It.IsAny<IEnumerable<Test>>()), Times.Once());
            Assert.That(Tests, Is.EquivalentTo(new List<Test>() { TestCase.Tests["two"] }));
        }

        [Test]
        public void RunNotExecutedLinkClick_OnlyRunsNotExecutedTests()
        {
            // Act
            RunEventHandler("OnRunNotExecutedTestsMenuItemClick");
            // Assert
            RunnerMock.Verify(runner => runner.RunAsync(It.IsAny<IEnumerable<Test>>()), Times.Once());
            Assert.That(Tests, Is.EquivalentTo(new List<Test>() { TestCase.Tests["four"] }));
        }

        [Test]
        public void RunSelectedLinkClick_OnlyRunsSelectedTests()
        {
            // Act
            RunEventHandler("OnRunSelectedTestsMenuItemClick");
            // Assert
            RunnerMock.Verify(runner => runner.RunAsync(It.IsAny<IEnumerable<Test>>()), Times.Once());
            Assert.That(Tests, Is.EquivalentTo(new List<Test>() { TestCase.Tests["two"], TestCase.Tests["four"] }));
        }

        private void RunEventHandler(string handler)
        {
            var method = RunnerControl.GetType().GetMethod(
                handler, BindingFlags.Instance | BindingFlags.NonPublic, null,
                new Type[] { typeof(object), typeof(EventArgs) }, null
            );
            if (method == null)
            {
                Assert.Fail("Unable to find {0} event handler.", handler);
            }

            method.Invoke(RunnerControl, new object[] { null, EventArgs.Empty });
        }
    }


    [TestFixture]
    [TestOf(typeof(PmlTestRunner))]
    [Apartment(ApartmentState.STA)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TestRunnerControlSelectionTest
    {
        private TestCase TestCase;
        private TestRunnerControl RunnerControl;
        private TestListView TestList;
        private TestListViewModel Model;
        private TestSummaryView TestSummary;
        private TestDetailsView TestDetails;

        [SetUp]
        public void Setup()
        {
            TestCase = new TestCase("Test", "test.pmlobj");
            TestCase.Tests.Add("one");
            TestCase.Tests.Add("two");
            TestCase.Tests.Add("three");
            RunnerControl = new TestRunnerControl(Mock.Of<TestCaseProvider>(), Mock.Of<AsyncTestRunner>(), Mock.Of<SettingsProvider>());
            TestSummary = RunnerControl.FindControl<TestSummaryView>("TestSummary");
            TestDetails = RunnerControl.FindControl<TestDetailsView>("TestDetails");
            TestList = RunnerControl.FindControl<TestListView>("TestList");
            TestList.TestCases.Add(TestCase);
            Model = TestList.GetModel();
        }

        [TearDown]
        public void TearDown()
        {
            RunnerControl.Dispose();
        }

        [Test]
        public void SelectionChanged_AssignsTestOfSingleSelectedEntryToTestDetails()
        {
            foreach (var entry in Model.Entries)
            {
                var testEntry = entry as TestListTestEntry;
                if (testEntry != null)
                {
                    testEntry.IsSelected = true;
                    Assert.AreSame(testEntry.Test, TestDetails.Test, "Should assign test {0}.", testEntry.Test);
                    testEntry.IsSelected = false;
                }
            }
        }

        [Test]
        public void SelectionChanged_ShowsTestDetailsIfExactlyOneEntryIsSelected()
        {
            foreach (var entry in Model.Entries)
            {
                var testEntry = entry as TestListTestEntry;
                if (testEntry != null)
                {
                    testEntry.IsSelected = true;
                    Assert.IsTrue(TestDetails.Visible, "Should show test details for test {0}", testEntry.Test);
                    Assert.IsFalse(TestSummary.Visible, "Should not show test summary for test {0}", testEntry.Test);
                    testEntry.IsSelected = false;
                }
            }
        }

        [Test]
        public void SelectionChanged_ShowsTestSummaryUnlessExactlyOneEntryIsSelected()
        {
            // Act & Assert
            Assert.IsFalse(TestDetails.Visible);
            Assert.IsTrue(TestSummary.Visible);

            foreach (var entry in Model.Entries)
            {
                var testEntry = entry as TestListTestEntry;
                if (testEntry != null)
                    entry.IsSelected = true;
            }

            Assert.IsFalse(TestDetails.Visible);
            Assert.IsTrue(TestSummary.Visible);

            foreach (var entry in Model.Entries)
            {
                var testEntry = entry as TestListTestEntry;
                if (testEntry != null)
                {
                    testEntry.IsSelected = false;
                    Assert.IsFalse(TestDetails.Visible, "Should not show test details for test {0}", testEntry.Test);
                    Assert.True(TestSummary.Visible, "Should show test summary for test {0}", testEntry.Test);
                    testEntry.IsSelected = true;
                }
            }
        }
    }
}
