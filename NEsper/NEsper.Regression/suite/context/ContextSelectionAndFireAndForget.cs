///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextSelectionAndFireAndForget
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextSelectionAndFireAndForgetInvalid());
            execs.Add(new ContextSelectionIterateStatement());
            execs.Add(new ContextSelectionAndFireAndForgetNamedWindowQuery());
            execs.Add(new ContextSelectionFAFNestedNamedWindowQuery());
            return execs;
        }

        private static void RunQueryAll(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string fields,
            object[][] expected,
            int numStreams)
        {
            var selectors = new ContextPartitionSelector[numStreams];
            for (var i = 0; i < numStreams; i++) {
                selectors[i] = ContextPartitionSelectorAll.INSTANCE;
            }

            RunQuery(env, path, epl, fields, expected, selectors);

            // run same query without selector
            var compiled = env.CompileFAF(epl, path);
            var result = env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.SplitCsv(), expected);
        }

        private static void RunQuery(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string fields,
            object[][] expected,
            ContextPartitionSelector[] selectors)
        {
            // try FAF without prepare
            var compiled = env.CompileFAF(epl, path);
            var result = env.Runtime.FireAndForgetService.ExecuteQuery(compiled, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.SplitCsv(), expected);

            // test unparameterized prepare and execute
            var preparedQuery = env.Runtime.FireAndForgetService.PrepareQuery(compiled);
            var resultPrepared = preparedQuery.Execute(selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPrepared.Array, fields.SplitCsv(), expected);

            // test unparameterized prepare and execute
            var preparedParameterizedQuery = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);
            var resultPreparedParameterized =
                env.Runtime.FireAndForgetService.ExecuteQuery(preparedParameterizedQuery, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPreparedParameterized.Array, fields.SplitCsv(), expected);

            // test SODA prepare and execute
            var model = env.EplToModel(epl);
            var compiledFromModel = env.CompileFAF(model, path);
            var preparedQueryModel = env.Runtime.FireAndForgetService.PrepareQuery(compiledFromModel);
            var resultPreparedModel = preparedQueryModel.Execute(selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPreparedModel.Array, fields.SplitCsv(), expected);

            // test model query
            result = env.Runtime.FireAndForgetService.ExecuteQuery(compiledFromModel, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.SplitCsv(), expected);
        }

        private static void TryInvalidRuntimeQuery(
            RegressionEnvironment env,
            RegressionPath path,
            ContextPartitionSelector[] selectors,
            string epl,
            string expected)
        {
            var faf = env.CompileFAF(epl, path);
            try {
                env.Runtime.FireAndForgetService.ExecuteQuery(faf, selectors);
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual(ex.Message, expected);
            }
        }

        private static void TryInvalidCompileQuery(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string expected)
        {
            SupportMessageAssertUtil.TryInvalidFAFCompile(env, path, epl, expected);
        }

        internal class ContextSelectionAndFireAndForgetInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context SegmentedSB as partition by TheString from SupportBean", path);
                env.CompileDeploy("create context SegmentedS0 as partition by P00 from SupportBean_S0", path);
                env.CompileDeploy("context SegmentedSB create window WinSB#keepall as SupportBean", path);
                env.CompileDeploy("context SegmentedS0 create window WinS0#keepall as SupportBean_S0", path);
                env.CompileDeploy("create window WinS1#keepall as SupportBean_S1", path);

                // when a context is declared, it must be the same context that applies to all named windows
                TryInvalidCompileQuery(
                    env,
                    path,
                    "context SegmentedSB select * from WinSB, WinS0",
                    "Joins in runtime queries for context partitions are not supported [context SegmentedSB select * from WinSB, WinS0]");

                // test join
                env.CompileDeploy("create context PartitionedByString partition by TheString from SupportBean", path);
                env.CompileDeploy("context PartitionedByString create window MyWindowOne#keepall as SupportBean", path);

                env.CompileDeploy("create context PartitionedByP00 partition by P00 from SupportBean_S0", path);
                env.CompileDeploy("context PartitionedByP00 create window MyWindowTwo#keepall as SupportBean_S0", path);

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 11));
                env.SendEventBean(new SupportBean_S0(1, "G2"));
                env.SendEventBean(new SupportBean_S0(2, "G1"));

                TryInvalidCompileQuery(
                    env,
                    path,
                    "select mw1.IntPrimitive as c1, mw2.Id as c2 from MyWindowOne mw1, MyWindowTwo mw2 where mw1.TheString = mw2.P00",
                    "Joins against named windows that are under context are not supported");

                env.UndeployAll();
            }
        }

        internal class ContextSelectionAndFireAndForgetNamedWindowQuery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context PartitionedByString partition by TheString from SupportBean", path);
                env.CompileDeploy("context PartitionedByString create window MyWindow#keepall as SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 21));

                // test no context
                RunQueryAll(
                    env,
                    path,
                    "select sum(IntPrimitive) as c1 from MyWindow",
                    "c1",
                    new[] {new object[] {51}},
                    1);
                RunQueryAll(
                    env,
                    path,
                    "select sum(IntPrimitive) as c1 from MyWindow where IntPrimitive > 15",
                    "c1",
                    new[] {new object[] {41}},
                    1);
                RunQuery(
                    env,
                    path,
                    "select sum(IntPrimitive) as c1 from MyWindow",
                    "c1",
                    new[] {new object[] {41}},
                    new ContextPartitionSelector[]
                        {new SupportSelectorPartitioned(Collections.SingletonList(new object[] {"E2"}))});
                RunQuery(
                    env,
                    path,
                    "select sum(IntPrimitive) as c1 from MyWindow",
                    "c1",
                    new[] {new object[] {41}},
                    new ContextPartitionSelector[] {new SupportSelectorById(Collections.SingletonList(1))});

                // test with context props
                RunQueryAll(
                    env,
                    path,
                    "context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow",
                    "c0,c1",
                    new[] {new object[] {"E1", 10}, new object[] {"E2", 20}, new object[] {"E2", 21}},
                    1);
                RunQueryAll(
                    env,
                    path,
                    "context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow where IntPrimitive > 15",
                    "c0,c1",
                    new[] {new object[] {"E2", 20}, new object[] {"E2", 21}},
                    1);

                // test targeted context partition
                RunQuery(
                    env,
                    path,
                    "context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow where IntPrimitive > 15",
                    "c0,c1",
                    new[] {new object[] {"E2", 20}, new object[] {"E2", 21}},
                    new[] {new SupportSelectorPartitioned(Collections.SingletonList(new object[] {"E2"}))});

                var compiled = env.CompileFAF("context PartitionedByString select * from MyWindow", path);
                try {
                    env.Runtime.FireAndForgetService.ExecuteQuery(
                        compiled,
                        new ContextPartitionSelector[] {
                            new ProxyContextPartitionSelectorCategory {
                                ProcLabels = () => null
                            }
                        });
                }
                catch (InvalidContextPartitionSelector ex) {
                    Assert.IsTrue(
                        ex.Message.StartsWith(
                            "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com"),
                        "message: " + ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class ContextSelectionFAFNestedNamedWindowQuery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as S0 terminated by SupportBean_S1(Id=S0.Id), " +
                    "context BCtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean",
                    path);
                env.CompileDeploy("context NestedContext create window MyWindow#keepall as SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                env.SendEventBean(new SupportBean_S0(1, "S0_1"));
                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(2, "S0_2"));
                env.SendEventBean(new SupportBean("E2", -1));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 5));
                env.SendEventBean(new SupportBean("E1", 2));

                RunQueryAll(
                    env,
                    path,
                    "select TheString as c1, sum(IntPrimitive) as c2 from MyWindow group by TheString",
                    "c1,c2",
                    new[] {new object[] {"E1", 5}, new object[] {"E2", -2}, new object[] {"E3", 10}},
                    1);
                RunQuery(
                    env,
                    path,
                    "select TheString as c1, sum(IntPrimitive) as c2 from MyWindow group by TheString",
                    "c1,c2",
                    new[] {new object[] {"E1", 3}, new object[] {"E3", 5}},
                    new ContextPartitionSelector[] {new SupportSelectorById(Collections.SingletonSet(2))});

                RunQuery(
                    env,
                    path,
                    "context NestedContext select context.ACtx.S0.P00 as c1, context.BCtx.label as c2, TheString as c3, sum(IntPrimitive) as c4 from MyWindow group by TheString",
                    "c1,c2,c3,c4",
                    new[] {new object[] {"S0_1", "grp3", "E1", 3}, new object[] {"S0_1", "grp3", "E3", 5}},
                    new ContextPartitionSelector[] {new SupportSelectorById(Collections.SingletonSet(2))});

                env.UndeployAll();
            }
        }

        internal class ContextSelectionIterateStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var epl = "create context PartitionedByString partition by TheString from SupportBean;\n" +
                          "@name('s0') context PartitionedByString select context.key1 as c0, sum(IntPrimitive) as c1 from SupportBean#length(5);\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E2", 21));

                env.Milestone(0);

                object[][] expectedAll = {new object[] {"E1", 10}, new object[] {"E2", 41}};
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expectedAll);

                // test iterator ALL
                ContextPartitionSelector selector = ContextPartitionSelectorAll.INSTANCE;
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(selector),
                    env.Statement("s0").GetSafeEnumerator(selector),
                    fields,
                    expectedAll);

                // test iterator by context partition id
                selector = new SupportSelectorById(new HashSet<int>(Arrays.AsList(0, 1, 2)));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(selector),
                    env.Statement("s0").GetSafeEnumerator(selector),
                    fields,
                    expectedAll);

                selector = new SupportSelectorById(new HashSet<int>(Arrays.AsList(1)));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(selector),
                    env.Statement("s0").GetSafeEnumerator(selector),
                    fields,
                    new[] {new object[] {"E2", 41}});

                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(new SupportSelectorById(Collections.GetEmptySet<int>()))
                        .MoveNext());
                Assert.IsFalse(env.Statement("s0").GetEnumerator(new SupportSelectorById(null)).MoveNext());

                try {
                    env.Statement("s0").GetEnumerator(null);
                    Assert.Fail();
                }
                catch (ArgumentException ex) {
                    Assert.AreEqual(ex.Message, "No selector provided");
                }

                try {
                    env.Statement("s0").GetSafeEnumerator(null);
                    Assert.Fail();
                }
                catch (ArgumentException ex) {
                    Assert.AreEqual(ex.Message, "No selector provided");
                }

                env.CompileDeploy("@name('s2') select * from SupportBean");
                try {
                    env.Statement("s2").GetEnumerator(null);
                    Assert.Fail();
                }
                catch (UnsupportedOperationException ex) {
                    Assert.AreEqual(
                        ex.Message,
                        "Enumerator with context selector is only supported for statements under context");
                }

                try {
                    env.Statement("s2").GetSafeEnumerator(null);
                    Assert.Fail();
                }
                catch (UnsupportedOperationException ex) {
                    Assert.AreEqual(
                        ex.Message,
                        "Enumerator with context selector is only supported for statements under context");
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace