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
    public class TestWeightedAverageView 
    {
        private WeightedAverageView _myView;
        private SupportBeanClassView _childView;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            // Set up sum view and a test child view
            EventType type = WeightedAverageView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);

            WeightedAverageViewFactory factory = new WeightedAverageViewFactory();
            factory.FieldNameX = SupportExprNodeFactory.MakeIdentNodeMD("Price");
            factory.EventType = type;
            factory.FieldNameWeight = SupportExprNodeFactory.MakeIdentNodeMD("Volume");
            _myView = new WeightedAverageView(factory, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            
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
    
            // Send a first event, check values
            EventBean marketData = MakeBean("IBM", 10, 1000);
            stream.Insert(marketData);
            CheckOld(Double.NaN);
            CheckNew(10);
    
            // Send a second event, check values
            marketData = MakeBean("IBM", 11, 2000);
            stream.Insert(marketData);
            CheckOld(10);
            CheckNew(10.66666667);
    
            // Send a third event, check values
            marketData = MakeBean("IBM", 10.5, 1500);
            stream.Insert(marketData);
            CheckOld(10.66666667);
            CheckNew(10.61111111);
    
            // Send a 4th event, this time the first event should be gone
            marketData = MakeBean("IBM", 9.5, 600);
            stream.Insert(marketData);
            CheckOld(10.61111111);
            CheckNew(10.59756098);
        }
    
        [Test]
        public void TestGetSchema()
        {
            Assert.IsTrue(_myView.EventType.GetPropertyType(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE.GetName()) == typeof(double?));
        }
    
        [Test]
        public void TestCopyView()
        {
            WeightedAverageView copied = (WeightedAverageView) _myView.CloneView();
            Assert.IsTrue(_myView.FieldNameWeight.Equals(copied.FieldNameWeight));
            Assert.IsTrue(_myView.FieldNameX.Equals(copied.FieldNameX));
        }
    
        private void CheckNew(double avgE)
        {
            IEnumerator<EventBean> iterator = _myView.GetEnumerator();
            CheckValues(iterator.Advance(), avgE);
            Assert.IsTrue(iterator.MoveNext() == false);
    
            Assert.IsTrue(_childView.LastNewData.Length == 1);
            EventBean childViewValues = _childView.LastNewData[0];
            CheckValues(childViewValues, avgE);
        }
    
        private void CheckOld(double avgE)
        {
            Assert.IsTrue(_childView.LastOldData.Length == 1);
            EventBean childViewValues = _childView.LastOldData[0];
            CheckValues(childViewValues, avgE);
        }
    
        private void CheckValues(EventBean values, double avgE)
        {
            double avg = GetDoubleValue(ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE, values);
    
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg,  avgE, 6));
        }
    
        private double GetDoubleValue(ViewFieldEnum field, EventBean eventBean)
        {
            return eventBean.Get(field.GetName()).AsDouble();
        }
    
        private EventBean MakeBean(String symbol, double price, long volume)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, volume, "");
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
