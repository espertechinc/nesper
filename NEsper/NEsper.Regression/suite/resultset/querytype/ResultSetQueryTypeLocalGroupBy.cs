///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeLocalGroupBy
    {
        public static readonly string PLAN_CALLBACK_HOOK =
            "@Hook(HookType=" +
            typeof(HookType).FullName +
            ".INTERNAL_AGGLOCALLEVEL,Hook='" +
            typeof(SupportAggLevelPlanHook).FullName +
            "')";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedSumSimple());
            execs.Add(new ResultSetLocalUngroupedAggSQLStandard());
            execs.Add(new ResultSetLocalUngroupedAggEvent());
            execs.Add(new ResultSetLocalUngroupedAggIterator());
            execs.Add(new ResultSetLocalUngroupedParenSODA(false));
            execs.Add(new ResultSetLocalUngroupedParenSODA(true));
            execs.Add(new ResultSetLocalUngroupedColNameRendering());
            execs.Add(new ResultSetLocalUngroupedHaving());
            execs.Add(new ResultSetLocalUngroupedUnidirectionalJoin());
            execs.Add(new ResultSetLocalUngroupedThreeLevelWTop());
            execs.Add(new ResultSetLocalGroupedSimple());
            execs.Add(new ResultSetLocalGroupedMultiLevelMethod());
            execs.Add(new ResultSetLocalGroupedSolutionPattern());
            execs.Add(new ResultSetLocalGroupedMultiLevelAccess());
            execs.Add(new ResultSetLocalGroupedMultiLevelNoDefaultLvl());
            execs.Add(new ResultSetLocalPlanning());
            execs.Add(new ResultSetLocalInvalid());
            execs.Add(new ResultSetAggregateFullyVersusNotFullyAgg());
            execs.Add(new ResultSetLocalUngroupedSameKey());
            execs.Add(new ResultSetLocalGroupedSameKey());
            execs.Add(new ResultSetLocalUngroupedRowRemove());
            execs.Add(new ResultSetLocalGroupedRowRemove());
            execs.Add(new ResultSetLocalGroupedOnSelect());
            execs.Add(new ResultSetLocalUngroupedOrderBy());
            execs.Add(new ResultSetLocalEnumMethods(true));
            execs.Add(new ResultSetLocalUngroupedAggAdditionalAndPlugin());
            return execs;
        }

        private static void TryAssertionAggAndFullyAgg(
            RegressionEnvironment env,
            string selected,
            MyAssertion assertion)
        {
            var epl = "create context StartS0EndS1 start SupportBean_S0 end SupportBean_S1;" +
                      "@Name('s0') context StartS0EndS1 " +
                      selected +
                      " output snapshot when terminated;";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(0));
            MakeSendEvent(env, "E1", 10, 100);
            MakeSendEvent(env, "E2", 20, 200);
            MakeSendEvent(env, "E2", 30, 300);
            env.SendEventBean(new SupportBean_S1(0));

            assertion.Invoke(env.Listener("s0"));

            // try an empty batch
            env.SendEventBean(new SupportBean_S0(1));
            env.SendEventBean(new SupportBean_S1(1));

            env.UndeployAll();
        }

        private static void SendTime(
            RegressionEnvironment env,
            long msec)
        {
            env.AdvanceTime(msec);
        }

        private static void SendEventMany(
            RegressionEnvironment env,
            params string[] theString)
        {
            foreach (var value in theString) {
                SendEvent(env, value);
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }

        private static SupportBean MakeSendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            env.SendEventBean(b);
            return b;
        }

        private static SupportBean MakeSendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            env.SendEventBean(b);
            return b;
        }

        private static void AssertCountColsAndLevels(
            RegressionEnvironment env,
            string epl,
            int colCount,
            int lvlCount)
        {
            var theEpl = PLAN_CALLBACK_HOOK + epl;
            env.Compile(theEpl);
            var plan =
                SupportAggLevelPlanHook.GetAndReset();
            Assert.AreEqual(colCount, plan.First.NumColumns);
            Assert.AreEqual(lvlCount, plan.First.Levels.Length);
        }

        private static void AssertNoPlan(
            RegressionEnvironment env,
            string epl)
        {
            var theEpl = PLAN_CALLBACK_HOOK + epl;
            env.Compile(theEpl);
            Assert.IsNull(SupportAggLevelPlanHook.GetAndReset());
        }

        private static void AssertScalarColl(
            EventBean eventBean,
            int?[] expectedC6,
            int?[] expectedC7)
        {
            var c6 = eventBean.Get("c6").UnwrapIntoArray<int?>();
            var c7 = eventBean.Get("c7").UnwrapIntoArray<int?>();
            EPAssertionUtil.AssertEqualsExactOrder(expectedC6, c6.ToArray());
            EPAssertionUtil.AssertEqualsExactOrder(expectedC7, c7.ToArray());
        }

        public class ResultSetLocalUngroupedSumSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2", "c3" };
                env.Milestone(0);

                env.AdvanceTime(0);
                var epl = "@Name('s0') select " +
                          "sum(LongPrimitive, group_by:(TheString, IntPrimitive)) as c0, " +
                          "sum(LongPrimitive, group_by:(TheString)) as c1, " +
                          "sum(LongPrimitive, group_by:(IntPrimitive)) as c2, " +
                          "sum(LongPrimitive) as c3 " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                MakeSendEvent(env, "E1", 1, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10L, 10L, 10L, 10L});

                env.Milestone(2);

                MakeSendEvent(env, "E2", 2, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11L, 11L, 11L, 21L});

                env.Milestone(3);

                MakeSendEvent(env, "E1", 2, 12);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12L, 10 + 12L, 11 + 12L, 10 + 11 + 12L});

                env.Milestone(4);

                env.Milestone(5);

                MakeSendEvent(env, "E1", 1, 13);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10 + 13L, 10 + 12 + 13L, 10 + 13L, 10 + 11 + 12 + 13L});

                MakeSendEvent(env, "E2", 1, 14);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {14L, 11 + 14L, 10 + 13 + 14L, 10 + 11 + 12 + 13 + 14L});

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // not valid with count-min-sketch
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create table MyTable(approx countMinSketch(group_by:TheString) @type(SupportBean))",
                    "Failed to validate table-column expression 'countMinSketch(group_by:TheString)': The 'group_by' and 'filter' parameter is not allowed in create-table statements");

                // not allowed with tables
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create table MyTable(col sum(int, group_by:TheString) @type(SupportBean))",
                    "Failed to validate table-column expression 'sum(int,group_by:TheString)': The 'group_by' and 'filter' parameter is not allowed in create-table statements");

                // invalid named parameter
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select sum(IntPrimitive, xxx:TheString) from SupportBean",
                    "Failed to validate select-clause expression 'sum(IntPrimitive,xxx:TheString)': Invalid named parameter 'xxx' (did you mean 'group_by' or 'filter'?) [");

                // invalid group-by expression
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select sum(IntPrimitive, group_by:sum(IntPrimitive)) from SupportBean",
                    "Failed to validate select-clause expression 'sum(IntPrimitive,group_by:sum(intPr...(44 chars)': Group-by expressions cannot contain aggregate functions");

                // other functions don't accept this named parameter
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select coalesce(0, 1, group_by:TheString) from SupportBean",
                    "Incorrect syntax near ':' at line 1 column 30");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select " +
                    typeof(SupportStaticMethodLib).Name +
                    ".staticMethod(group_by:IntPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'com.espertech.esper.regressionlib.s...(104 chars)': Named parameters are not allowed");

                // not allowed in combination with roll-up
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select sum(IntPrimitive, group_by:TheString) from SupportBean group by rollup(TheString)",
                    "Roll-up and group-by parameters cannot be combined ");

                // not allowed in combination with into-table
                var path = new RegressionPath();
                env.CompileDeploy("create table mytable (thesum sum(int))", path);
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "into table mytable select sum(IntPrimitive, group_by:TheString) as thesum from SupportBean",
                    "Into-table and group-by parameters cannot be combined");

                // not allowed for match-recognize measure clauses
                var eplMatchRecog = "select * from SupportBean match_recognize (" +
                                    "  measures count(B.IntPrimitive, group_by:B.TheString) pattern (A B* C))";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    eplMatchRecog,
                    "Match-recognize does not allow aggregation functions to specify a group-by");

                // disallow subqueries to specify their own local group-by
                var eplSubq =
                    "select (select sum(IntPrimitive, group_by:TheString) from SupportBean#keepall) from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    eplSubq,
                    "Failed to plan subquery number 1 querying SupportBean: Subselect aggregations functions cannot specify a group-by");

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalPlanning : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                AssertNoPlan(env, "select sum(group_by:(),IntPrimitive) as c0 from SupportBean");
                AssertNoPlan(
                    env,
                    "select sum(group_by:(TheString),IntPrimitive) as c0 from SupportBean group by TheString");
                AssertNoPlan(
                    env,
                    "select sum(group_by:(TheString, IntPrimitive),LongPrimitive) as c0 from SupportBean group by TheString, IntPrimitive");
                AssertNoPlan(
                    env,
                    "select sum(group_by:(IntPrimitive, TheString),LongPrimitive) as c0 from SupportBean group by TheString, IntPrimitive");

                // provide column count stays at 1
                AssertCountColsAndLevels(
                    env,
                    "select sum(group_by:(TheString),IntPrimitive) as c0, sum(group_by:(TheString),IntPrimitive) as c1 from SupportBean",
                    1,
                    1);

                // prove order of group-by expressions does not matter
                AssertCountColsAndLevels(
                    env,
                    "select sum(group_by:(IntPrimitive, TheString),LongPrimitive) as c0, sum(LongPrimitive, group_by:(TheString, IntPrimitive)) as c1 from SupportBean",
                    1,
                    1);

                // prove the number of levels stays the same even when group-by expressions vary
                AssertCountColsAndLevels(
                    env,
                    "select sum(group_by:(IntPrimitive, TheString),LongPrimitive) as c0, count(*, group_by:(TheString, IntPrimitive)) as c1 from SupportBean",
                    2,
                    1);

                // prove there is one shared state factory
                var theEpl = PLAN_CALLBACK_HOOK +
                             "@Name('s0') select window(*, group_by:TheString), last(*, group_by:TheString) from SupportBean#length(2)";
                env.Compile(theEpl);
                var plan =
                    SupportAggLevelPlanHook.GetAndReset();
                Assert.AreEqual(1, plan.Second.AllLevelsForges.Length);
                Assert.AreEqual(1, plan.Second.AllLevelsForges[0].AccessStateForges.Length);
            }
        }

        internal class ResultSetAggregateFullyVersusNotFullyAgg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var colsC0 = new [] { "c0" };

                // full-aggregated and un-grouped (row for all)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(group_by:(),IntPrimitive) as c0 from SupportBean",
                    listener => {
                        EPAssertionUtil.AssertProps(
                            env.Listener("s0").AssertOneGetNewAndReset(),
                            colsC0,
                            new object[] {60});
                    });

                // aggregated and un-grouped (row for event)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(group_by:TheString, IntPrimitive) as c0 from SupportBean#keepall",
                    listener => {
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            env.Listener("s0").GetAndResetLastNewData(),
                            colsC0,
                            new[] {new object[] {10}, new object[] {50}, new object[] {50}});
                    });

                // fully aggregated and grouped (row for group)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(IntPrimitive, group_by:()) as c0, sum(group_by:TheString, IntPrimitive) as c1, TheString " +
                    "from SupportBean group by TheString",
                    listener => {
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            env.Listener("s0").GetAndResetLastNewData(),
                            "TheString,c0,c1".SplitCsv(),
                            new[] {new object[] {"E1", 60, 10}, new object[] {"E2", 60, 50}});
                    });

                // aggregated and grouped (row for event)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(LongPrimitive, group_by:()) as c0," +
                    " sum(LongPrimitive, group_by:TheString) as c1, " +
                    " sum(LongPrimitive, group_by:IntPrimitive) as c2, " +
                    " TheString " +
                    "from SupportBean#keepall group by TheString",
                    listener => {
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            env.Listener("s0").GetAndResetLastNewData(),
                            "TheString,c0,c1,c2".SplitCsv(),
                            new[] {
                                new object[] {"E1", 600L, 100L, 100L}, new object[] {"E2", 600L, 500L, 200L},
                                new object[] {"E2", 600L, 500L, 300L}
                            });
                    });
            }
        }

        public class ResultSetLocalUngroupedRowRemove : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols = "TheString,IntPrimitive,c0,c1".SplitCsv();
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_S0 delete from MyWindow where P00 = TheString and Id = IntPrimitive;\n" +
                          "on SupportBean_S1 delete from MyWindow;\n" +
                          "@Name('s0') select TheString, IntPrimitive, sum(LongPrimitive) as c0, " +
                          "  sum(LongPrimitive, group_by:TheString) as c1 from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 101);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 10, 101L, 101L});

                env.SendEventBean(new SupportBean_S0(10, "E1")); // delete event {"E1", 10}
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                MakeSendEvent(env, "E1", 20, 102);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 20, 102L, 102L});

                MakeSendEvent(env, "E2", 30, 103);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E2", 30, 102 + 103L, 103L});

                MakeSendEvent(env, "E1", 40, 104);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 40, 102 + 103 + 104L, 102 + 104L});

                env.SendEventBean(new SupportBean_S0(40, "E1")); // delete event {"E1", 40}
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                MakeSendEvent(env, "E1", 50, 105);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 50, 102 + 103 + 105L, 102 + 105L});

                env.SendEventBean(new SupportBean_S1(-1)); // delete all
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                MakeSendEvent(env, "E1", 60, 106);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 60, 106L, 106L});

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedRowRemove : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols = "TheString,IntPrimitive,c0,c1".SplitCsv();
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_S0 delete from MyWindow where P00 = TheString and Id = IntPrimitive;\n" +
                          "on SupportBean_S1 delete from MyWindow;\n" +
                          "@Name('s0') select TheString, IntPrimitive, sum(LongPrimitive) as c0, " +
                          "  sum(LongPrimitive, group_by:TheString) as c1 " +
                          "  from MyWindow group by TheString, IntPrimitive;\n";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 101);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 10, 101L, 101L});

                env.SendEventBean(new SupportBean_S0(10, "E1")); // delete event {"E1", 10}
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 10, null, null});

                MakeSendEvent(env, "E1", 20, 102);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 20, 102L, 102L});

                MakeSendEvent(env, "E2", 30, 103);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E2", 30, 103L, 103L});

                MakeSendEvent(env, "E1", 40, 104);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 40, 104L, 102 + 104L});

                env.SendEventBean(new SupportBean_S0(40, "E1")); // delete event {"E1", 40}
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 40, null, 102L});

                MakeSendEvent(env, "E1", 50, 105);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 50, 105L, 102 + 105L});

                env.SendEventBean(new SupportBean_S1(-1)); // delete all
                env.Listener("s0").Reset();

                MakeSendEvent(env, "E1", 60, 106);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {"E1", 60, 106L, 106L});

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedMultiLevelMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "TheString,IntPrimitive,c0,c1,c2,c3,c4".SplitCsv();
                var epl = "@Name('s0') select" +
                          "   TheString, IntPrimitive," +
                          "   sum(LongPrimitive, group_by:(IntPrimitive, TheString)) as c0," +
                          "   sum(LongPrimitive) as c1," +
                          "   sum(LongPrimitive, group_by:(TheString)) as c2," +
                          "   sum(LongPrimitive, group_by:(IntPrimitive)) as c3," +
                          "   sum(LongPrimitive, group_by:()) as c4" +
                          " from SupportBean" +
                          " group by TheString, IntPrimitive" +
                          " output snapshot every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 100);
                MakeSendEvent(env, "E1", 20, 202);
                MakeSendEvent(env, "E2", 10, 303);
                MakeSendEvent(env, "E1", 10, 404);
                MakeSendEvent(env, "E2", 10, 505);
                SendTime(env, 10000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 504L, 504L, 706L, 1312L, 1514L},
                        new object[] {"E1", 20, 202L, 202L, 706L, 202L, 1514L},
                        new object[] {"E2", 10, 808L, 808L, 808L, 1312L, 1514L}
                    });

                MakeSendEvent(env, "E1", 10, 1);
                SendTime(env, 20000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 505L, 505L, 707L, 1313L, 1515L},
                        new object[] {"E1", 20, 202L, 202L, 707L, 202L, 1515L},
                        new object[] {"E2", 10, 808L, 808L, 808L, 1313L, 1515L}
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedMultiLevelAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "TheString,IntPrimitive,c0,c1,c2,c3,c4".SplitCsv();
                var epl = "@Name('s0') select" +
                          "   TheString, IntPrimitive," +
                          "   window(*, group_by:(IntPrimitive, TheString)) as c0," +
                          "   window(*) as c1," +
                          "   window(*, group_by:TheString) as c2," +
                          "   window(*, group_by:IntPrimitive) as c3," +
                          "   window(*, group_by:()) as c4" +
                          " from SupportBean#keepall" +
                          " group by TheString, IntPrimitive" +
                          " output snapshot every 10 seconds" +
                          " order by TheString, IntPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                var b1 = MakeSendEvent(env, "E1", 10, 100);
                var b2 = MakeSendEvent(env, "E1", 20, 202);
                var b3 = MakeSendEvent(env, "E2", 10, 303);
                var b4 = MakeSendEvent(env, "E1", 10, 404);
                var b5 = MakeSendEvent(env, "E2", 10, 505);
                SendTime(env, 10000);

                object[] all = {b1, b2, b3, b4, b5};
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {
                        "E1", 10,
                        new object[] {b1, b4},
                        new object[] {b1, b4},
                        new object[] {b1, b2, b4},
                        new object[] {b1, b3, b4, b5}, all
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[1],
                    fields,
                    new object[] {
                        "E1", 20,
                        new object[] {b2},
                        new object[] {b2},
                        new object[] {b1, b2, b4},
                        new object[] {b2}, all
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[2],
                    fields,
                    new object[] {
                        "E2", 10,
                        new object[] {b3, b5},
                        new object[] {b3, b5},
                        new object[] {b3, b5},
                        new object[] {b1, b3, b4, b5}, all
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedMultiLevelNoDefaultLvl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "TheString,IntPrimitive,c0,c1,c2".SplitCsv();
                var epl = "@Name('s0') select" +
                          "   TheString, IntPrimitive," +
                          "   sum(LongPrimitive, group_by:(TheString)) as c0," +
                          "   sum(LongPrimitive, group_by:(IntPrimitive)) as c1," +
                          "   sum(LongPrimitive, group_by:()) as c2" +
                          " from SupportBean" +
                          " group by TheString, IntPrimitive" +
                          " output snapshot every 10 seconds";

                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 100);
                MakeSendEvent(env, "E1", 20, 202);
                MakeSendEvent(env, "E2", 10, 303);
                MakeSendEvent(env, "E1", 10, 404);
                MakeSendEvent(env, "E2", 10, 505);
                SendTime(env, 10000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 706L, 1312L, 1514L}, new object[] {"E1", 20, 706L, 202L, 1514L},
                        new object[] {"E2", 10, 808L, 1312L, 1514L}
                    });

                MakeSendEvent(env, "E1", 10, 1);
                SendTime(env, 20000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 707L, 1313L, 1515L}, new object[] {"E1", 20, 707L, 202L, 1515L},
                        new object[] {"E2", 10, 808L, 1313L, 1515L}
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedSolutionPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "TheString,pct".SplitCsv();
                var epl = "@Name('s0') select TheString, count(*) / count(*, group_by:()) as pct" +
                          " from SupportBean#time(30 sec)" +
                          " group by TheString" +
                          " output snapshot every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventMany(env, "A", "B", "C", "B", "B", "C");
                SendTime(env, 10000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"A", 1 / 6d}, new object[] {"B", 3 / 6d}, new object[] {"C", 2 / 6d}
                    });

                SendEventMany(env, "A", "B", "B", "B", "B", "A");
                SendTime(env, 20000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"A", 3 / 12d}, new object[] {"B", 7 / 12d}, new object[] {"C", 2 / 12d}
                    });

                SendEventMany(env, "C", "A", "A", "A", "B", "A");
                SendTime(env, 30000);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"A", 6 / 12d}, new object[] {"B", 5 / 12d}, new object[] {"C", 1 / 12d}
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedParenSODA : RegressionExecution
        {
            private readonly bool soda;

            public ResultSetLocalUngroupedParenSODA(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var cols = new [] { "c0", "c1", "c2", "c3", "c4" };
                var epl = "@Name('s0') select LongPrimitive, " +
                          "sum(LongPrimitive) as c0, " +
                          "sum(group_by:(),LongPrimitive) as c1, " +
                          "sum(LongPrimitive,group_by:()) as c2, " +
                          "sum(LongPrimitive,group_by:TheString) as c3, " +
                          "sum(LongPrimitive,group_by:(TheString,IntPrimitive)) as c4" +
                          " from SupportBean";
                env.CompileDeploy(soda, epl).AddListener("s0");

                MakeSendEvent(env, "E1", 1, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {10L, 10L, 10L, 10L, 10L});

                MakeSendEvent(env, "E1", 2, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {21L, 21L, 21L, 21L, 11L});

                MakeSendEvent(env, "E2", 1, 12);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {33L, 33L, 33L, 12L, 12L});

                MakeSendEvent(env, "E2", 2, 13);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {46L, 46L, 46L, 25L, 13L});

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedAggAdditionalAndPlugin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols = "c0,c1,c2,c3,c4,c5,c8,c9,c10,c11,c12,c13".SplitCsv();
                var epl = "@Name('s0') select IntPrimitive, " +
                          " countever(*, IntPrimitive>0, group_by:(TheString)) as c0," +
                          " countever(*, IntPrimitive>0, group_by:()) as c1," +
                          " countever(*, group_by:(TheString)) as c2," +
                          " countever(*, group_by:()) as c3," +
                          " concatstring(Convert.ToString(IntPrimitive), group_by:(TheString)) as c4," +
                          " concatstring(Convert.ToString(IntPrimitive), group_by:()) as c5," +
                          " sc(IntPrimitive, group_by:(TheString)) as c6," +
                          " sc(IntPrimitive, group_by:()) as c7," +
                          " leaving(group_by:(TheString)) as c8," +
                          " leaving(group_by:()) as c9," +
                          " rate(3, group_by:(TheString)) as c10," +
                          " rate(3, group_by:()) as c11," +
                          " nth(IntPrimitive, 1, group_by:(TheString)) as c12," +
                          " nth(IntPrimitive, 1, group_by:()) as c13" +
                          " from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10);
                AssertScalarColl(env.Listener("s0").LastNewData[0], new int?[] {10}, new int?[] {10});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        1L, 1L, 1L, 1L, "10", "10", false, false,
                        null, null, null, null
                    });

                MakeSendEvent(env, "E2", 20);
                AssertScalarColl(env.Listener("s0").LastNewData[0], new int?[] {20}, new int?[] {10, 20});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        1L, 2L, 1L, 2L, "20", "10 20", false, false,
                        null, null, null, 10
                    });

                MakeSendEvent(env, "E1", -1);
                AssertScalarColl(env.Listener("s0").LastNewData[0], new int?[] {10, -1}, new int?[] {10, 20, -1});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        1L, 2L, 2L, 3L, "10 -1", "10 20 -1", false, false,
                        null, null, 10, 20
                    });

                MakeSendEvent(env, "E2", 30);
                AssertScalarColl(env.Listener("s0").LastNewData[0], new int?[] {20, 30}, new int?[] {10, 20, -1, 30});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        2L, 3L, 2L, 4L, "20 30", "10 20 -1 30", false, false,
                        null, null, 20, -1
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedAggEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols =
                    "first0,first1,last0,last1,window0,window1,maxby0,maxby1,minby0,minby1,sorted0,sorted1,maxbyever0,maxbyever1,minbyever0,minbyever1,firstever0,firstever1,lastever0,lastever1"
                        .SplitCsv();
                var epl = "@Name('s0') select IntPrimitive as c0, " +
                          " first(sb, group_by:(TheString)) as first0," +
                          " first(sb, group_by:()) as first1," +
                          " last(sb, group_by:(TheString)) as last0," +
                          " last(sb, group_by:()) as last1," +
                          " window(sb, group_by:(TheString)) as window0," +
                          " window(sb, group_by:()) as window1," +
                          " maxby(IntPrimitive, group_by:(TheString)) as maxby0," +
                          " maxby(IntPrimitive, group_by:()) as maxby1," +
                          " minby(IntPrimitive, group_by:(TheString)) as minby0," +
                          " minby(IntPrimitive, group_by:()) as minby1," +
                          " sorted(IntPrimitive, group_by:(TheString)) as sorted0," +
                          " sorted(IntPrimitive, group_by:()) as sorted1," +
                          " maxbyever(IntPrimitive, group_by:(TheString)) as maxbyever0," +
                          " maxbyever(IntPrimitive, group_by:()) as maxbyever1," +
                          " minbyever(IntPrimitive, group_by:(TheString)) as minbyever0," +
                          " minbyever(IntPrimitive, group_by:()) as minbyever1," +
                          " firstever(sb, group_by:(TheString)) as firstever0," +
                          " firstever(sb, group_by:()) as firstever1," +
                          " lastever(sb, group_by:(TheString)) as lastever0," +
                          " lastever(sb, group_by:()) as lastever1" +
                          " from SupportBean#length(3) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                var b1 = MakeSendEvent(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        b1, b1, b1, b1,
                        new object[] {b1},
                        new object[] {b1},
                        b1, b1, b1, b1,
                        new object[] {b1},
                        new object[] {b1}, b1, b1, b1, b1,
                        b1, b1, b1, b1
                    });

                var b2 = MakeSendEvent(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        b2, b1, b2, b2,
                        new object[] {b2},
                        new object[] {b1, b2},
                        b2, b2, b2, b1,
                        new object[] {b2},
                        new object[] {b1, b2}, b2, b2, b2, b1,
                        b2, b1, b2, b2
                    });

                var b3 = MakeSendEvent(env, "E1", 15);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        b1, b1, b3, b3,
                        new object[] {b1, b3},
                        new object[] {b1, b2, b3},
                        b3, b2, b1, b1,
                        new object[] {b1, b3},
                        new object[] {b1, b3, b2}, b3, b2, b1, b1,
                        b1, b1, b3, b3
                    });

                var b4 = MakeSendEvent(env, "E3", 16);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {
                        b4, b2, b4, b4,
                        new object[] {b4},
                        new object[] {b2, b3, b4},
                        b4, b2, b4, b3,
                        new object[] {b4},
                        new object[] {b3, b4, b2}, b4, b2, b4, b1,
                        b4, b1, b4, b4
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedAggSQLStandard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields =
                    "c0,sum0,sum1,avedev0,avg0,max0,fmax0,min0,fmin0,maxever0,fmaxever0,minever0,fminever0,median0,stddev0"
                        .SplitCsv();
                var epl = "@Name('s0') select IntPrimitive as c0, " +
                          "sum(IntPrimitive, group_by:()) as sum0, " +
                          "sum(IntPrimitive, group_by:(TheString)) as sum1," +
                          "avedev(IntPrimitive, group_by:(TheString)) as avedev0," +
                          "avg(IntPrimitive, group_by:(TheString)) as avg0," +
                          "max(IntPrimitive, group_by:(TheString)) as max0," +
                          "fmax(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fmax0," +
                          "min(IntPrimitive, group_by:(TheString)) as min0," +
                          "fmin(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fmin0," +
                          "maxever(IntPrimitive, group_by:(TheString)) as maxever0," +
                          "fmaxever(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fmaxever0," +
                          "minever(IntPrimitive, group_by:(TheString)) as minever0," +
                          "fminever(IntPrimitive, IntPrimitive>0, group_by:(TheString)) as fminever0," +
                          "median(IntPrimitive, group_by:(TheString)) as median0," +
                          "Math.Round(coalesce(stddev(IntPrimitive, group_by:(TheString)), 0)) as stddev0" +
                          " from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        10, 10, 10,
                        0.0d, 10d, 10, 10, 10, 10, 10, 10, 10, 10, 10.0, 0L
                    });

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        20, 10 + 20, 20,
                        0.0d, 20d, 20, 20, 20, 20, 20, 20, 20, 20, 20.0, 0L
                    });

                env.SendEventBean(new SupportBean("E1", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        30, 10 + 20 + 30, 10 + 30,
                        10.0d, 20d, 30, 30, 10, 10, 30, 30, 10, 10, 20.0, 14L
                    });

                env.SendEventBean(new SupportBean("E2", 40));
                object[] expected = {
                    40, 10 + 20 + 30 + 40, 20 + 40,
                    10.0d, 30d, 40, 40, 20, 20, 40, 40, 20, 20, 30.0, 14L
                };
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedSameKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create objectarray schema MyEventOne (d1 String, d2 String, val int);\n" +
                          "@Name('s0') select sum(val, group_by: d1) as c0, sum(val, group_by: d2) as c1 from MyEventOne";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                var cols = new [] { "c0", "c1" };

                env.SendEventObjectArray(new object[] {"E1", "E1", 10}, "MyEventOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {10, 10});

                env.SendEventObjectArray(new object[] {"E1", "E2", 11}, "MyEventOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {21, 11});

                env.SendEventObjectArray(new object[] {"E2", "E1", 12}, "MyEventOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {12, 22});

                env.SendEventObjectArray(new object[] {"E3", "E1", 13}, "MyEventOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {13, 35});

                env.SendEventObjectArray(new object[] {"E3", "E3", 14}, "MyEventOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {27, 14});

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedSameKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create objectarray schema MyEventTwo (g1 String, d1 String, d2 String, val int);\n" +
                          "@Name('s0') select sum(val) as c0, sum(val, group_by: d1) as c1, sum(val, group_by: d2) as c2 from MyEventTwo group by g1";
                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                var cols = new [] { "c0", "c1", "c2" };

                env.SendEventObjectArray(new object[] {"E1", "E1", "E1", 10}, "MyEventTwo");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {10, 10, 10});

                env.SendEventObjectArray(new object[] {"E1", "E1", "E2", 11}, "MyEventTwo");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {21, 21, 11});

                env.SendEventObjectArray(new object[] {"E1", "E2", "E1", 12}, "MyEventTwo");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {33, 12, 22});

                env.SendEventObjectArray(new object[] {"X", "E1", "E1", 13}, "MyEventTwo");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {13, 10 + 11 + 13, 10 + 12 + 13});

                env.SendEventObjectArray(new object[] {"E1", "E2", "E3", 14}, "MyEventTwo");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    cols,
                    new object[] {10 + 11 + 12 + 14, 12 + 14, 14});

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedAggIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,sum0,sum1".SplitCsv();
                var epl = "@Name('s0') select IntPrimitive as c0, " +
                          "sum(IntPrimitive, group_by:()) as sum0, " +
                          "sum(IntPrimitive, group_by:(TheString)) as sum1 " +
                          " from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {10, 10, 10}});

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {10, 30, 10}, new object[] {20, 30, 20}});

                env.SendEventBean(new SupportBean("E1", 30));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {10, 60, 40}, new object[] {20, 60, 20}, new object[] {30, 60, 40}});

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean having sum(IntPrimitive, group_by:TheString) > 100";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 95);
                MakeSendEvent(env, "E2", 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                MakeSendEvent(env, "E1", 10);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedOrderBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context StartS0EndS1 start SupportBean_S0 end SupportBean_S1;" +
                          "@Name('s0') context StartS0EndS1 select TheString, sum(IntPrimitive, group_by:TheString) as c0 " +
                          " from SupportBean#keepall " +
                          " output snapshot when terminated" +
                          " order by sum(IntPrimitive, group_by:TheString)" +
                          ";";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(0));
                MakeSendEvent(env, "E1", 10);
                MakeSendEvent(env, "E2", 20);
                MakeSendEvent(env, "E1", 30);
                MakeSendEvent(env, "E3", 40);
                MakeSendEvent(env, "E2", 50);
                env.SendEventBean(new SupportBean_S1(0));

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString,c0".SplitCsv(),
                    new[] {
                        new object[] {"E1", 40}, new object[] {"E1", 40}, new object[] {"E3", 40},
                        new object[] {"E2", 70}, new object[] {"E2", 70}
                    });

                // try an empty batch
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(1));

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindow#keepall as SupportBean;" +
                          "insert into MyWindow select * from SupportBean;" +
                          "@Name('s0') on SupportBean_S0 select TheString, sum(IntPrimitive) as c0, sum(IntPrimitive, group_by:()) as c1" +
                          " from MyWindow group by TheString;";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10);
                MakeSendEvent(env, "E2", 20);
                MakeSendEvent(env, "E1", 30);
                MakeSendEvent(env, "E3", 40);
                MakeSendEvent(env, "E2", 50);

                env.SendEventBean(new SupportBean_S0(0));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString,c0,c1".SplitCsv(),
                    new[] {
                        new object[] {"E1", 40, 150}, new object[] {"E2", 70, 150}, new object[] {"E3", 40, 150}
                    });

                MakeSendEvent(env, "E1", 60);

                env.SendEventBean(new SupportBean_S0(0));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString,c0,c1".SplitCsv(),
                    new[] {
                        new object[] {"E1", 100, 210}, new object[] {"E2", 70, 210}, new object[] {"E3", 40, 210}
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select TheString, sum(IntPrimitive, group_by:TheString) as c0 from SupportBean#keepall, SupportBean_S0 unidirectional";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10);
                MakeSendEvent(env, "E2", 20);
                MakeSendEvent(env, "E1", 30);

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString,c0".SplitCsv(),
                    new[] {new object[] {"E1", 40}, new object[] {"E1", 40}, new object[] {"E2", 20}});

                MakeSendEvent(env, "E1", 40);

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString,c0".SplitCsv(),
                    new[] {
                        new object[] {"E1", 80}, new object[] {"E1", 80}, new object[] {"E1", 80},
                        new object[] {"E2", 20}
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalUngroupedThreeLevelWTop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9" };
                var epl = "@Name('s0') select " +
                          "sum(LongPrimitive, group_by:TheString) as c0," +
                          "count(*, group_by:TheString) as c1," +
                          "window(*, group_by:TheString) as c2," +
                          "sum(LongPrimitive, group_by:IntPrimitive) as c3," +
                          "count(*, group_by:IntPrimitive) as c4," +
                          "window(*, group_by:IntPrimitive) as c5," +
                          "sum(LongPrimitive, group_by:(TheString, IntPrimitive)) as c6," +
                          "count(*, group_by:(TheString, IntPrimitive)) as c7," +
                          "window(*, group_by:(TheString, IntPrimitive)) as c8," +
                          "sum(LongPrimitive) as c9 " +
                          "from SupportBean#length(4)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                var b1 = MakeSendEvent(env, "E1", 10, 100L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        100L, 1L,
                        new object[] {b1}, 100L, 1L,
                        new object[] {b1}, 100L, 1L,
                        new object[] {b1}, 100L
                    });

                env.Milestone(1);

                var b2 = MakeSendEvent(env, "E2", 10, 101L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        101L, 1L,
                        new object[] {b2}, 201L, 2L,
                        new object[] {b1, b2}, 101L, 1L,
                        new object[] {b2}, 201L
                    });

                env.Milestone(2);

                var b3 = MakeSendEvent(env, "E1", 20, 102L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        202L, 2L,
                        new object[] {b1, b3}, 102L, 1L,
                        new object[] {b3}, 102L, 1L,
                        new object[] {b3}, 303L
                    });

                env.Milestone(3);

                var b4 = MakeSendEvent(env, "E1", 10, 103L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        305L, 3L,
                        new object[] {b1, b3, b4}, 304L, 3L,
                        new object[] {b1, b2, b4}, 203L, 2L,
                        new object[] {b1, b4}, 406L
                    });

                env.Milestone(4);

                var b5 = MakeSendEvent(env, "E1", 10, 104L); // expires b1
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        309L, 3L,
                        new object[] {b3, b4, b5}, 308L, 3L,
                        new object[] {b2, b4, b5}, 207L, 2L,
                        new object[] {b4, b5}, 410L
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalGroupedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9" };
                var epl = "@Name('s0') select " +
                          "sum(LongPrimitive, group_by:TheString) as c0," +
                          "count(*, group_by:TheString) as c1," +
                          "window(*, group_by:TheString) as c2," +
                          "sum(LongPrimitive, group_by:IntPrimitive) as c3," +
                          "count(*, group_by:IntPrimitive) as c4," +
                          "window(*, group_by:IntPrimitive) as c5," +
                          "sum(LongPrimitive, group_by:()) as c6," +
                          "count(*, group_by:()) as c7," +
                          "window(*, group_by:()) as c8," +
                          "sum(LongPrimitive) as c9 " +
                          "from SupportBean#length(4)" +
                          "group by TheString, IntPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                var b1 = MakeSendEvent(env, "E1", 10, 100L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        100L, 1L,
                        new object[] {b1}, 100L, 1L,
                        new object[] {b1}, 100L, 1L,
                        new object[] {b1}, 100L
                    });

                env.Milestone(1);

                var b2 = MakeSendEvent(env, "E2", 10, 101L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        101L, 1L,
                        new object[] {b2}, 201L, 2L,
                        new object[] {b1, b2}, 201L, 2L,
                        new object[] {b1, b2}, 101L
                    });

                env.Milestone(2);

                var b3 = MakeSendEvent(env, "E1", 20, 102L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        202L, 2L,
                        new object[] {b1, b3}, 102L, 1L,
                        new object[] {b3}, 303L, 3L,
                        new object[] {b1, b2, b3}, 102L
                    });

                env.Milestone(3);

                var b4 = MakeSendEvent(env, "E1", 10, 103L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        305L, 3L,
                        new object[] {b1, b3, b4}, 304L, 3L,
                        new object[] {b1, b2, b4}, 406L, 4L,
                        new object[] {b1, b2, b3, b4}, 203L
                    });

                env.Milestone(4);

                var b5 = MakeSendEvent(env, "E1", 10, 104L); // expires b1
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        309L, 3L,
                        new object[] {b3, b4, b5}, 308L, 3L,
                        new object[] {b2, b4, b5}, 410L, 4L,
                        new object[] {b2, b3, b4, b5}, 207L
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetLocalEnumMethods : RegressionExecution
        {
            private readonly bool grouped;

            public ResultSetLocalEnumMethods(bool grouped)
            {
                this.grouped = grouped;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select" +
                          " window(*, group_by:()).firstOf() as c0," +
                          " window(*, group_by:TheString).firstOf() as c1," +
                          " window(IntPrimitive, group_by:()).firstOf() as c2," +
                          " window(IntPrimitive, group_by:TheString).firstOf() as c3," +
                          " first(*, group_by:()).IntPrimitive as c4," +
                          " first(*, group_by:TheString).IntPrimitive as c5 " +
                          " from SupportBean#keepall " +
                          (grouped ? "group by TheString, IntPrimitive" : "");
                env.CompileDeploy(epl).AddListener("s0");

                var b1 = MakeSendEvent(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1", "c2", "c3", "c4", "c5" },
                    new object[] {b1, b1, 10, 10, 10, 10});

                env.UndeployAll();
            }
        }

        protected delegate void MyAssertion(SupportListener listener);

        public class ResultSetLocalUngroupedColNameRendering : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "count(*, group_by:(TheString, IntPrimitive)), " +
                          "count(group_by:TheString, *) " +
                          "from SupportBean";
                env.CompileDeploy(epl);
                Assert.AreEqual(
                    "count(*,group_by:(TheString,IntPrimitive))",
                    env.Statement("s0").EventType.PropertyNames[0]);
                Assert.AreEqual("count(group_by:TheString,*)", env.Statement("s0").EventType.PropertyNames[1]);
                env.UndeployAll();
            }
        }
    }
} // end of namespace