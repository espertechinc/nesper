///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.define
{
	public class ExprDefineEventParameterNonStream
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprDefineEventParamPatternPONO());
			execs.Add(new ExprDefineEventParamPatternMap());
			execs.Add(new ExprDefineEventParamContextProperty());
			execs.Add(new ExprDefineEventParamSubqueryPONO());
			execs.Add(new ExprDefineEventParamSubqueryMap());
			execs.Add(new ExprDefineEventParamSubqueryMapWithWhere());
			return execs;
		}

		private class ExprDefineEventParamSubqueryPONO : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"@Name('s0') expression combineProperties {v -> v.P00 || v.P01} select combineProperties((select * from SupportBean_S0#keepall)) as c0 from SupportBean_S1 as p";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssertS1(env, null);

				SendS0(env, "a", "b");
				SendAssertS1(env, "ab");

				SendS0(env, "d", "e");
				SendAssertS1(env, null); // since subquery returns two rows

				env.UndeployAll();
			}
		}

		private class ExprDefineEventParamSubqueryMap : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"@public @buseventtype create schema EventOne(p0 string, p1 string);\n" +
					"@public @buseventtype create schema EventTwo();\n" +
					"@Name('s0') expression combineProperties {v -> v.p0 || v.p1} select combineProperties((select * from EventOne#lastevent)) as c0 from EventTwo as p";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssertEventTwo(env, null);

				SendEventOne(env, "a", "b");
				SendAssertEventTwo(env, "ab");

				SendEventOne(env, "c", "d");
				SendAssertEventTwo(env, "cd");

				env.UndeployAll();
			}
		}

		private class ExprDefineEventParamSubqueryMapWithWhere : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"@public @buseventtype create schema EventOne(p0 string, p1 string);\n" +
					"@public @buseventtype create schema EventTwo();\n" +
					"@Name('s0') expression combineProperties {v -> v.p0 || v.p1} select combineProperties((select * from EventOne#keepall where p0='a')) as c0 from EventTwo as p";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssertEventTwo(env, null);

				SendEventOne(env, "c", "d");
				SendAssertEventTwo(env, null);

				SendEventOne(env, "a", "d");
				SendAssertEventTwo(env, "ad");

				SendEventOne(env, "a", "e");
				SendAssertEventTwo(env, null);

				env.UndeployAll();
			}
		}

		private class ExprDefineEventParamContextProperty : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"@public @buseventtype create schema EventOne(p0 string, p1 string);\n" +
					"@public @buseventtype create schema EventTwo();\n" +
					"create context PerEventOne initiated by EventOne e1;\n" +
					"@Name('s0') expression combineProperties {v -> v.p0 || v.p1} \n" +
					"context PerEventOne select combineProperties(context.e1) as c0 from EventTwo;\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendEventOne(env, "a", "b");
				SendAssertEventTwo(env, "ab");

				env.UndeployAll();
			}
		}

		private class ExprDefineEventParamPatternMap : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"@public @buseventtype create schema EventOne(p0 string, p1 string);\n" +
					"@public @buseventtype create schema EventTwo();\n" +
					"@Name('s0') expression combineProperties {v -> v.p0 || v.p1} select combineProperties(p.a) as c0 from pattern [a=EventOne -> EventTwo] as p";
				env.CompileDeploy(epl).AddListener("s0");

				SendEventOne(env, "a", "b");
				SendAssertEventTwo(env, "ab");

				env.UndeployAll();
			}
		}

		private class ExprDefineEventParamPatternPONO : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"@Name('s0') expression combineProperties {v -> v.P00 || v.P01} select combineProperties(p.a) as c0 from pattern [a=SupportBean_S0 -> SupportBean_S1] as p";
				env.CompileDeploy(epl).AddListener("s0");

				SendS0(env, "a", "b");
				SendAssertS1(env, "ab");

				env.UndeployAll();
			}
		}

		private static void SendEventOne(
			RegressionEnvironment env,
			string p0,
			string p1)
		{
			env.SendEventMap(CollectionUtil.BuildMap("p0", p0, "p1", p1), "EventOne");
		}

		private static void SendAssertEventTwo(
			RegressionEnvironment env,
			object expected)
		{
			env.SendEventMap(EmptyDictionary<string, object>.Instance, "EventTwo");
			AssertReceived(env, expected);
		}

		private static void SendS0(
			RegressionEnvironment env,
			string p00,
			string p01)
		{
			env.SendEventBean(new SupportBean_S0(0, p00, p01));
		}

		private static void SendAssertS1(
			RegressionEnvironment env,
			object expected)
		{
			env.SendEventBean(new SupportBean_S1(0));
			AssertReceived(env, expected);
		}

		private static void AssertReceived(
			RegressionEnvironment env,
			object expected)
		{
			Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
		}
	}
} // end of namespace
