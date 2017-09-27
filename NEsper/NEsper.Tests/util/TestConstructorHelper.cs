///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestConstructorHelper 
    {
        [Test]
        public void TestValidInvokeConstructor()
        {
            Object[] paramList = new Object[] { "test", 1 };
            SupportCtorObjectArray objOne = (SupportCtorObjectArray) ConstructorHelper.InvokeConstructor(typeof(SupportCtorObjectArray), paramList);
            Assert.AreEqual(paramList, objOne.Arguments);
    
            SupportCtorInt objTwo = (SupportCtorInt) ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new Object[] { 99 });
            Assert.AreEqual(99, objTwo.SomeValue);
            objTwo = (SupportCtorInt) ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new Object[] { 13 });
            Assert.AreEqual(13, objTwo.SomeValue);
    
            SupportCtorIntObjectArray objThree = (SupportCtorIntObjectArray) ConstructorHelper.InvokeConstructor(typeof(SupportCtorIntObjectArray), new Object[] { 1 });
            Assert.AreEqual(1, objThree.SomeValue);
            objThree = (SupportCtorIntObjectArray) ConstructorHelper.InvokeConstructor(typeof(SupportCtorIntObjectArray), paramList);
            Assert.AreEqual(paramList, objThree.Arguments);
        }
    
        [Test]
        public void TestInvalidInvokeConstructor()
        {
            // No Ctor
            try
            {
                ConstructorHelper.InvokeConstructor(typeof(SupportCtorNone), new Object[0]);
                Assert.Fail();
            }
            catch (MissingMethodException)
            {
                // Expected
            }
    
            // Not matching Ctor - number of paramList
            try
            {
                ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new Object[0]);
                Assert.Fail();
            }
            catch (MissingMethodException)
            {
                // Expected
            }
    
            // Type not matching
            try
            {
                ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new Object[] { "a" });
                Assert.Fail();
            }
            catch (MissingMethodException)
            {
                // Expected
            }
        }
    }
}
