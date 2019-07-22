///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
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
            execs.Add(new EPLDatabaseConstants());
            execs.Add(new EPLDatabaseRangeIndex());
            execs.Add(new EPLDatabaseKeyAndRangeIndex());
            execs.Add(new EPLDatabaseSelectLargeResultSet());
            execs.Add(new EPLDatabaseSelectLargeResultSetCoercion());
            execs.Add(new EPLDatabase2StreamOuterJoin());
            execs.Add(new EPLDatabaseOuterJoinPlusWhere());
            execs.Add(new EPLDatabaseInKeywordSingleIndex());
            execs.Add(new EPLDatabaseInKeywordMultiIndex());
            return execs;
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
                Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get(field));
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            //log.info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            env.UndeployAll();
        }

        internal class EPLDatabaseConstants : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl =
                    "@Name('s0') select * from SupportBean sbr, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 951";
                TryAssertion(env, epl, "s1.mycol1", "951");

                epl =
                    "@Name('s0') select * from SupportBean sbr, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 950 and mycol1 = '950'";
                TryAssertion(env, epl, "s1.mycol1", "950");

                epl =
                    "@Name('s0') select sum(s1.mycol3) as val from SupportBean sbr unIdirectional, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between 950 and 953";
                TryAssertion(env, epl, "val", 950 + 951 + 952 + 953);

                epl =
                    "@Name('s0') select sum(s1.mycol3) as val from SupportBean sbr unIdirectional, sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = '950' and mycol3 between 950 and 953";
                TryAssertion(env, epl, "val", 950);
            }
        }

        internal class EPLDatabaseRangeIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from SupportBeanRange sbr, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between rangeStart and rangeEnd";
                env.CompileDeploy(stmtText).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", 10, 12));
                    Assert.AreEqual(3, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.IsTrue(delta < 500, "Delta=" + delta);

                // test coercion
                env.UndeployAll();
                stmtText = "@Name('s0') select * from SupportBeanRange sbr, " +
                           " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between rangeStartLong and rangeEndLong";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(SupportBeanRange.MakeLong("R", "K", 10L, 12L));
                Assert.AreEqual(3, env.Listener("s0").GetAndResetLastNewData().Length);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseKeyAndRangeIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from SupportBeanRange sbr, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = key and mycol3 between rangeStart and rangeEnd";
                env.CompileDeploy(stmtText).AddListener("s0");

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBeanRange("R", "11", 10, 12));
                    Assert.AreEqual(1, env.Listener("s0").GetAndResetLastNewData().Length);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.IsTrue(delta < 500, "Delta=" + delta);

                // test coercion
                env.UndeployAll();
                stmtText = "@Name('s0') select * from SupportBeanRange sbr, " +
                           " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = key and mycol3 between rangeStartLong and rangeEndLong";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(SupportBeanRange.MakeLong("R", "11", 10L, 12L));
                Assert.AreEqual(1, env.Listener("s0").GetAndResetLastNewData().Length);

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     Test for selecting from a table a large result set and then joining the result outside of the cache.
        ///     Verifies performance of indexes cached for resolving join criteria fast.
        /// </summary>
        internal class EPLDatabaseSelectLargeResultSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Id, mycol3, mycol2 from " +
                               "SupportBean_S0#keepall as s0," +
                               " sql:MyDBWithLRU100000 ['select mycol3, mycol2 from mytesttable_large'] as s1 where s0.Id = s1.mycol3";
                env.CompileDeploy(stmtText).AddListener("s0");

                // Send 100 events which all perform the join
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 200; i++) {
                    var num = i + 1;
                    var col2 = Convert.ToString(Math.Round((float) num / 10));
                    var bean = new SupportBean_S0(num);
                    env.SendEventBean(bean);
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        new[] {"Id", "mycol3", "mycol2"},
                        new object[] {num, num, col2});
                }

                var endTime = PerformanceObserver.MilliTime;

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseSelectLargeResultSetCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select TheString, mycol3, mycol4 from " +
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
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        new[] {"TheString", "mycol3", "mycol4"},
                        new object[] {"E" + i, 100, 10});
                }

                var endTime = PerformanceObserver.MilliTime;

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);

                env.UndeployAll();
            }
        }

        internal class EPLDatabase2StreamOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select TheString, mycol3, mycol1 from " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 right outer join " +
                               "SupportBean as s0 on TheString = mycol1";
                env.CompileDeploy(stmtText).AddListener("s0");

                // Send events which all perform the join
                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 200; i++) {
                    var beanX = new SupportBean();
                    beanX.TheString = "50";
                    env.SendEventBean(beanX);
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        new[] {"TheString", "mycol3", "mycol1"},
                        new object[] {"50", 50, "50"});
                }

                var endTime = PerformanceObserver.MilliTime;

                // no matching
                var bean = new SupportBean();
                bean.TheString = "-1";
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"TheString", "mycol3", "mycol1"},
                    new object[] {"-1", null, null});

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseOuterJoinPlusWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select TheString, mycol3, mycol1 from " +
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
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        new[] {"TheString", "mycol3", "mycol1"},
                        new object[] {"50", 50, "50"});
                }

                var endTime = PerformanceObserver.MilliTime;

                // no matching on-clause
                var bean = new SupportBean();
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // matching on-clause not matching where
                bean = new SupportBean();
                bean.TheString = "50";
                bean.IntPrimitive = 49;
                env.SendEventBean(bean);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // log.info("delta=" + (endTime - startTime));
                Assert.IsTrue(endTime - startTime < 500);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseInKeywordSingleIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') " +
                               INDEX_CALLBACK_HOOK +
                               "select * from SupportBean_S0 s0, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol3 from mytesttable_large'] as s1 " +
                               " where mycol1 in (p00, p01, p02)";
                env.CompileDeploy(stmtText).AddListener("s0");

                var historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
                Assert.AreEqual(typeof(PollResultIndexingStrategyHashForge).Name, historical.IndexName);
                Assert.AreEqual(
                    typeof(HistoricalIndexLookupStrategyInKeywordSingleForge).Name,
                    historical.StrategyName);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 2000; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "x", "y", "815"));
                    Assert.AreEqual(815, env.Listener("s0").AssertOneGetNewAndReset().Get("s1.mycol3"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.IsTrue(delta < 500, "Delta=" + delta);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseInKeywordMultiIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') " +
                               INDEX_CALLBACK_HOOK +
                               "select * from SupportBean_S0 s0, " +
                               " sql:MyDBWithLRU100000 ['select mycol1, mycol2, mycol3 from mytesttable_large'] as s1 " +
                               " where p00 in (mycol2, mycol1)";
                env.CompileDeploy(stmtText).AddListener("s0");

                var historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
                Assert.AreEqual(typeof(PollResultIndexingStrategyInKeywordMultiForge).Name, historical.IndexName);
                Assert.AreEqual(typeof(HistoricalIndexLookupStrategyInKeywordMultiForge).Name, historical.StrategyName);

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 2000; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "815"));
                    Assert.AreEqual(815, env.Listener("s0").AssertOneGetNewAndReset().Get("s1.mycol3"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                // log.info("delta=" + delta);
                Assert.IsTrue(delta < 500, "Delta=" + delta);

                env.UndeployAll();
            }
        }
    }
} // end of namespace