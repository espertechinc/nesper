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
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeLocalGroupBy
    {
        public static readonly string PLAN_CALLBACK_HOOK =
            $"@Hook(type={typeof(HookType).FullName}.INTERNAL_AGGLOCALLEVEL,hook='{typeof(SupportAggLevelPlanHook).FullName}')";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithLocalUngroupedSumSimple(execs);
            WithLocalUngroupedAggSQLStandard(execs);
            WithLocalUngroupedAggEvent(execs);
            WithLocalUngroupedAggIterator(execs);
            WithLocalUngroupedParenSODA(execs);
            WithLocalUngroupedColNameRendering(execs);
            WithLocalUngroupedHaving(execs);
            WithLocalUngroupedUnidirectionalJoin(execs);
            WithLocalUngroupedThreeLevelWTop(execs);
            WithLocalGroupedSimple(execs);
            WithLocalGroupedMultiLevelMethod(execs);
            WithLocalGroupedSolutionPattern(execs);
            WithLocalGroupedMultiLevelAccess(execs);
            WithLocalGroupedMultiLevelNoDefaultLvl(execs);
            WithLocalPlanning(execs);
            WithLocalInvalid(execs);
            WithAggregateFullyVersusNotFullyAgg(execs);
            WithLocalUngroupedSameKey(execs);
            WithLocalGroupedSameKey(execs);
            WithLocalUngroupedRowRemove(execs);
            WithLocalGroupedRowRemove(execs);
            WithLocalGroupedOnSelect(execs);
            WithLocalUngroupedOrderBy(execs);
            WithLocalEnumMethods(execs);
            WithLocalUngroupedAggAdditionalAndPlugin(execs);
            WithLocalMultikeyWArray(execs);
            WithLocalUngroupedOnlyWGroupBy(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedOnlyWGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedOnlyWGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedAggAdditionalAndPlugin(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedAggAdditionalAndPlugin());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalEnumMethods(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalEnumMethods(true));
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedOrderBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedOrderBy());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedOnSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedOnSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedRowRemove(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedRowRemove());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedRowRemove(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedRowRemove());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedSameKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedSameKey());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedSameKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedSameKey());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregateFullyVersusNotFullyAgg(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFullyVersusNotFullyAgg());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalPlanning(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalPlanning());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedMultiLevelNoDefaultLvl(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedMultiLevelNoDefaultLvl());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedMultiLevelAccess(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedMultiLevelAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedSolutionPattern(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedSolutionPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedMultiLevelMethod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedMultiLevelMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalGroupedSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalGroupedSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedThreeLevelWTop(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedThreeLevelWTop());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedUnidirectionalJoin(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedUnidirectionalJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedColNameRendering(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedColNameRendering());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedParenSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedParenSODA(false));
            execs.Add(new ResultSetLocalUngroupedParenSODA(true));
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedAggIterator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedAggIterator());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedAggEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedAggEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedAggSQLStandard(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedAggSQLStandard());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalUngroupedSumSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLocalUngroupedSumSimple());
            return execs;
        }

        internal class ResultSetLocalUngroupedOnlyWGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select" +
                          " first(*, group_by:()).intPrimitive as c0 " +
                          " from SupportBean#keepall " +
                          "group by theString, intPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, 1, 1);
                SendAssert(env, 2, 1);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                int intPrimitive,
                int expected)
            {
                env.SendEventBean(new SupportBean("E" + intPrimitive, intPrimitive));
                env.AssertEqualsNew("s0", "c0", expected);
            }
        }

        internal class ResultSetLocalMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "sum(value, group_by:(intArray)) as c0, " +
                          "sum(value, group_by:(longArray)) as c1, " +
                          "sum(value, group_by:(doubleArray)) as c2, " +
                          "sum(value, group_by:(intArray, longArray, doubleArray)) as c3, " +
                          "sum(value) as c4 " +
                          "from SupportThreeArrayEvent";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, "E1", 10, new int[] { 1 }, new long[] { 10 }, new double[] { 100 }, 10, 10, 10, 10, 10);
                SendAssert(env, "E2", 11, new int[] { 2 }, new long[] { 20 }, new double[] { 200 }, 11, 11, 11, 11, 21);

                env.Milestone(0);

                SendAssert(env, "E3", 12, new int[] { 3 }, new long[] { 10 }, new double[] { 300 }, 12, 22, 12, 12, 33);
                SendAssert(
                    env,
                    "E4",
                    13,
                    new int[] { 1 },
                    new long[] { 20 },
                    new double[] { 200 },
                    10 + 13,
                    11 + 13,
                    11 + 13,
                    13,
                    33 + 13);
                SendAssert(
                    env,
                    "E5",
                    14,
                    new int[] { 1 },
                    new long[] { 10 },
                    new double[] { 100 },
                    10 + 13 + 14,
                    10 + 12 + 14,
                    10 + 14,
                    10 + 14,
                    33 + 13 + 14);

                env.Milestone(1);

                SendAssert(
                    env,
                    "E6",
                    15,
                    new int[] { 3 },
                    new long[] { 20 },
                    new double[] { 300 },
                    12 + 15,
                    11 + 13 + 15,
                    12 + 15,
                    15,
                    33 + 13 + 14 + 15);
                SendAssert(
                    env,
                    "E7",
                    16,
                    new int[] { 2 },
                    new long[] { 20 },
                    new double[] { 200 },
                    11 + 16,
                    11 + 13 + 15 + 16,
                    11 + 13 + 16,
                    11 + 16,
                    33 + 13 + 14 + 15 + 16);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string id,
                int value,
                int[] ints,
                long[] longs,
                double[] doubles,
                int c0,
                int c1,
                int c2,
                int c3,
                int c4)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                env.SendEventBean(new SupportThreeArrayEvent(id, value, ints, longs, doubles));
                env.AssertPropsNew("s0", fields, new object[] { c0, c1, c2, c3, c4 });
            }
        }

        internal class ResultSetLocalUngroupedSumSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                env.Milestone(0);

                env.AdvanceTime(0);
                var epl = "@name('s0') select " +
                          "sum(longPrimitive, group_by:(theString, intPrimitive)) as c0, " +
                          "sum(longPrimitive, group_by:(theString)) as c1, " +
                          "sum(longPrimitive, group_by:(intPrimitive)) as c2, " +
                          "sum(longPrimitive) as c3 " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                MakeSendEvent(env, "E1", 1, 10);
                env.AssertPropsNew("s0", fields, new object[] { 10L, 10L, 10L, 10L });

                env.Milestone(2);

                MakeSendEvent(env, "E2", 2, 11);
                env.AssertPropsNew("s0", fields, new object[] { 11L, 11L, 11L, 21L });

                env.Milestone(3);

                MakeSendEvent(env, "E1", 2, 12);
                env.AssertPropsNew("s0", fields, new object[] { 12L, 10 + 12L, 11 + 12L, 10 + 11 + 12L });

                env.Milestone(4);

                env.Milestone(5);

                MakeSendEvent(env, "E1", 1, 13);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 10 + 13L, 10 + 12 + 13L, 10 + 13L, 10 + 11 + 12 + 13L });

                MakeSendEvent(env, "E2", 1, 14);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 14L, 11 + 14L, 10 + 13 + 14L, 10 + 11 + 12 + 13 + 14L });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // not valid with count-min-sketch
                env.TryInvalidCompile(
                    "create table MyTable(approx countMinSketch(group_by:theString) @type(SupportBean))",
                    "Failed to validate table-column expression 'countMinSketch(group_by:theString)': The 'group_by' and 'filter' parameter is not allowed in create-table statements");

                // not allowed with tables
                env.TryInvalidCompile(
                    "create table MyTable(col sum(int, group_by:theString) @type(SupportBean))",
                    "Failed to validate table-column expression 'sum(int,group_by:theString)': The 'group_by' and 'filter' parameter is not allowed in create-table statements");

                // invalid named parameter
                env.TryInvalidCompile(
                    "select sum(intPrimitive, xxx:theString) from SupportBean",
                    "Failed to validate select-clause expression 'sum(intPrimitive,xxx:theString)': Invalid named parameter 'xxx' (did you mean 'group_by' or 'filter'?) [");

                // invalid group-by expression
                env.TryInvalidCompile(
                    "select sum(intPrimitive, group_by:sum(intPrimitive)) from SupportBean",
                    "Failed to validate select-clause expression 'sum(intPrimitive,group_by:sum(intPr...(44 chars)': Group-by expressions cannot contain aggregate functions");

                // other functions don't accept this named parameter
                env.TryInvalidCompile(
                    "select coalesce(0, 1, group_by:theString) from SupportBean",
                    "Failed to validate select-clause expression 'coalesce(0,1,group_by:theString)': Named parameters are not allowed");
                env.TryInvalidCompile(
                    "select " +
                    typeof(SupportStaticMethodLib).FullName +
                    ".staticMethod(group_by:intPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'com.espertech.esper.regressionlib.s...(104 chars)': Named parameters are not allowed");

                // not allowed in combination with roll-up
                env.TryInvalidCompile(
                    "select sum(intPrimitive, group_by:theString) from SupportBean group by rollup(theString)",
                    "Roll-up and group-by parameters cannot be combined ");

                // not allowed in combination with into-table
                var path = new RegressionPath();
                env.CompileDeploy("@public create table mytable (thesum sum(int))", path);
                env.TryInvalidCompile(
                    path,
                    "into table mytable select sum(intPrimitive, group_by:theString) as thesum from SupportBean",
                    "Into-table and group-by parameters cannot be combined");

                // not allowed for match-rezognize measure clauses
                var eplMatchRecog = "select * from SupportBean match_recognize (" +
                                    "  measures count(B.intPrimitive, group_by:B.theString) pattern (A B* C))";
                env.TryInvalidCompile(
                    eplMatchRecog,
                    "Match-recognize does not allow aggregation functions to specify a group-by");

                // disallow subqueries to specify their own local group-by
                var eplSubq =
                    "select (select sum(intPrimitive, group_by:theString) from SupportBean#keepall) from SupportBean_S0";
                env.TryInvalidCompile(
                    eplSubq,
                    "Failed to plan subquery number 1 querying SupportBean: Subselect aggregations functions cannot specify a group-by");

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalPlanning : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                AssertNoPlan(env, "select sum(group_by:(),intPrimitive) as c0 from SupportBean");
                AssertNoPlan(
                    env,
                    "select sum(group_by:(theString),intPrimitive) as c0 from SupportBean group by theString");
                AssertNoPlan(
                    env,
                    "select sum(group_by:(theString, intPrimitive),longPrimitive) as c0 from SupportBean group by theString, intPrimitive");
                AssertNoPlan(
                    env,
                    "select sum(group_by:(intPrimitive, theString),longPrimitive) as c0 from SupportBean group by theString, intPrimitive");

                // provide column count stays at 1
                AssertCountColsAndLevels(
                    env,
                    "select sum(group_by:(theString),intPrimitive) as c0, sum(group_by:(theString),intPrimitive) as c1 from SupportBean",
                    1,
                    1);

                // prove order of group-by expressions does not matter
                AssertCountColsAndLevels(
                    env,
                    "select sum(group_by:(intPrimitive, theString),longPrimitive) as c0, sum(longPrimitive, group_by:(theString, intPrimitive)) as c1 from SupportBean",
                    1,
                    1);

                // prove the number of levels stays the same even when group-by expressions vary
                AssertCountColsAndLevels(
                    env,
                    "select sum(group_by:(intPrimitive, theString),longPrimitive) as c0, count(*, group_by:(theString, intPrimitive)) as c1 from SupportBean",
                    2,
                    1);

                // prove there is one shared state factory
                var theEpl = PLAN_CALLBACK_HOOK +
                             "@name('s0') select window(*, group_by:theString), last(*, group_by:theString) from SupportBean#length(2)";
                env.Compile(theEpl);
                env.AssertThat(
                    () => {
                        var plan =
                            SupportAggLevelPlanHook.GetAndReset();
                        Assert.AreEqual(1, plan.Second.AllLevelsForges.Length);
                        Assert.AreEqual(1, plan.Second.AllLevelsForges[0].AccessStateForges.Length);
                    });
            }
        }

        internal class ResultSetAggregateFullyVersusNotFullyAgg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var colsC0 = "c0".SplitCsv();

                // full-aggregated and un-grouped (row for all)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(group_by:(),intPrimitive) as c0 from SupportBean",
                    listener => env.AssertPropsNew("s0", colsC0, new object[] { 60 }));

                // aggregated and un-grouped (row for event)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(group_by:theString, intPrimitive) as c0 from SupportBean#keepall",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        colsC0,
                        new object[][] { new object[] { 10 }, new object[] { 50 }, new object[] { 50 } }));

                // fully aggregated and grouped (row for group)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(intPrimitive, group_by:()) as c0, sum(group_by:theString, intPrimitive) as c1, theString " +
                    "from SupportBean group by theString",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        "theString,c0,c1".SplitCsv(),
                        new object[][] { new object[] { "E1", 60, 10 }, new object[] { "E2", 60, 50 } }));

                // aggregated and grouped (row for event)
                TryAssertionAggAndFullyAgg(
                    env,
                    "select sum(longPrimitive, group_by:()) as c0," +
                    " sum(longPrimitive, group_by:theString) as c1, " +
                    " sum(longPrimitive, group_by:intPrimitive) as c2, " +
                    " theString " +
                    "from SupportBean#keepall group by theString",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.GetAndResetLastNewData(),
                        "theString,c0,c1,c2".SplitCsv(),
                        new object[][] {
                            new object[] { "E1", 600L, 100L, 100L }, new object[] { "E2", 600L, 500L, 200L },
                            new object[] { "E2", 600L, 500L, 300L }
                        }));
            }
        }

        internal class ResultSetLocalUngroupedRowRemove : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols = "theString,intPrimitive,c0,c1".SplitCsv();
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_S0 delete from MyWindow where p00 = theString and id = intPrimitive;\n" +
                          "on SupportBean_S1 delete from MyWindow;\n" +
                          "@name('s0') select theString, intPrimitive, sum(longPrimitive) as c0, " +
                          "  sum(longPrimitive, group_by:theString) as c1 from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 101);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 10, 101L, 101L });

                env.SendEventBean(new SupportBean_S0(10, "E1")); // delete event {"E1", 10}
                env.AssertListenerNotInvoked("s0");

                MakeSendEvent(env, "E1", 20, 102);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 20, 102L, 102L });

                MakeSendEvent(env, "E2", 30, 103);
                env.AssertPropsNew("s0", cols, new object[] { "E2", 30, 102 + 103L, 103L });

                MakeSendEvent(env, "E1", 40, 104);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 40, 102 + 103 + 104L, 102 + 104L });

                env.SendEventBean(new SupportBean_S0(40, "E1")); // delete event {"E1", 40}
                env.AssertListenerNotInvoked("s0");

                MakeSendEvent(env, "E1", 50, 105);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 50, 102 + 103 + 105L, 102 + 105L });

                env.SendEventBean(new SupportBean_S1(-1)); // delete all
                env.AssertListenerNotInvoked("s0");

                MakeSendEvent(env, "E1", 60, 106);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 60, 106L, 106L });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedRowRemove : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols = "theString,intPrimitive,c0,c1".SplitCsv();
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_S0 delete from MyWindow where p00 = theString and id = intPrimitive;\n" +
                          "on SupportBean_S1 delete from MyWindow;\n" +
                          "@name('s0') select theString, intPrimitive, sum(longPrimitive) as c0, " +
                          "  sum(longPrimitive, group_by:theString) as c1 " +
                          "  from MyWindow group by theString, intPrimitive;\n";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 101);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 10, 101L, 101L });

                env.SendEventBean(new SupportBean_S0(10, "E1")); // delete event {"E1", 10}
                env.AssertPropsNew("s0", cols, new object[] { "E1", 10, null, null });

                MakeSendEvent(env, "E1", 20, 102);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 20, 102L, 102L });

                MakeSendEvent(env, "E2", 30, 103);
                env.AssertPropsNew("s0", cols, new object[] { "E2", 30, 103L, 103L });

                MakeSendEvent(env, "E1", 40, 104);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 40, 104L, 102 + 104L });

                env.SendEventBean(new SupportBean_S0(40, "E1")); // delete event {"E1", 40}
                env.AssertPropsNew("s0", cols, new object[] { "E1", 40, null, 102L });

                MakeSendEvent(env, "E1", 50, 105);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 50, 105L, 102 + 105L });

                env.SendEventBean(new SupportBean_S1(-1)); // delete all
                env.ListenerReset("s0");

                MakeSendEvent(env, "E1", 60, 106);
                env.AssertPropsNew("s0", cols, new object[] { "E1", 60, 106L, 106L });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedMultiLevelMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "theString,intPrimitive,c0,c1,c2,c3,c4".SplitCsv();
                var epl = "@name('s0') select" +
                          "   theString, intPrimitive," +
                          "   sum(longPrimitive, group_by:(intPrimitive, theString)) as c0," +
                          "   sum(longPrimitive) as c1," +
                          "   sum(longPrimitive, group_by:(theString)) as c2," +
                          "   sum(longPrimitive, group_by:(intPrimitive)) as c3," +
                          "   sum(longPrimitive, group_by:()) as c4" +
                          " from SupportBean" +
                          " group by theString, intPrimitive" +
                          " output snapshot every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 100);
                MakeSendEvent(env, "E1", 20, 202);
                MakeSendEvent(env, "E2", 10, 303);
                MakeSendEvent(env, "E1", 10, 404);
                MakeSendEvent(env, "E2", 10, 505);
                SendTime(env, 10000);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 10, 504L, 504L, 706L, 1312L, 1514L },
                        new object[] { "E1", 20, 202L, 202L, 706L, 202L, 1514L },
                        new object[] { "E2", 10, 808L, 808L, 808L, 1312L, 1514L }
                    });

                MakeSendEvent(env, "E1", 10, 1);
                SendTime(env, 20000);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 10, 505L, 505L, 707L, 1313L, 1515L },
                        new object[] { "E1", 20, 202L, 202L, 707L, 202L, 1515L },
                        new object[] { "E2", 10, 808L, 808L, 808L, 1313L, 1515L }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedMultiLevelAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "theString,intPrimitive,c0,c1,c2,c3,c4".SplitCsv();
                var epl = "@name('s0') select" +
                          "   theString, intPrimitive," +
                          "   window(*, group_by:(intPrimitive, theString)) as c0," +
                          "   window(*) as c1," +
                          "   window(*, group_by:theString) as c2," +
                          "   window(*, group_by:intPrimitive) as c3," +
                          "   window(*, group_by:()) as c4" +
                          " from SupportBean#keepall" +
                          " group by theString, intPrimitive" +
                          " output snapshot every 10 seconds" +
                          " order by theString, intPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                var b1 = MakeSendEvent(env, "E1", 10, 100);
                var b2 = MakeSendEvent(env, "E1", 20, 202);
                var b3 = MakeSendEvent(env, "E2", 10, 303);
                var b4 = MakeSendEvent(env, "E1", 10, 404);
                var b5 = MakeSendEvent(env, "E2", 10, 505);
                SendTime(env, 10000);

                var all = new object[] { b1, b2, b3, b4, b5 };
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertProps(
                            listener.LastNewData[0],
                            fields,
                            new object[] {
                                "E1",
                                10,
                                new object[] { b1, b4 },
                                new object[] { b1, b4 },
                                new object[] { b1, b2, b4 },
                                new object[] { b1, b3, b4, b5 },
                                all
                            });
                        EPAssertionUtil.AssertProps(
                            listener.LastNewData[1],
                            fields,
                            new object[] {
                                "E1",
                                20,
                                new object[] { b2 },
                                new object[] { b2 },
                                new object[] { b1, b2, b4 },
                                new object[] { b2 },
                                all
                            });
                        EPAssertionUtil.AssertProps(
                            listener.LastNewData[2],
                            fields,
                            new object[] {
                                "E2",
                                10,
                                new object[] { b3, b5 },
                                new object[] { b3, b5 },
                                new object[] { b3, b5 },
                                new object[] { b1, b3, b4, b5 },
                                all
                            });
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedMultiLevelNoDefaultLvl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "theString,intPrimitive,c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select" +
                          "   theString, intPrimitive," +
                          "   sum(longPrimitive, group_by:(theString)) as c0," +
                          "   sum(longPrimitive, group_by:(intPrimitive)) as c1," +
                          "   sum(longPrimitive, group_by:()) as c2" +
                          " from SupportBean" +
                          " group by theString, intPrimitive" +
                          " output snapshot every 10 seconds";

                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10, 100);
                MakeSendEvent(env, "E1", 20, 202);
                MakeSendEvent(env, "E2", 10, 303);
                MakeSendEvent(env, "E1", 10, 404);
                MakeSendEvent(env, "E2", 10, 505);
                SendTime(env, 10000);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 10, 706L, 1312L, 1514L }, new object[] { "E1", 20, 706L, 202L, 1514L },
                        new object[] { "E2", 10, 808L, 1312L, 1514L }
                    });

                MakeSendEvent(env, "E1", 10, 1);
                SendTime(env, 20000);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 10, 707L, 1313L, 1515L }, new object[] { "E1", 20, 707L, 202L, 1515L },
                        new object[] { "E2", 10, 808L, 1313L, 1515L }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedSolutionPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTime(env, 0);
                var fields = "theString,pct".SplitCsv();
                var epl = "@name('s0') select theString, count(*) / count(*, group_by:()) as pct" +
                          " from SupportBean#time(30 sec)" +
                          " group by theString" +
                          " output snapshot every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventMany(env, "A", "B", "C", "B", "B", "C");
                SendTime(env, 10000);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A", 1 / 6d }, new object[] { "B", 3 / 6d }, new object[] { "C", 2 / 6d }
                    });

                SendEventMany(env, "A", "B", "B", "B", "B", "A");
                SendTime(env, 20000);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A", 3 / 12d }, new object[] { "B", 7 / 12d }, new object[] { "C", 2 / 12d }
                    });

                SendEventMany(env, "C", "A", "A", "A", "B", "A");
                SendTime(env, 30000);

                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A", 6 / 12d }, new object[] { "B", 5 / 12d }, new object[] { "C", 1 / 12d }
                    });

                env.UndeployAll();
            }
        }

        private static void TryAssertionAggAndFullyAgg(
            RegressionEnvironment env,
            string selected,
            MyAssertion assertion)
        {
            var epl = "@public create context StartS0EndS1 start SupportBean_S0 end SupportBean_S1;" +
                      "@name('s0') context StartS0EndS1 " +
                      selected +
                      " output snapshot when terminated;";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(0));
            MakeSendEvent(env, "E1", 10, 100);
            MakeSendEvent(env, "E2", 20, 200);
            MakeSendEvent(env, "E2", 30, 300);
            env.SendEventBean(new SupportBean_S1(0));

            env.AssertListener("s0", assertion.Invoke);

            // try an empty batch
            env.SendEventBean(new SupportBean_S0(1));
            env.SendEventBean(new SupportBean_S1(1));

            env.UndeployAll();
        }

        internal class ResultSetLocalUngroupedParenSODA : RegressionExecution
        {
            private readonly bool soda;

            public ResultSetLocalUngroupedParenSODA(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var cols = "c0,c1,c2,c3,c4".SplitCsv();
                var epl = "@name('s0') select longPrimitive, " +
                          "sum(longPrimitive) as c0, " +
                          "sum(group_by:(),longPrimitive) as c1, " +
                          "sum(longPrimitive,group_by:()) as c2, " +
                          "sum(longPrimitive,group_by:theString) as c3, " +
                          "sum(longPrimitive,group_by:(theString,intPrimitive)) as c4" +
                          " from SupportBean";
                env.CompileDeploy(soda, epl).AddListener("s0");

                MakeSendEvent(env, "E1", 1, 10);
                env.AssertPropsNew("s0", cols, new object[] { 10L, 10L, 10L, 10L, 10L });

                MakeSendEvent(env, "E1", 2, 11);
                env.AssertPropsNew("s0", cols, new object[] { 21L, 21L, 21L, 21L, 11L });

                MakeSendEvent(env, "E2", 1, 12);
                env.AssertPropsNew("s0", cols, new object[] { 33L, 33L, 33L, 12L, 12L });

                MakeSendEvent(env, "E2", 2, 13);
                env.AssertPropsNew("s0", cols, new object[] { 46L, 46L, 46L, 25L, 13L });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        internal class ResultSetLocalUngroupedAggAdditionalAndPlugin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols = "c0,c1,c2,c3,c4,c5,c8,c9,c10,c11,c12,c13".SplitCsv();
                var epl = "@name('s0') select intPrimitive, " +
                          " countever(*, intPrimitive>0, group_by:(theString)) as c0," +
                          " countever(*, intPrimitive>0, group_by:()) as c1," +
                          " countever(*, group_by:(theString)) as c2," +
                          " countever(*, group_by:()) as c3," +
                          " concatstring(Integer.toString(intPrimitive), group_by:(theString)) as c4," +
                          " concatstring(Integer.toString(intPrimitive), group_by:()) as c5," +
                          " sc(intPrimitive, group_by:(theString)) as c6," +
                          " sc(intPrimitive, group_by:()) as c7," +
                          " leaving(group_by:(theString)) as c8," +
                          " leaving(group_by:()) as c9," +
                          " rate(3, group_by:(theString)) as c10," +
                          " rate(3, group_by:()) as c11," +
                          " nth(intPrimitive, 1, group_by:(theString)) as c12," +
                          " nth(intPrimitive, 1, group_by:()) as c13" +
                          " from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10);
                env.AssertListener(
                    "s0",
                    listener => AssertScalarColl(listener.LastNewData[0], new int?[] { 10 }, new int?[] { 10 }));
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        1L, 1L, 1L, 1L, "10", "10", false, false,
                        null, null, null, null
                    });

                MakeSendEvent(env, "E2", 20);
                env.AssertListener(
                    "s0",
                    listener => AssertScalarColl(listener.LastNewData[0], new int?[] { 20 }, new int?[] { 10, 20 }));
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        1L, 2L, 1L, 2L, "20", "10 20", false, false,
                        null, null, null, 10
                    });

                MakeSendEvent(env, "E1", -1);
                env.AssertListener(
                    "s0",
                    listener => AssertScalarColl(
                        listener.LastNewData[0],
                        new int?[] { 10, -1 },
                        new int?[] { 10, 20, -1 }));
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        1L, 2L, 2L, 3L, "10 -1", "10 20 -1", false, false,
                        null, null, 10, 20
                    });

                MakeSendEvent(env, "E2", 30);
                env.AssertListener(
                    "s0",
                    listener => AssertScalarColl(
                        listener.LastNewData[0],
                        new int?[] { 20, 30 },
                        new int?[] { 10, 20, -1, 30 }));
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        2L, 3L, 2L, 4L, "20 30", "10 20 -1 30", false, false,
                        null, null, 20, -1
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedAggEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var cols =
                    "first0,first1,last0,last1,window0,window1,maxby0,maxby1,minby0,minby1,sorted0,sorted1,maxbyever0,maxbyever1,minbyever0,minbyever1,firstever0,firstever1,lastever0,lastever1"
                        .SplitCsv();
                var epl = "@name('s0') select intPrimitive as c0, " +
                          " first(sb, group_by:(theString)) as first0," +
                          " first(sb, group_by:()) as first1," +
                          " last(sb, group_by:(theString)) as last0," +
                          " last(sb, group_by:()) as last1," +
                          " window(sb, group_by:(theString)) as window0," +
                          " window(sb, group_by:()) as window1," +
                          " maxby(intPrimitive, group_by:(theString)) as maxby0," +
                          " maxby(intPrimitive, group_by:()) as maxby1," +
                          " minby(intPrimitive, group_by:(theString)) as minby0," +
                          " minby(intPrimitive, group_by:()) as minby1," +
                          " sorted(intPrimitive, group_by:(theString)) as sorted0," +
                          " sorted(intPrimitive, group_by:()) as sorted1," +
                          " maxbyever(intPrimitive, group_by:(theString)) as maxbyever0," +
                          " maxbyever(intPrimitive, group_by:()) as maxbyever1," +
                          " minbyever(intPrimitive, group_by:(theString)) as minbyever0," +
                          " minbyever(intPrimitive, group_by:()) as minbyever1," +
                          " firstever(sb, group_by:(theString)) as firstever0," +
                          " firstever(sb, group_by:()) as firstever1," +
                          " lastever(sb, group_by:(theString)) as lastever0," +
                          " lastever(sb, group_by:()) as lastever1" +
                          " from SupportBean#length(3) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                var b1 = MakeSendEvent(env, "E1", 10);
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        b1,
                        b1,
                        b1,
                        b1,
                        new object[] { b1 },
                        new object[] { b1 },
                        b1,
                        b1,
                        b1,
                        b1,
                        new object[] { b1 },
                        new object[] { b1 },
                        b1,
                        b1,
                        b1,
                        b1,
                        b1,
                        b1,
                        b1,
                        b1
                    });

                var b2 = MakeSendEvent(env, "E2", 20);
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        b2,
                        b1,
                        b2,
                        b2,
                        new object[] { b2 },
                        new object[] { b1, b2 },
                        b2,
                        b2,
                        b2,
                        b1,
                        new object[] { b2 },
                        new object[] { b1, b2 },
                        b2,
                        b2,
                        b2,
                        b1,
                        b2,
                        b1,
                        b2,
                        b2
                    });

                var b3 = MakeSendEvent(env, "E1", 15);
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        b1,
                        b1,
                        b3,
                        b3,
                        new object[] { b1, b3 },
                        new object[] { b1, b2, b3 },
                        b3,
                        b2,
                        b1,
                        b1,
                        new object[] { b1, b3 },
                        new object[] { b1, b3, b2 },
                        b3,
                        b2,
                        b1,
                        b1,
                        b1,
                        b1,
                        b3,
                        b3
                    });

                var b4 = MakeSendEvent(env, "E3", 16);
                env.AssertPropsNew(
                    "s0",
                    cols,
                    new object[] {
                        b4,
                        b2,
                        b4,
                        b4,
                        new object[] { b4 },
                        new object[] { b2, b3, b4 },
                        b4,
                        b2,
                        b4,
                        b3,
                        new object[] { b4 },
                        new object[] { b3, b4, b2 },
                        b4,
                        b2,
                        b4,
                        b1,
                        b4,
                        b1,
                        b4,
                        b4
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedAggSQLStandard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields =
                    "c0,sum0,sum1,avedev0,avg0,max0,fmax0,min0,fmin0,maxever0,fmaxever0,minever0,fminever0,median0,stddev0"
                        .SplitCsv();
                var epl = "@name('s0') select intPrimitive as c0, " +
                          "sum(intPrimitive, group_by:()) as sum0, " +
                          "sum(intPrimitive, group_by:(theString)) as sum1," +
                          "avedev(intPrimitive, group_by:(theString)) as avedev0," +
                          "avg(intPrimitive, group_by:(theString)) as avg0," +
                          "max(intPrimitive, group_by:(theString)) as max0," +
                          "fmax(intPrimitive, intPrimitive>0, group_by:(theString)) as fmax0," +
                          "min(intPrimitive, group_by:(theString)) as min0," +
                          "fmin(intPrimitive, intPrimitive>0, group_by:(theString)) as fmin0," +
                          "maxever(intPrimitive, group_by:(theString)) as maxever0," +
                          "fmaxever(intPrimitive, intPrimitive>0, group_by:(theString)) as fmaxever0," +
                          "minever(intPrimitive, group_by:(theString)) as minever0," +
                          "fminever(intPrimitive, intPrimitive>0, group_by:(theString)) as fminever0," +
                          "median(intPrimitive, group_by:(theString)) as median0," +
                          "Math.round(coalesce(stddev(intPrimitive, group_by:(theString)), 0)) as stddev0" +
                          " from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        10,
                        10,
                        10,
                        0.0d,
                        10d,
                        10,
                        10,
                        10,
                        10,
                        10,
                        10,
                        10,
                        10,
                        10.0,
                        0L
                    });

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        20,
                        10 + 20,
                        20,
                        0.0d,
                        20d,
                        20,
                        20,
                        20,
                        20,
                        20,
                        20,
                        20,
                        20,
                        20.0,
                        0L
                    });

                env.SendEventBean(new SupportBean("E1", 30));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        30,
                        10 + 20 + 30,
                        10 + 30,
                        10.0d,
                        20d,
                        30,
                        30,
                        10,
                        10,
                        30,
                        30,
                        10,
                        10,
                        20.0,
                        14L
                    });

                env.SendEventBean(new SupportBean("E2", 40));
                var expected = new object[] {
                    40,
                    10 + 20 + 30 + 40,
                    20 + 40,
                    10.0d,
                    30d,
                    40,
                    40,
                    20,
                    20,
                    40,
                    40,
                    20,
                    20,
                    30.0,
                    14L
                };
                env.AssertPropsNew("s0", fields, expected);

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedSameKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create objectarray schema MyEventOne (d1 String, d2 String, val int);\n" +
                    "@name('s0') select sum(val, group_by: d1) as c0, sum(val, group_by: d2) as c1 from MyEventOne";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                var cols = "c0,c1".SplitCsv();

                env.SendEventObjectArray(new object[] { "E1", "E1", 10 }, "MyEventOne");
                env.AssertPropsNew("s0", cols, new object[] { 10, 10 });

                env.SendEventObjectArray(new object[] { "E1", "E2", 11 }, "MyEventOne");
                env.AssertPropsNew("s0", cols, new object[] { 21, 11 });

                env.SendEventObjectArray(new object[] { "E2", "E1", 12 }, "MyEventOne");
                env.AssertPropsNew("s0", cols, new object[] { 12, 22 });

                env.SendEventObjectArray(new object[] { "E3", "E1", 13 }, "MyEventOne");
                env.AssertPropsNew("s0", cols, new object[] { 13, 35 });

                env.SendEventObjectArray(new object[] { "E3", "E3", 14 }, "MyEventOne");
                env.AssertPropsNew("s0", cols, new object[] { 27, 14 });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedSameKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create objectarray schema MyEventTwo (g1 String, d1 String, d2 String, val int);\n" +
                    "@name('s0') select sum(val) as c0, sum(val, group_by: d1) as c1, sum(val, group_by: d2) as c2 from MyEventTwo group by g1";
                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                var cols = "c0,c1,c2".SplitCsv();

                env.SendEventObjectArray(new object[] { "E1", "E1", "E1", 10 }, "MyEventTwo");
                env.AssertPropsNew("s0", cols, new object[] { 10, 10, 10 });

                env.SendEventObjectArray(new object[] { "E1", "E1", "E2", 11 }, "MyEventTwo");
                env.AssertPropsNew("s0", cols, new object[] { 21, 21, 11 });

                env.SendEventObjectArray(new object[] { "E1", "E2", "E1", 12 }, "MyEventTwo");
                env.AssertPropsNew("s0", cols, new object[] { 33, 12, 22 });

                env.SendEventObjectArray(new object[] { "X", "E1", "E1", 13 }, "MyEventTwo");
                env.AssertPropsNew("s0", cols, new object[] { 13, 10 + 11 + 13, 10 + 12 + 13 });

                env.SendEventObjectArray(new object[] { "E1", "E2", "E3", 14 }, "MyEventTwo");
                env.AssertPropsNew("s0", cols, new object[] { 10 + 11 + 12 + 14, 12 + 14, 14 });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedAggIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,sum0,sum1".SplitCsv();
                var epl = "@name('s0') select intPrimitive as c0, " +
                          "sum(intPrimitive, group_by:()) as sum0, " +
                          "sum(intPrimitive, group_by:(theString)) as sum1 " +
                          " from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { 10, 10, 10 } });

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 10, 30, 10 }, new object[] { 20, 30, 20 } });

                env.SendEventBean(new SupportBean("E1", 30));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { 10, 60, 40 }, new object[] { 20, 60, 20 }, new object[] { 30, 60, 40 } });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean having sum(intPrimitive, group_by:theString) > 100";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 95);
                MakeSendEvent(env, "E2", 10);
                env.AssertListenerNotInvoked("s0");

                MakeSendEvent(env, "E1", 10);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedOrderBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context StartS0EndS1 start SupportBean_S0 end SupportBean_S1;" +
                          "@name('s0') context StartS0EndS1 select theString, sum(intPrimitive, group_by:theString) as c0 " +
                          " from SupportBean#keepall " +
                          " output snapshot when terminated" +
                          " order by sum(intPrimitive, group_by:theString)" +
                          ";";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(0));
                MakeSendEvent(env, "E1", 10);
                MakeSendEvent(env, "E2", 20);
                MakeSendEvent(env, "E1", 30);
                MakeSendEvent(env, "E3", 40);
                MakeSendEvent(env, "E2", 50);
                env.SendEventBean(new SupportBean_S1(0));

                env.AssertPropsPerRowLastNew(
                    "s0",
                    "theString,c0".SplitCsv(),
                    new object[][] {
                        new object[] { "E1", 40 }, new object[] { "E1", 40 }, new object[] { "E3", 40 },
                        new object[] { "E2", 70 }, new object[] { "E2", 70 }
                    });

                // try an empty batch
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(1));

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindow#keepall as SupportBean;" +
                          "insert into MyWindow select * from SupportBean;" +
                          "@name('s0') on SupportBean_S0 select theString, sum(intPrimitive) as c0, sum(intPrimitive, group_by:()) as c1" +
                          " from MyWindow group by theString;";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10);
                MakeSendEvent(env, "E2", 20);
                MakeSendEvent(env, "E1", 30);
                MakeSendEvent(env, "E3", 40);
                MakeSendEvent(env, "E2", 50);

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    "theString,c0,c1".SplitCsv(),
                    new object[][] {
                        new object[] { "E1", 40, 150 }, new object[] { "E2", 70, 150 }, new object[] { "E3", 40, 150 }
                    });

                MakeSendEvent(env, "E1", 60);

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    "theString,c0,c1".SplitCsv(),
                    new object[][] {
                        new object[] { "E1", 100, 210 }, new object[] { "E2", 70, 210 }, new object[] { "E3", 40, 210 }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select theString, sum(intPrimitive, group_by:theString) as c0 from SupportBean#keepall, SupportBean_S0 unidirectional";
                env.CompileDeploy(epl).AddListener("s0");

                MakeSendEvent(env, "E1", 10);
                MakeSendEvent(env, "E2", 20);
                MakeSendEvent(env, "E1", 30);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    "theString,c0".SplitCsv(),
                    new object[][] { new object[] { "E1", 40 }, new object[] { "E1", 40 }, new object[] { "E2", 20 } });

                MakeSendEvent(env, "E1", 40);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsPerRowLastNewAnyOrder(
                    "s0",
                    "theString,c0".SplitCsv(),
                    new object[][] {
                        new object[] { "E1", 80 }, new object[] { "E1", 80 }, new object[] { "E1", 80 },
                        new object[] { "E2", 20 }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalUngroupedThreeLevelWTop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".SplitCsv();
                var epl = "@name('s0') select " +
                          "sum(longPrimitive, group_by:theString) as c0," +
                          "count(*, group_by:theString) as c1," +
                          "window(*, group_by:theString) as c2," +
                          "sum(longPrimitive, group_by:intPrimitive) as c3," +
                          "count(*, group_by:intPrimitive) as c4," +
                          "window(*, group_by:intPrimitive) as c5," +
                          "sum(longPrimitive, group_by:(theString, intPrimitive)) as c6," +
                          "count(*, group_by:(theString, intPrimitive)) as c7," +
                          "window(*, group_by:(theString, intPrimitive)) as c8," +
                          "sum(longPrimitive) as c9 " +
                          "from SupportBean#length(4)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                var b1 = MakeSendEvent(env, "E1", 10, 100L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        100L, 1L, new object[] { b1 }, 100L, 1L, new object[] { b1 }, 100L, 1L, new object[] { b1 },
                        100L
                    });

                env.Milestone(1);

                var b2 = MakeSendEvent(env, "E2", 10, 101L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        101L, 1L, new object[] { b2 }, 201L, 2L, new object[] { b1, b2 }, 101L, 1L, new object[] { b2 },
                        201L
                    });

                env.Milestone(2);

                var b3 = MakeSendEvent(env, "E1", 20, 102L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        202L, 2L, new object[] { b1, b3 }, 102L, 1L, new object[] { b3 }, 102L, 1L, new object[] { b3 },
                        303L
                    });

                env.Milestone(3);

                var b4 = MakeSendEvent(env, "E1", 10, 103L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        305L, 3L, new object[] { b1, b3, b4 }, 304L, 3L, new object[] { b1, b2, b4 }, 203L, 2L,
                        new object[] { b1, b4 }, 406L
                    });

                env.Milestone(4);

                var b5 = MakeSendEvent(env, "E1", 10, 104L); // expires b1
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        309L, 3L, new object[] { b3, b4, b5 }, 308L, 3L, new object[] { b2, b4, b5 }, 207L, 2L,
                        new object[] { b4, b5 }, 410L
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalGroupedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".SplitCsv();
                var epl = "@name('s0') select " +
                          "sum(longPrimitive, group_by:theString) as c0," +
                          "count(*, group_by:theString) as c1," +
                          "window(*, group_by:theString) as c2," +
                          "sum(longPrimitive, group_by:intPrimitive) as c3," +
                          "count(*, group_by:intPrimitive) as c4," +
                          "window(*, group_by:intPrimitive) as c5," +
                          "sum(longPrimitive, group_by:()) as c6," +
                          "count(*, group_by:()) as c7," +
                          "window(*, group_by:()) as c8," +
                          "sum(longPrimitive) as c9 " +
                          "from SupportBean#length(4)" +
                          "group by theString, intPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                var b1 = MakeSendEvent(env, "E1", 10, 100L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        100L, 1L, new object[] { b1 }, 100L, 1L, new object[] { b1 }, 100L, 1L, new object[] { b1 },
                        100L
                    });

                env.Milestone(1);

                var b2 = MakeSendEvent(env, "E2", 10, 101L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        101L, 1L, new object[] { b2 }, 201L, 2L, new object[] { b1, b2 }, 201L, 2L,
                        new object[] { b1, b2 }, 101L
                    });

                env.Milestone(2);

                var b3 = MakeSendEvent(env, "E1", 20, 102L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        202L, 2L, new object[] { b1, b3 }, 102L, 1L, new object[] { b3 }, 303L, 3L,
                        new object[] { b1, b2, b3 }, 102L
                    });

                env.Milestone(3);

                var b4 = MakeSendEvent(env, "E1", 10, 103L);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        305L, 3L, new object[] { b1, b3, b4 }, 304L, 3L, new object[] { b1, b2, b4 }, 406L, 4L,
                        new object[] { b1, b2, b3, b4 }, 203L
                    });

                env.Milestone(4);

                var b5 = MakeSendEvent(env, "E1", 10, 104L); // expires b1
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] {
                        309L, 3L, new object[] { b3, b4, b5 }, 308L, 3L, new object[] { b2, b4, b5 }, 410L, 4L,
                        new object[] { b2, b3, b4, b5 }, 207L
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLocalEnumMethods : RegressionExecution
        {
            private readonly bool grouped;

            public ResultSetLocalEnumMethods(bool grouped)
            {
                this.grouped = grouped;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select" +
                          " window(*, group_by:()).firstOf() as c0," +
                          " window(*, group_by:theString).firstOf() as c1," +
                          " window(intPrimitive, group_by:()).firstOf() as c2," +
                          " window(intPrimitive, group_by:theString).firstOf() as c3," +
                          " first(*, group_by:()).intPrimitive as c4," +
                          " first(*, group_by:theString).intPrimitive as c5 " +
                          " from SupportBean#keepall " +
                          (grouped ? "group by theString, intPrimitive" : "");
                env.CompileDeploy(epl).AddListener("s0");

                var b1 = MakeSendEvent(env, "E1", 10);
                env.AssertPropsNew(
                    "s0",
                    "c0,c1,c2,c3,c4,c5".SplitCsv(),
                    new object[] { b1, b1, 10, 10, 10, 10 });

                env.UndeployAll();
            }
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

        protected delegate void MyAssertion(SupportListener listener);

        private static void AssertCountColsAndLevels(
            RegressionEnvironment env,
            string epl,
            int colCount,
            int lvlCount)
        {
            var theEpl = PLAN_CALLBACK_HOOK + epl;
            env.Compile(theEpl);
            env.AssertThat(
                () => {
                    var plan =
                        SupportAggLevelPlanHook.GetAndReset();
                    Assert.AreEqual(colCount, plan.First.NumColumns);
                    Assert.AreEqual(lvlCount, plan.First.Levels.Length);
                });
        }

        private static void AssertNoPlan(
            RegressionEnvironment env,
            string epl)
        {
            var theEpl = PLAN_CALLBACK_HOOK + epl;
            env.Compile(theEpl);
            Assert.IsNull(SupportAggLevelPlanHook.GetAndReset());
        }

        internal class ResultSetLocalUngroupedColNameRendering : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "count(*, group_by:(theString, intPrimitive)), " +
                          "count(group_by:theString, *) " +
                          "from SupportBean";
                env.CompileDeploy(epl);

                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(
                            "count(*,group_by:(theString,intPrimitive))",
                            statement.EventType.PropertyNames[0]);
                        Assert.AreEqual("count(group_by:theString,*)", statement.EventType.PropertyNames[1]);
                    });

                env.UndeployAll();
            }
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
    }
} // end of namespace