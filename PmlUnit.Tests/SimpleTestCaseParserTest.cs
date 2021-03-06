﻿// Copyright (c) 2019 Florian Zimmermann.
// Licensed under the MIT License: https://opensource.org/licenses/MIT
using System;
using System.IO;

using NUnit.Framework;

namespace PmlUnit.Tests
{
    [TestFixture]
    [TestOf(typeof(SimpleTestCaseParser))]
    public class SimpleTestCaseParserTest
    {
        [Test]
        public void Parse_ShouldCheckForNullArguments()
        {
            var parser = new SimpleTestCaseParser();
            Assert.Throws<ArgumentNullException>(() => parser.Parse(null));
            Assert.Throws<ArgumentNullException>(() => parser.Parse(""));
            Assert.Throws<ArgumentNullException>(() => parser.Parse(null, TextReader.Null));
            Assert.Throws<ArgumentNullException>(() => parser.Parse("", TextReader.Null));
            Assert.Throws<ArgumentNullException>(() => parser.Parse("dummy.pmlobj", null));
        }

        [Test]
        public void Parse_ShouldFindTestSuiteName()
        {
            var testCase = Parse(@"define object TestCase
endobject");
            Assert.That(testCase.Name, Is.EqualTo("TestCase"));
        }

        public void Parse_UsesSpecifiedFileNameAsTestCaseFileName()
        {
            var testCase = Parse(
                "C:\\temp\\foobar\\hello\\world.random",
                @"define object TestCase
endobject");
            Assert.That(testCase.FileName, Is.EqualTo("C:\\temp\\foobar\\hello\\world.random"));
        }

        [Test]
        public void Parse_ShouldBeCaseInsensitiveWhenSearchingForTestSuiteName()
        {
            var testCase = Parse(@"DEfinE OBJeCt CaseInsensitive
endobject");
            Assert.That(testCase.Name, Is.EqualTo("CaseInsensitive"));
        }

        [Test]
        public void Parse_ShouldIgnoreAdditionalWhitespaceWhenSearchingForTestSuiteName()
        {
            var testCase = Parse(@"   define    object   MoreWhitespace    
endobject");
            Assert.That(testCase.Name, Is.EqualTo("MoreWhitespace"));
        }

        [Test]
        public void Parse_ShouldThrowExceptionWithoutAnObjectDefinition()
        {
            Assert.Throws<ParserException>(() => Parse(""));
        }

        [Test]
        public void Parse_ShouldIgnoreCommentedObjectDefinitions()
        {
            var testCase = Parse(@"$(
define object IgnoreThisOne
$)
define object TakeThisOneInstead
endobject");
            Assert.That(testCase.Name, Is.EqualTo("TakeThisOneInstead"));
        }

        [Test]
        public void Parse_ShouldThrowExceptionWithMultipleObjectDefinitions()
        {
            Assert.Throws<ParserException>(() => Parse(@"
define object One
endobject

define object Two
endobject"));
        }

        [Test]
        public void Parse_ShouldFindOneTestCase()
        {
            var testCase = Parse(@"
define object TestCase
endobject

define method .testMethodA(!assert is PmlAssert)
endmethod");
            Assert.That(testCase.Tests.Count, Is.EqualTo(1));
            Assert.That(testCase.Tests.Contains("testMethodA"));
        }

        [Test]
        public void Parse_ShouldThrowExceptionForMethodsBeforeObjectDefinition()
        {
            Assert.Throws<ParserException>(() => Parse(@"
define method .testMethod(!assert is PmlAssert)
endmethod


define object TestSuite
endobject"));
        }

        [Test]
        public void Parse_ShouldIgnoreMethodsThatDoNotStartWithTest()
        {
            var testCase = Parse(@"
define object TestSuite
endobject


define method .otherMethod(!assert is PmlAssert)
endmethod");
            Assert.That(testCase.Tests.Count, Is.EqualTo(0));
        }

        [Test]
        public void Parse_ShouldIgnoreMethodsWithIncompatibleSignature()
        {
            var testCase = Parse(@"
define object TestSuite
endobject

define method .testMethod(!foo is Real)
endmethod");
            Assert.That(testCase.Tests.Count, Is.EqualTo(0));
        }

        [Test]
        public void Parse_ShouldNotConsiderArgumentNamesWhenDeterminingCompatibleMethodSignatures()
        {
            var testCase = Parse(@"
define object TestSuite
endobject

define method .testMethod(!somethingNotNamedAssert is PmlAssert)
endmethod");
            Assert.AreEqual(1, testCase.Tests.Count);
            Assert.IsTrue(testCase.Tests.Contains("testMethod"));
        }

        [Test]
        public void Parse_ShouldFindMultipleTestMethods()
        {
            var testCase = Parse(@"
define object TestSuite
endobject

define method .testMethodA(!assert is PmlAssert)
endmethod

define method .testMethodB(!assert is PmlAssert)
endmethod");
            Assert.That(testCase.Tests.Count, Is.EqualTo(2));
            Assert.That(testCase.Tests.Contains("testMethodA"));
            Assert.That(testCase.Tests.Contains("testMethodB"));
        }

        [Test]
        public void Parse_ShouldFindSetUpMethod()
        {
            var testCase = Parse(@"
define object TestSuite
endobject

define method .setUp()
endmethod");
            Assert.That(testCase.HasSetUp);
        }

        [Test]
        public void Parse_ShouldFindTearDownMethod()
        {
            var testCase = Parse(@"
define object TestSuite
endobject

define method .tearDown()
endmethod");
            Assert.That(testCase.HasTearDown);
        }

        private static TestCase Parse(string objectDefinition)
        {
            return Parse("dummy.pmlobj", objectDefinition);
        }

        private static TestCase Parse(string fileName, string objectDefinition)
        {
            var parser = new SimpleTestCaseParser();
            using (var reader = new StringReader(objectDefinition))
            {
                return parser.Parse(fileName, reader);
            }
        }
    }
}
