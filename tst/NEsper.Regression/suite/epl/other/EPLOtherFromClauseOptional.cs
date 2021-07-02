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
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil; // AssertPropsPerRow;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // TryInvalidCompile

namespace com.espertech.esper.regressionlib.suite.epl.other
{
	public class EPLOtherFromClauseOptional
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			WithContext(execs);
			WithNoContext(execs);
			WithFAFNoContext(execs);
			WithFAFContext(execs);
			WithInvalid(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new EPLOtherFromOptionalInvalid());
			return execs;
		}

		public static IList<RegressionExecution> WithFAFContext(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new EPLOtherFromOptionalFAFContext());
			return execs;
		}

		public static IList<RegressionExecution> WithFAFNoContext(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new EPLOtherFromOptionalFAFNoContext());
			return execs;
		}

		public static IList<RegressionExecution> WithNoContext(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new EPLOtherFromOptionalNoContext());
			return execs;
		}

		public static IList<RegressionExecution> WithContext(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new EPLOtherFromOptionalContext(false));
			execs.Add(new EPLOtherFromOptionalContext(true));
			return execs;
		}

		internal class EPLOtherFromOptionalContext : RegressionExecution
		{
			private readonly bool soda;

			public EPLOtherFromOptionalContext(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				env.CompileDeploy("create context MyContext initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(Id=s0.Id)", path);

				string eplOnInit = "@name('s0') context MyContext select context.s0 as ctxs0";
				env.CompileDeploy(soda, eplOnInit, path).AddListener("s0");

				string eplOnTerm = "@name('s1') context MyContext select context.s0 as ctxs0 output when terminated";
				env.CompileDeploy(soda, eplOnTerm, path).AddListener("s1");

				SupportBean_S0 s0A = new SupportBean_S0(10, "A");
				env.SendEventBean(s0A);

				Assert.AreEqual(s0A, env.Listener("s0").AssertOneGetNewAndReset().Get("ctxs0"));

				var enumerator = env.GetEnumerator("s0");
				Assert.IsTrue(enumerator.MoveNext());
				Assert.AreEqual(s0A, enumerator.Current.Get("ctxs0"));

				env.Milestone(0);

				SupportBean_S0 s0B = new SupportBean_S0(20, "B");
				env.SendEventBean(s0B);
				Assert.AreEqual(s0B, env.Listener("s0").AssertOneGetNewAndReset().Get("ctxs0"));
				AssertIterator(env, "s0", s0A, s0B);
				AssertIterator(env, "s1", s0A, s0B);

				env.Milestone(1);

				env.SendEventBean(new SupportBean_S1(10, "A"));
				Assert.AreEqual(s0A, env.Listener("s1").AssertOneGetNewAndReset().Get("ctxs0"));
				AssertIterator(env, "s0", s0B);
				AssertIterator(env, "s1", s0B);

				env.Milestone(2);

				env.SendEventBean(new SupportBean_S1(20, "A"));
				Assert.AreEqual(s0B, env.Listener("s1").AssertOneGetNewAndReset().Get("ctxs0"));
				AssertIterator(env, "s0");
				AssertIterator(env, "s1");

				env.UndeployAll();
			}
		}

		internal class EPLOtherFromOptionalFAFNoContext : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				env.AdvanceTime(1000);

				string eplObjects = "@public create variable string MYVAR = 'abc';\n" +
				                    "@public create window MyWindow#keepall as SupportBean;\n" +
				                    "on SupportBean merge MyWindow insert select *;\n" +
				                    "@public create table MyTable(field int);\n" +
				                    "on SupportBean merge MyTable insert select IntPrimitive as field;\n";
				env.CompileDeploy(eplObjects, path);
				env.SendEventBean(new SupportBean("E1", 1));

				RunSelectFAFSimpleCol(env, path, 1, "1");
				RunSelectFAFSimpleCol(env, path, 1000L, "current_timestamp()");
				RunSelectFAFSimpleCol(env, path, "abc", "MYVAR");
				RunSelectFAFSimpleCol(env, path, 1, "sum(1)");
				RunSelectFAFSimpleCol(env, path, 1L, "(select count(*) from MyWindow)");
				RunSelectFAFSimpleCol(env, path, 1L, "(select count(*) from MyTable)");
				RunSelectFAFSimpleCol(env, path, 1, "MyTable.field");

				RunSelectFAF(env, path, null, "select 1 as value where 'a'='b'");
				RunSelectFAF(env, path, 1, "select 1 as value where 1-0=1");
				RunSelectFAF(env, path, null, "select 1 as value having 'a'='b'");

				string eplScript = "expression string one() ['x']\n select one() as value";
				RunSelectFAF(env, path, "x", eplScript);

				string eplInlinedClass = "inlined_class \"\"\"\n" +
				                         "  public class Helper {\n" +
				                         "    public static String doit() { return \"y\";}\n" +
				                         "  }\n" +
				                         "\"\"\"\n select Helper.doit() as value";
				RunSelectFAF(env, path, "y", eplInlinedClass);

				env.UndeployAll();
			}
		}

		internal class EPLOtherFromOptionalNoContext : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0') select 1 as value");
				var enumerator = env.GetEnumerator("s0");
				Assert.IsTrue(enumerator.MoveNext());
				Assert.AreEqual(1, enumerator.Current.Get("value"));

				env.UndeployAll();
			}
		}

		private class EPLOtherFromOptionalInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string context = "create context MyContext initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(Id=s0.Id);";
				env.CompileDeploy(context, path);

				// subselect needs from clause
				TryInvalidCompile(env, "select (select 1)", "Incorrect syntax near ')'");

				// wildcard not allowed
				TryInvalidCompile(env, "select *", "Wildcard cannot be used when the from-clause is not provided");
				TryInvalidFAFCompile(env, path, "select *", "Wildcard cannot be used when the from-clause is not provided");

				// context requires a single selector
				EPCompiled compiled = env.CompileFAF("context MyContext select context.s0.P00 as Id", path);
				try {
					env.Runtime.FireAndForgetService.ExecuteQuery(compiled, new ContextPartitionSelector[2]);
					Assert.Fail();
				}
				catch (ArgumentException ex) {
					Assert.AreEqual("Fire-and-forget queries without a from-clause allow only a single context partition selector", ex.Message);
				}

				// context + order-by not allowed
				TryInvalidFAFCompile(
					env,
					path,
					"context MyContext select context.s0.P00 as p00 order by p00 desc",
					"Fire-and-forget queries without a from-clause and with context do not allow order-by");

				env.UndeployAll();
			}
		}

		internal class EPLOtherFromOptionalFAFContext : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string epl = "create context MyContext initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(Id=s0.Id);\n" +
				             "context MyContext select count(*) from SupportBean;\n";
				env.CompileDeploy(epl, path);

				env.SendEventBean(new SupportBean_S0(10, "A", "x"));
				env.SendEventBean(new SupportBean_S0(20, "B", "x"));
				string eplFAF = "context MyContext select context.s0.P00 as Id";
				EPCompiled compiled = env.CompileFAF(eplFAF, path);
				AssertPropsPerRow(env.Runtime.FireAndForgetService.ExecuteQuery(compiled).Array, new [] { "Id" }, new object[][] {new [] {"A"}, new [] {"B"}});

				// context partition selector
				ContextPartitionSelector selector = new SupportSelectorById(1);
				AssertPropsPerRow(
					env.Runtime.FireAndForgetService.ExecuteQuery(compiled, new ContextPartitionSelector[] {selector}).Array,
					new [] { "Id" },
					new object[][] {new [] {"B"}});

				// SODA
				EPStatementObjectModel model = env.EplToModel(eplFAF);
				Assert.AreEqual(eplFAF, model.ToEPL());
				compiled = env.CompileFAF(model, path);
				AssertPropsPerRow(env.Runtime.FireAndForgetService.ExecuteQuery(compiled).Array, new [] { "Id" }, new object[][] {new [] {"A"}, new [] {"B"}});

				// distinct
				string eplFAFDistint = "context MyContext select distinct context.s0.P01 as p01";
				EPFireAndForgetQueryResult result = env.CompileExecuteFAF(eplFAFDistint, path);
				AssertPropsPerRow(result.Array, new [] { "p01" }, new object[][] { new [] {"x"}});

				// where-clause and having-clause
				RunSelectFAF(env, path, null, "context MyContext select 1 as value where 'a'='b'");
				RunSelectFAF(env, path, "A", "context MyContext select context.s0.P00 as value where context.s0.Id=10");
				RunSelectFAF(env, path, "A", "context MyContext select context.s0.P00 as value having context.s0.Id=10");

				env.UndeployAll();
			}
		}

		private static void RunSelectFAFSimpleCol(
			RegressionEnvironment env,
			RegressionPath path,
			object expected,
			string col)
		{
			RunSelectFAF(env, path, expected, "select " + col + " as value");
		}

		private static void RunSelectFAF(
			RegressionEnvironment env,
			RegressionPath path,
			object expected,
			string epl)
		{
			EventBean[] result = env.CompileExecuteFAF(epl, path).Array;
			if (expected == null) {
				Assert.AreEqual(0, result == null ? 0 : result.Length);
			}
			else {
				Assert.AreEqual(expected, result[0].Get("value"));
			}
		}

		private static void AssertIterator(
			RegressionEnvironment env,
			string name,
			params SupportBean_S0[] s0)
		{
			IEnumerator<EventBean> it = env.GetEnumerator(name);
			for (int i = 0; i < s0.Length; i++) {
				Assert.IsTrue(it.MoveNext());
				Assert.AreEqual(s0[i], it.Current.Get("ctxs0"));
			}

			Assert.IsFalse(it.MoveNext());
		}
	}
} // end of namespace
