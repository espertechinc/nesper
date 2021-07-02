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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreAndOrNot
	{

		public static ICollection<RegressionExecution> Executions()
		{
			var executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreAndOrNotCombined());
			executions.Add(new ExprCoreNotWithVariable());
			executions.Add(new ExprCoreAndOrNotNull());
			return executions;
		}

		private class ExprCoreAndOrNotNull : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean")
					.WithExpression(fields[0], "cast(null, boolean) and cast(null, boolean)")
					.WithExpression(fields[1], "BoolPrimitive and BoolBoxed")
					.WithExpression(fields[2], "BoolBoxed and BoolPrimitive")
					.WithExpression(fields[3], "BoolPrimitive or BoolBoxed")
					.WithExpression(fields[4], "BoolBoxed or BoolPrimitive")
					.WithExpression(fields[5], "not BoolBoxed");
				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(bool?)));
				builder.WithAssertion(makeSB("E1", true, true)).Expect(fields, null, true, true, true, true, false);
				builder.WithAssertion(makeSB("E2", false, false)).Expect(fields, null, false, false, false, false, true);
				builder.WithAssertion(makeSB("E3", false, null)).Expect(fields, null, false, false, null, null, null);
				builder.WithAssertion(makeSB("E4", true, null)).Expect(fields, null, null, null, true, true, null);
				builder.WithAssertion(makeSB("E5", true, false)).Expect(fields, null, false, false, true, true, true);
				builder.WithAssertion(makeSB("E6", false, true)).Expect(fields, null, false, false, true, true, false);
				builder.Run(env);
				env.UndeployAll();
			}

			private SupportBean makeSB(
				string theString,
				bool boolPrimitive,
				bool? boolBoxed)
			{
				SupportBean sb = new SupportBean(theString, 0);
				sb.BoolPrimitive= boolPrimitive;
				sb.BoolBoxed = boolBoxed;
				return sb;
			}
		}

		private class ExprCoreNotWithVariable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"create variable string thing = \"Hello World\";" +
					"@Name('s0') select not thing.Contains(TheString) as c0 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendBeanAssert(env, "World", false);
				SendBeanAssert(env, "x", true);

				var newValues = new Dictionary<DeploymentIdNamePair, object>();
				newValues.Put(new DeploymentIdNamePair(env.DeploymentId("s0"), "thing"), "5 x 5");
				env.Runtime.VariableService.SetVariableValue(newValues);

				SendBeanAssert(env, "World", true);
				SendBeanAssert(env, "x", false);

				env.UndeployAll();
			}
		}

		private class ExprCoreAndOrNotCombined : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpressions(
						fields,
						"(IntPrimitive=1) or (IntPrimitive=2)",
						"(IntPrimitive>0) and (IntPrimitive<3)",
						"not(IntPrimitive=2)");
				builder.WithAssertion(new SupportBean("E1", 1)).Expect(fields, true, true, true);
				builder.WithAssertion(new SupportBean("E2", 2)).Expect(fields, true, true, false);
				builder.WithAssertion(new SupportBean("E3", 3)).Expect(fields, false, false, true);
				builder.Run(env);
				env.UndeployAll();
			}
		}

		private static void SendBeanAssert(
			RegressionEnvironment env,
			string theString,
			bool expected)
		{
			var bean = new SupportBean(theString, 0);
			env.SendEventBean(bean);
			Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
		}
	}
} // end of namespace
