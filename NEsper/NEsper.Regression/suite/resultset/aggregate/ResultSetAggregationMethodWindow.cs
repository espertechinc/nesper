///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.suite.resultset.aggregate.ResultSetAggregationMethodSorted;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
	public class ResultSetAggregationMethodWindow
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ResultSetAggregateWindowNonTable());
			execs.Add(new ResultSetAggregateWindowTableAccess());
			execs.Add(new ResultSetAggregateWindowTableIdentWCount());
			execs.Add(new ResultSetAggregateWindowListReference());
			execs.Add(new ResultSetAggregateWindowInvalid());
			return execs;
		}

		internal class ResultSetAggregateWindowListReference : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create table MyTable(windowcol window(*) @type('SupportBean'));\n" +
				             "into table MyTable select window(*) as windowcol from SupportBean;\n" +
				             "@name('s0') select MyTable.windowcol.listReference() as collref from SupportBean_S0";
				env.CompileDeploy(epl).AddListener("s0");

				AssertType(env, typeof(IList<EventBean>), "collref");

				SupportBean sb1 = MakeSendBean(env, "E1", 10);
				SupportBean sb2 = MakeSendBean(env, "E1", 10);
				env.SendEventBean(new SupportBean_S0(-1));
				IList<EventBean> events = (IList<EventBean>) env.Listener("s0").AssertOneGetNewAndReset().Get("collref");
				Assert.AreEqual(2, events.Count);
				EPAssertionUtil.AssertEqualsExactOrder(new object[] {
					events[0].Underlying, 
					events[1].Underlying
				}, new SupportBean[] {sb1, sb2});

				env.UndeployAll();
			}
		}

		internal class ResultSetAggregateWindowTableIdentWCount : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create table MyTable(windowcol window(*) @type('SupportBean'));\n" +
				             "into table MyTable select window(*) as windowcol from SupportBean;\n" +
				             "@name('s0') select windowcol.first(intPrimitive) as c0, windowcol.last(intPrimitive) as c1, windowcol.countEvents() as c2 from SupportBean_S0, MyTable";
				env.CompileDeploy(epl).AddListener("s0");

				AssertType(env, typeof(int?), "c0,c1,c2");

				MakeSendBean(env, "E1", 10);
				MakeSendBean(env, "E2", 20);
				MakeSendBean(env, "E3", 30);

				env.SendEventBean(new SupportBean_S0(-1));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0,c1,c2".SplitCsv(), new object[] {10, 30, 3});

				env.UndeployAll();
			}
		}

		internal class ResultSetAggregateWindowTableAccess : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create table MyTable(windowcol window(*) @type('SupportBean'));\n" +
				             "into table MyTable select window(*) as windowcol from SupportBean#length(2);\n" +
				             "@name('s0') select MyTable.windowcol.first() as c0, MyTable.windowcol.last() as c1 from SupportBean_S0";
				env.CompileDeploy(epl).AddListener("s0");

				AssertType(env, typeof(SupportBean), "c0,c1");

				SendAssert(env, null, null);

				SupportBean sb1 = MakeSendBean(env, "E1", 10);
				SendAssert(env, sb1, sb1);

				SupportBean sb2 = MakeSendBean(env, "E2", 20);
				SendAssert(env, sb1, sb2);

				SupportBean sb3 = MakeSendBean(env, "E3", 0);
				SendAssert(env, sb2, sb3);

				env.UndeployAll();
			}

			private void SendAssert(
				RegressionEnvironment env,
				SupportBean first,
				SupportBean last)
			{
				string[] fields = "c0,c1".SplitCsv();
				env.SendEventBean(new SupportBean_S0(-1));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {first, last});
			}
		}

		internal class ResultSetAggregateWindowNonTable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "theString,c0,c1".SplitCsv();
				string epl =
					"@name('s0') select theString, window(*).first() as c0, window(*).last() as c1 from SupportBean#length(3) as sb group by theString";
				env.CompileDeploy(epl).AddListener("s0");

				AssertType(env, typeof(SupportBean), "c0,c1");

				SupportBean sb1 = MakeSendBean(env, "A", 1);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"A", sb1, sb1});

				SupportBean sb2 = MakeSendBean(env, "A", 2);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"A", sb1, sb2});

				SupportBean sb3 = MakeSendBean(env, "A", 3);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"A", sb1, sb3});

				SupportBean sb4 = MakeSendBean(env, "A", 4);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"A", sb2, sb4});

				SupportBean sb5 = MakeSendBean(env, "B", 5);
				EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Listener("s0").GetAndResetLastNewData(), fields, new object[][] {
					new object[] {"B", sb5, sb5}, 
					new object[] {"A", sb3, sb4}
				});

				SupportBean sb6 = MakeSendBean(env, "A", 6);
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"A", sb4, sb6});

				env.UndeployAll();
			}
		}

		internal class ResultSetAggregateWindowInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				env.CompileDeploy("create table MyTable(windowcol window(*) @type('SupportBean'));\n", path);

				TryInvalidCompile(
					env,
					path,
					"select MyTable.windowcol.first(id) from SupportBean_S0",
					"Failed to validate select-clause expression 'MyTable.windowcol.first(id)': Failed to validate aggregation function parameter expression 'id': Property named 'id' is not valid in any stream");

				TryInvalidCompile(
					env,
					path,
					"select MyTable.windowcol.listReference(intPrimitive) from SupportBean_S0",
					"Failed to validate select-clause expression 'MyTable.windowcol.listReference(int...(45 chars)': Invalid number of parameters");

				env.UndeployAll();
			}
		}

		private static SupportBean MakeSendBean(
			RegressionEnvironment env,
			string theString,
			int intPrimitive)
		{
			SupportBean sb = new SupportBean(theString, intPrimitive);
			env.SendEventBean(sb);
			return sb;
		}
	}
} // end of namespace
