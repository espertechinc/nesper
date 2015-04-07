///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;
using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestTransformEvent
    {
        private IEnumerator<EventBean> enumerator;

        private static IEnumerator<EventBean> MakeEnumerator(int[] values)
        {
            IList<EventBean> events = new List<EventBean>();
            for (int i = 0; i < values.Length; i++) {
                var bean = new SupportBean();
                bean.IntPrimitive = values[i];
                EventBean theEvent = SupportEventBeanFactory.CreateObject(bean);
                events.Add(theEvent);
            }

            return TransformEventUtil.Transform(
                events.GetEnumerator(),
                eventBean => SupportEventBeanFactory.CreateObject(
                                 new SupportBean_S0((int) eventBean.Get("IntPrimitive"))));
        }

        [Test]
        public void TestEmpty()
        {
            enumerator = MakeEnumerator(new int[0]);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void TestOne()
        {
            enumerator = MakeEnumerator(new[] {10});
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(10, enumerator.Current.Get("Id"));
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void TestTwo()
        {
            enumerator = MakeEnumerator(new[] {10, 20});
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(10, enumerator.Current.Get("Id"));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(20, enumerator.Current.Get("Id"));
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}
