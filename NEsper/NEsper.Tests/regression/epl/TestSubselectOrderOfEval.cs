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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectOrderOfEval 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestCorrelatedSubqueryOrder()
        {
            // ESPER-564
    
            Configuration config = new Configuration();
            config.EngineDefaults.ViewResourcesConfig.IsShareViews = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType<TradeEvent>("TradeEvent");
            _listener = new SupportUpdateListener();
    
            _epService.EPAdministrator.CreateEPL("select * from TradeEvent.std:lastevent()");
    
            _epService.EPAdministrator.CreateEPL(
                    "select window(tl.*) as longItems, " +
                    "       (SELECT window(ts.*) AS shortItems FROM TradeEvent.win:time(20 minutes) as ts WHERE ts.SecurityID=tl.SecurityID) " +
                    "from TradeEvent.win:time(20 minutes) as tl " +
                    "where tl.SecurityID = 1000" +
                    "group by tl.SecurityID "
            ).Events += _listener.Update;
    		
            _epService.EPRuntime.SendEvent(new TradeEvent(PerformanceObserver.MilliTime, 1000, 50, 1));
            Assert.AreEqual(1, ((Object[]) _listener.AssertOneGetNew().Get("longItems")).Length);
            Assert.AreEqual(1, ((Object[]) _listener.AssertOneGetNew().Get("shortItems")).Length);
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new TradeEvent(PerformanceObserver.MilliTime + 10, 1000, 50, 1));
            Assert.AreEqual(2, ((Object[]) _listener.AssertOneGetNew().Get("longItems")).Length);
            Assert.AreEqual(2, ((Object[]) _listener.AssertOneGetNew().Get("shortItems")).Length);
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestOrderOfEvaluationSubselectFirst()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ExpressionConfig.IsSelfSubselectPreeval = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            String viewExpr = "select * from SupportBean(IntPrimitive<10) where IntPrimitive not in (select IntPrimitive from SupportBean.std:unique(IntPrimitive))";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
            stmtOne.Dispose();
    
            String viewExprTwo = "select * from SupportBean where IntPrimitive not in (select IntPrimitive from SupportBean(IntPrimitive<10).std:unique(IntPrimitive))";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(viewExprTwo);
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestOrderOfEvaluationSubselectLast()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ExpressionConfig.IsSelfSubselectPreeval = false;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            String viewExpr = "select * from SupportBean(IntPrimitive<10) where IntPrimitive not in (select IntPrimitive from SupportBean.std:unique(IntPrimitive))";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            stmtOne.Dispose();
    
            String viewExprTwo = "select * from SupportBean where IntPrimitive not in (select IntPrimitive from SupportBean(IntPrimitive<10).std:unique(IntPrimitive))";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(viewExprTwo);
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        public class TradeEvent {
            private long time;
            private int securityID;
            private double price;
            private long volume;
    
            public TradeEvent(long time, int securityID, double price, long volume) {
                this.time = time;
                this.securityID = securityID;
                this.price = price;
                this.volume = volume;
            }
    
            public int GetSecurityID() {
                return securityID;
            }
    
            public long GetTime() {
                return time;
            }
    
            public double GetPrice() {
                return price;
            }
    
            public long GetVolume() {
                return volume;
            }
        }
    }
}
