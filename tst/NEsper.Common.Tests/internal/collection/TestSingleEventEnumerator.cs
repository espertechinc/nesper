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

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestSingleEventEnumerator : AbstractCommonTest
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
        public void TestMoveNext()
        {
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.Not.Null);
            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        public void TestCurrent()
        {
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(() => enumerator.Current, Throws.Nothing);
            Assert.That(enumerator.MoveNext(), Is.False);
            Assert.That(() => enumerator.Current, Throws.InstanceOf<InvalidOperationException>());
        }
    }
} // end of namespace
