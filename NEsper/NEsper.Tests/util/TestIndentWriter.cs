///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;

using NUnit.Framework;


namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestIndentWriter 
    {
        private static String NEWLINE = Environment.NewLine;
        private StringWriter stringWriter;
        private IndentWriter indentWriter;
    
        [SetUp]
        public void SetUp()
        {
            stringWriter = new StringWriter();
            indentWriter = new IndentWriter(stringWriter, 0, 2);
        }
    
        [Test]
        public void TestWrite()
        {
            indentWriter.WriteLine("a");
            AssertWritten("a");
    
            indentWriter.IncrIndent();
            indentWriter.WriteLine("a");
            AssertWritten("  a");
    
            indentWriter.IncrIndent();
            indentWriter.WriteLine("a");
            AssertWritten("    a");
    
            indentWriter.DecrIndent();
            indentWriter.WriteLine("a");
            AssertWritten("  a");
    
            indentWriter.DecrIndent();
            indentWriter.WriteLine("a");
            AssertWritten("a");
    
            indentWriter.DecrIndent();
            indentWriter.WriteLine("a");
            AssertWritten("a");
        }
    
        [Test]
        public void TestCtor()
        {
            try
            {
                new IndentWriter(stringWriter, 0, -1);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
            
            try
            {
                new IndentWriter(stringWriter, -1, 11);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    
        private void AssertWritten(String text)
        {
            Assert.AreEqual(text + NEWLINE, stringWriter.ToString());
            stringWriter.GetStringBuilder().Length = 0;
        }
    }
}
