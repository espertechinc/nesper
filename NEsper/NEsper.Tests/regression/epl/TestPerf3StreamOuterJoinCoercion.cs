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
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.compat.logging;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf3StreamOuterJoinCoercion 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestPerfCoercion3WaySceneOne()
        {
            String stmtText = "select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) s1 " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='C').win:length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                SendEvent("B", 0, i, 0);
                SendEvent("C", 0, 0, i);
            }
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                SendEvent("A", index, 0, 0);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();
                Assert.AreEqual(index, theEvent.Get("v1"));
                Assert.AreEqual((long)index, theEvent.Get("v2"));
                Assert.AreEqual((double)index, theEvent.Get("v3"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;

            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        [Test]
        public void TestPerfCoercion3WaySceneTwo()
        {
            String stmtText = "select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) s1 " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='C').win:length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                SendEvent("B", 0, i, 0);
                SendEvent("A", i, 0, 0);
            }
    
            _listener.Reset();
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                SendEvent("C", 0, 0, index);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();
                Assert.AreEqual(index, theEvent.Get("v1"));
                Assert.AreEqual((long)index, theEvent.Get("v2"));
                Assert.AreEqual((double)index, theEvent.Get("v3"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;

            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        [Test]
        public void TestPerfCoercion3WaySceneThree()
        {
            String stmtText = "select s1.IntBoxed as v1, s2.LongBoxed as v2, s3.DoubleBoxed as v3 from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) s1 " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) s2 on s1.IntBoxed=s2.LongBoxed " +
                    " left outer join " +
                    typeof(SupportBean).FullName + "(TheString='C').win:length(1000000) s3 on s1.IntBoxed=s3.DoubleBoxed";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++)
            {
                SendEvent("A", i, 0, 0);
                SendEvent("C", 0, 0, i);
            }
    
            _listener.Reset();
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                SendEvent("B", 0, index, 0);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();
                Assert.AreEqual(index, theEvent.Get("v1"));
                Assert.AreEqual((long)index, theEvent.Get("v2"));
                Assert.AreEqual((double)index, theEvent.Get("v3"));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;

            Assert.IsTrue(delta < 1500, "Failed perf test, delta=" + delta);
        }
    
        [Test]
        public void TestPerfCoercion3WayRange()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            String stmtText = "select * from " +
                    "SupportBeanRange.win:keepall() sbr " +
                    " left outer join " +
                    "SupportBean_ST0.win:keepall() s0 on s0.key0=sbr.key" +
                    " left outer join " +
                    "SupportBean_ST1.win:keepall() s1 on s1.key1=s0.key0" +
                    " where s0.P00 between sbr.rangeStartLong and sbr.rangeEndLong";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload
            log.Info("Preload");
            for (int i = 0; i < 10; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1_" + i, "K", i));
            }
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0_" + i, "K", i));
            }
            log.Info("Preload done");
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 100; i++)
            {
                long index = 5000 + i;
                _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R", "K", index, index + 2));
                Assert.AreEqual(30, _listener.GetAndResetLastNewData().Length);
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0X", "K", 5000));
            Assert.AreEqual(10, _listener.GetAndResetLastNewData().Length);
    
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1X", "K", 5004));
            Assert.AreEqual(301, _listener.GetAndResetLastNewData().Length);

            Assert.IsTrue(delta < 500, "Failed perf test, delta=" + delta);
        }
    
    
        private void SendEvent(String stringValue, int intBoxed, long longBoxed, double doubleBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
