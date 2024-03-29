///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestTransformEventEnumerator : AbstractCommonTest
    {
        private TransformEventEnumerator enumerator;

        [Test]
        public void TestEmpty()
        {
            enumerator = MakeIterator(new int[0]);
            ClassicAssert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void TestOne()
        {
            enumerator = MakeIterator(new int[] { 10 });
            ClassicAssert.IsTrue(enumerator.MoveNext());
            ClassicAssert.AreEqual(10, enumerator.Current.Get("Id"));
            ClassicAssert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void TestTwo()
        {
            enumerator = MakeIterator(new int[] { 10, 20 });
            ClassicAssert.IsTrue(enumerator.MoveNext());
            ClassicAssert.AreEqual(10, enumerator.Current.Get("Id"));
            ClassicAssert.IsTrue(enumerator.MoveNext());
            ClassicAssert.AreEqual(20, enumerator.Current.Get("Id"));
            ClassicAssert.IsFalse(enumerator.MoveNext());
        }

        private TransformEventEnumerator MakeIterator(int[] values)
        {
            IList<EventBean> events = new List<EventBean>();
            for (int i = 0; i < values.Length; i++)
            {
                SupportBean bean = new SupportBean();
                bean.IntPrimitive = values[i];
                EventBean theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
                events.Add(theEvent);
            }
            return new TransformEventEnumerator(events.GetEnumerator(), new MyTransform(supportEventTypeFactory));
        }

        public class MyTransform : TransformEventMethod
        {
            private readonly SupportEventTypeFactory supportEventTypeFactory;

            public MyTransform(SupportEventTypeFactory supportEventTypeFactory)
            {
                this.supportEventTypeFactory = supportEventTypeFactory;
            }

            public EventBean Transform(EventBean theEvent)
            {
                int value = theEvent.Get("IntPrimitive").AsInt32();
                return SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean_S0(value));
            }

            public EventBean[] Transform(EventBean[] events)
            {
                return new EventBean[0];
            }
        }
    }
} // end of namespace
