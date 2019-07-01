///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestIndentWriter : AbstractTestBase
    {
        [SetUp]
        public void SetUp()
        {
            stringWriter = new StringWriter();
            writer = new IndentWriter(stringWriter, 0, 2);
        }

        private static readonly string NEWLINE = Environment.NewLine;

        private StringWriter stringWriter;
        private IndentWriter writer;

        private void AssertWritten(string text)
        {
            Assert.AreEqual(text + NEWLINE, stringWriter.ToString());
            var buffer = stringWriter.GetStringBuilder();
            stringWriter.GetStringBuilder().Remove(0, buffer.Length);
        }

        [Test]
        public void TestCtor()
        {
            try
            {
                new IndentWriter(stringWriter, 0, -1);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // expected
            }

            try
            {
                new IndentWriter(stringWriter, -1, 11);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // expected
            }
        }

        [Test]
        public void TestWrite()
        {
            writer.WriteLine("a");
            AssertWritten("a");

            writer.IncrIndent();
            writer.WriteLine("a");
            AssertWritten("  a");

            writer.IncrIndent();
            writer.WriteLine("a");
            AssertWritten("    a");

            writer.DecrIndent();
            writer.WriteLine("a");
            AssertWritten("  a");

            writer.DecrIndent();
            writer.WriteLine("a");
            AssertWritten("a");

            writer.DecrIndent();
            writer.WriteLine("a");
            AssertWritten("a");
        }
    }
} // end of namespace
