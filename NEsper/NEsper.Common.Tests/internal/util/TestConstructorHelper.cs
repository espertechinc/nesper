///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestConstructorHelper : CommonTest
    {
        [Test]
        public void TestValidInvokeConstructor()
        {
            object[] parameters = new object[] { "test", 1 };
            SupportCtorObjectArray objOne = (SupportCtorObjectArray) ConstructorHelper.InvokeConstructor(typeof(SupportCtorObjectArray), parameters);
            Assert.AreEqual(parameters, objOne.Arguments);

            SupportCtorInt objTwo = (SupportCtorInt) ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new object[] { 99 });
            Assert.AreEqual(99, objTwo.SomeValue);
            objTwo = (SupportCtorInt) ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new object[] { new int?(13) });
            Assert.AreEqual(13, objTwo.SomeValue);

            SupportCtorIntObjectArray objThree = (SupportCtorIntObjectArray) ConstructorHelper.InvokeConstructor(typeof(SupportCtorIntObjectArray), new object[] { 1 });
            Assert.AreEqual(1, objThree.SomeValue);
            objThree = (SupportCtorIntObjectArray) ConstructorHelper.InvokeConstructor(typeof(SupportCtorIntObjectArray), parameters);
            Assert.AreEqual(parameters, objThree.Arguments);
        }

        [Test]
        public void TestInvalidInvokeConstructor()
        {
            // No Ctor
            Assert.That(
                ConstructorHelper.InvokeConstructor(typeof(SupportCtorNone), new object[0]),
                Is.Null);

            // Not matching Ctor - number of params
            Assert.That(
                ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new object[0]),
                Is.Null);

            // Type not matching
            Assert.That(
                ConstructorHelper.InvokeConstructor(typeof(SupportCtorInt), new object[] {"a"}),
                Is.Null);
        }
    }
} // end of namespace