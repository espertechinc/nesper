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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestUniqueByPropertyView
    {
        private UniqueByPropertyView _myView;
        private SupportBeanClassView _childView;

        [SetUp]
        public void SetUp()
        {
            // Set up length window view and a test child view
            UniqueByPropertyViewFactory factory = new UniqueByPropertyViewFactory();
            factory.CriteriaExpressions = SupportExprNodeFactory.MakeIdentNodesMD("Symbol");
            _myView = new UniqueByPropertyView(factory, null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }

        [Test]
        public void TestViewPush()
        {
            // Set up a feed for the view under test - it will have a depth of 3 trades
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportMarketDataBean), 3);
            stream.AddView(_myView);

            EventBean[] tradeBeans = new EventBean[10];

            // Send some events
            tradeBeans[0] = MakeTradeBean("IBM", 70);
            stream.Insert(tradeBeans[0]);
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { tradeBeans[0] }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { tradeBeans[0] });

            // Send 2 more events
            tradeBeans[1] = MakeTradeBean("IBM", 75);
            tradeBeans[2] = MakeTradeBean("CSCO", 100);
            stream.Insert(new EventBean[] { tradeBeans[1], tradeBeans[2] });
            EPAssertionUtil.AssertEqualsAnyOrder(new EventBean[] { tradeBeans[1], tradeBeans[2] }, _myView.ToArray());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { tradeBeans[0] });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { tradeBeans[1], tradeBeans[2] });

            // And 1 more events
            tradeBeans[3] = MakeTradeBean("CSCO", 99);
            stream.Insert(new EventBean[] { tradeBeans[3] });
            EPAssertionUtil.AssertEqualsAnyOrder(new EventBean[] { tradeBeans[1], tradeBeans[3] }, _myView.ToArray());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { tradeBeans[2] });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { tradeBeans[3] });

            // And 3 more events, that throws CSCO out as the stream size was 3
            tradeBeans[4] = MakeTradeBean("MSFT", 55);
            tradeBeans[5] = MakeTradeBean("IBM", 77);
            tradeBeans[6] = MakeTradeBean("IBM", 78);
            stream.Insert(new EventBean[] { tradeBeans[4], tradeBeans[5], tradeBeans[6] });
            EPAssertionUtil.AssertEqualsAnyOrder(new EventBean[] { tradeBeans[6], tradeBeans[4] }, _myView.ToArray());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { tradeBeans[1], tradeBeans[5], tradeBeans[3] });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { tradeBeans[4], tradeBeans[5], tradeBeans[6] });  // Yes the event is both in old and new data

            // Post as old data an event --> unique event is thrown away and posted as old data
            _myView.Update(null, new EventBean[] { tradeBeans[6] });
            EPAssertionUtil.AssertEqualsAnyOrder(new EventBean[] { tradeBeans[4] }, _myView.ToArray());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { tradeBeans[6] });
            SupportViewDataChecker.CheckNewData(_childView, null);
        }

        [Test]
        public void TestCopyView()
        {
            SupportBeanClassView parent = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.Parent = parent;

            UniqueByPropertyView copied = (UniqueByPropertyView)_myView.CloneView();
            Assert.AreEqual(_myView.CriteriaExpressions[0], copied.CriteriaExpressions[0]);
        }

        private EventBean MakeTradeBean(String symbol, int price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, "");
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
