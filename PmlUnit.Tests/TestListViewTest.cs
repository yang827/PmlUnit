﻿// Copyright (c) 2019 Florian Zimmermann.
// Licensed under the MIT License: https://opensource.org/licenses/MIT
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;

namespace PmlUnit.Tests
{
    [TestFixture]
    [TestOf(typeof(TestListView))]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TestListViewTest
    {
        private CultureInfo InitialCulture;

        private TestListView TestList;
        private TreeView InnerList;

        [OneTimeSetUp]
        public void ClassSetup()
        {
            InitialCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            CultureInfo.CurrentCulture = InitialCulture;
        }


        [SetUp]
        public void Setup()
        {
            TestList = new TestListView();
            InnerList = TestList.FindControl<TreeView>("TestList");
        }

        [TearDown]
        public void TearDown()
        {
            TestList.Dispose();
        }

        [Test]
        public void SetTests_ChecksForNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => TestList.SetTests(null));
        }

        [Test]
        public void SetTests_AssignsTestsToList()
        {
            // Arrange
            var testCase = new TestCaseBuilder("TestCase").AddTest("one").AddTest("two").AddTest("three").Build();
            // Act
            TestList.SetTests(testCase.Tests);
            // Assert
            Assert.AreEqual(1, InnerList.Nodes.Count);
            var node = InnerList.Nodes[0];
            Assert.AreEqual(testCase.Tests.Count, node.Nodes.Count);
            for (int i = 0; i < testCase.Tests.Count; i++)
                Assert.AreEqual(testCase.Tests[i].Name, node.Nodes[i].Text);
        }

        [Test]
        public void SetTests_GroupsTestsByTestCase()
        {
            // Arrange
            var first = new TestCaseBuilder("TestCaseOne").AddTest("one").AddTest("two").AddTest("three").Build();
            var second = new TestCaseBuilder("TestCaseTwo").AddTest("four").AddTest("five").Build();
            // Act
            TestList.SetTests(first.Tests.Concat(second.Tests));
            // Assert
            Assert.AreEqual(2, InnerList.Nodes.Count);
            Assert.AreEqual(first.Tests.Count, InnerList.Nodes[0].Nodes.Count);
            for (int i = 0; i < first.Tests.Count; i++)
                Assert.AreEqual(first.Tests[i].Name, InnerList.Nodes[0].Nodes[i].Text);
            Assert.AreEqual(second.Tests.Count, InnerList.Nodes[1].Nodes.Count);
            for (int i = 0; i < second.Tests.Count; i++)
                Assert.AreEqual(second.Tests[i].Name, InnerList.Nodes[1].Nodes[i].Text);
        }

        [Test]
        public void AllTests_ReturnsAllRegisteredTests()
        {
            // Arrange
            var first = new TestCaseBuilder("TestCaseOne").AddTest("one").AddTest("two").AddTest("three").Build();
            var second = new TestCaseBuilder("TestCaseTwo").AddTest("four").AddTest("five").Build();
            TestList.SetTests(first.Tests.Concat(second.Tests));
            // Act
            var tests = TestList.AllTests;
            // Assert
            Assert.AreEqual(5, tests.Count);
            Assert.AreSame(first.Tests[0], tests[0].Test);
            Assert.AreSame(first.Tests[1], tests[1].Test);
            Assert.AreSame(first.Tests[2], tests[2].Test);
            Assert.AreSame(second.Tests[0], tests[3].Test);
            Assert.AreSame(second.Tests[1], tests[4].Test);
        }

        [Test]
        public void SucceededTests_ReturnsSucceededTests()
        {
            // Arrange
            var first = new TestCaseBuilder("TestCaseOne").AddTest("one").AddTest("two").AddTest("three").Build();
            var second = new TestCaseBuilder("TestCaseTwo").AddTest("four").AddTest("five").Build();
            TestList.SetTests(first.Tests.Concat(second.Tests));
            TestList.AllTests[2].Result = new TestResult(TimeSpan.FromSeconds(1));
            TestList.AllTests[4].Result = new TestResult(TimeSpan.FromSeconds(1));
            // Act
            var tests = TestList.SucceededTests;
            // Assert
            Assert.AreEqual(2, tests.Count);
            Assert.AreSame(first.Tests[2], tests[0].Test);
            Assert.AreSame(second.Tests[1], tests[1].Test);
        }

        [Test]
        public void FailedTests_ReturnsFailedTests()
        {
            // Arrange
            var first = new TestCaseBuilder("TestCaseOne").AddTest("one").AddTest("two").AddTest("three").Build();
            var second = new TestCaseBuilder("TestCaseTwo").AddTest("four").AddTest("five").Build();
            TestList.SetTests(first.Tests.Concat(second.Tests));
            TestList.AllTests[0].Result = new TestResult(TimeSpan.FromSeconds(1), new PmlException("foo"));
            TestList.AllTests[2].Result = new TestResult(TimeSpan.FromSeconds(1), new PmlException("bar"));
            TestList.AllTests[3].Result = new TestResult(TimeSpan.FromSeconds(1), new PmlException("baz"));
            // Act
            var tests = TestList.FailedTests;
            // Assert
            Assert.AreEqual(3, tests.Count);
            Assert.AreSame(first.Tests[0], tests[0].Test);
            Assert.AreSame(first.Tests[2], tests[1].Test);
            Assert.AreSame(second.Tests[0], tests[2].Test);
        }

        [Test]
        public void NotExecutedTests_ReturnsNotExecutedTests()
        {
            // Arrange
            var first = new TestCaseBuilder("TestCaseOne").AddTest("one").AddTest("two").AddTest("three").Build();
            var second = new TestCaseBuilder("TestCaseTwo").AddTest("four").AddTest("five").Build();
            TestList.SetTests(first.Tests.Concat(second.Tests));
            TestList.AllTests[1].Result = new TestResult(TimeSpan.FromSeconds(1));
            TestList.AllTests[3].Result = new TestResult(TimeSpan.FromSeconds(1), new PmlException("error"));
            // Act
            var tests = TestList.NotExecutedTests;
            // Assert
            Assert.AreEqual(3, tests.Count);
            Assert.AreSame(first.Tests[0], tests[0].Test);
            Assert.AreSame(first.Tests[2], tests[1].Test);
            Assert.AreSame(second.Tests[1], tests[2].Test);
        }

        [Test]
        public void SetTests_AssignsUnknownTestStatusIconToListItems()
        {
            // Arrange
            var testCase = new TestCaseBuilder("TestCase").AddTest("one").Build();
            // Act
            TestList.SetTests(testCase.Tests);
            // Assert
            Assert.AreEqual("Unknown", InnerList.Nodes[0].Nodes[0].ImageKey);
        }
    }
}
