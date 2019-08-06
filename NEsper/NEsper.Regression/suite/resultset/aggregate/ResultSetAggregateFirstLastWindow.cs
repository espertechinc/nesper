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

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateFirstLastWindow
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateStar());
            execs.Add(new ResultSetAggregateUnboundedSimple());
            execs.Add(new ResultSetAggregateUnboundedStream());
            execs.Add(new ResultSetAggregateWindowedUnGrouped());
            execs.Add(new ResultSetAggregateWindowedGrouped());
            execs.Add(new ResultSetAggregateFirstLastIndexed());
            execs.Add(new ResultSetAggregatePrevNthIndexedFirstLast());
            execs.Add(new ResultSetAggregateInvalid());
            execs.Add(new ResultSetAggregateSubquery());
            execs.Add(new ResultSetAggregateMethodAndAccessTogether());
            execs.Add(new ResultSetAggregateTypeAndColNameAndEquivalency());
            execs.Add(new ResultSetAggregateJoin2Access());
            execs.Add(new ResultSetAggregateOuterJoin1Access());
            execs.Add(new ResultSetAggregateBatchWindow());
            execs.Add(new ResultSetAggregateBatchWindowGrouped());
            execs.Add(new ResultSetAggregateFirstLastWindowNoGroup());
            execs.Add(new ResultSetAggregateFirstLastWindowGroup());
            execs.Add(new ResultSetAggregateWindowAndSumWGroup());
            execs.Add(new ResultSetAggregateOutputRateLimiting());
            execs.Add(new ResultSetAggregateOnDelete());
            execs.Add(new ResultSetAggregateLastMaxMixedOnSelect());
            execs.Add(new ResultSetAggregateLateInitialize());
            execs.Add(new ResultSetAggregateMixedNamedWindow());
            execs.Add(new ResultSetAggregateNoParamChainedAndProperty());
            execs.Add(new ResultSetAggregateOnDemandQuery());
            return execs;
        }

        private static void TryAssertionGrouped(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "theString,firststring,firstint,laststring,lastint,allint".SplitCsv();

            env.SendEventBean(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", "E1", 10, "E1", 10, new[] {10}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", "E2", 11, "E2", 11, new[] {11}});

            env.SendEventBean(new SupportBean("E1", 12));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", "E1", 10, "E1", 12, new[] {10, 12}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 13));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", "E2", 11, "E2", 13, new[] {11, 13}});

            env.SendEventBean(new SupportBean("E2", 14));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", "E2", 11, "E2", 14, new[] {11, 13, 14}});

            env.SendEventBean(new SupportBean("E1", 15)); // push out E1/10
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", "E1", 12, "E1", 15, new[] {12, 15}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E1", 16)); // push out E2/11 -=> 2 events
            var received = env.Listener("s0").GetAndResetLastNewData();
            EPAssertionUtil.AssertPropsPerRow(
                received,
                fields,
                new[] {
                    new object[] {"E1", "E1", 12, "E1", 16, new[] {12, 15, 16}},
                    new object[] {"E2", "E2", 13, "E2", 14, new[] {13, 14}}
                });
        }

        private static void TryAssertionFirstLastIndexed(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "f0,f1,f2,f3,l0,l1,l2,l3".SplitCsv();
            env.SendEventBean(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, null, null, null, 10, null, null, null});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, 11, null, null, 11, 10, null, null});

            env.SendEventBean(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, 11, 12, null, 12, 11, 10, null});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E4", 13));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {11, 12, 13, null, 13, 12, 11, null});
        }

        private static void TryAssertionStar(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "firststar,firststarsb,laststar,laststarsb,windowstar,windowstarsb,firsteverstar,lasteverstar"
                .SplitCsv();

            object beanE1 = new SupportBean("E1", 10);
            env.SendEventBean(beanE1);
            object[] window = {beanE1};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {beanE1, beanE1, beanE1, beanE1, window, window, beanE1, beanE1});

            env.Milestone(0);

            object beanE2 = new SupportBean("E2", 20);
            env.SendEventBean(beanE2);
            window = new[] {beanE1, beanE2};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {beanE1, beanE1, beanE2, beanE2, window, window, beanE1, beanE2});

            env.Milestone(1);

            object beanE3 = new SupportBean("E3", 30);
            env.SendEventBean(beanE3);
            window = new[] {beanE2, beanE3};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new[] {beanE2, beanE2, beanE3, beanE3, window, window, beanE1, beanE3});

            env.Milestone(2);
        }

        private static void TryAssertionUngrouped(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "firststring,firstint,laststring,lastint,allint".SplitCsv();

            env.SendEventBean(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 10, "E1", 10, new[] {10}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 10, "E2", 11, new[] {10, 11}});

            env.SendEventBean(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 11, "E3", 12, new[] {11, 12}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E4", 13));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3", 12, "E4", 13, new[] {12, 13}});
        }

        private static object Split(string s)
        {
            if (s == null) {
                return new object[0];
            }

            return s.SplitCsv();
        }

        private static int[] IntArray(params int[] value)
        {
            if (value == null) {
                return new int[0];
            }

            return value;
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

        public class ResultSetAggregateFirstLastWindowGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select TheString, " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "first(IntPrimitive) as firstint, " +
                          "last(IntPrimitive) as lastint, " +
                          "window(IntPrimitive) as allint " +
                          "from SupportBean#length(5) group by TheString order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "theString,firststring,firstint,laststring,lastint,allint".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", 10, "E1", 10, new[] {10}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E2", 11, "E2", 11, new[] {11}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", 10, "E1", 12, new[] {10, 12}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 13));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E2", 11, "E2", 13, new[] {11, 13}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E2", 14));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E2", 11, "E2", 14, new[] {11, 13, 14}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E1", 15)); // push out E1/10
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", 12, "E1", 15, new[] {12, 15}});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E1", 16)); // push out E2/11 -=> 2 events
                var received = env.Listener("s0").GetAndResetLastNewData();
                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    fields,
                    new[] {
                        new object[] {"E1", "E1", 12, "E1", 16, new[] {12, 15, 16}},
                        new object[] {"E2", "E2", 13, "E2", 14, new[] {13, 14}}
                    });

                env.UndeployAll();
            }
        }

        public class ResultSetAggregateFirstLastWindowNoGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "first(IntPrimitive) as firstint, " +
                          "last(IntPrimitive) as lastint, " +
                          "window(IntPrimitive) as allint " +
                          "from SupportBean.win:length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "firststring,firstint,laststring,lastint,allint".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, "E1", 10, new[] {10}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, "E2", 11, new[] {10, 11}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 11, "E3", 12, new[] {11, 12}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 13));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 12, "E4", 13, new[] {12, 13}});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateNoParamChainedAndProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select first().property as val0, first().myMethod() as val1, window() as val2 from SupportEventPropertyWithMethod#lastevent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportEventPropertyWithMethod("p1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "val0,val1".SplitCsv(),
                    new object[] {"p1", "abc"});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateLastMaxMixedOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowOne#keepall as SupportBean;\n" +
                          "insert into MyWindowOne select * from SupportBean(TheString like 'A%');\n" +
                          "@Name('s0') on SupportBean(TheString like 'B%') select last(mw.IntPrimitive) as li, max(mw.IntPrimitive) as mi from MyWindowOne mw;";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "li,mi".SplitCsv();

                env.SendEventBean(new SupportBean("A1", 10));
                env.SendEventBean(new SupportBean("B1", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 10});

                env.Milestone(0);

                for (var i = 11; i < 20; i++) {
                    env.SendEventBean(new SupportBean("A1", i));
                    env.SendEventBean(new SupportBean("Bx", -1));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {i, i});
                }

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("B1", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 19});

                env.SendEventBean(new SupportBean("A1", 2));
                env.SendEventBean(new SupportBean("B1", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, 19});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregatePrevNthIndexedFirstLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
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
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, null, null, 10, null, null, 10, null, null});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11, 10, null, 11, 10, null, 11, 10, null});

                env.SendEventBean(new SupportBean("E3", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12, 11, 10, 12, 11, 10, 12, 11, 10});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 13));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {13, 12, 11, 13, 12, 11, 13, 12, 11});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateFirstLastIndexed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@Name('s0') select " +
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
                env.CompileDeploy("@Name('var') create variable int indexvar = 2", path);
                epl = "@Name('s0') select first(IntPrimitive, indexvar) as f0 from SupportBean#keepall";
                env.CompileDeploy(epl, path).AddListener("s0");

                var fields = "f0".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E1", 11));
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E1", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12});

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "indexvar", 0);
                env.SendEventBean(new SupportBean("E1", 13));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});
                env.UndeployAll();

                // test as part of function
                env.CompileDeploy("select Math.abs(last(IntPrimitive)) from SupportBean").UndeployAll();
            }
        }

        internal class ResultSetAggregateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select window(distinct IntPrimitive) from SupportBean",
                    "Incorrect syntax near '(' ('distinct' is a reserved keyword) at line 1 column 13 near reserved keyword 'distinct' [");

                TryInvalidCompile(
                    env,
                    "select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean#lastevent sa, SupportBean#lastevent sb",
                    "Failed to validate select-clause expression 'window(sa.IntPrimitive+sb.IntPrimitive)': The 'window' aggregation function requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead [select window(sa.IntPrimitive + sb.IntPrimitive) from SupportBean#lastevent sa, SupportBean#lastevent sb]");

                TryInvalidCompile(
                    env,
                    "select last(*) from SupportBean#lastevent sa, SupportBean#lastevent sb",
                    "Failed to validate select-clause expression 'last(*)': The 'last' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select last(*) from SupportBean#lastevent sa, SupportBean#lastevent sb]");

                TryInvalidCompile(
                    env,
                    "select TheString, (select first(*) from SupportBean#lastevent sa) from SupportBean#lastevent sb",
                    "Failed to plan subquery number 1 querying SupportBean: Failed to validate select-clause expression 'first(*)': The 'first' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead [select TheString, (select first(*) from SupportBean#lastevent sa) from SupportBean#lastevent sb]");

                TryInvalidCompile(
                    env,
                    "select window(x.*) from SupportBean#lastevent",
                    "Failed to validate select-clause expression 'window(x.*)': Stream by name 'x' could not be found among all streams [select window(x.*) from SupportBean#lastevent]");

                TryInvalidCompile(
                    env,
                    "select window(*) from SupportBean x",
                    "Failed to validate select-clause expression 'window(*)': The 'window' aggregation function requires that the aggregated events provIde a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(*) from SupportBean x]");
                TryInvalidCompile(
                    env,
                    "select window(x.*) from SupportBean x",
                    "Failed to validate select-clause expression 'window(x.*)': The 'window' aggregation function requires that the aggregated events provIde a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.*) from SupportBean x]");
                TryInvalidCompile(
                    env,
                    "select window(x.IntPrimitive) from SupportBean x",
                    "Failed to validate select-clause expression 'window(x.IntPrimitive)': The 'window' aggregation function requires that the aggregated events provIde a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead [select window(x.IntPrimitive) from SupportBean x]");

                TryInvalidCompile(
                    env,
                    "select window(x.IntPrimitive, 10) from SupportBean#keepall x",
                    "Failed to validate select-clause expression 'window(x.IntPrimitive,10)': The 'window' aggregation function does not accept an index expression; Use 'first' or 'last' instead [");

                TryInvalidCompile(
                    env,
                    "select first(x.*, 10d) from SupportBean#lastevent as x",
                    "Failed to validate select-clause expression 'first(x.*,10.0)': The 'first' aggregation function requires an index expression that returns an integer value [select first(x.*, 10d) from SupportBean#lastevent as x]");
            }
        }

        internal class ResultSetAggregateSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select Id, (select window(sb.*) from SupportBean#length(2) as sb) as w from SupportBean_A";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "id,w".SplitCsv();

                env.SendEventBean(new SupportBean_A("A1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A1", null});

                env.Milestone(0);

                var beanOne = SendEvent(env, "E1", 0, 1);
                env.SendEventBean(new SupportBean_A("A2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "A2",
                        new object[] {beanOne}
                    });

                env.Milestone(1);

                var beanTwo = SendEvent(env, "E2", 0, 1);
                env.SendEventBean(new SupportBean_A("A3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "A3",
                        new object[] {beanOne, beanTwo}
                    });

                env.Milestone(2);

                var beanThree = SendEvent(env, "E2", 0, 1);
                env.SendEventBean(new SupportBean_A("A4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "A4",
                        new object[] {beanTwo, beanThree}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMethodAndAccessTogether : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl =
                    "@Name('s0') select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#length(2) as sa";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "si,wi".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, IntArray(1)});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, IntArray(1, 2)});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, IntArray(2, 3)});

                env.MilestoneInc(milestone);

                env.UndeployAll();

                epl =
                    "@Name('s0') select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#keepall as sa group by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, IntArray(1)});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, IntArray(2)});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, IntArray(2, 3)});

                env.SendEventBean(new SupportBean("E1", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, IntArray(1, 4)});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateOutputRateLimiting : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select sum(IntPrimitive) as si, window(sa.IntPrimitive) as wi from SupportBean#keepall as sa output every 2 events";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "si,wi".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {1, IntArray(1)},
                        new object[] {3, IntArray(1, 2)}
                    });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 3));
                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {6, IntArray(1, 2, 3)},
                        new object[] {10, IntArray(1, 2, 3, 4)}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateTypeAndColNameAndEquivalency : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "first(sa.DoublePrimitive + sa.IntPrimitive), " +
                          "first(sa.IntPrimitive), " +
                          "window(sa.*), " +
                          "last(*) from SupportBean#length(2) as sa";
                env.CompileDeploy(epl).AddListener("s0");

                object[][] rows = {
                    new object[] {"first(sa.DoublePrimitive+sa.IntPrimitive)", typeof(double?)},
                    new object[] {"first(sa.IntPrimitive)", typeof(int?)},
                    new object[] {"window(sa.*)", typeof(SupportBean[])},
                    new object[] {"last(*)", typeof(SupportBean)}
                };
                for (var i = 0; i < rows.Length; i++) {
                    var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
                    Assert.AreEqual(rows[i][0], prop.PropertyName);
                    Assert.AreEqual(rows[i][1], prop.PropertyType);
                }

                env.UndeployAll();

                epl = "@Name('s0') select " +
                      "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
                      "first(sa.IntPrimitive) as f2, " +
                      "window(sa.*) as w1, " +
                      "last(*) as l1 " +
                      "from SupportBean#length(2) as sa";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionType(env, false);

                env.UndeployAll();

                epl = "@Name('s0') select " +
                      "first(sa.DoublePrimitive + sa.IntPrimitive) as f1, " +
                      "first(sa.IntPrimitive) as f2, " +
                      "window(sa.*) as w1, " +
                      "last(*) as l1 " +
                      "from SupportBean#length(2) as sa " +
                      "having SupportStaticMethodLib.alwaysTrue({first(sa.DoublePrimitive + sa.IntPrimitive), " +
                      "first(sa.IntPrimitive), window(sa.*), last(*)})";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionType(env, true);

                env.UndeployAll();
            }

            private void TryAssertionType(
                RegressionEnvironment env,
                bool isCheckStatic)
            {
                var fields = "f1,f2,w1,l1".SplitCsv();
                SupportStaticMethodLib.Invocations.Clear();

                var beanOne = SendEvent(env, "E1", 10d, 100);
                object[] expected = {
                    110d, 100,
                    new object[] {beanOne}, beanOne
                };
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);
                if (isCheckStatic) {
                    var parameters = SupportStaticMethodLib.Invocations[0];
                    SupportStaticMethodLib.Invocations.Clear();
                    EPAssertionUtil.AssertEqualsExactOrder(expected, parameters);
                }
            }
        }

        internal class ResultSetAggregateJoin2Access : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "sa.Id as ast, " +
                          "sb.Id as bst, " +
                          "first(sa.Id) as fas, " +
                          "window(sa.Id) as was, " +
                          "last(sa.Id) as las, " +
                          "first(sb.Id) as fbs, " +
                          "window(sb.Id) as wbs, " +
                          "last(sb.Id) as lbs " +
                          "from SupportBean_A#length(2) as sa, SupportBean_B#length(2) as sb " +
                          "order by ast, bst";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "ast,bst,fas,was,las,fbs,wbs,lbs".SplitCsv();

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean_B("B1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"A1", "B1", "A1", Split("A1"), "A1", "B1", Split("B1"), "B1"});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("A2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new[] {"A2", "B1", "A1", Split("A1,A2"), "A2", "B1", Split("B1"), "B1"}
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("A3"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new[] {"A3", "B1", "A2", Split("A2,A3"), "A3", "B1", Split("B1"), "B1"}
                    });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_B("B2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new[] {"A2", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2"},
                        new[] {"A3", "B2", "A2", Split("A2,A3"), "A3", "B1", Split("B1,B2"), "B2"}
                    });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_B("B3"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new[] {"A2", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3"},
                        new[] {"A3", "B3", "A2", Split("A2,A3"), "A3", "B2", Split("B2,B3"), "B3"}
                    });

                env.Milestone(4);

                env.SendEventBean(new SupportBean_A("A4"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new[] {"A4", "B2", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3"},
                        new[] {"A4", "B3", "A3", Split("A3,A4"), "A4", "B2", Split("B2,B3"), "B3"}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateOuterJoin1Access : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "sa.Id as aId, " +
                          "sb.Id as bId, " +
                          "first(sb.P10) as fb, " +
                          "window(sb.P10) as wb, " +
                          "last(sb.P10) as lb " +
                          "from SupportBean_S0#keepall as sa " +
                          "left outer join " +
                          "SupportBean_S1#keepall as sb " +
                          "on sa.Id = sb.Id";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "aId,bId,fb,wb,lb".SplitCsv();

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, null, null, null, null});

                env.SendEventBean(new SupportBean_S1(1, "A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {1, 1, "A", Split("A"), "A"});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(2, "B"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(2, "A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {2, 2, "A", Split("A,B"), "B"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(3, "C"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(3, "C"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {3, 3, "A", Split("A,B,C"), "C"});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream " +
                          "first(TheString) as fs, " +
                          "window(TheString) as ws, " +
                          "last(TheString) as ls " +
                          "from SupportBean#length_batch(2) as sb";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "fs,ws,ls".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {null, null, null});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new[] {"E1", Split("E1,E2"), "E2"});
                env.Listener("s0").Reset();

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 0));
                env.SendEventBean(new SupportBean("E4", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new[] {"E1", Split("E1,E2"), "E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new[] {"E3", Split("E3,E4"), "E4"});
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E5", 0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E6", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new[] {"E3", Split("E3,E4"), "E4"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new[] {"E5", Split("E5,E6"), "E6"});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateBatchWindowGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "theString, " +
                          "first(IntPrimitive) as fi, " +
                          "window(IntPrimitive) as wi, " +
                          "last(IntPrimitive) as li " +
                          "from SupportBean#length_batch(6) as sb group by TheString order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "theString,fi,wi,li".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 11));
                env.SendEventBean(new SupportBean("E3", 30));

                env.SendEventBean(new SupportBean("E3", 31));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("E1", 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, IntArray(10, 11, 12), 12},
                        new object[] {"E2", 20, IntArray(20), 20},
                        new object[] {"E3", 30, IntArray(30, 31), 31}
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 13));
                env.SendEventBean(new SupportBean("E1", 14));
                env.SendEventBean(new SupportBean("E1", 15));
                env.SendEventBean(new SupportBean("E1", 16));
                env.SendEventBean(new SupportBean("E1", 17));
                env.SendEventBean(new SupportBean("E1", 18));
                var result = env.Listener("s0").GetAndResetLastNewData();
                EPAssertionUtil.AssertPropsPerRow(
                    result,
                    fields,
                    new[] {
                        new object[] {"E1", 13, IntArray(13, 14, 15, 16, 17, 18), 18},
                        new object[] {"E2", null, null, null},
                        new object[] {"E3", null, null, null}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateLateInitialize : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyWindowTwo#keepall as select * from SupportBean;\n" +
                          "insert into MyWindowTwo select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                var fields = "firststring,windowstring,laststring".SplitCsv();
                epl = "@Name('s0') select " +
                      "first(TheString) as firststring, " +
                      "window(TheString) as windowstring, " +
                      "last(TheString) as laststring " +
                      "from MyWindowTwo";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E3", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E1", Split("E1,E2,E3"), "E3"});

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
                          "@Name('s0') select " +
                          "first(TheString) as firststring, " +
                          "window(TheString) as windowstring, " +
                          "last(TheString) as laststring " +
                          "from MyWindowThree";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E1", Split("E1"), "E1"});

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E1", Split("E1,E2"), "E2"});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E1", Split("E1,E2,E3"), "E3"});

                env.SendEventBean(new SupportBean_A("E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E1", Split("E1,E3"), "E3"});

                env.SendEventBean(new SupportBean_A("E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E1", Split("E1"), "E1"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_A("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null});

                env.SendEventBean(new SupportBean("E4", 40));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E4", Split("E4"), "E4"});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E5", 50));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E4", Split("E4,E5"), "E5"});

                env.SendEventBean(new SupportBean_A("E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E5", Split("E5"), "E5"});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E6", 60));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {"E5", Split("E5,E6"), "E6"});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateOnDemandQuery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyWindowFour#keepall as select * from SupportBean;\n" +
                          "insert into MyWindowFour select * from SupportBean;";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E3", 30));
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
                    new[] {new object[] {10, IntArray(10, 20, 30, 31, 11, 12), 12}});

                env.SendEventBean(new SupportBean("E1", 13));
                EPAssertionUtil.AssertPropsPerRow(
                    q.Execute().Array,
                    "f,w,l".SplitCsv(),
                    new[] {new object[] {10, IntArray(10, 20, 30, 31, 11, 12, 13), 13}});

                qc = env.CompileFAF(
                    "select TheString as s, first(IntPrimitive) as f, window(IntPrimitive) as w, last(IntPrimitive) as l from MyWindowFour as s group by TheString order by TheString asc",
                    path);
                q = env.Runtime.FireAndForgetService.PrepareQuery(qc);
                object[][] expected = {
                    new object[] {"E1", 10, IntArray(10, 11, 12, 13), 13},
                    new object[] {"E2", 20, IntArray(20), 20},
                    new object[] {"E3", 30, IntArray(30, 31), 31}
                };
                EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".SplitCsv(), expected);
                EPAssertionUtil.AssertPropsPerRow(q.Execute().Array, "s,f,w,l".SplitCsv(), expected);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@Name('s0') select " +
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

                var props = env.Statement("s0").EventType.PropertyDescriptors;
                for (var i = 0; i < props.Length; i++) {
                    Assert.AreEqual(
                        i == 4 || i == 5 ? typeof(SupportBean[]) : typeof(SupportBean),
                        props[i].PropertyType);
                }

                TryAssertionStar(env, milestone);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionStar(env, milestone);

                env.UndeployAll();
            }
        }

        public class ResultSetAggregateUnboundedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select first(TheString) as c0, last(TheString) as c1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "c0,c1".SplitCsv();

                env.Milestone(0);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1"});

                env.Milestone(1);

                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E2"});

                env.Milestone(2);

                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E3"});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateUnboundedStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
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
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", beanOne, beanOne, "E1", beanOne, beanOne});

                env.Milestone(0);

                var beanTwo = SendEvent(env, "E2", 2d, 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", beanOne, beanOne, "E2", beanTwo, beanTwo});

                env.Milestone(1);

                var beanThree = SendEvent(env, "E3", 3d, 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", beanOne, beanOne, "E3", beanThree, beanThree});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateWindowedUnGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@Name('s0') select " +
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
                epl = "@Name('s0') select window(IntBoxed).take(10) from SupportBean#length(2)";
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

                var epl = "@Name('s0') select " +
                          "theString, " +
                          "first(TheString) as firststring, " +
                          "last(TheString) as laststring, " +
                          "first(IntPrimitive) as firstint, " +
                          "last(IntPrimitive) as lastint, " +
                          "window(IntPrimitive) as allint " +
                          "from SupportBean#length(5) " +
                          "group by TheString order by TheString";
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
                var eplFirstLast = "@Name('s0') select " +
                                   "last(IntPrimitive), " +
                                   "last(IntPrimitive,1), " +
                                   "first(IntPrimitive), " +
                                   "first(IntPrimitive,1) " +
                                   "from SupportBean#length(3)";
                env.EplToModelCompileDeploy(eplFirstLast);
                env.UndeployAll();
            }
        }

        public class ResultSetAggregateMixedNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "create window ABCWin.win:keepall() as SupportBean;\n" +
                          "insert into ABCWin select * from SupportBean;\n" +
                          "on SupportBean_S0 delete from ABCWin where IntPrimitive = Id;\n" +
                          "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1, window(IntPrimitive) as c2 from ABCWin group by TheString;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, new[] {10}});

                env.Milestone(1);

                SendSupportBean(env, "E2", 100);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 100, new[] {100}});

                env.Milestone(2);

                SendSupportBean_S0(env, 100); // delete E2 group
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", null, null});

                env.Milestone(3);

                SendSupportBean(env, "E1", 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10 + 11, new[] {10, 11}});

                env.Milestone(4);

                SendSupportBean_S0(env, 10); // delete from E1 group
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 11, new[] {11}});

                env.Milestone(5);

                env.Milestone(6); // no change

                SendSupportBean(env, "E2", 101);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 101, new[] {101}});
                SendSupportBean(env, "E2", 102);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 101 + 102, new[] {101, 102}});
                SendSupportBean(env, "E1", 12);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 11 + 12, new[] {11, 12}});

                env.UndeployAll();
            }
        }

        public class ResultSetAggregateWindowAndSumWGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();

                var epl = "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1," +
                          "window(IntPrimitive*LongPrimitive) as c2 from SupportBean#length(3) group by TheString order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 10, 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, new long[] {5 * 10}});

                env.Milestone(1);

                SendSupportBean(env, "E2", 100, 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 100, new long[] {20 * 100}});

                env.Milestone(2);

                SendSupportBean(env, "E1", 15, 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10 + 15, new long[] {5 * 10, 2 * 15}});

                env.Milestone(3);

                SendSupportBean(env, "E1", 18, 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 15 + 18, new long[] {2 * 15, 3 * 18}});

                env.Milestone(4);

                SendSupportBean(env, "E1", 19, 4); // pushed out E2
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 15 + 18 + 19, new long[] {2 * 15, 3 * 18, 4 * 19}},
                        new object[] {"E2", null, null}
                    });

                env.Milestone(5);

                env.Milestone(6);

                SendSupportBean(env, "E1", 17, -1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 18 + 19 + 17, new long[] {3 * 18, 4 * 19, -1 * 17}});
                SendSupportBean(env, "E2", 1, 1000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 19 + 17, new long[] {4 * 19, -1 * 17}},
                        new object[] {"E2", 1, new long[] {1 * 1000}}
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace