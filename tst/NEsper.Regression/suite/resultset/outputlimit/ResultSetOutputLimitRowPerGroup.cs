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
    public class ResultSetOutputLimitRowPerGroup
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private const string CATEGORY = "Fully-Aggregated and Grouped";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if TEMPORARY
            WithOutputFirstWhenThen(execs);
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
            With13LastNoHavingNoJoinWOrderBy(execs);
            With14LastNoHavingJoinWOrderBy(execs);
            With15LastHavingNoJoin(execs);
            With16LastHavingJoin(execs);
            With17FirstNoHavingNoJoin(execs);
            With17FirstNoHavingJoin(execs);
            With18SnapshotNoHavingNoJoin(execs);
            With18SnapshotNoHavingJoin(execs);
            WithJoinSortWindow(execs);
            WithLimitSnapshot(execs);
            WithLimitSnapshotLimit(execs);
            WithGroupByAll(execs);
            WithGroupByDefault(execs);
            WithMaxTimeWindow(execs);
            WithNoJoinLast(execs);
            WithNoOutputClauseView(execs);
            WithNoOutputClauseJoin(execs);
            WithNoJoinAll(execs);
            WithJoinLast(execs);
            WithJoinAll(execs);
            WithCrontabNumberSetVariations(execs);
            WithOutputFirstHavingJoinNoJoin(execs);
            WithOutputFirstCrontab(execs);
            WithOutputFirstEveryNEvents(execs);
            WithOutputFirstMultikeyWArray(execs);
            WithOutputAllMultikeyWArray(execs);
            WithOutputLastMultikeyWArray(execs);
            WithOutputSnapshotMultikeyWArray(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithOutputSnapshotMultikeyWArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputSnapshotMultikeyWArray());
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

        public static IList<RegressionExecution> WithOutputFirstMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirstEveryNEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstEveryNEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirstCrontab(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstCrontab());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirstHavingJoinNoJoin(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstHavingJoinNoJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithCrontabNumberSetVariations(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetCrontabNumberSetVariations());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinAll());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetJoinLast());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoJoinAll());
            return execs;
        }

        public static IList<RegressionExecution> WithNoOutputClauseJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoOutputClauseJoin());
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

        public static IList<RegressionExecution> WithGroupByDefault(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetGroupByDefault());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupByAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetGroupByAll());
            return execs;
        }

        public static IList<RegressionExecution> WithLimitSnapshotLimit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLimitSnapshotLimit());
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

        public static IList<RegressionExecution> With18SnapshotNoHavingJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet18SnapshotNoHavingJoin());
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

        public static IList<RegressionExecution> With14LastNoHavingJoinWOrderBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet14LastNoHavingJoinWOrderBy());
            return execs;
        }

        public static IList<RegressionExecution> With13LastNoHavingNoJoinWOrderBy(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet13LastNoHavingNoJoinWOrderBy());
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

        public static IList<RegressionExecution> WithOutputFirstWhenThen(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstWhenThen());
            return execs;
        }

        internal class ResultSetOutputSnapshotMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl =
                    "@name('s0') select TheString as c0, LongPrimitive as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString, LongPrimitive " +
                    "output snapshot every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanEvent(env, "A", 0, 10);
                SendBeanEvent(env, "B", 1, 11);
                SendBeanEvent(env, "A", 0, 12);
                SendBeanEvent(env, "B", 1, 13);

                env.Milestone(0);

                env.AdvanceTime(10000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A", 0L, 22 }, new object[] { "B", 1L, 24 }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputLastMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = "TheString,LongPrimitive,thesum".SplitCsv();
                var epl =
                    "@name('s0') select TheString, LongPrimitive, sum(IntPrimitive) as thesum from SupportBean#keepall " +
                    "group by TheString, LongPrimitive output last every 1 seconds";
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
                            new object[] { "A", 0L, 22 }, new object[] { "B", 1L, 11 }, new object[] { "C", 0L, 13 }
                        }));

                SendBeanEvent(env, "A", 0, 14);

                env.AdvanceTime(2000);
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        fields,
                        new object[][] {
                            new object[] { "A", 0L, 36 }
                        }));

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputAllMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = "TheString,LongPrimitive,thesum".SplitCsv();
                var epl =
                    "@name('s0') select TheString, LongPrimitive, sum(IntPrimitive) as thesum from SupportBean#keepall " +
                    "group by TheString, LongPrimitive output all every 1 seconds";
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
                            new object[] { "A", 0L, 22 }, new object[] { "B", 1L, 11 }, new object[] { "C", 0L, 13 }
                        }));

                SendBeanEvent(env, "A", 0, 14);

                env.AdvanceTime(2000);
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        fields,
                        new object[][] {
                            new object[] { "A", 0L, 36 }, new object[] { "B", 1L, 11 }, new object[] { "C", 0L, 13 }
                        }));

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new string[] { "thesum" };
                var epl =
                    "@name('s0') select sum(value) as thesum from SupportEventWithIntArray group by array output first every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] { 1, 2 }, 10));
                env.AssertPropsNew("s0", fields, new object[] { 10 });

                env.Milestone(0);

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] { 1, 2 }, 10));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetCrontabNumberSetVariations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "select TheString from SupportBean output all at (*/2, 8:17, lastweekday, [1, 1], *)");
                env.SendEventBean(new SupportBean());
                env.UndeployAll();

                env.CompileDeploy("select TheString from SupportBean output all at (*/2, 8:17, 30 weekday, [1, 1], *)");
                env.SendEventBean(new SupportBean());
                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstHavingJoinNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtText);

                var stmtTextJoin =
                    "@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.Id = mv.TheString " +
                    "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtTextJoin);

                var stmtTextOrder =
"@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events Order by TheString asc";
                TryOutputFirstHaving(env, stmtTextOrder);

                var stmtTextOrderJoin =
                    "@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.Id = mv.TheString " +
"group by TheString having sum(IntPrimitive) > 20 output first every 2 events Order by TheString asc";
                TryOutputFirstHaving(env, stmtTextOrderJoin);
            }
        }

        internal class ResultSetOutputFirstCrontab : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var fields = "TheString,value".SplitCsv();
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n" +
                          "@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first at (*/2, *, *, *, *)";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanEvent(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10 });

                SendTimer(env, 2 * 60 * 1000 - 1);
                SendBeanEvent(env, "E1", 11);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 2 * 60 * 1000);
                SendBeanEvent(env, "E1", 12);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 33 });

                SendBeanEvent(env, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });

                SendBeanEvent(env, "E2", 21);
                SendTimer(env, 4 * 60 * 1000 - 1);
                SendBeanEvent(env, "E2", 22);
                SendBeanEvent(env, "E1", 13);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 4 * 60 * 1000);
                SendBeanEvent(env, "E2", 23);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 86 });
                SendBeanEvent(env, "E1", 14);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 60 });

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstWhenThen : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString,value".SplitCsv();
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n" +
                          "@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first when varoutone then set varoutone = false;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanEvent(env, "E1", 10);

                env.Milestone(0);

                SendBeanEvent(env, "E1", 11);
                env.AssertListenerNotInvoked("s0");

                env.RuntimeSetVariable(null, "varoutone", true);
                SendBeanEvent(env, "E1", 12);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 33 });
                env.AssertRuntime(
                    runtime => Assert.AreEqual(false, env.Runtime.VariableService.GetVariableValue(null, "varoutone")));

                env.Milestone(1);

                env.RuntimeSetVariable(null, "varoutone", true);
                SendBeanEvent(env, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });
                env.AssertRuntime(
                    runtime => Assert.AreEqual(false, env.Runtime.VariableService.GetVariableValue(null, "varoutone")));

                SendBeanEvent(env, "E1", 13);
                SendBeanEvent(env, "E2", 21);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstEveryNEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString,value".SplitCsv();
                var path = new RegressionPath();
                var epl = "@public create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n";
                env.CompileDeploy(epl, path);

                epl =
                    "@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every 3 events;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendBeanEvent(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10 });

                SendBeanEvent(env, "E1", 12);
                SendBeanEvent(env, "E1", 11);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, "E1", 13);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 46 });

                SendMDEvent(env, "S1", 12);
                SendMDEvent(env, "S1", 11);
                env.AssertListenerNotInvoked("s0");

                SendMDEvent(env, "S1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 13 });

                SendBeanEvent(env, "E1", 14);
                SendBeanEvent(env, "E1", 15);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });
                env.UndeployModuleContaining("s0");

                // test variable
                env.CompileDeploy("@name('var') @public create variable int myvar_local = 1", path);
                env.CompileDeploy(
                    "@name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every myvar_local events",
                    path);
                env.AddListener("s0");

                SendBeanEvent(env, "E3", 10);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E3", 10 } });

                SendBeanEvent(env, "E1", 5);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E1", 47 } });

                env.RuntimeSetVariable("var", "myvar_local", 2);

                SendBeanEvent(env, "E1", 6);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, "E1", 7);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E1", 60 } });

                SendBeanEvent(env, "E1", 1);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, "E1", 1);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E1", 62 } });

                env.UndeployAll();
            }
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec)" +
"group by Symbol "+
"Order by Symbol asc";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
"Order by Symbol asc";
                TryAssertion12(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec) " +
"group by Symbol "+
                               " having sum(Price) > 50";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                               "having sum(Price) > 50";
                TryAssertion34(env, stmtText, "none", new AtomicLong());
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec) " +
"group by Symbol "+
"output every 1 seconds Order by Symbol asc";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
"output every 1 seconds Order by Symbol asc";
                TryAssertion56(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
"group by Symbol "+
                               "having sum(Price) > 50" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                               "having sum(Price) > 50" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default", new AtomicLong());
            }
        }

        internal class ResultSet9AllNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec) " +
"group by Symbol "+
                               "output all every 1 seconds " +
"Order by Symbol";
                TryAssertion9_10(env, stmtText, "all", new AtomicLong());
            }
        }

        internal class ResultSet10AllNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                               "output all every 1 seconds " +
"Order by Symbol";
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

        private static void RunAssertion11AllHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
"@name('s0') select Symbol, sum(Price) "+
                           "from SupportMarketDataBean#time(5.5 sec) " +
"group by Symbol "+
                           "having sum(Price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all", milestone);
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

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
"@name('s0') select Symbol, sum(Price) "+
                           "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                           "having sum(Price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all", milestone);
        }

        internal class ResultSet13LastNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec)" +
"group by Symbol "+
                               "output last every 1 seconds";
                TryAssertion13_14(env, stmtText, "last", true, new AtomicLong());
            }
        }

        internal class ResultSet14LastNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                               "output last every 1 seconds";
                TryAssertion13_14(env, stmtText, "last", true, new AtomicLong());
            }
        }

        internal class ResultSet13LastNoHavingNoJoinWOrderBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec)" +
"group by Symbol "+
                               "output last every 1 seconds " +
"Order by Symbol";
                TryAssertion13_14(env, stmtText, "last", false, new AtomicLong());
            }
        }

        internal class ResultSet14LastNoHavingJoinWOrderBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                               "output last every 1 seconds " +
"Order by Symbol";
                TryAssertion13_14(env, stmtText, "last", false, new AtomicLong());
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

        private static void RunAssertion15LastHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
"@name('s0') select Symbol, sum(Price) "+
                           "from SupportMarketDataBean#time(5.5 sec)" +
"group by Symbol "+
                           "having sum(Price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
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
"@name('s0') select Symbol, sum(Price) "+
                           "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                           "having sum(Price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last", milestone);
        }

        internal class ResultSet17FirstNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec) " +
"group by Symbol "+
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet17FirstNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first", new AtomicLong());
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec) " +
"group by Symbol "+
                               "output snapshot every 1 seconds " +
"Order by Symbol";
                TryAssertion18(env, stmtText, "snapshot", new AtomicLong());
            }
        }

        internal class ResultSet18SnapshotNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Symbol, sum(Price) "+
                               "from SupportMarketDataBean#time(5.5 sec), " +
"SupportBean#keepall where TheString=Symbol "+
"group by Symbol "+
                               "output snapshot every 1 seconds " +
"Order by Symbol";
                TryAssertion18(env, stmtText, "snapshot", new AtomicLong());
            }
        }

        internal class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var fields = "Symbol,maxVol".SplitCsv();
                var epl = "@name('s0') select irstream Symbol, max(Price) as maxVol"+
                          " from SupportMarketDataBean#sort(1, Volume desc) as s0," +
                          "SupportBean#keepall as s1 " +
"group by Symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("JOIN_KEY", -1));

                SendMDEvent(env, "JOIN_KEY", 1d);
                SendMDEvent(env, "JOIN_KEY", 2d);
                env.ListenerReset("s0");

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                env.AssertListener(
                    "s0",
                    listener => {
                        var result = listener.DataListsFlattened;
                        Assert.AreEqual(2, result.First.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            result.First,
                            fields,
                            new object[][] { new object[] { "JOIN_KEY", 1.0 }, new object[] { "JOIN_KEY", 2.0 } });
                        Assert.AreEqual(2, result.Second.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            result.Second,
                            fields,
                            new object[][] { new object[] { "JOIN_KEY", null }, new object[] { "JOIN_KEY", 1.0 } });
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@name('s0') select Symbol, min(Price) as minprice from SupportMarketDataBean"+
"#time(10 seconds) group by Symbol output snapshot every 1 seconds Order by Symbol asc";

                env.CompileDeploy(selectStmt).AddListener("s0");

                SendMDEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendMDEvent(env, "IBM", 16);
                SendMDEvent(env, "ABC", 14);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                var fields = new string[] { "Symbol", "minprice" };
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "ABC", 14d }, new object[] { "IBM", 16d } });

                SendTimer(env, 1500);
                SendMDEvent(env, "IBM", 18);
                SendMDEvent(env, "MSFT", 30);

                SendTimer(env, 10000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "ABC", 14d }, new object[] { "IBM", 16d }, new object[] { "MSFT", 30d } });

                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "IBM", 18d }, new object[] { "MSFT", 30d } });

                SendTimer(env, 12000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshotLimit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@name('s0') select Symbol, min(Price) as minprice from SupportMarketDataBean"+
                                 "#time(10 seconds) as m, " +
"SupportBean#keepall as s where s.TheString = m.Symbol "+
"group by Symbol output snapshot every 1 seconds Order by Symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                foreach (var theString in "ABC,IBM,MSFT".SplitCsv()) {
                    env.SendEventBean(new SupportBean(theString, 1));
                }

                SendMDEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendMDEvent(env, "IBM", 16);
                SendMDEvent(env, "ABC", 14);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 1000);
                var fields = new string[] { "Symbol", "minprice" };
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "ABC", 14d }, new object[] { "IBM", 16d } });

                SendTimer(env, 1500);
                SendMDEvent(env, "IBM", 18);
                SendMDEvent(env, "MSFT", 30);

                SendTimer(env, 10000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "ABC", 14d }, new object[] { "IBM", 16d }, new object[] { "MSFT", 30d } });

                SendTimer(env, 10500);
                SendTimer(env, 11000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "IBM", 18d }, new object[] { "MSFT", 30d } });

                SendTimer(env, 11500);
                SendTimer(env, 12000);
                env.AssertPropsPerRowLastNew("s0", fields, null);

                env.UndeployAll();
            }
        }

        internal class ResultSetGroupByAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Symbol,sum(Price)".SplitCsv();
                var statementString =
"@name('s0') select irstream Symbol, sum(Price) from SupportMarketDataBean#length(5) group by Symbol output all every 5 events";
                env.CompileDeploy(statementString).AddListener("s0");

                // send some events and check that only the most recent
                // ones are kept
                SendMDEvent(env, "IBM", 1D);
                SendMDEvent(env, "IBM", 2D);
                SendMDEvent(env, "HP", 1D);
                SendMDEvent(env, "IBM", 3D);
                SendMDEvent(env, "MAC", 1D);

                env.AssertListener(
                    "s0",
                    listener => {
                        var newData = listener.LastNewData;
                        Assert.AreEqual(3, newData.Length);
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            newData,
                            fields,
                            new object[][] {
                                new object[] { "IBM", 6d }, new object[] { "HP", 1d }, new object[] { "MAC", 1d }
                            });
                        var oldData = listener.LastOldData;
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            oldData,
                            fields,
                            new object[][] {
                                new object[] { "IBM", null }, new object[] { "HP", null }, new object[] { "MAC", null }
                            });
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetGroupByDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Symbol,sum(Price)".SplitCsv();
                var epl =
"@name('s0') select irstream Symbol, sum(Price) from SupportMarketDataBean#length(5) group by Symbol output every 5 events";
                env.CompileDeploy(epl).AddListener("s0");

                // send some events and check that only the most recent
                // ones are kept
                SendMDEvent(env, "IBM", 1D);
                SendMDEvent(env, "IBM", 2D);
                SendMDEvent(env, "HP", 1D);
                SendMDEvent(env, "IBM", 3D);
                SendMDEvent(env, "MAC", 1D);
                env.AssertListener(
                    "s0",
                    listener => {
                        var newData = listener.LastNewData;
                        var oldData = listener.LastOldData;
                        Assert.AreEqual(5, newData.Length);
                        Assert.AreEqual(5, oldData.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            newData,
                            fields,
                            new object[][] {
                                new object[] { "IBM", 1d }, new object[] { "IBM", 3d }, new object[] { "HP", 1d },
                                new object[] { "IBM", 6d }, new object[] { "MAC", 1d }
                            });
                        EPAssertionUtil.AssertPropsPerRow(
                            oldData,
                            fields,
                            new object[][] {
                                new object[] { "IBM", null }, new object[] { "IBM", 1d }, new object[] { "HP", null },
                                new object[] { "IBM", 3d }, new object[] { "MAC", null }
                            });
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var fields = "Symbol,maxVol".SplitCsv();
                var epl = "@name('s0') select irstream Symbol, max(Price) as maxVol"+
                          " from SupportMarketDataBean#time(1 sec) " +
"group by Symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendMDEvent(env, "SYM1", 1d);
                SendMDEvent(env, "SYM1", 2d);
                env.ListenerReset("s0");

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                env.AssertListener(
                    "s0",
                    listener => {
                        var result = listener.DataListsFlattened;
                        Assert.AreEqual(3, result.First.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            result.First,
                            fields,
                            new object[][] {
                                new object[] { "SYM1", 1.0 }, new object[] { "SYM1", 2.0 },
                                new object[] { "SYM1", null }
                            });
                        Assert.AreEqual(3, result.Second.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            result.Second,
                            fields,
                            new object[][] {
                                new object[] { "SYM1", null }, new object[] { "SYM1", 1.0 },
                                new object[] { "SYM1", 2.0 }
                            });
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

            private static void TryAssertionNoJoinLast(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
"@name('s0') select irstream Symbol,"+
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(3) " +
"where Symbol='DELL' or Symbol='IBM' or Symbol='GE' "+
"group by Symbol "+
                          "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionLast(env);
                env.UndeployAll();
            }
        }

        internal class ResultSetNoOutputClauseView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol,"+
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(3) " +
"where Symbol='DELL' or Symbol='IBM' or Symbol='GE' "+
"group by Symbol";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSingle(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetNoOutputClauseJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream Symbol,"+
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
"where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') "+
"       and one.TheString = two.Symbol "+
"group by Symbol";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionSingle(env);

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
                var epl = opt.GetHint() +
"@name('s0') select irstream Symbol,"+
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(5) " +
"where Symbol='DELL' or Symbol='IBM' or Symbol='GE' "+
"group by Symbol "+
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

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

            private static void TryAssertionJoinLast(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
"@name('s0') select irstream Symbol,"+
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
"where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') "+
"       and one.TheString = two.Symbol "+
"group by Symbol "+
                          "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionLast(env);

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
        }

        private static void TryAssertionJoinAll(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var epl = opt.GetHint() +
"@name('s0') select irstream Symbol,"+
                      "sum(Price) as mySum," +
                      "avg(Price) as myAvg " +
                      "from SupportBeanString#length(100) as one, " +
                      "SupportMarketDataBean#length(5) as two " +
"where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') "+
"       and one.TheString = two.Symbol "+
"group by Symbol "+
                      "output all every 2 events";

            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
            env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
            env.SendEventBean(new SupportBeanString("AAA"));

            TryAssertionAll(env);

            env.UndeployAll();
        }

        private static void TryAssertionLast(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mySum"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("myAvg"));
                });

            SendMDEvent(env, SYMBOL_DELL, 10);
            env.AssertListenerNotInvoked("s0");

            SendMDEvent(env, SYMBOL_DELL, 20);
            AssertEvent(
                env,
                SYMBOL_DELL,
                null,
                null,
                30d,
                15d);
            env.ListenerReset("s0");

            SendMDEvent(env, SYMBOL_DELL, 100);
            env.AssertListenerNotInvoked("s0");

            SendMDEvent(env, SYMBOL_DELL, 50);
            AssertEvent(
                env,
                SYMBOL_DELL,
                30d,
                15d,
                170d,
                170 / 3d);
        }

        private static void TryOutputFirstHaving(
            RegressionEnvironment env,
            string statementText)
        {
            var fields = "TheString,value".SplitCsv();
            var epl = "create window MyWindow#keepall as SupportBean;\n" +
                      "insert into MyWindow select * from SupportBean;\n" +
                      "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n" +
                      statementText;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_A("E1"));
            env.SendEventBean(new SupportBean_A("E2"));

            SendBeanEvent(env, "E1", 10);
            SendBeanEvent(env, "E2", 15);
            SendBeanEvent(env, "E1", 10);
            SendBeanEvent(env, "E2", 5);
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, "E2", 5);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 25 });

            SendBeanEvent(env, "E2", -6); // to 19, does not count toward condition
            SendBeanEvent(env, "E2", 2); // to 21, counts toward condition
            env.AssertListenerNotInvoked("s0");
            SendBeanEvent(env, "E2", 1);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 22 });

            SendBeanEvent(env, "E2", 1); // to 23, counts toward condition
            env.AssertListenerNotInvoked("s0");
            SendBeanEvent(env, "E2", 1); // to 24
            env.AssertPropsNew("s0", fields, new object[] { "E2", 24 });

            SendBeanEvent(env, "E2", -10); // to 14
            SendBeanEvent(env, "E2", 10); // to 24, counts toward condition
            env.AssertListenerNotInvoked("s0");
            SendBeanEvent(env, "E2", 0); // to 24, counts toward condition
            env.AssertPropsNew("s0", fields, new object[] { "E2", 24 });

            SendBeanEvent(env, "E2", -10); // to 14
            SendBeanEvent(env, "E2", 1); // to 15
            SendBeanEvent(env, "E2", 5); // to 20
            SendBeanEvent(env, "E2", 0); // to 20
            SendBeanEvent(env, "E2", 1); // to 21    // counts
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, "E2", 0); // to 21
            env.AssertPropsNew("s0", fields, new object[] { "E2", 21 });

            // remove events
            SendMDEvent(env, "E2", 0);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 21 });

            // remove events
            SendMDEvent(env, "E2", -10);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 41 });

            // remove events
            SendMDEvent(env, "E2", -6); // since there is 3*-10 we output the next one
            env.AssertPropsNew("s0", fields, new object[] { "E2", 47 });

            SendMDEvent(env, "E2", 2);
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void TryAssertionSingle(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mySum"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("myAvg"));
                });

            SendMDEvent(env, SYMBOL_DELL, 10);
            AssertEvent(
                env,
                SYMBOL_DELL,
                null,
                null,
                10d,
                10d);

            SendMDEvent(env, SYMBOL_IBM, 20);
            AssertEvent(
                env,
                SYMBOL_IBM,
                null,
                null,
                20d,
                20d);
        }

        private static void TryAssertionAll(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mySum"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("myAvg"));
                });

            SendMDEvent(env, SYMBOL_IBM, 70);
            env.AssertListenerNotInvoked("s0");

            SendMDEvent(env, SYMBOL_DELL, 10);
            AssertEvents(
                env,
                SYMBOL_IBM,
                null,
                null,
                70d,
                70d,
                SYMBOL_DELL,
                null,
                null,
                10d,
                10d);
            env.ListenerReset("s0");

            SendMDEvent(env, SYMBOL_DELL, 20);
            env.AssertListenerNotInvoked("s0");

            SendMDEvent(env, SYMBOL_DELL, 100);
            AssertEvents(
                env,
                SYMBOL_IBM,
                70d,
                70d,
                70d,
                70d,
                SYMBOL_DELL,
                10d,
                10d,
                130d,
                130d / 3d);
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new object[][] { new object[] { "IBM", 25d } },
                new object[][] { new object[] { "IBM", null } });
            expected.AddResultInsRem(
                800,
                1,
                new object[][] { new object[] { "MSFT", 9d } },
                new object[][] { new object[] { "MSFT", null } });
            expected.AddResultInsRem(
                1500,
                1,
                new object[][] { new object[] { "IBM", 49d } },
                new object[][] { new object[] { "IBM", 25d } });
            expected.AddResultInsRem(
                1500,
                2,
                new object[][] { new object[] { "YAH", 1d } },
                new object[][] { new object[] { "YAH", null } });
            expected.AddResultInsRem(
                2100,
                1,
                new object[][] { new object[] { "IBM", 75d } },
                new object[][] { new object[] { "IBM", 49d } });
            expected.AddResultInsRem(
                3500,
                1,
                new object[][] { new object[] { "YAH", 3d } },
                new object[][] { new object[] { "YAH", 1d } });
            expected.AddResultInsRem(
                4300,
                1,
                new object[][] { new object[] { "IBM", 97d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                4900,
                1,
                new object[][] { new object[] { "YAH", 6d } },
                new object[][] { new object[] { "YAH", 3d } });
            expected.AddResultInsRem(
                5700,
                0,
                new object[][] { new object[] { "IBM", 72d } },
                new object[][] { new object[] { "IBM", 97d } });
            expected.AddResultInsRem(
                5900,
                1,
                new object[][] { new object[] { "YAH", 7d } },
                new object[][] { new object[] { "YAH", 6d } });
            expected.AddResultInsRem(
                6300,
                0,
                new object[][] { new object[] { "MSFT", null } },
                new object[][] { new object[] { "MSFT", 9d } });
            expected.AddResultInsRem(
                7000,
                0,
                new object[][] { new object[] { "IBM", 48d }, new object[] { "YAH", 6d } },
                new object[][] { new object[] { "IBM", 72d }, new object[] { "YAH", 7d } });

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

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(2100, 1, new object[][] { new object[] { "IBM", 75d } }, null);
            expected.AddResultInsRem(
                4300,
                1,
                new object[][] { new object[] { "IBM", 97d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                5700,
                0,
                new object[][] { new object[] { "IBM", 72d } },
                new object[][] { new object[] { "IBM", 97d } });
            expected.AddResultInsRem(7000, 0, null, new object[][] { new object[] { "IBM", 72d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void TryAssertion13_14(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            bool assertAllowAnyOrder,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new object[][] { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d } },
                new object[][] { new object[] { "IBM", null }, new object[] { "MSFT", null } });
            expected.AddResultInsRem(
                2200,
                0,
                new object[][] { new object[] { "IBM", 75d }, new object[] { "YAH", 1d } },
                new object[][] { new object[] { "IBM", 25d }, new object[] { "YAH", null } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] { new object[] { "YAH", 3d } },
                new object[][] { new object[] { "YAH", 1d } });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 97d }, new object[] { "YAH", 6d } },
                new object[][] { new object[] { "IBM", 75d }, new object[] { "YAH", 3d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 72d }, new object[] { "YAH", 7d } },
                new object[][] { new object[] { "IBM", 97d }, new object[] { "YAH", 6d } });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][]
                    { new object[] { "IBM", 48d }, new object[] { "MSFT", null }, new object[] { "YAH", 6d } },
                new object[][]
                    { new object[] { "IBM", 72d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 7d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(assertAllowAnyOrder, milestone);
        }

        private static void TryAssertion15_16(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit,
            AtomicLong milestone)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new object[][] { new object[] { "IBM", 75d } }, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 97d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 72d } },
                new object[][] { new object[] { "IBM", 97d } });
            expected.AddResultInsRem(7200, 0, null, new object[][] { new object[] { "IBM", 72d } });

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

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new object[][] { new object[] { "IBM", 75d } }, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 97d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 72d } },
                new object[][] { new object[] { "IBM", 97d } });
            expected.AddResultInsRem(7200, 0, null, new object[][] { new object[] { "IBM", 72d } });

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

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new object[][] { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d } },
                new object[][] { new object[] { "IBM", null }, new object[] { "MSFT", null } });
            expected.AddResultInsRem(
                2200,
                0,
                new object[][] { new object[] { "IBM", 49d }, new object[] { "IBM", 75d }, new object[] { "YAH", 1d } },
                new object[][]
                    { new object[] { "IBM", 25d }, new object[] { "IBM", 49d }, new object[] { "YAH", null } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] { new object[] { "YAH", 3d } },
                new object[][] { new object[] { "YAH", 1d } });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 97d }, new object[] { "YAH", 6d } },
                new object[][] { new object[] { "IBM", 75d }, new object[] { "YAH", 3d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 72d }, new object[] { "YAH", 7d } },
                new object[][] { new object[] { "IBM", 97d }, new object[] { "YAH", 6d } });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][]
                    { new object[] { "IBM", 48d }, new object[] { "MSFT", null }, new object[] { "YAH", 6d } },
                new object[][]
                    { new object[] { "IBM", 72d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 7d } });

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

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new object[][] { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d } },
                new object[][] { new object[] { "IBM", null }, new object[] { "MSFT", null } });
            expected.AddResultInsRem(
                2200,
                0,
                new object[][] { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d } },
                new object[][]
                    { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d }, new object[] { "YAH", null } });
            expected.AddResultInsRem(
                3200,
                0,
                new object[][] { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d } },
                new object[][]
                    { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d } });
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 3d } },
                new object[][]
                    { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d } });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 97d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 6d } },
                new object[][]
                    { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 3d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 72d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 7d } },
                new object[][]
                    { new object[] { "IBM", 97d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 6d } });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][]
                    { new object[] { "IBM", 48d }, new object[] { "MSFT", null }, new object[] { "YAH", 6d } },
                new object[][]
                    { new object[] { "IBM", 72d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 7d } });

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

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new object[][] { new object[] { "IBM", 75d } }, null);
            expected.AddResultInsRem(
                3200,
                0,
                new object[][] { new object[] { "IBM", 75d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] { new object[] { "IBM", 75d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 97d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 72d } },
                new object[][] { new object[] { "IBM", 97d } });
            expected.AddResultInsRem(7200, 0, null, new object[][] { new object[] { "IBM", 72d } });

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

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new object[][] { new object[] { "IBM", 25d } },
                new object[][] { new object[] { "IBM", null } });
            expected.AddResultInsRem(
                800,
                1,
                new object[][] { new object[] { "MSFT", 9d } },
                new object[][] { new object[] { "MSFT", null } });
            expected.AddResultInsRem(
                1500,
                1,
                new object[][] { new object[] { "IBM", 49d } },
                new object[][] { new object[] { "IBM", 25d } });
            expected.AddResultInsRem(
                1500,
                2,
                new object[][] { new object[] { "YAH", 1d } },
                new object[][] { new object[] { "YAH", null } });
            expected.AddResultInsRem(
                3500,
                1,
                new object[][] { new object[] { "YAH", 3d } },
                new object[][] { new object[] { "YAH", 1d } });
            expected.AddResultInsRem(
                4300,
                1,
                new object[][] { new object[] { "IBM", 97d } },
                new object[][] { new object[] { "IBM", 75d } });
            expected.AddResultInsRem(
                4900,
                1,
                new object[][] { new object[] { "YAH", 6d } },
                new object[][] { new object[] { "YAH", 3d } });
            expected.AddResultInsRem(
                5700,
                0,
                new object[][] { new object[] { "IBM", 72d } },
                new object[][] { new object[] { "IBM", 97d } });
            expected.AddResultInsRem(
                5900,
                1,
                new object[][] { new object[] { "YAH", 7d } },
                new object[][] { new object[] { "YAH", 6d } });
            expected.AddResultInsRem(
                6300,
                0,
                new object[][] { new object[] { "MSFT", null } },
                new object[][] { new object[] { "MSFT", 9d } });
            expected.AddResultInsRem(
                7000,
                0,
                new object[][] { new object[] { "IBM", 48d }, new object[] { "YAH", 6d } },
                new object[][] { new object[] { "IBM", 72d }, new object[] { "YAH", 7d } });

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

            var fields = new string[] { "Symbol", "sum(Price)" };
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new object[][] { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d } });
            expected.AddResultInsert(
                2200,
                0,
                new object[][]
                    { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d } });
            expected.AddResultInsert(
                3200,
                0,
                new object[][]
                    { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d } });
            expected.AddResultInsert(
                4200,
                0,
                new object[][]
                    { new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 3d } });
            expected.AddResultInsert(
                5200,
                0,
                new object[][]
                    { new object[] { "IBM", 97d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 6d } });
            expected.AddResultInsert(
                6200,
                0,
                new object[][]
                    { new object[] { "IBM", 72d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 7d } });
            expected.AddResultInsert(
                7200,
                0,
                new object[][] { new object[] { "IBM", 48d }, new object[] { "YAH", 6d } });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false, milestone);
        }

        private static void AssertEvent(
            RegressionEnvironment env,
            string symbol,
            double? oldSum,
            double? oldAvg,
            double? newSum,
            double? newAvg)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var oldData = listener.LastOldData;
                    var newData = listener.LastNewData;

                    Assert.AreEqual(1, oldData.Length);
                    Assert.AreEqual(1, newData.Length);

                    Assert.AreEqual(symbol, oldData[0].Get("Symbol"));
                    Assert.AreEqual(oldSum, oldData[0].Get("mySum"));
                    Assert.AreEqual(oldAvg, oldData[0].Get("myAvg"));

                    Assert.AreEqual(symbol, newData[0].Get("Symbol"));
                    Assert.AreEqual(newSum, newData[0].Get("mySum"));
                    Assert.AreEqual(newAvg, newData[0].Get("myAvg"), "newData myAvg wrong");

                    listener.Reset();
                });
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbolOne,
            double? oldSumOne,
            double? oldAvgOne,
            double newSumOne,
            double newAvgOne,
            string symbolTwo,
            double? oldSumTwo,
            double? oldAvgTwo,
            double newSumTwo,
            double newAvgTwo)
        {
            env.AssertListener(
                "s0",
                listener => {
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetDataListsFlattened(),
                        "mySum,myAvg".SplitCsv(),
                        new object[][] { new object[] { newSumOne, newAvgOne }, new object[] { newSumTwo, newAvgTwo } },
                        new object[][]
                            { new object[] { oldSumOne, oldAvgOne }, new object[] { oldSumTwo, oldAvgTwo } });
                });
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
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

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }
    }
} // end of namespace