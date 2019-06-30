///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestSingleEventEnumerator : CommonTest
    {
        private SingleEventEnumerator enumerator;
        private EventBean eventBean;

        [SetUp]
        public void SetUp()
        {
            eventBean = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean("a", 0));
            enumerator = new SingleEventEnumerator(eventBean);
        }

        [Test]
        public void TestNext()
        {
            Assert.AreEqual(eventBean, enumerator.MoveNext());
            Assert.That(() => enumerator.Current, Throws.InstanceOf<NoSuchElementException>());
        }

        [Test]
        public void TestHasNext()
        {
            Assert.IsTrue(enumerator.MoveNext());
            Assert.That(() => enumerator.Current, Throws.Nothing);
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
} // end of namespace