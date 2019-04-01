///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.supportunit.bean;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestSerializableObjectCopier 
    {
        [Test]
        public void TestCopyEnum()
        {
            SupportEnum enumOne = SupportEnum.ENUM_VALUE_2;
            Object result = SerializableObjectCopier.Copy(null, enumOne);
            Assert.AreEqual(result, enumOne);
            Assert.IsTrue(Equals(result, enumOne));
        }
    }
}
