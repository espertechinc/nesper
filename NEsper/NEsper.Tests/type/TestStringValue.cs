///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestStringValue 
    {
        [Test]
        public void TestParse()
        {
            Assert.AreEqual("a", StringValue.ParseString("\"a\""));
            Assert.AreEqual("", StringValue.ParseString("\"\""));
            Assert.AreEqual("", StringValue.ParseString("''"));
            Assert.AreEqual("b", StringValue.ParseString("'b'"));
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("\"");
            TryInvalid("'");
        }
    
        private void TryInvalid(String invalidString)
        {
            try
            {
                StringValue.ParseString(invalidString);
            }
            catch (ArgumentException)
            {
                // Expected exception
            }
    
        }
    }
}
