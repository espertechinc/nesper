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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.RegressionFlag;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateFirstLastWindow
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithStar(execs);
            WithUnboundedSimple(execs);
            WithUnboundedStream(execs);
            WithWindowedUnGrouped(execs);
            WithWindowedGrouped(execs);
            WithFirstLastIndexed(execs);
            WithPrevNthIndexedFirstLast(execs);
            WithInvalid(execs);
            WithSubquery(execs);
            WithMethodAndAccessTogether(execs);
            WithTypeAndColNameAndEquivalency(execs);
            WithJoin2Access(execs);
            WithOuterJoin1Access(execs);
            WithBatchWindow(execs);
            WithBatchWindowGrouped(execs);
            WithFirstLastWindowNoGroup(execs);
            WithFirstLastWindowGroup(execs);
            WithWindowAndSumWGroup(execs);
            WithOutputRateLimiting(execs);
            WithOnDelete(execs);
            WithLastMaxMixedOnSelect(execs);
            WithLateInitialize(execs);
            WithMixedNamedWindow(execs);
            WithNoParamChainedAndProperty(execs);
            WithOnDemandQuery(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOnDemandQuery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateOnDemandQuery());
            return execs;
        }

        public static IList<RegressionExecution> WithNoParamChainedAndProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateNoParamChainedAndProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithMixedNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMixedNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithLateInitialize(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateLateInitialize());
            return execs;
        }

        public static IList<RegressionExecution> WithLastMaxMixedOnSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateLastMaxMixedOnSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithOnDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputRateLimiting(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateOutputRateLimiting());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowAndSumWGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowAndSumWGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLastWindowGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstLastWindowGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLastWindowNoGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstLastWindowNoGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithBatchWindowGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateBatchWindowGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithBatchWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateBatchWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithOuterJoin1Access(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateOuterJoin1Access());
            return execs;
        }

        public static IList<RegressionExecution> WithJoin2Access(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateJoin2Access());
            return execs;
        }

        public static IList<RegressionExecution> WithTypeAndColNameAndEquivalency(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateTypeAndColNameAndEquivalency());
            return execs;
        }

        public static IList<RegressionExecution> WithMethodAndAccessTogether(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMethodAndAccessTogether());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithPrevNthIndexedFirstLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregatePrevNthIndexedFirstLast());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLastIndexed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstLastIndexed());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowedGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowedGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithWindowedUnGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateWindowedUnGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundedStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateUnboundedStream());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundedSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateUnboundedSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithStar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateStar());
            return execs;
        }

        internal class ResultSetAggregateFirstLastWindowGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select TheString, " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "first(IntPrimitive) as firstint, " +
                          "last(IntPrimitive) as lastint, " +
                          "window(IntPrimitive) as allint " +
"from SupportBean#length(5) group by TheString Order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "TheString,firststring,firstint,laststring,lastint,allint".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 10, "E1", 10, new int?[] { 10 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 11));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E2", 11, "E2", 11, new int?[] { 11 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 12));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 10, "E1", 12, new int?[] { 10, 12 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 13));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E2", 11, "E2", 13, new int?[] { 11, 13 } });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E2", 14));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E2", 11, "E2", 14, new int?[] { 11, 13, 14 } });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E1", 15)); // push out E1/10
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 12, "E1", 15, new int?[] { 12, 15 } });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E1", 16)); // push out E2/11 --> 2 events
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", "E1", 12, "E1", 16, new int?[] { 12, 15, 16 } },
                        new object[] { "E2", "E2", 13, "E2", 14, new int?[] { 13, 14 } }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateFirstLastWindowNoGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "first(IntPrimitive) as firstint, " +
                          "last(IntPrimitive) as lastint, " +
                          "window(IntPrimitive) as allint " +
                          "from SupportBean.win:length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "firststring,firstint,laststring,lastint,allint".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, "E1", 10, new int?[] { 10 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 11));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, "E2", 11, new int?[] { 10, 11 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 12));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 11, "E3", 12, new int?[] { 11, 12 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 13));
                env.AssertPropsNew("s0", fields, new object[] { "E3", 12, "E4", 13, new int?[] { 12, 13 } });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateNoParamChainedAndProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select first().property as val0, first().myMethod() as val1, window() as val2 from SupportEventPropertyWithMethod#lastevent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportEventPropertyWithMethod("p1"));
                env.AssertPropsNew("s0", "val0,val1".SplitCsv(), new object[] { "p1", "abc" });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateLastMaxMixedOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowOne#keepall as SupportBean;\n" +
                          "insert into MyWindowOne select * from SupportBean(TheString like 'A%');\n" +
                          "@name('s0') on SupportBean(TheString like 'B%') select last(mw.IntPrimitive) as li, max(mw.IntPrimitive) as mi from MyWindowOne mw;";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "li,mi".SplitCsv();

                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("B1", -1));
                env.AssertPropsNew("s0", fields, new object[] { 10, 10 });

                env.Milestone(0);

                for (var i = 11; i < 20; i++) {
                    env.SendEventBean(new SupportBean("A1", i));
                    env.SendEventBean(new SupportBean("Bx", -1));
                    env.AssertPropsNew("s0", fields, new object[] { i, i });
                }

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("B1", -1));
                env.AssertPropsNew("s0", fields, new object[] { 1, 19 });

                env.SendEventBean(new SupportBean("A1", 2));
                env.SendEventBean(new SupportBean("B1", -1));
                env.AssertPropsNew("s0", fields, new object[] { 2, 19 });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregatePrevNthIndexedFirstLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "prev(IntPrimitive, 0) as p0, " +
                          "prev(IntPrimitive, 1) as p1, " +
                          "prev(IntPrimitive, 2) as p2, " +
                          "nth(IntPrimitive, 0) as n0, " +
                          "nth(IntPrimitive, 1) as n1, " +
                          "nth(IntPrimitive, 2) as n2, " +
                          "last(IntPrimitive, 0) as l1, " +
                          "last(IntPrimitive, 1) as l2, " +
                          "last(IntPrimitive, 2) as l3 " +
                          "from SupportBean#length(3)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var fields = "p0,p1,p2,n0,n1,n2,l1,l2,l3".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { 10, null, null, 10, null, null, 10, null, null });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 11));
                env.AssertPropsNew("s0", fields, new object[] { 11, 10, null, 11, 10, null, 11, 10, null });

                env.SendEventBean(new SupportBean("E3", 12));
                env.AssertPropsNew("s0", fields, new object[] { 12, 11, 10, 12, 11, 10, 12, 11, 10 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 13));
                env.AssertPropsNew("s0", fields, new object[] { 13, 12, 11, 13, 12, 11, 13, 12, 11 });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateFirstLastIndexed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@name('s0') select " +
                          "first(IntPrimitive, 0) as f0, " +
                          "first(IntPrimitive, 1) as f1, " +
                          "first(IntPrimitive, 2) as f2, " +
                          "first(IntPrimitive, 3) as f3, " +
                          "last(IntPrimitive, 0) as l0, " +
                          "last(IntPrimitive, 1) as l1, " +
                          "last(IntPrimitive, 2) as l2, " +
                          "last(IntPrimitive, 3) as l3 " +
                          "from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionFirstLastIndexed(env, milestone);

                // test join
                env.UndeployAll();
                epl += ", SupportBean_A#lastevent";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));

                TryAssertionFirstLastIndexed(env, milestone);

                // test variable
                env.UndeployAll();

                var path = new RegressionPath();
                env.CompileDeploy("@name('var') @public create variable int indexvar = 2", path);
                epl = "@name('s0') select first(IntPrimitive, indexvar) as f0 from SupportBean#keepall";
                env.CompileDeploy(epl, path).AddListener("s0");

                var fields = "f0".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E1", 11));
                env.ListenerReset("s0");

                env.SendEventBean(new SupportBean("E1", 12));
                env.AssertPropsNew("s0", fields, new object[] { 12 });

                env.RuntimeSetVariable("var", "indexvar", 0);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 13));
                env.AssertPropsNew("s0", fields, new object[] { 10 });
                env.UndeployAll();

                // test as part of function
                env.CompileDeploy("select Math.abs(last(IntPrimitive)) from SupportBean").UndeployAll();
            }
        }

        internal class ResultSetAggregateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select window(distinct IntPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'window(IntPrimitive)': The 'window' aggregation function requires");

                env.TryInvalidCompile(
                    "select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean#lastevent sa, SupportBean#lastevent sb",
                    "Failed to validate select-clause expression 'window(sa.IntPrimitive+sb.IntPrimitive)': The 'window' aggregation function requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead [select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean#lastevent sa, SupportBean#lastevent sb]");

                env.TryInvalidCompile(
                    "select last(*) from SupportBean#lastevent sa, SupportBean#lastevent sb",
                    "Failed to validate select-clause expression 'last(*)': The 'last' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select last(*) from SupportBean#lastevent sa, SupportBean#lastevent sb]");

                env.TryInvalidCompile(
                    "select TheString, (select first(*) from SupportBean#lastevent sa) from SupportBean#lastevent sb",
                    "Failed to plan subquery number 1 querying SupportBean: Failed to validate select-clause expression 'first(*)': The 'first' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select TheString, (select first(*) from SupportBean#lastevent sa) from SupportBean#lastevent sb]");

                env.TryInvalidCompile(
                    "select window(x.*) from SupportBean#lastevent",
                    "Failed to validate select-clause expression 'window(x.*)': Stream by name 'x' could not be found among all streams [select window(x.*) from SupportBean#lastevent]");

                env.TryInvalidCompile(
                    "select window(*) from SupportBean x",
                    "Failed to validate select-clause expression 'window(*)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(*) from SupportBean x]");
                env.TryInvalidCompile(
                    "select window(x.*) from SupportBean x",
                    "Failed to validate select-clause expression 'window(x.*)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.*) from SupportBean x]");
                env.TryInvalidCompile(
                    "select window(x.IntPrimitive) from SupportBean x",
                    "Failed to validate select-clause expression 'window(x.IntPrimitive)': The 'window' aggregation function requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.IntPrimitive) from SupportBean x]");

                env.TryInvalidCompile(
                    "select window(x.IntPrimitive, 10) from SupportBean#keepall x",
                    "Failed to validate select-clause expression 'window(x.IntPrimitive,10)': The 'window' aggregation function does not accept an index expression; Use 'first' or 'last' instead [");

                env.TryInvalidCompile(
                    "select first(x.*, 10d) from SupportBean#lastevent as x",
                    "Failed to validate select-clause expression 'first(x.*,10.0)': The 'first' aggregation function requires an index expression that returns an integer value [select first(x.*, 10d) from SupportBean#lastevent as x]");
            }
        }

        internal class ResultSetAggregateSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Id, (select window(sb.*) from SupportBean#length(2) as sb) as w from SupportBean_A";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "Id,w".SplitCsv();

                env.SendEventBean(new SupportBean_A("A1"));
                env.AssertPropsNew("s0", fields, new object[] { "A1", null });

                env.Milestone(0);

                var beanOne = SendEvent(env, "E1", 0, 1);
                env.SendEventBean(new SupportBean_A("A2"));
                env.AssertPropsNew("s0", fields, new object[] { "A2", new object[] { beanOne } });

                env.Milestone(1);

                var beanTwo = SendEvent(env, "E2", 0, 1);
                env.SendEventBean(new SupportBean_A("A3"));
                env.AssertPropsNew("s0", fields, new object[] { "A3", new object[] { beanOne, beanTwo } });

                env.Milestone(2);

                var beanThree = SendEvent(env, "E2", 0, 1);
                env.SendEventBean(new SupportBean_A("A4"));
                env.AssertPropsNew("s0", fields, new object[] { "A4", new object[] { beanTwo, beanThree } });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAndAccessTogether : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl =
                    "@name('s0') select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#length(2) as sa";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "si,wi".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { 1, IntArray(1) });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { 3, IntArray(1, 2) });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("s0", fields, new object[] { 5, IntArray(2, 3) });

                env.MilestoneInc(milestone);

                env.UndeployAll();

                epl =
                    "@name('s0') select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#keepall as sa group by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { 1, IntArray(1) });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { 2, IntArray(2) });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 3));
                env.AssertPropsNew("s0", fields, new object[] { 5, IntArray(2, 3) });

                env.SendEventBean(new SupportBean("E1", 4));
                env.AssertPropsNew("s0", fields, new object[] { 5, IntArray(1, 4) });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateOutputRateLimiting : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#keepall as sa output every 2 events";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "si,wi".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 1, IntArray(1) },
                        new object[] { 3, IntArray(1, 2) },
                    });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 3));
                env.SendEventBean(new SupportBean("E4", 4));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { 6, IntArray(1, 2, 3) },
                        new object[] { 10, IntArray(1, 2, 3, 4) },
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateTypeAndColNameAndEquivalency : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@name('s0') select " +
                          "first(sa.DoublePrimitive + sa.IntPrimitive), " +
                          "first(sa.IntPrimitive), " +
                          "window(sa.*), " +
                          "last(*) from SupportBean#length(2) as sa";
                env.CompileDeploy(epl).AddListener("s0");

                var rows = new object[][] {
                    new object[] { "first(sa.DoublePrimitive+sa.IntPrimitive)", typeof(double?) },
                    new object[] { "first(sa.IntPrimitive)", typeof(int?) },
                    new object[] { "window(sa.*)", typeof(SupportBean[]) },
                    new object[] { "last(*)", typeof(SupportBean) },
                };
                for (var i = 0; i < rows.Length; i++) {
                    var index = i;
                    env.AssertStatement(
                        "s0",
                        statement => {
                            var prop = statement.EventType.PropertyDescriptors[index];
                            Assert.AreEqual(rows[index][0], prop.PropertyName);
                            Assert.AreEqual(rows[index][1], prop.PropertyType);
                        });
                }

                env.UndeployAll();

                epl = "@name('s0') select " +
                      "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
                      "first(sa.IntPrimitive) as f2, " +
                      "window(sa.*) as w1, " +
                      "last(*) as l1 " +
                      "from SupportBean#length(2) as sa";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionType(env, false, milestone);

                env.UndeployAll();

                epl = "@name('s0') select " +
                      "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
                      "first(sa.IntPrimitive) as f2, " +
                      "window(sa.*) as w1, " +
                      "last(*) as l1 " +
                      "from SupportBean#length(2) as sa " +
                      "having SupportStaticMethodLib.AlwaysTrue({first(sa.DoublePrimitive + sa.IntPrimitive), " +
                      "first(sa.IntPrimitive), window(sa.*), last(*)})";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionType(env, true, milestone);

                env.UndeployAll();
            }

            private void TryAssertionType(
                RegressionEnvironment env,
                bool isCheckStatic,
                AtomicLong milestone)
            {
                var fields = "f1,f2,w1,l1".SplitCsv();
                SupportStaticMethodLib.Invocations.Clear();

                var beanOne = SendEvent(env, "E1", 10d, 100);
                var expected = new object[] { 110d, 100, new object[] { beanOne }, beanOne };
                env.AssertPropsNew("s0", fields, expected);
                if (isCheckStatic) {
                    env.AssertThat(
                        () => {
                            var parameters = SupportStaticMethodLib.Invocations[0];
                            SupportStaticMethodLib.Invocations.Clear();
                            EPAssertionUtil.AssertEqualsExactOrder(expected, parameters);
                        });
                }

                env.MilestoneInc(milestone);
            }
        }

        internal class ResultSetAggregateJoin2Access : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "sa.Id as ast, " +
                          "sb.Id as bst, " +
                          "first(sa.Id) as fas, " +
                          "window(sa.Id) as was, " +
                          "last(sa.Id) as las, " +
                          "first(sb.Id) as fbs, " +
                          "window(sb.Id) as wbs, " +
                          "last(sb.Id) as lbs " +
                          "from SupportBean_A#length(2) as sa, SupportBean_B#length(2) as sb " +
"Order by ast, bst";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "ast,bst,fas,was,las,fbs,wbs,lbs".SplitCsv();

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { "A1", "B1", "A1", Split("A1"), "A1", "B1", Split("B1"), "B1" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("A2"));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A2", "B1", "A1", Split("A1,A2"), "A2", "B1", Split("B1"), "B1" }
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("A3"));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A3", "B1", "A2", Split("A2,A3"), "A3", "B1", Split("B1"), "B1" }
                    });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_B("B2"));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A2", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2" },
                        new object[] { "A3", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2" }
                    });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_B("B3"));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A2", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3" },
                        new object[] { "A3", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3" }
                    });

                env.Milestone(4);

                env.SendEventBean(new SupportBean_A("A4"));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "A4", "B2", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3" },
                        new object[] { "A4", "B3", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3" }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateOuterJoin1Access : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "sa.Id as aid, " +
                          "sb.Id as bid, " +
                          "first(sb.P10) as fb, " +
                          "window(sb.P10) as wb, " +
                          "last(sb.P10) as lb " +
                          "from SupportBean_S0#keepall as sa " +
                          "left outer join " +
                          "SupportBean_S1#keepall as sb " +
                          "on sa.Id = sb.Id";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "aid,bid,fb,wb,lb".SplitCsv();

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1, null, null, null, null });

                env.SendEventBean(new SupportBean_S1(1, "A"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1, 1, "A", Split("A"), "A" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(2, "B"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(2, "A"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 2, 2, "A", Split("A,B"), "B" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(3, "C"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(3, "C"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 3, 3, "A", Split("A,B,C"), "C" });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream " +
                          "first(TheString) as fs, " +
                          "window(TheString) as ws, " +
                          "last(TheString) as ls " +
                          "from SupportBean#length_batch(2) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "fs,ws,ls".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "E1", Split("E1,E2"), "E2" },
                    new object[] { null, null, null });
                env.ListenerReset("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 0));
                env.SendEventBean(new SupportBean("E4", 0));
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "E3", Split("E3,E4"), "E4" },
                    new object[] { "E1", Split("E1,E2"), "E2" });
                env.ListenerReset("s0");

                env.SendEventBean(new SupportBean("E5", 0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E6", 0));
                env.AssertPropsIRPair(
                    "s0",
                    fields,
                    new object[] { "E5", Split("E5,E6"), "E6" },
                    new object[] { "E3", Split("E3,E4"), "E4" });
                env.ListenerReset("s0");

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateBatchWindowGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "TheString, " +
                          "first(IntPrimitive) as fi, " +
                          "window(IntPrimitive) as wi, " +
                          "last(IntPrimitive) as li " +
"from SupportBean#length_batch(6) as sb group by TheString Order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "TheString,fi,wi,li".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 11));
                env.SendEventBean(new SupportBean("E3", 30));

                env.SendEventBean(new SupportBean("E3", 31));
                env.AssertListenerNotInvoked("s0");
                env.SendEventBean(new SupportBean("E1", 12));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 10, IntArray(10, 11, 12), 12 },
                        new object[] { "E2", 20, IntArray(20), 20 },
                        new object[] { "E3", 30, IntArray(30, 31), 31 }
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 13));
                env.SendEventBean(new SupportBean("E1", 14));
                env.SendEventBean(new SupportBean("E1", 15));
                env.SendEventBean(new SupportBean("E1", 16));
                env.SendEventBean(new SupportBean("E1", 17));
                env.SendEventBean(new SupportBean("E1", 18));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 13, IntArray(13, 14, 15, 16, 17, 18), 18 },
                        new object[] { "E2", null, null, null },
                        new object[] { "E3", null, null, null }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateLateInitialize : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create window MyWindowTwo#keepall as select * from SupportBean;\n" +
                          "insert into MyWindowTwo select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                env.Milestone(0);

                var fields = "firststring,windowstring,laststring".SplitCsv();
                epl = "@name('s0') select " +
                      "first(TheString) as firststring, " +
                      "window(TheString) as windowstring, " +
                      "last(TheString) as laststring " +
                      "from MyWindowTwo";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E3", 30));
                env.AssertPropsNew("s0", fields, new object[] { "E1", Split("E1,E2,E3"), "E3" });

                env.Milestone(1);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "firststring,windowstring,laststring".SplitCsv();
                var epl = "create window MyWindowThree#keepall as select * from SupportBean;\n" +
                          "insert into MyWindowThree select * from SupportBean;\n" +
                          "on SupportBean_A delete from MyWindowThree where TheString = Id;\n" +
                          "@name('s0') select " +
                          "first(TheString) as firststring, " +
                          "window(TheString) as windowstring, " +
                          "last(TheString) as laststring " +
                          "from MyWindowThree";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "E1", Split("E1"), "E1" });

                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsNew("s0", fields, new object[] { "E1", Split("E1,E2"), "E2" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 30));
                env.AssertPropsNew("s0", fields, new object[] { "E1", Split("E1,E2,E3"), "E3" });

                env.SendEventBean(new SupportBean_A("E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E1", Split("E1,E3"), "E3" });

                env.SendEventBean(new SupportBean_A("E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E1", Split("E1"), "E1" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("E1"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E4", 40));
                env.AssertPropsNew("s0", fields, new object[] { "E4", Split("E4"), "E4" });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E5", 50));
                env.AssertPropsNew("s0", fields, new object[] { "E4", Split("E4,E5"), "E5" });

                env.SendEventBean(new SupportBean_A("E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E5", Split("E5"), "E5" });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E6", 60));
                env.AssertPropsNew("s0", fields, new object[] { "E5", Split("E5,E6"), "E6" });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateOnDemandQuery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create window MyWindowFour#keepall as select * from SupportBean;\n" +
                          "insert into MyWindowFour select * from SupportBean;";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E3", 30));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 31));
                env.SendEventBean(new SupportBean("E1", 11));
                env.SendEventBean(new SupportBean("E1", 12));

                var qc = env.CompileFAF(
                    "select first(IntPrimitive) as f, window(IntPrimitive) as w, last(IntPrimitive) as l from MyWindowFour as s",
                    path);
                var q = env.Runtime.FireAndForgetService.PrepareQuery(qc);
                EPAssertionUtil.AssertPropsPerRow(
                    q.Execute().Array,
                    "f,w,l".SplitCsv(),
                    new object[][] { new object[] { 10, IntArray(10, 20, 30, 31, 11, 12), 12 } });

                env.SendEventBean(new SupportBean("E1", 13));
                EPAssertionUtil.AssertPropsPerRow(
                    q.Execute().Array,
                    "f,w,l".SplitCsv(),
                    new object[][] { new object[] { 10, IntArray(10, 20, 30, 31, 11, 12, 13), 13 } });

                env.Milestone(1);

                qc = env.CompileFAF(
"select TheString as s, first(IntPrimitive) as f, window(IntPrimitive) as w, last(IntPrimitive) as l from MyWindowFour as s group by TheString Order by TheString asc",
                    path);
                q = env.Runtime.FireAndForgetService.PrepareQuery(qc);
                var expected = new object[][] {
                    new object[] { "E1", 10, IntArray(10, 11, 12, 13), 13 },
                    new object[] { "E2", 20, IntArray(20), 20 },
                    new object[] { "E3", 30, IntArray(30, 31), 31 }
                };
                EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".SplitCsv(), expected);
                EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".SplitCsv(), expected);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(FIREANDFORGET);
            }
        }

        internal class ResultSetAggregateStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@name('s0') select " +
                          "first(*) as firststar, " +
                          "first(sb.*) as firststarsb, " +
                          "last(*) as laststar, " +
                          "last(sb.*) as laststarsb, " +
                          "window(*) as windowstar, " +
                          "window(sb.*) as windowstarsb, " +
                          "firstever(*) as firsteverstar, " +
                          "lastever(*) as lasteverstar " +
                          "from SupportBean#length(2) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var props = statement.EventType.PropertyDescriptors;
                        for (var i = 0; i < props.Count; i++) {
                            Assert.AreEqual(
                                i == 4 || i == 5 ? typeof(SupportBean[]) : typeof(SupportBean),
                                props[i].PropertyType);
                        }
                    });

                TryAssertionStar(env, milestone);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionStar(env, milestone);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateUnboundedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select first(TheString) as c0, last(TheString) as c1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "c0,c1".SplitCsv();

                env.Milestone(0);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1" });

                env.Milestone(1);

                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E2" });

                env.Milestone(2);

                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E3" });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateUnboundedStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "first(TheString) as f1, " +
                          "first(sb.*) as f2, " +
                          "first(*) as f3, " +
                          "last(TheString) as l1, " +
                          "last(sb.*) as l2, " +
                          "last(*) as l3 " +
                          "from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "f1,f2,f3,l1,l2,l3".SplitCsv();

                var beanOne = SendEvent(env, "E1", 1d, 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1", beanOne, beanOne, "E1", beanOne, beanOne });

                env.Milestone(0);

                var beanTwo = SendEvent(env, "E2", 2d, 2);
                env.AssertPropsNew("s0", fields, new object[] { "E1", beanOne, beanOne, "E2", beanTwo, beanTwo });

                env.Milestone(1);

                var beanThree = SendEvent(env, "E3", 3d, 3);
                env.AssertPropsNew("s0", fields, new object[] { "E1", beanOne, beanOne, "E3", beanThree, beanThree });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateWindowedUnGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@name('s0') select " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "first(IntPrimitive) as firstint, " +
                          "last(IntPrimitive) as lastint, " +
                          "window(IntPrimitive) as allint " +
                          "from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionUngrouped(env, milestone);

                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionUngrouped(env, milestone);

                env.UndeployAll();

                // test null-value provided
                epl = "@name('s0') select window(IntBoxed).take(10) from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateWindowedGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@name('s0') select " +
                          "TheString, " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "first(IntPrimitive) as firstint, " +
                          "last(IntPrimitive) as lastint, " +
                          "window(IntPrimitive) as allint " +
                          "from SupportBean#length(5) " +
"group by TheString Order by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionGrouped(env, milestone);

                env.UndeployAll();

                // SODA
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionGrouped(env, milestone);

                env.UndeployAll();

                // test hints
                var newEPL = "@Hint('disable_reclaim_group') " + epl;
                env.CompileDeploy(newEPL).AddListener("s0");

                TryAssertionGrouped(env, milestone);

                // test hints
                env.UndeployAll();
                newEPL = "@Hint('reclaim_group_aged=10,reclaim_group_freq=5') " + epl;
                env.CompileDeploy(newEPL).AddListener("s0");

                TryAssertionGrouped(env, milestone);

                env.UndeployAll();

                // test SODA indexes
                var eplFirstLast = "@name('s0') select " +
                                   "last(IntPrimitive), " +
                                   "last(IntPrimitive,1), " +
                                   "first(IntPrimitive), " +
                                   "first(IntPrimitive,1) " +
                                   "from SupportBean#length(3)";
                env.EplToModelCompileDeploy(eplFirstLast);
                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMixedNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "create window ABCWin.win:keepall() as SupportBean;\n" +
                          "insert into ABCWin select * from SupportBean;\n" +
                          "on SupportBean_S0 delete from ABCWin where IntPrimitive = Id;\n" +
                          "@name('s0') select TheString as c0, sum(IntPrimitive) as c1, window(IntPrimitive) as c2 from ABCWin group by TheString;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 10);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, new int?[] { 10 } });

                env.Milestone(1);

                SendSupportBean(env, "E2", 100);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 100, new int?[] { 100 } });

                env.Milestone(2);

                SendSupportBean_S0(env, 100); // delete E2 group
                env.AssertPropsNew("s0", fields, new object[] { "E2", null, null });

                env.Milestone(3);

                SendSupportBean(env, "E1", 11);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10 + 11, new int?[] { 10, 11 } });

                env.Milestone(4);

                SendSupportBean_S0(env, 10); // delete from E1 group
                env.AssertPropsNew("s0", fields, new object[] { "E1", 11, new int?[] { 11 } });

                env.Milestone(5);

                env.Milestone(6); // no change

                SendSupportBean(env, "E2", 101);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 101, new int?[] { 101 } });
                SendSupportBean(env, "E2", 102);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 101 + 102, new int?[] { 101, 102 } });
                SendSupportBean(env, "E1", 12);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 11 + 12, new int?[] { 11, 12 } });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateWindowAndSumWGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();

                var epl = "@name('s0') select TheString as c0, sum(IntPrimitive) as c1," +
"window(IntPrimitive*LongPrimitive) as c2 from SupportBean#length(3) group by TheString Order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 10, 5);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, new long?[] { 5 * 10L } });

                env.Milestone(1);

                SendSupportBean(env, "E2", 100, 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 100, new long?[] { 20 * 100L } });

                env.Milestone(2);

                SendSupportBean(env, "E1", 15, 2);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10 + 15, new long?[] { 5 * 10L, 2 * 15L } });

                env.Milestone(3);

                SendSupportBean(env, "E1", 18, 3);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 15 + 18, new long?[] { 2 * 15L, 3 * 18L } });

                env.Milestone(4);

                SendSupportBean(env, "E1", 19, 4); // pushed out E2
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 15 + 18 + 19, new long?[] { 2 * 15L, 3 * 18L, 4 * 19L } },
                        new object[] { "E2", null, null }
                    });

                env.Milestone(5);

                env.Milestone(6);

                SendSupportBean(env, "E1", 17, -1);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { "E1", 18 + 19 + 17, new long?[] { 3 * 18L, 4 * 19L, -1 * 17L } });
                SendSupportBean(env, "E2", 1, 1000);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 19 + 17, new long?[] { 4 * 19L, -1 * 17L } },
                        new object[] { "E2", 1, new long?[] { 1 * 1000L } }
                    });

                env.UndeployAll();
            }
        }

        private static void TryAssertionGrouped(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "TheString,firststring,firstint,laststring,lastint,allint".SplitCsv();

            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 10, "E1", 10, new int?[] { 10 } });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 11));
            env.AssertPropsNew("s0", fields, new object[] { "E2", "E2", 11, "E2", 11, new int?[] { 11 } });

            env.SendEventBean(new SupportBean("E1", 12));
            env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 10, "E1", 12, new int?[] { 10, 12 } });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 13));
            env.AssertPropsNew("s0", fields, new object[] { "E2", "E2", 11, "E2", 13, new int?[] { 11, 13 } });

            env.SendEventBean(new SupportBean("E2", 14));
            env.AssertPropsNew("s0", fields, new object[] { "E2", "E2", 11, "E2", 14, new int?[] { 11, 13, 14 } });

            env.SendEventBean(new SupportBean("E1", 15)); // push out E1/10
            env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", 12, "E1", 15, new int?[] { 12, 15 } });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E1", 16)); // push out E2/11 --> 2 events
            env.AssertPropsPerRowNewOnly(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E1", "E1", 12, "E1", 16, new int?[] { 12, 15, 16 } },
                    new object[] { "E2", "E2", 13, "E2", 14, new int?[] { 13, 14 } }
                });
        }

        private static void TryAssertionFirstLastIndexed(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "f0,f1,f2,f3,l0,l1,l2,l3".SplitCsv();
            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertPropsNew("s0", fields, new object[] { 10, null, null, null, 10, null, null, null });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 11));
            env.AssertPropsNew("s0", fields, new object[] { 10, 11, null, null, 11, 10, null, null });

            env.SendEventBean(new SupportBean("E3", 12));
            env.AssertPropsNew("s0", fields, new object[] { 10, 11, 12, null, 12, 11, 10, null });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E4", 13));
            env.AssertPropsNew("s0", fields, new object[] { 11, 12, 13, null, 13, 12, 11, null });
        }

        private static void TryAssertionStar(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "firststar,firststarsb,laststar,laststarsb,windowstar,windowstarsb,firsteverstar,lasteverstar"
                .SplitCsv();

            object beanE1 = new SupportBean("E1", 10);
            env.SendEventBean(beanE1);
            var window = new object[] { beanE1 };
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { beanE1, beanE1, beanE1, beanE1, window, window, beanE1, beanE1 });

            env.MilestoneInc(milestone);

            object beanE2 = new SupportBean("E2", 20);
            env.SendEventBean(beanE2);
            window = new object[] { beanE1, beanE2 };
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { beanE1, beanE1, beanE2, beanE2, window, window, beanE1, beanE2 });

            env.MilestoneInc(milestone);

            object beanE3 = new SupportBean("E3", 30);
            env.SendEventBean(beanE3);
            window = new object[] { beanE2, beanE3 };
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { beanE2, beanE2, beanE3, beanE3, window, window, beanE1, beanE3 });

            env.MilestoneInc(milestone);
        }

        private static void TryAssertionUngrouped(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "firststring,firstint,laststring,lastint,allint".SplitCsv();

            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertPropsNew("s0", fields, new object[] { "E1", 10, "E1", 10, new int?[] { 10 } });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 11));
            env.AssertPropsNew("s0", fields, new object[] { "E1", 10, "E2", 11, new int?[] { 10, 11 } });

            env.SendEventBean(new SupportBean("E3", 12));
            env.AssertPropsNew("s0", fields, new object[] { "E2", 11, "E3", 12, new int?[] { 11, 12 } });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E4", 13));
            env.AssertPropsNew("s0", fields, new object[] { "E3", 12, "E4", 13, new int?[] { 12, 13 } });
        }

        private static object Split(string s)
        {
            if (s == null) {
                return Array.Empty<object>();
            }

            return s.SplitCsv();
        }

        private static int?[] IntArray(params int[] value)
        {
            if (value == null) {
                return Array.Empty<int?>();
            }

            var ints = new int?[value.Length];
            for (var i = 0; i < value.Length; i++) {
                ints[i] = value[i];
            }

            return ints;
        }

        private static SupportBean SendEvent(
            RegressionEnvironment env,
            string theString,
            double doublePrimitive,
            int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.DoublePrimitive = doublePrimitive;
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string e1)
        {
            env.SendEventBean(new SupportBean(e1, 0));
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static void SendSupportBean_S0(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportBean_S0(id));
        }
    }
} // end of namespace