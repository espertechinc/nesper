///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoinPerfWithCache : IndexBackingTableInfo
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithConstants(execs);
            WithRangeIndex(execs);
            WithKeyAndRangeIndex(execs);
            WithSelectLargeResultSet(execs);
            WithSelectLargeResultSetCoercion(execs);
            With2StreamOuterJoin(execs);
            WithOuterJoinPlusWhere(execs);
            WithInKeywordSingleIndex(execs);
            WithInKeywordMultiIndex(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInKeywordMultiIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInKeywordMultiIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithInKeywordSingleIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInKeywordSingleIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithOuterJoinPlusWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseOuterJoinPlusWhere());
            return execs;
        }

        public static IList<RegressionExecution> With2StreamOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabase2StreamOuterJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectLargeResultSetCoercion(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseSelectLargeResultSetCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectLargeResultSet(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseSelectLargeResultSet());
            return execs;
        }

        public static IList<RegressionExecution> WithKeyAndRangeIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseKeyAndRangeIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithRangeIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseRangeIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithConstants(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseConstants());
            return execs;
        }

        private class EPLDatabaseConstants : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl =
                    "@name('s0') select * from SupportBean sbr, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 951";
                TryAssertion(env, epl, "s1.mycol1", "951");

                epl =
                    "@name('s0') select * from SupportBean sbr, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 950 and mycol1 = '950'";
                TryAssertion(env, epl, "s1.mycol1", "950");

                epl =
                    "@name('s0') select sum(s1.mycol3) as val from SupportBean sbr unidirectional, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between 950 and 953";
                TryAssertion(env, epl, "val", 950 + 951 + 952 + 953);

                epl =
                    "@name('s0') select sum(s1.mycol3) as val from SupportBean sbr unidirectional, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = '950' and mycol3 between 950 and 953";
                TryAssertion(env, epl, "val", 950);
            }
        }

        private class EPLDatabaseRangeIndex : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from SupportBeanRange sbr, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between RangeStart and RangeEnd";
                env.CompileDeploy(stmtText).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", 10, 12));
                    env.AssertListener("s0", listener => Assert.AreEqual(3, listener.GetAndResetLastNewData().Length));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.That(delta, Is.LessThan(500), "Delta=" + delta);

                // test coercion
                env.UndeployAll();
                stmtText = "@name('s0') select * from SupportBeanRange sbr, " +
                           " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between RangeStartLong and RangeEndLong";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(SupportBeanRange.MakeLong("R", "K", 10L, 12L));
                env.AssertListener("s0", listener => Assert.AreEqual(3, listener.GetAndResetLastNewData().Length));

                env.UndeployAll();
            }
        }

        private class EPLDatabaseKeyAndRangeIndex : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select * from SupportBeanRange sbr, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = Key and mycol3 between RangeStart and RangeEnd";
                env.CompileDeploy(stmtText).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "11", 10, 12));
                    env.AssertListener("s0", listener => Assert.AreEqual(1, listener.GetAndResetLastNewData().Length));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.That(delta, Is.LessThan(500), "Delta=" + delta);

                // test coercion
                env.UndeployAll();
                stmtText = "@name('s0') select * from SupportBeanRange sbr, " +
                           " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = Key and mycol3 between RangeStartLong and RangeEndLong";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(SupportBeanRange.MakeLong("R", "11", 10L, 12L));
                env.AssertListener("s0", listener => Assert.AreEqual(1, listener.GetAndResetLastNewData().Length));

                env.UndeployAll();
            }
        }

        /// <summary>
        /// Test for selecting from a table a large result set and then joining the result outside of the cache.
        /// Verifies performance of indexes cached for resolving join criteria fast.
        /// </summary>
        private class EPLDatabaseSelectLargeResultSet : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select Id, mycol3, mycol2 from " +
                    "SupportBean_S0#keepall as s0," +
                    " sql:MyDBWithLRU100000 ['select mycol3, mycol2 from mytesttable_large'] as s1 where s0.Id = s1.mycol3";
                env.CompileDeploy(stmtText).AddListener("s0");

                // Send 100 events which all perform the join
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 200; i++) {
                    var num = i + 1;
                    var col2 = Convert.ToString(Math.Round(num / 10.0, MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture);
                    var bean = new SupportBean_S0(num);
                    env.SendEventBean(bean);
                    env.AssertPropsNew(
                        "s0",
                        new string[] { "Id", "mycol3", "mycol2" },
                        new object[] { num, num, col2 });
                }

                var endTime = PerformanceObserver.MilliTime;

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLDatabaseSelectLargeResultSetCoercion : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select TheString, mycol3, mycol4 from " +
                               " sql:MyDBWithLRU100000 ['select mycol3, mycol4 from mytesttable_large'] as s0, " +
                               "SupportBean#keepall as s1 where s1.DoubleBoxed = s0.mycol3 and s1.ByteBoxed = s0.mycol4";
                env.CompileDeploy(stmtText).AddListener("s0");

                // Send events which all perform the join
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 200; i++) {
                    var bean = new SupportBean();
                    bean.DoubleBoxed = 100d;
                    bean.ByteBoxed = 10;
                    bean.TheString = "E" + i;
                    env.SendEventBean(bean);
                    env.AssertPropsNew(
                        "s0",
                        new string[] { "TheString", "mycol3", "mycol4" },
                        new object[] { "E" + i, 100, 10 });
                }

                var endTime = PerformanceObserver.MilliTime;

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);

                env.UndeployAll();
            }
        }

        private class EPLDatabase2StreamOuterJoin : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select TheString, mycol3, mycol1 from " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 right outer join " +
                               "SupportBean as s0 on TheString = mycol1";
                env.CompileDeploy(stmtText).AddListener("s0");

                // Send events which all perform the join
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 200; i++) {
                    var beanX = new SupportBean();
                    beanX.TheString = "50";
                    env.SendEventBean(beanX);
                    env.AssertPropsNew(
                        "s0",
                        new string[] { "TheString", "mycol3", "mycol1" },
                        new object[] { "50", 50, "50" });
                }

                var endTime = PerformanceObserver.MilliTime;

                // no matching
                var bean = new SupportBean();
                bean.TheString = "-1";
                env.SendEventBean(bean);
                env.AssertPropsNew(
                    "s0",
                    new string[] { "TheString", "mycol3", "mycol1" },
                    new object[] { "-1", null, null });

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);

                env.UndeployAll();
            }
        }

        private class EPLDatabaseOuterJoinPlusWhere : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select TheString, mycol3, mycol1 from " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 right outer join " +
                               "SupportBean as s0 on TheString = mycol1 where s1.mycol3 = s0.IntPrimitive";
                env.CompileDeploy(stmtText).AddListener("s0");

                // Send events which all perform the join
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 200; i++) {
                    var beanX = new SupportBean();
                    beanX.TheString = "50";
                    beanX.IntPrimitive = 50;
                    env.SendEventBean(beanX);
                    env.AssertPropsNew(
                        "s0",
                        new string[] { "TheString", "mycol3", "mycol1" },
                        new object[] { "50", 50, "50" });
                }

                var endTime = PerformanceObserver.MilliTime;

                // no matching on-clause
                var bean = new SupportBean();
                env.AssertListenerNotInvoked("s0");

                // matching on-clause not matching where
                bean = new SupportBean();
                bean.TheString = "50";
                bean.IntPrimitive = 49;
                env.SendEventBean(bean);
                env.AssertListenerNotInvoked("s0");

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);

                env.UndeployAll();
            }
        }

        private class EPLDatabaseInKeywordSingleIndex : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') " +
                               IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                               "select * from SupportBean_S0 s0, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 " +
                               " where mycol1 in (P00, P01, P02)";
                env.CompileDeploy(stmtText).AddListener("s0");

                var historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
                Assert.AreEqual(nameof(PollResultIndexingStrategyHashForge), historical.IndexName);
                Assert.AreEqual(nameof(HistoricalIndexLookupStrategyInKeywordSingleForge), historical.StrategyName);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 2000; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "x", "y", "815"));
                    env.AssertEqualsNew("s0", "s1.mycol3", 815);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.That(delta, Is.LessThan(500), "Delta=" + delta);

                env.UndeployAll();
            }
        }

        private class EPLDatabaseInKeywordMultiIndex : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') " +
                               IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                               "select * from SupportBean_S0 s0, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol2, mycol3 from mytesttable_large'] as s1 " +
                               " where P00 in (mycol2, mycol1)";
                env.CompileDeploy(stmtText).AddListener("s0");

                var historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
                Assert.AreEqual(nameof(PollResultIndexingStrategyInKeywordMultiForge), historical.IndexName);
                Assert.AreEqual(nameof(HistoricalIndexLookupStrategyInKeywordMultiForge), historical.StrategyName);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 2000; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "815"));
                    env.AssertEqualsNew("s0", "s1.mycol3", 815);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.That(delta, Is.LessThan(500), "Delta=" + delta);

                env.UndeployAll();
            }
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string epl,
            string field,
            object expected)
        {
            env.CompileDeploy(epl).AddListener("s0");

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(new SupportBean("E", 0));
                env.AssertEqualsNew("s0", field, expected);
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            //log.info("delta=" + delta);
            Assert.That(delta, Is.LessThan(500), "Delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace