///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;
using com.espertech.esper.view;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewLengthWindowStats 
    {
        private const String SYMBOL = "CSCO.O";
        private const String FEED = "feed1";
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestIterator()
        {
            String viewExpr = "select Symbol, Price from " + typeof(SupportMarketDataBean).FullName + ".win:length(2)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(viewExpr);
            statement.Events += _testListener.Update;
    
            SendEvent("ABC", 20);
            SendEvent("DEF", 100);
    
            // check iterator results
            var events = new LookaheadEnumerator<EventBean>(statement.GetEnumerator());
            EventBean theEvent = events.Next();
            Assert.AreEqual("ABC", theEvent.Get("Symbol"));
            Assert.AreEqual(20d, theEvent.Get("Price"));
    
            theEvent = events.Next();
            Assert.AreEqual("DEF", theEvent.Get("Symbol"));
            Assert.AreEqual(100d, theEvent.Get("Price"));
            Assert.IsFalse(events.HasNext());
    
            SendEvent("EFG", 50);
    
            // check iterator results
            events = new LookaheadEnumerator<EventBean>(statement.GetEnumerator());
            theEvent = events.Next();
            Assert.AreEqual("DEF", theEvent.Get("Symbol"));
            Assert.AreEqual(100d, theEvent.Get("Price"));

            theEvent = events.Next();
            Assert.AreEqual("EFG", theEvent.Get("Symbol"));
            Assert.AreEqual(50d, theEvent.Get("Price"));
        }
    
        [Test]
        public void TestWindowStats()
        {
            String viewExpr = "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                    "(Symbol='" + SYMBOL + "').win:length(3).stat:uni(Price, Symbol, feed)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(viewExpr);
            statement.Events += _testListener.Update;
            _testListener.Reset();

            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("average"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("variance"));
            Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("datapoints"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("total"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("stddev"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("stddevpa"));
    
            SendEvent(SYMBOL, 100);
            CheckOld(true, 0, 0, Double.NaN, Double.NaN, Double.NaN, Double.NaN);
            CheckNew(statement, 1, 100, 100, 0, Double.NaN, Double.NaN);
    
            SendEvent(SYMBOL, 100.5);
            CheckOld(false, 1, 100, 100, 0, Double.NaN, Double.NaN);
            CheckNew(statement, 2, 200.5, 100.25, 0.25, 0.353553391, 0.125);
    
            SendEvent("DUMMY", 100.5);
            Assert.IsTrue(_testListener.LastNewData == null);
            Assert.IsTrue(_testListener.LastOldData == null);
    
            SendEvent(SYMBOL, 100.7);
            CheckOld(false, 2, 200.5, 100.25, 0.25, 0.353553391, 0.125);
            CheckNew(statement, 3, 301.2, 100.4, 0.294392029, 0.360555128, 0.13);
    
            SendEvent(SYMBOL, 100.6);
            CheckOld(false, 3, 301.2, 100.4, 0.294392029, 0.360555128, 0.13);
            CheckNew(statement, 3, 301.8, 100.6, 0.081649658, 0.1, 0.01);
    
            SendEvent(SYMBOL, 100.9);
            CheckOld(false, 3, 301.8, 100.6, 0.081649658, 0.1, 0.01);
            CheckNew(statement, 3, 302.2, 100.733333333, 0.124721913, 0.152752523, 0.023333333);
            statement.Dispose();
    
            // Test copying all properties
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            String viewExprWildcard = "select * from SupportBean.win:length(3).stat:uni(IntPrimitive, *)";
            statement = _epService.EPAdministrator.CreateEPL(viewExprWildcard);
            statement.Events += _testListener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(1.0, theEvent.Get("average"));
            Assert.AreEqual("E1", theEvent.Get("TheString"));
            Assert.AreEqual(1, theEvent.Get("IntPrimitive"));
        }
    
        private void SendEvent(String symbol, double price)
        {
            SupportMarketDataBean theEvent = new SupportMarketDataBean(symbol, price, 0L, FEED);
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void CheckNew(EPStatement statement, long countE, double sumE, double avgE, double stdevpaE, double stdevE, double varianceE)
        {
            var iterator = new LookaheadEnumerator<EventBean>(statement.GetEnumerator());
            CheckValues(iterator.Next(), false, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
            Assert.IsTrue(iterator.HasNext() == false);
    
            Assert.IsTrue(_testListener.LastNewData.Length == 1);
            EventBean childViewValues = _testListener.LastNewData[0];
            CheckValues(childViewValues, false, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
    
            _testListener.Reset();
        }
    
        private void CheckOld(bool isFirst, long countE, double sumE, double avgE, double stdevpaE, double stdevE, double varianceE)
        {
            Assert.IsTrue(_testListener.LastOldData.Length == 1);
            EventBean childViewValues = _testListener.LastOldData[0];
            CheckValues(childViewValues, isFirst, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
        }
    
        private void CheckValues(EventBean values, bool isFirst, bool isNewData, long countE, double sumE, double avgE, double stdevpaE, double stdevE, double varianceE)
        {
            long count = GetLongValue(ViewFieldEnum.UNIVARIATE_STATISTICS__DATAPOINTS, values);
            double sum = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__TOTAL, values);
            double avg = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE, values);
            double stdevpa = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEVPA, values);
            double stdev = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEV, values);
            double variance = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__VARIANCE, values);
    
            Assert.AreEqual(count, countE);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(sum,  sumE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg,  avgE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stdevpa,  stdevpaE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stdev,  stdevE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(variance,  varianceE, 6));
            if (isFirst && !isNewData) {
                Assert.AreEqual(null, values.Get("Symbol"));
                Assert.AreEqual(null, values.Get("feed"));
            }
            else {
                Assert.AreEqual(SYMBOL, values.Get("Symbol"));
                Assert.AreEqual(FEED, values.Get("feed"));
            }
        }
    
        private static double GetDoubleValue(ViewFieldEnum field, EventBean values)
        {
            return values.Get(field.GetName()).AsDouble();
        }
    
        private static long GetLongValue(ViewFieldEnum field, EventBean values)
        {
            return values.Get(field.GetName()).AsLong();
        }
    }
}
