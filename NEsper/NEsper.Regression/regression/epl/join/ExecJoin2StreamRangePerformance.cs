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

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoin2StreamRangePerformance : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            RunAssertionPerfKeyAndRangeOuterJoin(epService);
            RunAssertionPerfRelationalOp(epService);
            RunAssertionPerfKeyAndRange(epService);
            RunAssertionPerfKeyAndRangeInverted(epService);
            RunAssertionPerfUnidirectionalRelOp(epService);
        }
    
        private void RunAssertionPerfKeyAndRangeOuterJoin(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            epService.EPAdministrator.CreateEPL("create window SBR#keepall as SupportBeanRange");
            epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            epService.EPAdministrator.CreateEPL("create window SB#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");
    
            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("G", i));
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", "G", i - 1, i + 2));
            }
            Log.Info("Done preloading");
    
            // create
            string epl = "select * " +
                    "from SB sb " +
                    "full outer join " +
                    "SBR sbr " +
                    "on TheString = key " +
                    "where IntPrimitive between rangeStart and rangeEnd";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // Repeat
            Log.Info("Querying");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("G", 9990));
                Assert.AreEqual(4, listener.GetAndResetLastNewData().Length);
    
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", "G", 4, 10));
                Assert.AreEqual(7, listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info("delta=" + (endTime - startTime));
    
            Assert.IsTrue((endTime - startTime) < 500);
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("SBR", true);
            epService.EPAdministrator.Configuration.RemoveEventType("SB", true);
        }
    
        private void RunAssertionPerfRelationalOp(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window SBR#keepall as SupportBeanRange");
            epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            epService.EPAdministrator.CreateEPL("create window SB#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");
    
            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
                epService.EPRuntime.SendEvent(new SupportBeanRange("E", i, -1));
            }
            Log.Info("Done preloading");
    
            // start query
            string epl = "select * from SBR a, SB b where a.rangeStart < b.IntPrimitive";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // Repeat
            Log.Info("Querying");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("B", 10));
                Assert.AreEqual(10, listener.GetAndResetLastNewData().Length);
    
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", 9990, -1));
                Assert.AreEqual(9, listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info("delta=" + (endTime - startTime));
    
            Assert.IsTrue((endTime - startTime) < 500);
            stmt.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("SBR", true);
            epService.EPAdministrator.Configuration.RemoveEventType("SB", true);
        }
    
        private void RunAssertionPerfKeyAndRange(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window SBR#keepall as SupportBeanRange");
            epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            epService.EPAdministrator.CreateEPL("create window SB#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");
    
            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 100; i++) {
                for (int j = 0; j < 100; j++) {
                    epService.EPRuntime.SendEvent(new SupportBean(Convert.ToString(i), j));
                    epService.EPRuntime.SendEvent(new SupportBeanRange("R", Convert.ToString(i), j - 1, j + 1));
                }
            }
            Log.Info("Done preloading");
    
            // start query
            string epl = "select * from SBR sbr, SB sb where sbr.key = sb.TheString and sb.IntPrimitive between sbr.rangeStart and sbr.rangeEnd";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // repeat
            Log.Info("Querying");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("55", 10));
                Assert.AreEqual(3, listener.GetAndResetLastNewData().Length);
    
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", "56", 12, 20));
                Assert.AreEqual(9, listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info("delta=" + (endTime - startTime));
    
            // test no event found
            epService.EPRuntime.SendEvent(new SupportBeanRange("R", "56", 2000, 3000));
            epService.EPRuntime.SendEvent(new SupportBeanRange("R", "X", 2000, 3000));
            Assert.IsFalse(listener.IsInvoked);
    
            Assert.IsTrue((endTime - startTime) < 1500, "delta=" + (endTime - startTime));
            stmt.Dispose();
    
            // delete all events
            epService.EPAdministrator.CreateEPL("on SupportBean delete from SBR");
            epService.EPAdministrator.CreateEPL("on SupportBean delete from SB");
            epService.EPRuntime.SendEvent(new SupportBean("D", -1));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("SBR", true);
            epService.EPAdministrator.Configuration.RemoveEventType("SB", true);
        }
    
        private void RunAssertionPerfKeyAndRangeInverted(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create window SB#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");
    
            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E", i));
            }
            Log.Info("Done preloading");
    
            // start query
            string epl = "select * from SupportBeanRange#lastevent sbr, SB sb where sbr.key = sb.TheString and sb.IntPrimitive not in [sbr.rangeStart:sbr.rangeEnd]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // repeat
            Log.Info("Querying");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", "E", 5, 9995));
                Assert.AreEqual(9, listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info("delta=" + (endTime - startTime));
    
            Assert.IsTrue((endTime - startTime) < 500);
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("SB", true);
        }
    
        private void RunAssertionPerfUnidirectionalRelOp(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create window SB#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('I') insert into SB select * from SupportBean");
    
            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 100000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            Log.Info("Done preloading");
    
            // Test range
            string rangeEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            string rangeEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                    "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            string rangeEplThree = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange#lastevent r, SB a " +
                    "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            string rangeEplFour = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange#lastevent r " +
                    "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            string rangeEplFive = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a\n" +
                    "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive <= r.rangeEnd";
            string rangeEplSix = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive <= r.rangeEnd and a.IntPrimitive >= r.rangeStart";
            var rangeCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 50000, iteration + 50100),
                ProcGetExpectedValue = (iteration) => new object[]{50000 + iteration, 50100 + iteration}
            };

            TryAssertion(epService, rangeEplOne, 100, rangeCallback);
            TryAssertion(epService, rangeEplTwo, 100, rangeCallback);
            TryAssertion(epService, rangeEplThree, 100, rangeCallback);
            TryAssertion(epService, rangeEplFour, 100, rangeCallback);
            TryAssertion(epService, rangeEplFive, 100, rangeCallback);
            TryAssertion(epService, rangeEplSix, 100, rangeCallback);
    
            // Test Greater-Equals
            string geEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive <= 99200";
            string geEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                    "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive <= 99200";
            var geCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 99000, null),
                ProcGetExpectedValue = (iteration) => new object[]{99000 + iteration, 99200}
            };
            TryAssertion(epService, geEplOne, 100, geCallback);
            TryAssertion(epService, geEplTwo, 100, geCallback);
    
            // Test Greater-Then
            string gtEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            string gtEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                    "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            string gtEplThree = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange#lastevent r, SB a " +
                    "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            string gtEplFour = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange#lastevent r " +
                    "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            var gtCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 99000, null),
                ProcGetExpectedValue = (iteration) => new object[]{99001 + iteration, 99200}
            };
            TryAssertion(epService, gtEplOne, 100, gtCallback);
            TryAssertion(epService, gtEplTwo, 100, gtCallback);
            TryAssertion(epService, gtEplThree, 100, gtCallback);
            TryAssertion(epService, gtEplFour, 100, gtCallback);
    
            // Test Less-Then
            string ltEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive < r.rangeStart and a.IntPrimitive > 100";
            string ltEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                    "where a.IntPrimitive < r.rangeStart and a.IntPrimitive > 100";
            var ltCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 500, null),
                ProcGetExpectedValue = (iteration) => new object[]{101, 499 + iteration}
            };
            TryAssertion(epService, ltEplOne, 100, ltCallback);
            TryAssertion(epService, ltEplTwo, 100, ltCallback);
    
            // Test Less-Equals
            string leEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive <= r.rangeStart and a.IntPrimitive > 100";
            string leEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                    "where a.IntPrimitive <= r.rangeStart and a.IntPrimitive > 100";
            var leCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 500, null),
                ProcGetExpectedValue = (iteration) => new object[]{101, 500 + iteration}
            };
            TryAssertion(epService, leEplOne, 100, leCallback);
            TryAssertion(epService, leEplTwo, 100, leCallback);
    
            // Test open range
            string openEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive > r.rangeStart and a.IntPrimitive < r.rangeEnd";
            string openEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive in (r.rangeStart:r.rangeEnd)";
            var openCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 3, iteration + 7),
                ProcGetExpectedValue = (iteration) => new object[]{iteration + 4, iteration + 6}
            };
            TryAssertion(epService, openEplOne, 100, openCallback);
            TryAssertion(epService, openEplTwo, 100, openCallback);
    
            // Test half-open range
            string hopenEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive < r.rangeEnd";
            string hopenEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive in [r.rangeStart:r.rangeEnd)";
            var halfOpenCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 3, iteration + 7),
                ProcGetExpectedValue = (iteration) => new object[]{iteration + 3, iteration + 6}
            };
            TryAssertion(epService, hopenEplOne, 100, halfOpenCallback);
            TryAssertion(epService, hopenEplTwo, 100, halfOpenCallback);
    
            // Test half-closed range
            string hclosedEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= r.rangeEnd";
            string hclosedEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive in (r.rangeStart:r.rangeEnd]";
            var halfClosedCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", iteration + 3, iteration + 7),
                ProcGetExpectedValue = (iteration) => new object[]{iteration + 4, iteration + 7}
            };
            TryAssertion(epService, hclosedEplOne, 100, halfClosedCallback);
            TryAssertion(epService, hclosedEplTwo, 100, halfClosedCallback);
    
            // Test inverted closed range
            string invertedClosedEPLOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive not in [r.rangeStart:r.rangeEnd]";
            string invertedClosedEPLTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive not between r.rangeStart and r.rangeEnd";
            var invertedClosedCallback = new ProxyAssertionCallback() {
                ProcGetEvent = (iteration) => new SupportBeanRange("E", 20, 99990),
                ProcGetExpectedValue = (iteration) => new object[]{0, 99999}
            };
            TryAssertion(epService, invertedClosedEPLOne, 100, invertedClosedCallback);
            TryAssertion(epService, invertedClosedEPLTwo, 100, invertedClosedCallback);
    
            // Test inverted open range
            string invertedOpenEPLOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                    "where a.IntPrimitive not in (r.rangeStart:r.rangeEnd)";
            TryAssertion(epService, invertedOpenEPLOne, 100, invertedClosedCallback);
        }
    
        public void TryAssertion(EPServiceProvider epService, string epl, int numLoops, AssertionCallback assertionCallback) {
            string[] fields = "mini,maxi".Split(',');
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // Send range query events
            Log.Info("Querying");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < numLoops; i++) {
                //if (i % 10 == 0) {
                //    Log.Info("At loop #" + i);
                //}
                epService.EPRuntime.SendEvent(assertionCallback.GetEvent(i));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, assertionCallback.GetExpectedValue(i));
            }
            Log.Info("Done Querying");
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info("delta=" + (endTime - startTime));
    
            Assert.IsTrue((endTime - startTime) < 1500);
            stmt.Dispose();
        }

        public interface AssertionCallback
        {
            Object GetEvent(int iteration);
            object[] GetExpectedValue(int iteration);
        }

        public class ProxyAssertionCallback : AssertionCallback
        {
            public Func<int, object> ProcGetEvent;
            public Func<int, object[]> ProcGetExpectedValue;

            public object GetEvent(int iteration)
            {
                return ProcGetEvent(iteration);
            }

            public object[] GetExpectedValue(int iteration)
            {
                return ProcGetExpectedValue(iteration);
            }
        }
    }
} // end of namespace
