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

        [Test, RunInApplicationDomain]
        public void TestAdd()
        {
            Assert.IsTrue(window.OldestTimestamp == null);
            Assert.IsTrue(window.IsEmpty());

            window.Add(19, beans[0]);
            Assert.IsTrue(window.OldestTimestamp == 19L);
            Assert.IsFalse(window.IsEmpty());
            window.Add(19, beans[1]);
            Assert.IsTrue(window.OldestTimestamp == 19L);
            window.Add(20, beans[2]);
            Assert.IsTrue(window.OldestTimestamp == 19L);
            window.Add(20, beans[3]);
            window.Add(21, beans[4]);
            window.Add(22, beans[5]);
            Assert.IsTrue(window.OldestTimestamp == 19L);

            ArrayDeque<EventBean> beanList = window.ExpireEvents(19);
            Assert.IsTrue(beanList == null);

            beanList = window.ExpireEvents(20);
            Assert.IsTrue(beanList.Count == 2);
            Assert.IsTrue(beanList.Poll() == beans[0]);
            Assert.IsTrue(beanList.Poll() == beans[1]);

            beanList = window.ExpireEvents(21);
            Assert.IsTrue(beanList.Count == 2);
            Assert.IsTrue(beanList.Poll() == beans[2]);
            Assert.IsTrue(beanList.Poll() == beans[3]);
            Assert.IsFalse(window.IsEmpty());
            Assert.IsTrue(window.OldestTimestamp == 21);

            beanList = window.ExpireEvents(22);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.Poll() == beans[4]);
            Assert.IsFalse(window.IsEmpty());
            Assert.IsTrue(window.OldestTimestamp == 22);

            beanList = window.ExpireEvents(23);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.Poll() == beans[5]);
            Assert.IsTrue(window.IsEmpty());
            Assert.IsTrue(window.OldestTimestamp == null);

            beanList = window.ExpireEvents(23);
            Assert.IsTrue(beanList == null);
            Assert.IsTrue(window.IsEmpty());
            Assert.IsTrue(window.OldestTimestamp == null);
        }

        [Test, RunInApplicationDomain]
        public void TestAddRemove()
        {
            Assert.IsTrue(windowRemovable.OldestTimestamp == null);
            Assert.IsTrue(windowRemovable.IsEmpty());

            windowRemovable.Add(19, beans[0]);
            Assert.IsTrue(windowRemovable.OldestTimestamp == 19L);
            Assert.IsFalse(windowRemovable.IsEmpty());
            windowRemovable.Add(19, beans[1]);
            Assert.IsTrue(windowRemovable.OldestTimestamp == 19L);
            windowRemovable.Add(20, beans[2]);
            Assert.IsTrue(windowRemovable.OldestTimestamp == 19L);
            windowRemovable.Add(20, beans[3]);
            windowRemovable.Add(21, beans[4]);
            windowRemovable.Add(22, beans[5]);
            Assert.IsTrue(windowRemovable.OldestTimestamp == 19L);

            windowRemovable.Remove(beans[4]);
            windowRemovable.Remove(beans[0]);
            windowRemovable.Remove(beans[3]);

            ArrayDeque<EventBean> beanList = windowRemovable.ExpireEvents(19);
            Assert.IsTrue(beanList == null);

            beanList = windowRemovable.ExpireEvents(20);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.First == beans[1]);

            beanList = windowRemovable.ExpireEvents(21);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.First == beans[2]);
            Assert.IsFalse(windowRemovable.IsEmpty());
            Assert.IsTrue(windowRemovable.OldestTimestamp == 22);

            beanList = windowRemovable.ExpireEvents(22);
            Assert.IsTrue(beanList.Count == 0);

            beanList = windowRemovable.ExpireEvents(23);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.First == beans[5]);
            Assert.IsTrue(windowRemovable.IsEmpty());
            Assert.IsTrue(windowRemovable.OldestTimestamp == null);

            beanList = windowRemovable.ExpireEvents(23);
            Assert.IsTrue(beanList == null);
            Assert.IsTrue(windowRemovable.IsEmpty());
            Assert.IsTrue(windowRemovable.OldestTimestamp == null);

            Assert.AreEqual(0, windowRemovable.ReverseIndex.Count);
        }

        [Test, RunInApplicationDomain]
        public void TestTimeWindowPerformance()
        {
            log.Info(".testTimeWindowPerformance Starting");

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

            log.Info(".testTimeWindowPerformance Done");
        }

        private EventBean CreateBean()
        {
            return SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean());
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
