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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join {
    public class ExecJoin3StreamRangePerformance : RegressionExecution {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }

        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));

            RunAssertionPerf3StreamKeyAndRange(epService);
            RunAssertionPerf3StreamRangeOnly(epService);
            RunAssertionPerf3StreamUnidirectionalKeyAndRange(epService);
        }

        /// <summary>
        /// This join algorithm profits from merge join cartesian indicated via @hint.
        /// </summary>
        private void RunAssertionPerf3StreamKeyAndRange(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window ST0#keepall as SupportBean_ST0");
            epService.EPAdministrator.CreateEPL("@Name('I1') insert into ST0 select * from SupportBean_ST0");
            epService.EPAdministrator.CreateEPL("create window ST1#keepall as SupportBean_ST1");
            epService.EPAdministrator.CreateEPL("@Name('I2') insert into ST1 select * from SupportBean_ST1");

            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "G", i));
                epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", "G", i));
            }

            Log.Info("Done preloading");

            String epl = "@Hint('PREFER_MERGE_JOIN') select * from SupportBeanRange#lastevent a " +
                         "inner join ST0 st0 on st0.Key0 = a.Key " +
                         "inner join ST1 st1 on st1.Key1 = a.Key " +
                         "where " +
                         "st0.p00 between RangeStart and RangeEnd and st1.P10 between RangeStart and RangeEnd";
            TryAssertion(epService, epl);

            epl = "@Hint('PREFER_MERGE_JOIN') select * from SupportBeanRange#lastevent a, ST0 st0, ST1 st1 " +
                  "where st0.Key0 = a.Key and st1.Key1 = a.Key and " +
                  "st0.p00 between RangeStart and RangeEnd and st1.P10 between RangeStart and RangeEnd";
            TryAssertion(epService, epl);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("ST0", true);
            epService.EPAdministrator.Configuration.RemoveEventType("ST1", true);
        }

        /// <summary>
        /// This join algorithm uses merge join cartesian (not nested iteration).
        /// </summary>
        private void RunAssertionPerf3StreamRangeOnly(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window ST0#keepall as SupportBean_ST0");
            epService.EPAdministrator.CreateEPL("@Name('I1') insert into ST0 select * from SupportBean_ST0");
            epService.EPAdministrator.CreateEPL("create window ST1#keepall as SupportBean_ST1");
            epService.EPAdministrator.CreateEPL("@Name('I2') insert into ST1 select * from SupportBean_ST1");

            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "ST0", i));
                epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", "ST1", i));
            }

            Log.Info("Done preloading");

            // start query
            //String epl = "select * from SupportBeanRange#lastevent a, ST0 st0, ST1 st1 " +
            //        "where st0.Key0 = a.Key and st1.Key1 = a.Key";
            String epl = "select * from SupportBeanRange#lastevent a, ST0 st0, ST1 st1 " +
                         "where st0.p00 between RangeStart and RangeEnd and st1.P10 between RangeStart and RangeEnd";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            // Repeat
            Log.Info("Querying");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", "R", 100, 101));
                Assert.AreEqual(4, listener.GetAndResetLastNewData().Length);
            }

            Log.Info("Done Querying");
            long endTime = PerformanceObserver.MilliTime;
            Log.Info("delta=" + (endTime - startTime));

            Assert.IsTrue((endTime - startTime) < 500);
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("ST0", true);
            epService.EPAdministrator.Configuration.RemoveEventType("ST1", true);
        }

        /// <summary>
        /// This join algorithm profits from nested iteration execution.
        /// </summary>
        private void RunAssertionPerf3StreamUnidirectionalKeyAndRange(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window SBR#keepall as SupportBeanRange");
            epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            epService.EPAdministrator.CreateEPL("create window ST1#keepall as SupportBean_ST1");
            epService.EPAdministrator.CreateEPL("@Name('I2') insert into ST1 select * from SupportBean_ST1");

            // Preload
            Log.Info("Preloading events");
            epService.EPRuntime.SendEvent(new SupportBeanRange("ST1", "G", 4000, 4004));
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_ST1("ST1", "G", i));
            }

            Log.Info("Done preloading");

            String epl = "select * from SupportBean_ST0 st0 unidirectional, SBR a, ST1 st1 " +
                         "where st0.Key0 = a.Key and st1.Key1 = a.Key and " +
                         "st1.P10 between RangeStart and RangeEnd";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            // Repeat
            Log.Info("Querying");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 500; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_ST0("ST0", "G", -1));
                Assert.AreEqual(5, listener.GetAndResetLastNewData().Length);
            }

            Log.Info("Done Querying");
            long delta = PerformanceObserver.MilliTime - startTime;
            Log.Info("delta=" + delta);

            // This works best with a nested iteration join (and not a cardinal join)
            Assert.IsTrue(delta < 500, "delta=" + delta);
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("ST0", true);
            epService.EPAdministrator.Configuration.RemoveEventType("ST1", true);
        }

        private void TryAssertion(EPServiceProvider epService, String epl) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            // Repeat
            Log.Info("Querying");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", "G", 100, 101));
                Assert.AreEqual(4, listener.GetAndResetLastNewData().Length);
            }

            Log.Info("Done Querying");
            long endTime = PerformanceObserver.MilliTime;
            Log.Info("delta=" + (endTime - startTime));

            Assert.IsTrue((endTime - startTime) < 500);
            stmt.Dispose();
        }
    }
}