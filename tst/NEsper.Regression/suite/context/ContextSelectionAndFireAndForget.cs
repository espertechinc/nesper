///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

// fail
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
	public class ContextSelectionAndFireAndForget {

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ContextSelectionAndFireAndForgetInvalid());
	        execs.Add(new ContextSelectionIterateStatement());
	        execs.Add(new ContextSelectionAndFireAndForgetNamedWindowQuery());
	        execs.Add(new ContextSelectionFAFNestedNamedWindowQuery());
	        return execs;
	    }

	    private class ContextSelectionAndFireAndForgetInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var path = new RegressionPath();
	            env.CompileDeploy("@public create context SegmentedSB as partition by theString from SupportBean", path);
	            env.CompileDeploy("@public create context SegmentedS0 as partition by p00 from SupportBean_S0", path);
	            env.CompileDeploy("@public context SegmentedSB create window WinSB#keepall as SupportBean", path);
	            env.CompileDeploy("@public context SegmentedS0 create window WinS0#keepall as SupportBean_S0", path);
	            env.CompileDeploy("@public create window WinS1#keepall as SupportBean_S1", path);

	            // when a context is declared, it must be the same context that applies to all named windows
	            TryInvalidCompileQuery(env, path, "context SegmentedSB select * from WinSB, WinS0",
	                "Joins in runtime queries for context partitions are not supported [context SegmentedSB select * from WinSB, WinS0]");

	            // test join
	            env.CompileDeploy("@public create context PartitionedByString partition by theString from SupportBean", path);
	            env.CompileDeploy("@public context PartitionedByString create window MyWindowOne#keepall as SupportBean", path);

	            env.CompileDeploy("@public create context PartitionedByP00 partition by p00 from SupportBean_S0", path);
	            env.CompileDeploy("@public context PartitionedByP00 create window MyWindowTwo#keepall as SupportBean_S0", path);

	            env.SendEventBean(new SupportBean("G1", 10));
	            env.SendEventBean(new SupportBean("G2", 11));
	            env.SendEventBean(new SupportBean_S0(1, "G2"));
	            env.SendEventBean(new SupportBean_S0(2, "G1"));

	            TryInvalidCompileQuery(env, path, "select mw1.intPrimitive as c1, mw2.id as c2 from MyWindowOne mw1, MyWindowTwo mw2 where mw1.theString = mw2.p00",
	                "Joins against named windows that are under context are not supported");

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET, RegressionFlag.INVALIDITY);
	        }
	    }

	    private class ContextSelectionAndFireAndForgetNamedWindowQuery : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var path = new RegressionPath();
	            env.CompileDeploy("@public create context PartitionedByString partition by theString from SupportBean", path);
	            env.CompileDeploy("@public context PartitionedByString create window MyWindow#keepall as SupportBean", path);
	            env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.SendEventBean(new SupportBean("E2", 20));

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean("E2", 21));

	            // test no context
	            RunQueryAll(env, path, "select sum(intPrimitive) as c1 from MyWindow", "c1", new object[][]{new object[] {51}}, 1);
	            RunQueryAll(env, path, "select sum(intPrimitive) as c1 from MyWindow where intPrimitive > 15", "c1", new object[][]{new object[] {41}}, 1);
	            RunQuery(env, path, "select sum(intPrimitive) as c1 from MyWindow", "c1", new object[][]{new object[] {41}}, new ContextPartitionSelector[]{new SupportSelectorPartitioned(Collections.SingletonList(new object[]{"E2"}))});
	            RunQuery(env, path, "select sum(intPrimitive) as c1 from MyWindow", "c1", new object[][]{new object[] {41}}, new ContextPartitionSelector[]{new SupportSelectorById(Collections.SingletonSet(1))});

	            // test with context props
	            RunQueryAll(env, path, "context PartitionedByString select context.key1 as c0, intPrimitive as c1 from MyWindow",
	                "c0,c1", new object[][]{new object[] {"E1", 10}, new object[] {"E2", 20}, new object[] {"E2", 21}}, 1);
	            RunQueryAll(env, path, "context PartitionedByString select context.key1 as c0, intPrimitive as c1 from MyWindow where intPrimitive > 15",
	                "c0,c1", new object[][]{new object[] {"E2", 20}, new object[] {"E2", 21}}, 1);

	            // test targeted context partition
	            RunQuery(env, path, "context PartitionedByString select context.key1 as c0, intPrimitive as c1 from MyWindow where intPrimitive > 15",
	                "c0,c1", new object[][]{new object[] {"E2", 20}, new object[] {"E2", 21}}, new SupportSelectorPartitioned[]{new SupportSelectorPartitioned(Collections.SingletonList(new object[]{"E2"}))});

	            var compiled = env.CompileFAF("context PartitionedByString select * from MyWindow", path);
	            try {
		            env.Runtime.FireAndForgetService.ExecuteQuery(
			            compiled,
			            new ContextPartitionSelector[] {
				            new ProxyContextPartitionSelectorCategory(() => null)
			            });
	            } catch (InvalidContextPartitionSelector ex) {
		            Assert.IsTrue(
			            ex.Message.StartsWith(
				            "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com"),
			            "message: " + ex.Message);
	            }

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    private class ContextSelectionFAFNestedNamedWindowQuery : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var path = new RegressionPath();
	            env.CompileDeploy("@public create context NestedContext " +
	                "context ACtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id=s0.id), " +
	                "context BCtx group by intPrimitive < 0 as grp1, group by intPrimitive = 0 as grp2, group by intPrimitive > 0 as grp3 from SupportBean", path);
	            env.CompileDeploy("@public context NestedContext create window MyWindow#keepall as SupportBean", path);
	            env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

	            env.SendEventBean(new SupportBean_S0(1, "S0_1"));
	            env.SendEventBean(new SupportBean("E1", 1));

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean_S0(2, "S0_2"));
	            env.SendEventBean(new SupportBean("E2", -1));

	            env.Milestone(1);

	            env.SendEventBean(new SupportBean("E3", 5));
	            env.SendEventBean(new SupportBean("E1", 2));

	            RunQueryAll(env, path, "select theString as c1, sum(intPrimitive) as c2 from MyWindow group by theString", "c1,c2", new object[][]{new object[] {"E1", 5}, new object[] {"E2", -2}, new object[] {"E3", 10}}, 1);
	            RunQuery(env, path, "select theString as c1, sum(intPrimitive) as c2 from MyWindow group by theString", "c1,c2", new object[][]{new object[] {"E1", 3}, new object[] {"E3", 5}},
	                new ContextPartitionSelector[]{new SupportSelectorById(Collections.SingletonSet(2))});

	            RunQuery(env, path, "context NestedContext select context.ACtx.s0.p00 as c1, context.BCtx.label as c2, theString as c3, sum(intPrimitive) as c4 from MyWindow group by theString", "c1,c2,c3,c4", new object[][]{new object[] {"S0_1", "grp3", "E1", 3}, new object[] {"S0_1", "grp3", "E3", 5}},
	                new ContextPartitionSelector[]{new SupportSelectorById(Collections.SingletonSet(2))});

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    private class ContextSelectionIterateStatement : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1".SplitCsv();
	            var epl = "create context PartitionedByString partition by theString from SupportBean;\n" +
	                      "@name('s0') context PartitionedByString select context.key1 as c0, sum(intPrimitive) as c1 from SupportBean#length(5);\n";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.SendEventBean(new SupportBean("E2", 20));
	            env.SendEventBean(new SupportBean("E2", 21));

	            env.Milestone(0);

	            var expectedAll = new object[][]{new object[] {"E1", 10}, new object[] {"E2", 41}};
	            env.AssertPropsPerRowIterator("s0", fields, expectedAll);

	            // test iterator ALL
	            ContextPartitionSelector selectorOne = ContextPartitionSelectorAll.INSTANCE;
	            env.AssertStatement("s0", statement => EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(selectorOne), statement.GetSafeEnumerator(selectorOne), fields, expectedAll));

	            // test iterator by context partition id
	            ContextPartitionSelector selectorTwo = new SupportSelectorById(new HashSet<int>(Arrays.AsList(0, 1, 2)));
	            env.AssertStatement("s0", statement => EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(selectorTwo), statement.GetSafeEnumerator(selectorTwo), fields, expectedAll));

	            ContextPartitionSelector selectorThree = new SupportSelectorById(new HashSet<int>(Arrays.AsList(1)));
	            env.AssertStatement("s0", statement => EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(selectorThree), statement.GetSafeEnumerator(selectorThree), fields, new object[][]{new object[] {"E2", 41}}));

	            env.AssertStatement("s0", statement => {
	                Assert.IsFalse(statement.GetEnumerator(new SupportSelectorById(Collections.GetEmptySet<int>())).MoveNext());
	                Assert.IsFalse(statement.GetEnumerator(new SupportSelectorById(null)).MoveNext());

	                try {
	                    statement.GetEnumerator(null);
	                    Assert.Fail();
	                } catch (ArgumentException ex) {
	                    Assert.AreEqual(ex.Message, "No selector provided");
	                }

	                try {
	                    statement.GetSafeEnumerator(null);
	                    Assert.Fail();
	                } catch (ArgumentException ex) {
	                    Assert.AreEqual(ex.Message, "No selector provided");
	                }
	            });

	            env.CompileDeploy("@name('s2') select * from SupportBean");
	            env.AssertStatement("s2", statement => {
	                try {
	                    statement.GetEnumerator(null);
	                    Assert.Fail();
	                } catch (UnsupportedOperationException ex) {
	                    Assert.AreEqual(ex.Message, "Iterator with context selector is only supported for statements under context");
	                }

	                try {
	                    statement.GetSafeEnumerator(null);
	                    Assert.Fail();
	                } catch (UnsupportedOperationException ex) {
	                    Assert.AreEqual(ex.Message, "Iterator with context selector is only supported for statements under context");
	                }
	            });

	            env.UndeployAll();
	        }
	    }

	    private static void RunQueryAll(RegressionEnvironment env, RegressionPath path, string epl, string fields, object[][] expected, int numStreams) {
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

	    private static void RunQuery(RegressionEnvironment env, RegressionPath path, string epl, string fields, object[][] expected, ContextPartitionSelector[] selectors) {
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
	        var resultPreparedParameterized = env.Runtime.FireAndForgetService.ExecuteQuery(preparedParameterizedQuery, selectors);
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

	    private static void TryInvalidRuntimeQuery(RegressionEnvironment env, RegressionPath path, ContextPartitionSelector[] selectors, string epl, string expected) {
	        var faf = env.CompileFAF(epl, path);
	        try {
	            env.Runtime.FireAndForgetService.ExecuteQuery(faf, selectors);
	            Assert.Fail();
	        } catch (ArgumentException ex) {
	            Assert.AreEqual(ex.Message, expected);
	        }
	    }

	    private static void TryInvalidCompileQuery(RegressionEnvironment env, RegressionPath path, string epl, string expected) {
	        SupportMessageAssertUtil.TryInvalidFAFCompile(env, path, epl, expected);
	    }
	}
} // end of namespace
