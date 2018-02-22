///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestSizeView 
    {
        private SizeView _myView;
        private SupportBeanClassView _childView;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            // Set up length window view and a test child view
            EventType type = SizeView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
            _myView = new SizeView(SupportStatementContextFactory.MakeAgentInstanceContext(_container), type, null);
    
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }
    
        // Check values against Microsoft Excel computed values
        [Test]
        public void TestViewPush()
        {
            // Set up a feed for the view under test - it will have a depth of 5 trades
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportBean_A), 5);
            stream.AddView(_myView);
    
            CheckIterator(0);
    
            // View just counts the number of events received, removing those removed in the prior view as old data
            stream.Insert(MakeBeans("a", 1));
            CheckOldData(0);
            CheckNewData(1);
            CheckIterator(1);
    
            stream.Insert(MakeBeans("b", 2));
            CheckOldData(1);
            CheckNewData(3);
            CheckIterator(3);
    
            // The EventStream has a depth of 3, it will expire the first message now, ie. will keep the size of 3, always
            stream.Insert(MakeBeans("c", 1));
            CheckOldData(3);
            CheckNewData(4);
            CheckIterator(4);
    
            stream.Insert(MakeBeans("d", 1));
            CheckOldData(4);
            CheckNewData(5);
            CheckIterator(5);
    
            stream.Insert(MakeBeans("e", 2));
            Assert.IsNull(_childView.LastNewData);
            Assert.IsNull(_childView.LastOldData);
            CheckIterator(5);
    
            stream.Insert(MakeBeans("f", 1));
            Assert.IsNull(_childView.LastNewData);
            Assert.IsNull(_childView.LastOldData);
            CheckIterator(5);
        }
    
        [Test]
        public void TestUpdate()
        {
            // View should not post events if data didn't change
            _myView.Update(MakeBeans("f", 1), null);
    
            CheckOldData(0);
            CheckNewData(1);
            _childView.LastNewData = null;
            _childView.LastOldData = null;
    
            _myView.Update(MakeBeans("f", 1), MakeBeans("f", 1));
    
            Assert.IsNull(_childView.LastNewData);
            Assert.IsNull(_childView.LastOldData);
        }
    
        [Test]
        public void TestSchema()
        {
            EventType type = SizeView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
            SizeView view = new SizeView(SupportStatementContextFactory.MakeAgentInstanceContext(_container), type, null);
    
            EventType eventType = view.EventType;
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType(ViewFieldEnum.SIZE_VIEW__SIZE.GetName()));
        }
    
        [Test]
        public void TestCopyView()
        {
            Assert.IsTrue(_myView.CloneView() is SizeView);
        }
    
        private void CheckNewData(long expectedSize)
        {
            EventBean[] newData = _childView.LastNewData;
            CheckData(newData, expectedSize);
            _childView.LastNewData = null;
        }
    
        private void CheckOldData(long expectedSize)
        {
            EventBean[] oldData = _childView.LastOldData;
            CheckData(oldData, expectedSize);
            _childView.LastOldData = null;
        }
    
        private void CheckData(EventBean[] data, long expectedSize)
        {
            // The view posts in its Update data always just one object containing the size
            Assert.AreEqual(1, data.Length);
            var actualSize = (long?) data[0].Get(ViewFieldEnum.SIZE_VIEW__SIZE.GetName());
            Assert.AreEqual((long) expectedSize, (long) actualSize);
        }
    
        private void CheckIterator(long expectedSize)
        {
            Assert.IsTrue(_myView.HasFirst());
            EventBean eventBean = _myView.FirstOrDefault();
            var actualSize = (long?) eventBean.Get(ViewFieldEnum.SIZE_VIEW__SIZE.GetName());
            Assert.AreEqual((long?) expectedSize, (long?) actualSize);
        }
    
        private EventBean[] MakeBeans(String id, int numTrades)
        {
            EventBean[] trades = new EventBean[numTrades];
            for (int i = 0; i < numTrades; i++)
            {
                SupportBean_A bean = new SupportBean_A(id + i);
                trades[i] = SupportEventBeanFactory.CreateObject(bean);
            }
            return trades;
        }
    }
}
