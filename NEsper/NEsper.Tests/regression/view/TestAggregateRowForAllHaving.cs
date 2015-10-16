///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestAggregateRowForAllHaving 
    {
        private const String JOIN_KEY = "KEY";
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestSumOneView()
        {
            String viewExpr = "select irstream sum(LongBoxed) as mySum " +
                              "from " + typeof(SupportBean).FullName + ".win:time(10 seconds) " +
                              "having sum(LongBoxed) > 10";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;
    
            RunAssert(selectTestView);
        }
    
        [Test]
        public void TestSumJoin()
        {
            String viewExpr = "select irstream sum(LongBoxed) as mySum " +
                              "from " + typeof(SupportBeanString).FullName + ".win:time(10 seconds) as one, " +
                                        typeof(SupportBean).FullName + ".win:time(10 seconds) as two " +
                              "where one.TheString = two.TheString " +
                              "having sum(LongBoxed) > 10";
    
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));
    
            RunAssert(selectTestView);
        }
    
        private void RunAssert(EPStatement selectTestView)
        {
            // assert select result type
            Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("mySum"));
    
            SendTimerEvent(0);
            SendEvent(10);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimerEvent(5000);
            SendEvent(15);
            Assert.AreEqual(25L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
    
            SendTimerEvent(8000);
            SendEvent(-5);
            Assert.AreEqual(20L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(_listener.LastOldData);
    
            SendTimerEvent(10000);
            Assert.AreEqual(20L, _listener.LastOldData[0].Get("mySum"));
            Assert.IsNull(_listener.GetAndResetLastNewData());
        }
    
        [Test]
        public void TestAvgGroupWindow()
        {
            //String stmtText = "select istream avg(Price) as aPrice from "+ typeof(SupportMarketDataBean).FullName
            //        +".std:groupwin(symbol).win:length(1) having avg(Price) <= 0";
            String stmtText = "select istream avg(Price) as aPrice from "+ typeof(SupportMarketDataBean).FullName
                    +".std:unique(symbol) having avg(Price) <= 0";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;
    
            SendEvent("A", -1);
            Assert.AreEqual(-1.0d, _listener.LastNewData[0].Get("aPrice"));
            _listener.Reset();
    
            SendEvent("A", 5);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("B", -6);
            Assert.AreEqual(-.5d, _listener.LastNewData[0].Get("aPrice"));
            _listener.Reset();
    
            SendEvent("C", 2);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("C", 3);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("C", -2);
            Assert.AreEqual(-1d, _listener.LastNewData[0].Get("aPrice"));
            _listener.Reset();
        }
    
        private Object SendEvent(String symbol, double price) {
            Object theEvent = new SupportMarketDataBean(symbol, price, null, null);
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void SendEvent(long longBoxed, int intBoxed, short shortBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(long longBoxed)
        {
            SendEvent(longBoxed, 0, (short)0);
        }
    
        private void SendTimerEvent(long msec)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
