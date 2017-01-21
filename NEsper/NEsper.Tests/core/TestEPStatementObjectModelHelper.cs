///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;

using com.espertech.esper.core.service;
using NUnit.Framework;



namespace com.espertech.esper.core
{
    [TestFixture]
    public class TestEPStatementObjectModelHelper 
    {
        [Test]
        public void TestRenderEPL()
        {
            Assert.AreEqual("null", TryConstant(null));
            Assert.AreEqual("\"\"", TryConstant(""));
            Assert.AreEqual("1", TryConstant(1));
            Assert.AreEqual("\"abc\"", TryConstant("abc"));
        }
    
        private String TryConstant(Object value)
        {
            StringWriter writer = new StringWriter();
            EPStatementObjectModelHelper.RenderEPL(writer, value);
            return writer.ToString();
        }
    }
}
