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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestTimeWindow : AbstractCommonTest
    {
        private readonly TimeWindow window = new TimeWindow(false);
        private readonly TimeWindow windowRemovable = new TimeWindow(true);
        private readonly EventBean[] beans = new EventBean[6];

        [SetUp]
        public void SetUp()
        {
            for (int i = 0; i < beans.Length; i++)
            {
                beans[i] = CreateBean();
            }
        }

        [Test]
        public void TestAdd()
        {
            ClassicAssert.IsTrue(window.OldestTimestamp == null);
            ClassicAssert.IsTrue(window.IsEmpty());

            window.Add(19, beans[0]);
            ClassicAssert.IsTrue(window.OldestTimestamp == 19L);
            ClassicAssert.IsFalse(window.IsEmpty());
            window.Add(19, beans[1]);
            ClassicAssert.IsTrue(window.OldestTimestamp == 19L);
            window.Add(20, beans[2]);
            ClassicAssert.IsTrue(window.OldestTimestamp == 19L);
            window.Add(20, beans[3]);
            window.Add(21, beans[4]);
            window.Add(22, beans[5]);
            ClassicAssert.IsTrue(window.OldestTimestamp == 19L);

            ArrayDeque<EventBean> beanList = window.ExpireEvents(19);
            ClassicAssert.IsTrue(beanList == null);

            beanList = window.ExpireEvents(20);
            ClassicAssert.IsTrue(beanList.Count == 2);
            ClassicAssert.IsTrue(beanList.Poll() == beans[0]);
            ClassicAssert.IsTrue(beanList.Poll() == beans[1]);

            beanList = window.ExpireEvents(21);
            ClassicAssert.IsTrue(beanList.Count == 2);
            ClassicAssert.IsTrue(beanList.Poll() == beans[2]);
            ClassicAssert.IsTrue(beanList.Poll() == beans[3]);
            ClassicAssert.IsFalse(window.IsEmpty());
            ClassicAssert.IsTrue(window.OldestTimestamp == 21);

            beanList = window.ExpireEvents(22);
            ClassicAssert.IsTrue(beanList.Count == 1);
            ClassicAssert.IsTrue(beanList.Poll() == beans[4]);
            ClassicAssert.IsFalse(window.IsEmpty());
            ClassicAssert.IsTrue(window.OldestTimestamp == 22);

            beanList = window.ExpireEvents(23);
            ClassicAssert.IsTrue(beanList.Count == 1);
            ClassicAssert.IsTrue(beanList.Poll() == beans[5]);
            ClassicAssert.IsTrue(window.IsEmpty());
            ClassicAssert.IsTrue(window.OldestTimestamp == null);

            beanList = window.ExpireEvents(23);
            ClassicAssert.IsTrue(beanList == null);
            ClassicAssert.IsTrue(window.IsEmpty());
            ClassicAssert.IsTrue(window.OldestTimestamp == null);
        }

        [Test]
        public void TestAddRemove()
        {
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == null);
            ClassicAssert.IsTrue(windowRemovable.IsEmpty());

            windowRemovable.Add(19, beans[0]);
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == 19L);
            ClassicAssert.IsFalse(windowRemovable.IsEmpty());
            windowRemovable.Add(19, beans[1]);
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == 19L);
            windowRemovable.Add(20, beans[2]);
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == 19L);
            windowRemovable.Add(20, beans[3]);
            windowRemovable.Add(21, beans[4]);
            windowRemovable.Add(22, beans[5]);
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == 19L);

            windowRemovable.Remove(beans[4]);
            windowRemovable.Remove(beans[0]);
            windowRemovable.Remove(beans[3]);

            ArrayDeque<EventBean> beanList = windowRemovable.ExpireEvents(19);
            ClassicAssert.IsTrue(beanList == null);

            beanList = windowRemovable.ExpireEvents(20);
            ClassicAssert.IsTrue(beanList.Count == 1);
            ClassicAssert.IsTrue(beanList.First == beans[1]);

            beanList = windowRemovable.ExpireEvents(21);
            ClassicAssert.IsTrue(beanList.Count == 1);
            ClassicAssert.IsTrue(beanList.First == beans[2]);
            ClassicAssert.IsFalse(windowRemovable.IsEmpty());
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == 22);

            beanList = windowRemovable.ExpireEvents(22);
            ClassicAssert.IsTrue(beanList.Count == 0);

            beanList = windowRemovable.ExpireEvents(23);
            ClassicAssert.IsTrue(beanList.Count == 1);
            ClassicAssert.IsTrue(beanList.First == beans[5]);
            ClassicAssert.IsTrue(windowRemovable.IsEmpty());
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == null);

            beanList = windowRemovable.ExpireEvents(23);
            ClassicAssert.IsTrue(beanList == null);
            ClassicAssert.IsTrue(windowRemovable.IsEmpty());
            ClassicAssert.IsTrue(windowRemovable.OldestTimestamp == null);

            ClassicAssert.AreEqual(0, windowRemovable.ReverseIndex.Count);
        }

        [Test]
        public void TestTimeWindowPerformance()
        {
            Log.Info(".testTimeWindowPerformance Starting");

            TimeWindow window = new TimeWindow(false);

            // 1E7 yields for implementations...on 2.8GHz JDK 1.5
            // about 7.5 seconds for a LinkedList-backed
            // about 20 seconds for a LinkedHashMap-backed
            // about 30 seconds for a TreeMap-backed-backed
            for (int i = 0; i < 10; i++)
            {
                window.Add(i, SupportEventBeanFactory.CreateObject(
                    supportEventTypeFactory, new SupportBean("a" + i, i)));

                window.ExpireEvents(i - 100);
            }

            Log.Info(".testTimeWindowPerformance Done");
        }

        private EventBean CreateBean()
        {
            return SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean());
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
