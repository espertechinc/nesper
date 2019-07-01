///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    [TestFixture]
    public class TestKeyedMethodPropertyGetter : AbstractTestBase
    {
        private KeyedMethodPropertyGetter getter;
        private EventBean theEvent;
        private SupportBeanComplexProps bean;

        [SetUp]
        public void SetUp()
        {
            bean = SupportBeanComplexProps.MakeDefaultBean();
            theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
            var method = typeof(SupportBeanComplexProps).GetMethod("GetIndexed", new Type[] { typeof(int) });
            getter = new KeyedMethodPropertyGetter(method, 1, null, null);
        }

        [Test]
        public void TestGet()
        {
            Assert.AreEqual(bean.GetIndexed(1), getter.Get(theEvent));
            Assert.AreEqual(bean.GetIndexed(1), getter.Get(theEvent, 1));
        }
    }
} // end of namespace