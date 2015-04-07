///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestTimeWindow
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            for (int i = 0; i < _beans.Length; i++)
            {
                _beans[i] = CreateBean();
            }
        }

        #endregion

        private readonly TimeWindow _window = new TimeWindow(false);
        private readonly TimeWindow _windowRemovable = new TimeWindow(true);
        private readonly EventBean[] _beans = new EventBean[6];

        private static EventBean CreateBean()
        {
            return SupportEventBeanFactory.CreateObject(new SupportBean());
        }

        [Test]
        public void TestAdd()
        {
            Assert.IsTrue(_window.OldestTimestamp == null);
            Assert.IsTrue(_window.IsEmpty());

            _window.Add(19, _beans[0]);
            Assert.IsTrue(_window.OldestTimestamp == 19L);
            Assert.IsFalse(_window.IsEmpty());
            _window.Add(19, _beans[1]);
            Assert.IsTrue(_window.OldestTimestamp == 19L);
            _window.Add(20, _beans[2]);
            Assert.IsTrue(_window.OldestTimestamp == 19L);
            _window.Add(20, _beans[3]);
            _window.Add(21, _beans[4]);
            _window.Add(22, _beans[5]);
            Assert.IsTrue(_window.OldestTimestamp == 19L);

            var beanList = _window.ExpireEvents(19);
            Assert.IsTrue(beanList == null);

            beanList = _window.ExpireEvents(20);
            Assert.IsTrue(beanList.Count == 2);
            Assert.IsTrue(beanList.Poll() == _beans[0]);
            Assert.IsTrue(beanList.Poll() == _beans[1]);

            beanList = _window.ExpireEvents(21);
            Assert.IsTrue(beanList.Count == 2);
            Assert.IsTrue(beanList.Poll() == _beans[2]);
            Assert.IsTrue(beanList.Poll() == _beans[3]);
            Assert.IsFalse(_window.IsEmpty());
            Assert.IsTrue(_window.OldestTimestamp == 21);

            beanList = _window.ExpireEvents(22);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.Poll() == _beans[4]);
            Assert.IsFalse(_window.IsEmpty());
            Assert.IsTrue(_window.OldestTimestamp == 22);

            beanList = _window.ExpireEvents(23);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.Poll() == _beans[5]);
            Assert.IsTrue(_window.IsEmpty());
            Assert.IsTrue(_window.OldestTimestamp == null);

            beanList = _window.ExpireEvents(23);
            Assert.IsTrue(beanList == null);
            Assert.IsTrue(_window.IsEmpty());
            Assert.IsTrue(_window.OldestTimestamp == null);
        }

        [Test]
        public void TestAddRemove()
        {
            Assert.IsTrue(_windowRemovable.OldestTimestamp == null);
            Assert.IsTrue(_windowRemovable.IsEmpty());

            _windowRemovable.Add(19, _beans[0]);
            Assert.IsTrue(_windowRemovable.OldestTimestamp == 19L);
            Assert.IsFalse(_windowRemovable.IsEmpty());
            _windowRemovable.Add(19, _beans[1]);
            Assert.IsTrue(_windowRemovable.OldestTimestamp == 19L);
            _windowRemovable.Add(20, _beans[2]);
            Assert.IsTrue(_windowRemovable.OldestTimestamp == 19L);
            _windowRemovable.Add(20, _beans[3]);
            _windowRemovable.Add(21, _beans[4]);
            _windowRemovable.Add(22, _beans[5]);
            Assert.IsTrue(_windowRemovable.OldestTimestamp == 19L);

            _windowRemovable.Remove(_beans[4]);
            _windowRemovable.Remove(_beans[0]);
            _windowRemovable.Remove(_beans[3]);

            var beanList = _windowRemovable.ExpireEvents(19);
            Assert.IsTrue(beanList == null);

            beanList = _windowRemovable.ExpireEvents(20);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.First() == _beans[1]);

            beanList = _windowRemovable.ExpireEvents(21);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.First() == _beans[2]);
            Assert.IsFalse(_windowRemovable.IsEmpty());
            Assert.IsTrue(_windowRemovable.OldestTimestamp == 22);

            beanList = _windowRemovable.ExpireEvents(22);
            Assert.IsTrue(beanList.Count == 0);

            beanList = _windowRemovable.ExpireEvents(23);
            Assert.IsTrue(beanList.Count == 1);
            Assert.IsTrue(beanList.First() == _beans[5]);
            Assert.IsTrue(_windowRemovable.IsEmpty());
            Assert.IsTrue(_windowRemovable.OldestTimestamp == null);

            beanList = _windowRemovable.ExpireEvents(23);
            Assert.IsTrue(beanList == null);
            Assert.IsTrue(_windowRemovable.IsEmpty());
            Assert.IsTrue(_windowRemovable.OldestTimestamp == null);

            Assert.AreEqual(0, _windowRemovable.ReverseIndex.Count);
        }

        [Test]
        public void TestTimeWindowPerformance()
        {
            Log.Info(".testTimeWindowPerformance Starting");

            var window = new TimeWindow(false);

            for (int i = 0; i < 10; i++)
            {
                window.Add(i, SupportEventBeanFactory.CreateObject("a"));
                window.ExpireEvents(i - 100);
            }

            Log.Info(".testTimeWindowPerformance Done");
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
