///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.@join.pollindex;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.db {
    public class ExecDatabaseJoinPerfWithCache : RegressionExecution {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration) {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;

            // Turn this cache setting off to turn off indexing since without cache there is no point in indexing.
            configDB.LRUCache = (100000);
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = (true);
            configuration.AddDatabaseReference("MyDB", configDB);
        }

        public override void Run(EPServiceProvider epService) {
            RunAssertionConstants(epService);
            RunAssertionRangeIndex(epService);
            RunAssertionKeyAndRangeIndex(epService);
            RunAssertionSelectLargeResultSet(epService);
            RunAssertionSelectLargeResultSetCoercion(epService);
            RunAssertion2StreamOuterJoin(epService);
            RunAssertionOuterJoinPlusWhere(epService);
            RunAssertionInKeywordSingleIndex(epService);
            RunAssertionInKeywordMultiIndex(epService);
        }

        private void RunAssertionConstants(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));

            string epl;

            epl =
                "select * from SupportBean sbr, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 951";
            TryAssertion(epService, epl, "s1.mycol1", "951");

            epl =
                "select * from SupportBean sbr, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 950 and mycol1 = '950'";
            TryAssertion(epService, epl, "s1.mycol1", "950");

            epl =
                "select sum(s1.mycol3) as val from SupportBean sbr unidirectional, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between 950 and 953";
            TryAssertion(epService, epl, "val", 950 + 951 + 952 + 953);

            epl =
                "select sum(s1.mycol3) as val from SupportBean sbr unidirectional, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = '950' and mycol3 between 950 and 953";
            TryAssertion(epService, epl, "val", 950);
        }

        private void TryAssertion(EPServiceProvider epService, string epl, string field, object expected) {
            var statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E", 0));
                Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get(field));
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            statement.Dispose();
        }

        private void RunAssertionRangeIndex(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));

            var stmtText = "select * from SupportBeanRange sbr, " +
                              " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between rangeStart and rangeEnd";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", 10, 12));
                Assert.AreEqual(3, listener.GetAndResetLastNewData().Length);
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            // test coercion
            statement.Dispose();
            stmtText = "select * from SupportBeanRange sbr, " +
                       " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between rangeStartLong and rangeEndLong";

            statement = epService.EPAdministrator.CreateEPL(stmtText);
            listener = new SupportUpdateListener();
            statement.AddListener(listener);
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R", "K", 10L, 12L));
            Assert.AreEqual(3, listener.GetAndResetLastNewData().Length);

            statement.Dispose();
        }

        private void RunAssertionKeyAndRangeIndex(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));

            var stmtText = "select * from SupportBeanRange sbr, " +
                              " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = key and mycol3 between rangeStart and rangeEnd";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBeanRange("R", "11", 10, 12));
                Assert.AreEqual(1, listener.GetAndResetLastNewData().Length);
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            // test coercion
            statement.Dispose();
            stmtText = "select * from SupportBeanRange sbr, " +
                       " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = key and mycol3 between rangeStartLong and rangeEndLong";

            statement = epService.EPAdministrator.CreateEPL(stmtText);
            listener = new SupportUpdateListener();
            statement.AddListener(listener);
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R", "11", 10L, 12L));
            Assert.AreEqual(1, listener.GetAndResetLastNewData().Length);

            statement.Dispose();
        }

        /**
         * Test for selecting from a table a large result set and then joining the result outside of the cache.
         * Verifies performance of indexes cached for resolving join criteria fast.
         *
         * @param epService
         */
        private void RunAssertionSelectLargeResultSet(EPServiceProvider epService) {
            var stmtText = "select id, mycol3, mycol2 from " +
                              typeof(SupportBean_S0).FullName + "#keepall as s0," +
                              " sql:MyDB ['select mycol3, mycol2 from mytesttable_large'] as s1 where s0.id = s1.mycol3";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            // Send 100 events which all perform the join
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 200; i++) {
                var num = i + 1;
                var col2 = Convert.ToString(Math.Round((float) num / 10, MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture);
                var bean = new SupportBean_S0(num);
                epService.EPRuntime.SendEvent(bean);
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), new string[] {"id", "mycol3", "mycol2"},
                    new object[] {num, num, col2});
            }

            var endTime = PerformanceObserver.MilliTime;

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);
            Assert.IsFalse(listener.IsInvoked);

            statement.Dispose();
        }

        private void RunAssertionSelectLargeResultSetCoercion(EPServiceProvider epService) {
            var stmtText = "select TheString, mycol3, mycol4 from " +
                              " sql:MyDB ['select mycol3, mycol4 from mytesttable_large'] as s0, " +
                              typeof(SupportBean).FullName +
                              "#keepall as s1 where s1.DoubleBoxed = s0.mycol3 and s1.byteBoxed = s0.mycol4";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            // Send events which all perform the join
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 200; i++) {
                var bean = new SupportBean();
                bean.DoubleBoxed = (100d);
                bean.ByteBoxed = ((byte) 10);
                bean.TheString = ("E" + i);
                epService.EPRuntime.SendEvent(bean);
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), new string[] {"TheString", "mycol3", "mycol4"},
                    new object[] {"E" + i, 100, 10});
            }

            var endTime = PerformanceObserver.MilliTime;

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);

            statement.Dispose();
        }

        private void RunAssertion2StreamOuterJoin(EPServiceProvider epService) {
            var stmtText = "select TheString, mycol3, mycol1 from " +
                              " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 right outer join " +
                              typeof(SupportBean).FullName + " as s0 on TheString = mycol1";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            // Send events which all perform the join
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 200; i++) {
                var beanX = new SupportBean();
                beanX.TheString = ("50");
                epService.EPRuntime.SendEvent(beanX);
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), new string[] {"TheString", "mycol3", "mycol1"},
                    new object[] {"50", 50, "50"});
            }

            var endTime = PerformanceObserver.MilliTime;

            // no matching
            var bean = new SupportBean();
            bean.TheString = ("-1");
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), new string[] {"TheString", "mycol3", "mycol1"},
                new object[] {"-1", null, null});

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);

            statement.Dispose();
        }

        private void RunAssertionOuterJoinPlusWhere(EPServiceProvider epService) {
            var stmtText = "select TheString, mycol3, mycol1 from " +
                              " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 right outer join " +
                              typeof(SupportBean).FullName +
                              " as s0 on TheString = mycol1 where s1.mycol3 = s0.IntPrimitive";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            // Send events which all perform the join
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 200; i++) {
                var beanX = new SupportBean();
                beanX.TheString = ("50");
                beanX.IntPrimitive = (50);
                epService.EPRuntime.SendEvent(beanX);
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), new string[] {"TheString", "mycol3", "mycol1"},
                    new object[] {"50", 50, "50"});
            }

            var endTime = PerformanceObserver.MilliTime;

            // no matching on-clause
            var bean = new SupportBean();
            Assert.IsFalse(listener.IsInvoked);

            // matching on-clause not matching where
            bean = new SupportBean();
            bean.TheString = ("50");
            bean.IntPrimitive = (49);
            epService.EPRuntime.SendEvent(bean);
            Assert.IsFalse(listener.IsInvoked);

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);

            statement.Dispose();
        }

        private void RunAssertionInKeywordSingleIndex(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));

            var stmtText = INDEX_CALLBACK_HOOK + "select * from S0 s0, " +
                              " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 " +
                              " where mycol1 in (p00, p01, p02)";
            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            var historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
            Assert.AreEqual(typeof(PollResultIndexingStrategyIndexSingle).Name, historical.IndexName);
            Assert.AreEqual(typeof(HistoricalIndexLookupStrategyInKeywordSingle).Name, historical.StrategyName);

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 2000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i, "x", "y", "815"));
                Assert.AreEqual(815, listener.AssertOneGetNewAndReset().Get("s1.mycol3"));
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            statement.Dispose();
        }

        private void RunAssertionInKeywordMultiIndex(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));

            var stmtText = INDEX_CALLBACK_HOOK + "select * from S0 s0, " +
                              " sql:MyDB ['select mycol1, mycol2, mycol3 from mytesttable_large'] as s1 " +
                              " where p00 in (mycol2, mycol1)";
            var statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);

            var historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
            Assert.AreEqual(typeof(PollResultIndexingStrategyIndexSingleArray).Name, historical.IndexName);
            Assert.AreEqual(typeof(HistoricalIndexLookupStrategyInKeywordMulti).Name, historical.StrategyName);

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 2000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i, "815"));
                Assert.AreEqual(815, listener.AssertOneGetNewAndReset().Get("s1.mycol3"));
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            statement.Dispose();
        }
    }
}
