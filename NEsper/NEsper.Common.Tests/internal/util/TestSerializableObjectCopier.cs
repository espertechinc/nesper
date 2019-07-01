///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestSerializableObjectCopier : AbstractTestBase
    {
        [Test]
        public void TestCopyEnum()
        {
            SupportEnum enumOne = SupportEnum.ENUM_VALUE_2;
            var objectCopier  =SerializableObjectCopier.GetInstance(container);
            object result = objectCopier.Copy(enumOne);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SupportEnum>());
            Assert.That(result, Is.EqualTo(enumOne));
        }
    }
} // end of namespace