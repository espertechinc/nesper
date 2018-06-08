///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestMergeView 
    {
        private MergeView _myView;
        private SupportBeanClassView _childView;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            // Set up length window view and a test child view
            _myView = new MergeView(
                SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container),
                SupportExprNodeFactory.MakeIdentNodesMD("Symbol"),
                SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), false);
    
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }
    
        [Test]
        public void TestViewPush()
        {
            var stream = new SupportStreamImpl(typeof(SupportMarketDataBean), 2);
            stream.AddView(_myView);
    
            var tradeBeans = new EventBean[10];
    
            // Send events, expect just forwarded
            tradeBeans[0] = MakeTradeBean("IBM", 70);
            stream.Insert(tradeBeans[0]);
    
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { tradeBeans[0] });
    
            // Send some more events, expect forwarded
            tradeBeans[1] = MakeTradeBean("GE", 90);
            tradeBeans[2] = MakeTradeBean("CSCO", 20);
            stream.Insert(new EventBean[] { tradeBeans[1], tradeBeans[2] });
    
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { tradeBeans[0] });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { tradeBeans[1], tradeBeans[2] });
        }
    
        [Test]
        public void TestCopyView()
        {
            var parent = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.Parent = parent;
    
            var copied = (MergeView) _myView.CloneView();
            Assert.AreEqual(_myView.GroupFieldNames, copied.GroupFieldNames);
            Assert.AreEqual(_myView.EventType, SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)));
        }
    
        private EventBean MakeTradeBean(String symbol, int price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, "");
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
