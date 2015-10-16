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
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf3StreamRangeJoin 
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
    
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        /// <summary>This join algorithm profits from merge join cartesian indicated via @Hint. </summary>
        [Test]
        public void TestPerf3StreamKeyAndRange()
        {
            _epService.EPAdministrator.CreateEPL("create window ST0.win:keepall() as SupportBean_ST0");
            _epService.EPAdministrator.CreateEPL("@Name('I1') insert into ST0 select * from SupportBean_ST0");
            _epService.EPAdministrator.CreateEPL("create window ST1.win:keepall() as SupportBean_ST1");
            _epService.EPAdministrator.CreateEPL("@Name('I2') insert into ST1 select * from SupportBean_ST1");
    
            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "G", i));
                _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", "G", i));
            }
            Log.Info("Done preloading");
    
            String epl = "@Hint('PREFER_MERGE_JOIN') select * from SupportBeanRange.std:lastevent() a " +
                    "inner join ST0 st0 on st0.key0 = a.key " +
                    "inner join ST1 st1 on st1.key1 = a.key " +
                    "where " +
                    "st0.P00 between rangeStart and rangeEnd and st1.p10 between rangeStart and rangeEnd";
            RunAssertion(epl);
    
            epl = "@Hint('PREFER_MERGE_JOIN') select * from SupportBeanRange.std:lastevent() a, ST0 st0, ST1 st1 " +
                    "where st0.key0 = a.key and st1.key1 = a.key and " +
                    "st0.P00 between rangeStart and rangeEnd and st1.p10 between rangeStart and rangeEnd";
            RunAssertion(epl);
        }
    
        /// <summary>This join algorithm uses merge join cartesian (not nested iteration). </summary>
        [Test]
        public void TestPerf3StreamRangeOnly() {
            _epService.EPAdministrator.CreateEPL("create window ST0.win:keepall() as SupportBean_ST0");
            _epService.EPAdministrator.CreateEPL("@Name('I1') insert into ST0 select * from SupportBean_ST0");
            _epService.EPAdministrator.CreateEPL("create window ST1.win:keepall() as SupportBean_ST1");
            _epService.EPAdministrator.CreateEPL("@Name('I2') insert into ST1 select * from SupportBean_ST1");
    
            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "ST0", i));
                _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", "ST1", i));
            }
            Log.Info("Done preloading");
    
            // start query
            //String epl = "select * from SupportBeanRange.std:lastevent() a, ST0 st0, ST1 st1 " +
            //        "where st0.key0 = a.key and st1.key1 = a.key";
            String epl = "select * from SupportBeanRange.std:lastevent() a, ST0 st0, ST1 st1 " +
                    "where st0.P00 between rangeStart and rangeEnd and st1.p10 between rangeStart and rangeEnd";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            // Repeat
            Log.Info("Querying");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "R", 100, 101));
                Assert.AreEqual(4, _listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long endTime = PerformanceObserver.MilliTime;
            Log.Info("delta=" + (endTime - startTime));
    
            Assert.IsTrue((endTime - startTime) < 500);
            stmt.Dispose();
        }
    
        /// <summary>This join algorithm profits from nested iteration execution. </summary>
        [Test]
        public void TestPerf3StreamUnidirectionalKeyAndRange() {
            _epService.EPAdministrator.CreateEPL("create window SBR.win:keepall() as SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("create window ST1.win:keepall() as SupportBean_ST1");
            _epService.EPAdministrator.CreateEPL("@Name('I2') insert into ST1 select * from SupportBean_ST1");
    
            // Preload
            Log.Info("Preloading events");
            _epService.EPRuntime.SendEvent(new SupportBeanRange("ST1", "G", 4000, 4004));
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", "G", i));
            }
            Log.Info("Done preloading");
    
            String epl = "select * from SupportBean_ST0 st0 unidirectional, SBR a, ST1 st1 " +
                    "where st0.key0 = a.key and st1.key1 = a.key and " +
                    "st1.p10 between rangeStart and rangeEnd";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            // Repeat
            Log.Info("Querying");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 500; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "G", -1));
                Assert.AreEqual(5, _listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long delta = PerformanceObserver.MilliTime - startTime;
            Log.Info("delta=" + delta);
    
            // This works best with a nested iteration join (and not a cardinal join)
            Assert.IsTrue(delta < 500, "delta=" + delta);
            stmt.Dispose();
        }
    
        private void RunAssertion(String epl) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            // Repeat
            Log.Info("Querying");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "G", 100, 101));
                Assert.AreEqual(4, _listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long endTime = PerformanceObserver.MilliTime;
            Log.Info("delta=" + (endTime - startTime));
    
            Assert.IsTrue((endTime - startTime) < 500);
            stmt.Dispose();
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
