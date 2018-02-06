///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.stat
{
    [TestFixture]
    public class TestCorrelationView 
    {
        private CorrelationView _myView;
        private SupportBeanClassView _childView;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            // Set up sum view and a test child view
            EventType type = CorrelationView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
            CorrelationViewFactory factory = new CorrelationViewFactory();
            _myView = new CorrelationView(factory, 
                SupportStatementContextFactory.MakeAgentInstanceContext(_container),
                SupportExprNodeFactory.MakeIdentNodeMD("Price"),
                SupportExprNodeFactory.MakeIdentNodeMD("Volume"), type, null);
    
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }
    
        // Check values against Microsoft Excel computed values
        [Test]
        public void TestViewComputedValues()
        {
            // Set up feed for sum view
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportMarketDataBean), 3);
            stream.AddView(_myView);
    
            // Send a first event, checkNew values
            EventBean marketData = MakeBean("IBM", 70, 1000);
            stream.Insert(marketData);
            CheckOld(Double.NaN);
            CheckNew(Double.NaN);
    
            // Send a second event, checkNew values
            marketData = MakeBean("IBM", 70.5, 1500);
            stream.Insert(marketData);
            CheckOld(Double.NaN);
            CheckNew(1);
    
            // Send a third event, checkNew values
            marketData = MakeBean("IBM", 70.1, 1200);
            stream.Insert(marketData);
            CheckOld(1);
            CheckNew(0.97622104);
    
            // Send a 4th event, this time the first event should be gone, checkNew values
            marketData = MakeBean("IBM", 70.25, 1000);
            stream.Insert(marketData);
            CheckOld(0.97622104);
            CheckNew(0.70463404);
        }
    
        [Test]
        public void TestGetSchema()
        {
            Assert.That(_myView.EventType.GetPropertyType(ViewFieldEnum.CORRELATION__CORRELATION.GetName()),
                        Is.EqualTo(typeof(double?)));
        }
    
        [Test]
        public void TestCopyView()
        {
            CorrelationView copied = (CorrelationView) _myView.CloneView();
            Assert.IsTrue(_myView.ExpressionX.Equals(copied.ExpressionX));
            Assert.IsTrue(_myView.ExpressionY.Equals(copied.ExpressionY));
        }
    
        private void CheckNew(double correlationE)
        {
            IEnumerator<EventBean> iterator = _myView.GetEnumerator();
            CheckValues(iterator.Advance(), correlationE);
            Assert.IsTrue(iterator.MoveNext() == false);
    
            Assert.IsTrue(_childView.LastNewData.Length == 1);
            EventBean childViewValues = _childView.LastNewData[0];
            CheckValues(childViewValues, correlationE);
        }
    
        private void CheckOld(double correlationE)
        {
            Assert.IsTrue(_childView.LastOldData.Length == 1);
            EventBean childViewValues = _childView.LastOldData[0];
            CheckValues(childViewValues, correlationE);
        }
    
        private void CheckValues(EventBean values, double correlationE)
        {
            double correlation = GetDoubleValue(ViewFieldEnum.CORRELATION__CORRELATION, values);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(correlation,  correlationE, 6));
        }
    
        private double GetDoubleValue(ViewFieldEnum field, EventBean values)
        {
            return values.Get(field.GetName()).AsDouble();
        }
    
        private EventBean MakeBean(String symbol, double price, long volume)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, volume, "");
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
