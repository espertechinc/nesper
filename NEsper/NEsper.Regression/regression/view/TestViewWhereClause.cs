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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewWhereClause 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
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
        public void TestWhere()
        {
            String viewExpr = "select * from " + typeof(SupportMarketDataBean).FullName + "#length(3) where symbol='CSCO'";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            SendMarketDataEvent("IBM");
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendMarketDataEvent("CSCO");
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            SendMarketDataEvent("IBM");
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendMarketDataEvent("CSCO");
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            
            // invalid return type for filter during compilation time
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            try {
                _epService.EPAdministrator.CreateEPL("select TheString From SupportBean#time(30 seconds) where IntPrimitive group by TheString");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error validating expression: The where-clause filter expression must return a boolean value [select TheString From SupportBean#time(30 seconds) where IntPrimitive group by TheString]", ex.Message);
            }
    
            // invalid return type for filter at runtime
            IDictionary<String, Object> dict = new Dictionary<String, Object>();
            dict["criteria"] = typeof(Boolean);
            _epService.EPAdministrator.Configuration.AddEventType("MapEvent", dict);
            stmt = _epService.EPAdministrator.CreateEPL("select * From MapEvent#time(30 seconds) where criteria");
    
            try {
                _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("criteria", 15), "MapEvent");
                Assert.Fail(); // ensure exception handler rethrows
            }
            catch (EPException) {
                // fine
            }
            stmt.Dispose();
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
        }
    
        [Test]
        public void TestWhereNumericType()
        {
            String viewExpr = "select " +
                    " IntPrimitive + LongPrimitive as p1," +
                    " IntPrimitive * DoublePrimitive as p2," +
                    " FloatPrimitive / DoublePrimitive as p3" +
                    " from " + typeof(SupportBean).FullName + "#length(3) where " +
                    "IntPrimitive=LongPrimitive and IntPrimitive=DoublePrimitive and FloatPrimitive=DoublePrimitive";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            SendSupportBeanEvent(1,2,3,4);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            SendSupportBeanEvent(2, 2, 2, 2);
            EventBean theEvent = _listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(typeof(long?), theEvent.EventType.GetPropertyType("p1"));
            Assert.AreEqual(4L, theEvent.Get("p1"));
            Assert.AreEqual(typeof(double?), theEvent.EventType.GetPropertyType("p2"));
            Assert.AreEqual(4d, theEvent.Get("p2"));
            Assert.AreEqual(typeof(double?), theEvent.EventType.GetPropertyType("p3"));
            Assert.AreEqual(1d, theEvent.Get("p3"));
        }
    
        private void SendMarketDataEvent(String symbol)
        {
            SupportMarketDataBean theEvent = new SupportMarketDataBean(symbol, 0, 0L, "");
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(int intPrimitive, long longPrimitive, float floatPrimitive, double doublePrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.LongPrimitive = longPrimitive;
            theEvent.FloatPrimitive = floatPrimitive;
            theEvent.DoublePrimitive = doublePrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
