///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitAggregateGrouped
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
        private const string CATEGORY = "Aggregated and Grouped";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if TEMPORARY
            WithLastNoDataWindow(execs);
            WithWildcardRowPerGroup(execs);
            WithUnaggregatedOutputFirst(execs);
            With1NoneNoHavingNoJoin(execs);
            With2NoneNoHavingJoin(execs);
            With3NoneHavingNoJoin(execs);
            With4NoneHavingJoin(execs);
            With5DefaultNoHavingNoJoin(execs);
            With6DefaultNoHavingJoin(execs);
            With7DefaultHavingNoJoin(execs);
            With8DefaultHavingJoin(execs);
            With9AllNoHavingNoJoin(execs);
            With10AllNoHavingJoin(execs);
            With11AllHavingNoJoin(execs);
            With12AllHavingJoin(execs);
            With13LastNoHavingNoJoin(execs);
            With14LastNoHavingJoin(execs);
            With15LastHavingNoJoin(execs);
            With16LastHavingJoin(execs);
            With17FirstNoHavingNoJoin(execs);
            With17FirstNoHavingJoin(execs);
            With18SnapshotNoHavingNoJoin(execs);
            WithHaving(execs);
            WithHavingJoin(execs);
            WithJoinSortWindow(execs);
            WithLimitSnapshot(execs);
            WithLimitSnapshotJoin(execs);
            WithMaxTimeWindow(execs);
            WithNoJoinLast(execs);
            WithNoOutputClauseView(execs);
            WithNoJoinDefault(execs);
            WithJoinDefault(execs);
            WithNoJoinAll(execs);
            WithJoinAll(execs);
            WithJoinLast(execs);
            WithOutputFirstHavingJoinNoJoin(execs);
            WithOutputAllMultikeyWArray(execs);
            WithOutputLastMultikeyWArray(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithOutputLastMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputLastMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputAllMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputAllMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirstHavingJoinNoJoin(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstHavingJoinNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinLast());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinAll());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoJoinAll());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinDefault(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinDefault());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinDefault(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoJoinDefault());
            return execs;
        }

        public static IList<RegressionExecution> WithNoOutputClauseView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoOutputClauseView());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoJoinLast());
            return execs;
        }

        public static IList<RegressionExecution> WithMaxTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetMaxTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitSnapshotJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitSnapshotJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitSnapshot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitSnapshot());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinSortWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinSortWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetHaving());
            return execs;
        }

        public static IList<RegressionExecution> With18SnapshotNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With17FirstNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet17FirstNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With16LastHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet16LastHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With15LastHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet15LastHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With14LastNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet14LastNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With13LastNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet13LastNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With12AllHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet12AllHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With11AllHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet11AllHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With10AllNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet10AllNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With9AllNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet9AllNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With8DefaultHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet8DefaultHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With7DefaultHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet7DefaultHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With6DefaultNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet6DefaultNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With5DefaultNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet5DefaultNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With4NoneHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet4NoneHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With3NoneHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet3NoneHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> With2NoneNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet2NoneNoHavingJoin());
            return execs;
        }

        public static IList<RegressionExecution> With1NoneNoHavingNoJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet1NoneNoHavingNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithUnaggregatedOutputFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetUnaggregatedOutputFirst());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardRowPerGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetWildcardRowPerGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithLastNoDataWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLastNoDataWindow());
            return execs;
        }

        internal class ResultSetLastNoDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select theString, intPrimitive as intp from SupportBean group by theString output last every 1 seconds order by theString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E3", 31));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 2));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E2", 22));
                env.SendEventBean(new SupportBean("E2", 21));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 3));
                env.AdvanceTime(1000);

                env.AssertPropsPerRowLastNew(
                    "s0",
                    new string[] { "theString", "intp" },
                    new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 21 }, new object[] { "E3", 31 } });

                env.SendEventBean(new SupportBean("E3", 31));
                env.SendEventBean(new SupportBean("E1", 5));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E3", 33));
                env.AdvanceTime(2000);

                env.AssertPropsPerRowLastNew(
                    "s0",
                    new string[] { "theString", "intp" },
                    new object[][] { new object[] { "E1", 5 }, new object[] { "E3", 33 } });

                env.UndeployAll();
            }
        }

        internal class ResultSetWildcardRowPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportBean group by theString output last every 3 events order by theString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("IBM", 10));
                env.SendEventBean(new SupportBean("ATT", 11));
                env.SendEventBean(new SupportBean("IBM", 100));

                env.AssertListener(
                    "s0",
                    listener => {
                        var events = listener.NewDataListFlattened;
                        env.ListenerReset("s0");
                        Assert.AreEqual(2, events.Length);
                        Assert.AreEqual("ATT", events[0].Get("theString"));
                        Assert.AreEqual(11, events[0].Get("intPrimitive"));
                        Assert.AreEqual("IBM", events[1].Get("theString"));
                        Assert.AreEqual(100, events[1].Get("intPrimitive"));
                    });
                env.UndeployAll();

                // All means each event
                epl = "@name('s0') select * from SupportBean group by theString output all every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("IBM", 10));
                env.SendEventBean(new SupportBean("ATT", 11));
                env.SendEventBean(new SupportBean("IBM", 100));

                env.AssertListener(
                    "s0",
                    listener => {
                        var events = listener.NewDataListFlattened;
                        Assert.AreEqual(3, events.Length);
                        Assert.AreEqual("IBM", events[0].Get("theString"));
                        Assert.AreEqual(10, events[0].Get("intPrimitive"));
                        Assert.AreEqual("ATT", events[1].Get("theString"));
                        Assert.AreEqual(11, events[1].Get("intPrimitive"));
                        Assert.AreEqual("IBM", events[2].Get("theString"));
                        Assert.AreEqual(100, events[2].Get("intPrimitive"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputLastMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = "theString,longPrimitive,intPrimitive,thesum".SplitCsv();
                var epl =
                    "@name('s0') select theString, longPrimitive, intPrimitive, sum(intPrimitive) as thesum from SupportBean#keepall " +
                    "group by theString, longPrimitive output last every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanEvent(env, "A", 0, 10);
                SendBeanEvent(env, "B", 1, 11);

                env.Milestone(0);

                SendBeanEvent(env, "A", 0, 12);
                SendBeanEvent(env, "C", 0, 13);

                env.AdvanceTime(1000);
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        fields,
                        new object[][] {
                            new object[] { "B", 1L, 11, 11 }, new object[] { "A", 0L, 12, 22 },
                            new object[] { "C", 0L, 13, 13 }
                        }));

                SendBeanEvent(env, "A", 0, 14);

                env.AdvanceTime(2000);
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        fields,
                        new object[][] {
                            new object[] { "A", 0L, 14, 36 }
                        }));

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputAllMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = "theString,longPrimitive,intPrimitive,thesum".SplitCsv();
                var epl =
                    "@name('s0') select theString, longPrimitive, intPrimitive, sum(intPrimitive) as thesum from SupportBean#keepall " +
                    "group by theString, longPrimitive output all every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanEvent(env, "A", 0, 10);
                SendBeanEvent(env, "B", 1, 11);

                env.Milestone(0);

                SendBeanEvent(env, "A", 0, 12);
                SendBeanEvent(env, "C", 0, 13);

                env.AdvanceTime(1000);
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        fields,
                        new object[][] {
                            new object[] { "A", 0L, 10, 10 }, new object[] { "B", 1L, 11, 11 },
                            new object[] { "A", 0L, 12, 22 }, new object[] { "C", 0L, 13, 13 }
                        }));

                SendBeanEvent(env, "A", 0, 14);

                env.AdvanceTime(2000);
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        fields,
                        new object[][] {
                            new object[] { "A", 0L, 14, 36 }, new object[] { "B", 1L, 11, 11 },
                            new object[] { "C", 0L, 13, 13 }
                        }));

                env.UndeployAll();
            }
        }

        internal class ResultSetUnaggregatedOutputFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var fields = "theString,intPrimitive".SplitCsv();
                var epl = "@name('s0') select * from SupportBean\n" +
                          "     group by theString\n" +
                          "     output first every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E2", 3));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 3 });

                env.Milestone(1);

                SendTimer(env, 5000);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 4));
                env.AssertPropsNew("s0", fields, new object[] { "E3", 4 });

                env.SendEventBean(new SupportBean("E2", 5));
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 10000);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E3", 6));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E1", 7));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 7 });

                env.SendEventBean(new SupportBean("E1", 8));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E2", 9));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 9 });

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 11));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstHavingJoinNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtText =
                    "@name('s0') select theString, longPrimitive, sum(intPrimitive) as value from MyWindow group by theString having sum(intPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtText, milestone);

                var stmtTextJoin =
                    "@name('s0') select theString, longPrimitive, sum(intPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.theString " +
                    "group by theString having sum(intPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtTextJoin, milestone);

                var stmtTextOrder =
                    "@name('s0') select theString, longPrimitive, sum(intPrimitive) as value from MyWindow group by theString having sum(intPrimitive) > 20 output first every 2 events order by theString asc";
                TryOutputFirstHaving(env, stmtTextOrder, milestone);

                var stmtTextOrderJoin =
                    "@name('s0') select theString, longPrimitive, sum(intPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.id = mv.theString " +
                    "group by theString having sum(intPrimitive) > 20 output first every 2 events order by theString asc";
                TryOutputFirstHaving(env, stmtTextOrderJoin, milestone);
            }
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by symbol";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "group by symbol";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by symbol " +
                               " having sum(price) > 50";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "group by symbol " +
                               "having sum(price) > 50";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by symbol " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "group by symbol " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "group by symbol " +
                               "having sum(price) > 50" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "group by symbol " +
                               "having sum(price) > 50" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet9AllNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by symbol " +
                               "output all every 1 seconds " +
                               "order by symbol";
                TryAssertion9_10(env, stmtText, "all", new AtomicLong());
            }
        }

        internal class ResultSet10AllNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "group by symbol " +
                               "output all every 1 seconds " +
                               "order by symbol";
                TryAssertion9_10(env, stmtText, "all", new AtomicLong());
            }
        }

        internal class ResultSet11AllHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion11AllHavingNoJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        internal class ResultSet12AllHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion12AllHavingJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        internal class ResultSet13LastNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by symbol " +
                               "output last every 1 seconds " +
                               "order by symbol";
                TryAssertion13_14(env, stmtText, "last", new AtomicLong());
            }
        }

        internal class ResultSet14LastNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "group by symbol " +
                               "output last every 1 seconds " +
                               "order by symbol";
                TryAssertion13_14(env, stmtText, "last", new AtomicLong());
            }
        }

        internal class ResultSet15LastHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion15LastHavingNoJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        internal class ResultSet16LastHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion16LastHavingJoin(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion16LastHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select symbol, volume, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        internal class ResultSet17FirstNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by symbol " +
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet17FirstNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "group by symbol " +
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, volume, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by symbol " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "snapshot", new AtomicLong());
            }
        }

        internal class ResultSetHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream symbol, volume, sum(price) as sumprice" +
                          " from SupportMarketDataBean#time(10 sec) " +
                          "group by symbol " +
                          "having sum(price) >= 10 " +
                          "output every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionHavingDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream symbol, volume, sum(price) as sumprice" +
                          " from SupportMarketDataBean#time(10 sec) as s0," +
                          "SupportBean#keepall as s1 " +
                          "where s0.symbol = s1.theString " +
                          "group by symbol " +
                          "having sum(price) >= 10 " +
                          "output every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("IBM", 0));

                TryAssertionHavingDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream symbol, volume, max(price) as maxVol" +
                          " from SupportMarketDataBean#sort(1, volume) as s0," +
                          "SupportBean#keepall as s1 where s1.theString = s0.symbol " +
                          "group by symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("JOIN_KEY", -1));

                SendEvent(env, "JOIN_KEY", 1d);
                SendEvent(env, "JOIN_KEY", 2d);
                env.ListenerReset("s0");

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                env.AssertListener(
                    "s0",
                    listener => {
                        var result = listener.DataListsFlattened;
                        Assert.AreEqual(2, result.First.Length);
                        Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
                        Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
                        Assert.AreEqual(1, result.Second.Length);
                        Assert.AreEqual(2.0, result.Second[0].Get("maxVol"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt =
                    "@name('s0') select symbol, volume, sum(price) as sumprice from SupportMarketDataBean" +
                    "#time(10 seconds) group by symbol output snapshot every 1 seconds";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendEvent(env, "s0", 1, 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 2, 16);
                SendEvent(env, "s0", 3, 14);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                var fields = new string[] { "symbol", "volume", "sumprice" };
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "s0", 1L, 34d }, new object[] { "IBM", 2L, 16d }, new object[] { "s0", 3L, 34d }
                    });

                SendTimer(env, 1500);
                SendEvent(env, "MSFT", 4, 18);
                SendEvent(env, "IBM", 5, 30);

                SendTimer(env, 10000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "s0", 1L, 34d }, new object[] { "IBM", 2L, 46d }, new object[] { "s0", 3L, 34d },
                        new object[] { "MSFT", 4L, 18d }, new object[] { "IBM", 5L, 46d }
                    });

                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "MSFT", 4L, 18d }, new object[] { "IBM", 5L, 30d } });

                SendTimer(env, 12000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                SendTimer(env, 13000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshotJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt =
                    "@name('s0') select symbol, volume, sum(price) as sumprice from SupportMarketDataBean" +
                    "#time(10 seconds) as m, SupportBean" +
                    "#keepall as s where s.theString = m.symbol group by symbol output snapshot every 1 seconds order by symbol, volume asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                env.SendEventBean(new SupportBean("ABC", 1));
                env.SendEventBean(new SupportBean("IBM", 2));
                env.SendEventBean(new SupportBean("MSFT", 3));

                SendEvent(env, "ABC", 1, 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 2, 16);
                SendEvent(env, "ABC", 3, 14);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                var fields = new string[] { "symbol", "volume", "sumprice" };
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "ABC", 1L, 34d }, new object[] { "ABC", 3L, 34d },
                        new object[] { "IBM", 2L, 16d }
                    });

                SendTimer(env, 1500);
                SendEvent(env, "MSFT", 4, 18);
                SendEvent(env, "IBM", 5, 30);

                SendTimer(env, 10000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "ABC", 1L, 34d }, new object[] { "ABC", 3L, 34d },
                        new object[] { "IBM", 2L, 46d }, new object[] { "IBM", 5L, 46d },
                        new object[] { "MSFT", 4L, 18d }
                    });

                SendTimer(env, 10500);
                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "IBM", 5L, 30d }, new object[] { "MSFT", 4L, 18d } });

                SendTimer(env, 11500);
                SendTimer(env, 12000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                SendTimer(env, 13000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                env.UndeployAll();
            }
        }

        internal class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select irstream symbol, " +
                          "volume, max(price) as maxVol" +
                          " from SupportMarketDataBean#time(1 sec) " +
                          "group by symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "SYM1", 1d);
                SendEvent(env, "SYM1", 2d);
                env.ListenerReset("s0");

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                env.AssertListener(
                    "s0",
                    listener => {
                        var result = listener.DataListsFlattened;
                        Assert.AreEqual(2, result.First.Length);
                        Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
                        Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
                        Assert.AreEqual(2, result.Second.Length);
                        Assert.AreEqual(null, result.Second[0].Get("maxVol"));
                        Assert.AreEqual(null, result.Second[1].Get("maxVol"));
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetNoJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionNoJoinLast(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSetNoOutputClauseView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select symbol, volume, sum(price) as mySum " +
                          "from SupportMarketDataBean#length(5) " +
                          "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                          "group by symbol ";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSingle(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetNoJoinDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select symbol, volume, sum(price) as mySum " +
                          "from SupportMarketDataBean#length(5) " +
                          "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                          "group by symbol " +
                          "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select symbol, volume, sum(price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(5) as two " +
                          "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                          "  and one.theString = two.symbol " +
                          "group by symbol " +
                          "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetNoJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionNoJoinAll(env, outputLimitOpt);
                }
            }

            private static void TryAssertionNoJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = opt.GetHint() +
                          "@name('s0') select symbol, volume, sum(price) as mySum " +
                          "from SupportMarketDataBean#length(5) " +
                          "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                          "group by symbol " +
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionAll(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionJoinAll(env, outputLimitOpt);
                }
            }

            private static void TryAssertionJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = opt.GetHint() +
                          "@name('s0') select symbol, volume, sum(price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(5) as two " +
                          "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                          "  and one.theString = two.symbol " +
                          "group by symbol " +
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionAll(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionJoinLast(env, outputLimitOpt);
                }
            }
        }

        private static void TryAssertionJoinLast(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            var epl = opt.GetHint() +
                      "@name('s0') select symbol, volume, sum(price) as mySum " +
                      "from SupportBeanString#length(100) as one, " +
                      "SupportMarketDataBean#length(5) as two " +
                      "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                      "  and one.theString = two.symbol " +
                      "group by symbol " +
                      "output last every 2 events";

            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
            env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

            TryAssertionLast(env);

            env.UndeployAll();
        }

        private static void TryAssertionHavingDefault(RegressionEnvironment env)
        {
            SendEvent(env, "IBM", 1, 5);
            SendEvent(env, "IBM", 2, 6);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "IBM", 3, -3);
            var fields = "symbol,volume,sumprice".SplitCsv();
            env.AssertPropsNew("s0", fields, new object[] { "IBM", 2L, 11.0 });

            SendTimer(env, 5000);
            SendEvent(env, "IBM", 4, 10);
            SendEvent(env, "IBM", 5, 0);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "IBM", 6, 1);
            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(3, listener.LastNewData.Length);
                    EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "IBM", 4L, 18.0 });
                    EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "IBM", 5L, 18.0 });
                    EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "IBM", 6L, 19.0 });
                    env.ListenerReset("s0");
                });

            SendTimer(env, 11000);
            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(3, listener.LastOldData.Length);
                    EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "IBM", 1L, 11.0 });
                    EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "IBM", 2L, 11.0 });
                    env.ListenerReset("s0");
                });
        }

        private static void TryAssertionDefault(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    Assert.AreEqual(typeof(string), eventType.GetPropertyType("symbol"));
                    Assert.AreEqual(typeof(long?), eventType.GetPropertyType("volume"));
                    Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mySum"));
                });

            SendEvent(env, SYMBOL_IBM, 500, 20);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            var fields = "symbol,volume,mySum".SplitCsv();
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.DataListsFlattened;
                    if (events.First[0].Get("symbol").Equals(SYMBOL_IBM)) {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_IBM, 500L, 20.0 }, new object[] { SYMBOL_DELL, 10000L, 51.0 }
                            });
                    }
                    else {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_DELL, 10000L, 51.0 }, new object[] { SYMBOL_IBM, 500L, 20.0 }
                            });
                    }

                    Assert.IsNull(listener.LastOldData);
                    listener.Reset();
                });

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_DELL, 40000, 45);
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.DataListsFlattened;
                    EPAssertionUtil.AssertPropsPerRow(
                        events.First,
                        fields,
                        new object[][] {
                            new object[] { SYMBOL_DELL, 20000L, 51.0 + 52.0 },
                            new object[] { SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0 }
                        });
                    Assert.IsNull(listener.LastOldData);
                });
        }

        private static void TryAssertionAll(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    Assert.AreEqual(typeof(string), eventType.GetPropertyType("symbol"));
                    Assert.AreEqual(typeof(long?), eventType.GetPropertyType("volume"));
                    Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mySum"));
                });

            SendEvent(env, SYMBOL_IBM, 500, 20);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            var fields = "symbol,volume,mySum".SplitCsv();
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.DataListsFlattened;
                    if (events.First[0].Get("symbol").Equals(SYMBOL_IBM)) {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_IBM, 500L, 20.0 }, new object[] { SYMBOL_DELL, 10000L, 51.0 }
                            });
                    }
                    else {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_DELL, 10000L, 51.0 }, new object[] { SYMBOL_IBM, 500L, 20.0 }
                            });
                    }

                    Assert.IsNull(listener.LastOldData);
                    listener.Reset();
                });

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_DELL, 40000, 45);
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.DataListsFlattened;
                    if (events.First[0].Get("symbol").Equals(SYMBOL_IBM)) {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_IBM, 500L, 20.0 },
                                new object[] { SYMBOL_DELL, 20000L, 51.0 + 52.0 },
                                new object[] { SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0 }
                            });
                    }
                    else {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_DELL, 20000L, 51.0 + 52.0 },
                                new object[] { SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0 },
                                new object[] { SYMBOL_IBM, 500L, 20.0 }
                            });
                    }

                    Assert.IsNull(listener.LastOldData);
                });
        }

        private static void TryAssertionLast(RegressionEnvironment env)
        {
            var fields = "symbol,volume,mySum".SplitCsv();
            SendEvent(env, SYMBOL_DELL, 10000, 51);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.DataListsFlattened;
                    EPAssertionUtil.AssertPropsPerRow(
                        events.First,
                        fields,
                        new object[][] { new object[] { SYMBOL_DELL, 20000L, 103.0 } });
                    Assert.IsNull(listener.LastOldData);
                    listener.Reset();
                });

            SendEvent(env, SYMBOL_DELL, 30000, 70);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_IBM, 10000, 20);
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.DataListsFlattened;
                    if (events.First[0].Get("symbol").Equals(SYMBOL_DELL)) {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_DELL, 30000L, 173.0 }, new object[] { SYMBOL_IBM, 10000L, 20.0 }
                            });
                    }
                    else {
                        EPAssertionUtil.AssertPropsPerRow(
                            events.First,
                            fields,
                            new object[][] {
                                new object[] { SYMBOL_IBM, 10000L, 20.0 }, new object[] { SYMBOL_DELL, 30000L, 173.0 }
                            });
                    }

                    Assert.IsNull(listener.LastOldData);
                });
        }

        private static void TryOutputFirstHaving(
            RegressionEnvironment env,
            string statementText,
            AtomicLong milestone)
        {
            var fields = "theString,longPrimitive,value".SplitCsv();
            var fieldsLimited = "theString,value".SplitCsv();
            var epl = "create window MyWindow#keepall as SupportBean;\n" +
                      "insert into MyWindow select * from SupportBean;\n" +
                      "on SupportMarketDataBean md delete from MyWindow mw where mw.intPrimitive = md.price;\n" +
                      statementText;
            var compiled = env.Compile(epl);
            env.Deploy(compiled).AddListener("s0");

            env.SendEventBean(new SupportBean_A("E1"));
            env.SendEventBean(new SupportBean_A("E2"));

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E1", 101, 10);
            SendBeanEvent(env, "E2", 102, 15);
            SendBeanEvent(env, "E1", 103, 10);
            SendBeanEvent(env, "E2", 104, 5);
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E2", 105, 5);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 105L, 25 });

            SendBeanEvent(env, "E2", 106, -6); // to 19, does not count toward condition
            SendBeanEvent(env, "E2", 107, 2); // to 21, counts toward condition
            env.AssertListenerNotInvoked("s0");
            SendBeanEvent(env, "E2", 108, 1);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 108L, 22 });

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E2", 109, 1); // to 23, counts toward condition
            env.AssertListenerNotInvoked("s0");
            SendBeanEvent(env, "E2", 110, 1); // to 24
            env.AssertPropsNew("s0", fields, new object[] { "E2", 110L, 24 });

            SendBeanEvent(env, "E2", 111, -10); // to 14
            SendBeanEvent(env, "E2", 112, 10); // to 24, counts toward condition
            env.AssertListenerNotInvoked("s0");
            SendBeanEvent(env, "E2", 113, 0); // to 24, counts toward condition
            env.AssertPropsNew("s0", fields, new object[] { "E2", 113L, 24 });

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E2", 114, -10); // to 14
            SendBeanEvent(env, "E2", 115, 1); // to 15
            SendBeanEvent(env, "E2", 116, 5); // to 20
            SendBeanEvent(env, "E2", 117, 0); // to 20
            SendBeanEvent(env, "E2", 118, 1); // to 21    // counts
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, "E2", 119, 0); // to 21
            env.AssertPropsNew("s0", fields, new object[] { "E2", 119L, 21 });

            // remove events
            SendMDEvent(env, "E2", 0); // remove 113, 117, 119 (any order of delete!)
            env.AssertPropsNew("s0", fieldsLimited, new object[] { "E2", 21 });

            env.MilestoneInc(milestone);

            // remove events
            SendMDEvent(env, "E2", -10); // remove 111, 114
            env.AssertPropsNew("s0", fieldsLimited, new object[] { "E2", 41 });

            env.MilestoneInc(milestone);

            // remove events
            SendMDEvent(env, "E2", -6); // since there is 3*0 we output the next one
            env.AssertPropsNew("s0", fieldsLimited, new object[] { "E2", 47 });

            SendMDEvent(env, "E2", 2);
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(800, 1, new object[][] { new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 150L, 49d } });
            expected.AddResultInsert(1500, 2, new object[][] { new object[] { "YAH", 10000L, 1d } });
            expected.AddResultInsert(2100, 1, new object[][] { new object[] { "IBM", 155L, 75d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 11000L, 3d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 150L, 97d } });
            expected.AddResultInsert(4900, 1, new object[][] { new object[] { "YAH", 11500L, 6d } });
            expected.AddResultRemove(5700, 0, new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultInsert(5900, 1, new object[][] { new object[] { "YAH", 10500L, 7d } });
            expected.AddResultRemove(6300, 0, new object[][] { new object[] { "MSFT", 5000L, null } });
            expected.AddResultRemove(
                7000,
                0,
                new object[][] { new object[] { "IBM", 150L, 48d }, new object[] { "YAH", 10000L, 6d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion34(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(2100, 1, new object[][] { new object[] { "IBM", 155L, 75d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 150L, 97d } });
            expected.AddResultRemove(5700, 0, new object[][] { new object[] { "IBM", 100L, 72d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion13_14(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] { new object[] { "IBM", 155L, 75d }, new object[] { "YAH", 10000L, 1d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][] { new object[] { "YAH", 11000L, 3d } });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] { new object[] { "IBM", 150L, 97d }, new object[] { "YAH", 11500L, 6d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "YAH", 10500L, 7d } },
                new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultRemove(
                7200,
                0,
                new object[][] {
                    new object[] { "IBM", 150L, 48d }, new object[] { "MSFT", 5000L, null },
                    new object[] { "YAH", 10000L, 6d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion15_16(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(2200, 0, new object[][] { new object[] { "IBM", 155L, 75d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "IBM", 150L, 97d } });
            expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultInsRem(7200, 0, null, null);

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion78(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(2200, 0, new object[][] { new object[] { "IBM", 155L, 75d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "IBM", 150L, 97d } });
            expected.AddResultInsRem(6200, 0, null, new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultInsRem(7200, 0, null, null);

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion56(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] {
                    new object[] { "IBM", 150L, 49d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 75d }
                });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new object[][] { new object[] { "YAH", 11000L, 3d } });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] { new object[] { "IBM", 150L, 97d }, new object[] { "YAH", 11500L, 6d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "YAH", 10500L, 7d } },
                new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultRemove(
                7200,
                0,
                new object[][] {
                    new object[] { "MSFT", 5000L, null }, new object[] { "IBM", 150L, 48d },
                    new object[] { "YAH", 10000L, 6d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion9_10(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] {
                    new object[] { "IBM", 150L, 49d }, new object[] { "IBM", 155L, 75d },
                    new object[] { "MSFT", 5000L, 9d }, new object[] { "YAH", 10000L, 1d }
                });
            expected.AddResultInsert(
                3200,
                0,
                new object[][] {
                    new object[] { "IBM", 155L, 75d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "YAH", 10000L, 1d }
                });
            expected.AddResultInsert(
                4200,
                0,
                new object[][] {
                    new object[] { "IBM", 155L, 75d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "YAH", 11000L, 3d }
                });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] {
                    new object[] { "IBM", 150L, 97d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "YAH", 11500L, 6d }
                });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] {
                    new object[] { "IBM", 150L, 72d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "YAH", 10500L, 7d }
                },
                new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][] {
                    new object[] { "IBM", 150L, 48d }, new object[] { "MSFT", 5000L, null },
                    new object[] { "YAH", 10500L, 6d }
                },
                new object[][] {
                    new object[] { "IBM", 150L, 48d }, new object[] { "MSFT", 5000L, null },
                    new object[] { "YAH", 10000L, 6d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion11_12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(2200, 0, new object[][] { new object[] { "IBM", 155L, 75d } });
            expected.AddResultInsert(3200, 0, new object[][] { new object[] { "IBM", 155L, 75d } });
            expected.AddResultInsert(4200, 0, new object[][] { new object[] { "IBM", 155L, 75d } });
            expected.AddResultInsert(5200, 0, new object[][] { new object[] { "IBM", 150L, 97d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 150L, 72d } },
                new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultInsRem(7200, 0, null, null);

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion17(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new object[][] { new object[] { "IBM", 100L, 25d } });
            expected.AddResultInsert(800, 1, new object[][] { new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(1500, 1, new object[][] { new object[] { "IBM", 150L, 49d } });
            expected.AddResultInsert(1500, 2, new object[][] { new object[] { "YAH", 10000L, 1d } });
            expected.AddResultInsert(3500, 1, new object[][] { new object[] { "YAH", 11000L, 3d } });
            expected.AddResultInsert(4300, 1, new object[][] { new object[] { "IBM", 150L, 97d } });
            expected.AddResultInsert(4900, 1, new object[][] { new object[] { "YAH", 11500L, 6d } });
            expected.AddResultInsert(5700, 0, new object[][] { new object[] { "IBM", 100L, 72d } });
            expected.AddResultInsert(5900, 1, new object[][] { new object[] { "YAH", 10500L, 7d } });
            expected.AddResultInsert(6300, 0, new object[][] { new object[] { "MSFT", 5000L, null } });
            expected.AddResultInsert(
                7000,
                0,
                new object[][] { new object[] { "IBM", 150L, 48d }, new object[] { "YAH", 10000L, 6d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion18(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "volume", "sum(price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 100L, 25d }, new object[] { "MSFT", 5000L, 9d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 75d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 75d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 75d }
                });
            expected.AddResultInsert(
                3200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 75d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 75d }, new object[] { "YAH", 10000L, 1d },
                    new object[] { "IBM", 155L, 75d }
                });
            expected.AddResultInsert(
                4200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 75d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 75d }, new object[] { "YAH", 10000L, 3d },
                    new object[] { "IBM", 155L, 75d }, new object[] { "YAH", 11000L, 3d }
                });
            expected.AddResultInsert(
                5200,
                0,
                new object[][] {
                    new object[] { "IBM", 100L, 97d }, new object[] { "MSFT", 5000L, 9d },
                    new object[] { "IBM", 150L, 97d }, new object[] { "YAH", 10000L, 6d },
                    new object[] { "IBM", 155L, 97d }, new object[] { "YAH", 11000L, 6d },
                    new object[] { "IBM", 150L, 97d }, new object[] { "YAH", 11500L, 6d }
                });
            expected.AddResultInsert(
                6200,
                0,
                new object[][] {
                    new object[] { "MSFT", 5000L, 9d }, new object[] { "IBM", 150L, 72d },
                    new object[] { "YAH", 10000L, 7d }, new object[] { "IBM", 155L, 72d },
                    new object[] { "YAH", 11000L, 7d }, new object[] { "IBM", 150L, 72d },
                    new object[] { "YAH", 11500L, 7d }, new object[] { "YAH", 10500L, 7d }
                });
            expected.AddResultInsert(
                7200,
                0,
                new object[][] {
                    new object[] { "IBM", 155L, 48d }, new object[] { "YAH", 11000L, 6d },
                    new object[] { "IBM", 150L, 48d }, new object[] { "YAH", 11500L, 6d },
                    new object[] { "YAH", 10500L, 6d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void RunAssertion15LastHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select symbol, volume, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select symbol, volume, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all", milestone);
        }

        private static void RunAssertion6LastHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select symbol, volume, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        private static void RunAssertion11AllHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select symbol, volume, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "group by symbol " +
                           "having sum(price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all", milestone);
        }

        private static void TryAssertionNoJoinLast(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            var epl = opt.GetHint() +
                      "@name('s0') select symbol, volume, sum(price) as mySum " +
                      "from SupportMarketDataBean#length(5) " +
                      "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                      "group by symbol " +
                      "output last every 2 events";

            env.CompileDeploy(epl).AddListener("s0");

            TryAssertionLast(env);

            env.UndeployAll();
        }

        private static void AssertEvent(
            RegressionEnvironment env,
            string symbol,
            double? mySum,
            long? volume)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var newData = listener.LastNewData;
                    Assert.AreEqual(1, newData.Length);
                    Assert.AreEqual(symbol, newData[0].Get("symbol"));
                    Assert.AreEqual(mySum, newData[0].Get("mySum"));
                    Assert.AreEqual(volume, newData[0].Get("volume"));
                    listener.Reset();
                });
        }

        private static void TryAssertionSingle(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("symbol"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mySum"));
                    Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("volume"));
                });

            SendEvent(env, SYMBOL_DELL, 10, 100);
            AssertEvent(env, SYMBOL_DELL, 100d, 10L);

            SendEvent(env, SYMBOL_IBM, 15, 50);
            AssertEvent(env, SYMBOL_IBM, 50d, 15L);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            string theString,
            long longPrimitive,
            int intPrimitive)
        {
            var b = new SupportBean();
            b.TheString = theString;
            b.LongPrimitive = longPrimitive;
            b.IntPrimitive = intPrimitive;
            env.SendEventBean(b);
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }
    }
} // end of namespace