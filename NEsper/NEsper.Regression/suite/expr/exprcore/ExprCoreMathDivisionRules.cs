///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreMathDivisionRules
	{

		public static ICollection<RegressionExecution> Executions()
		{
			var executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreMathRulesBigInt());
			executions.Add(new ExprCoreMathRulesLong());
			executions.Add(new ExprCoreMathRulesFloat());
			executions.Add(new ExprCoreMathRulesDouble());
			executions.Add(new ExprCoreMathRulesInt());
			return executions;
		}

		public class ExprCoreMathRulesBigInt : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select BigInteger.valueOf(4)/BigInteger.valueOf(2) as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				Assert.AreEqual(typeof(BigInteger), env.Statement("s0").EventType.GetPropertyType("c0"));

				var fields = "c0".SplitCsv();
				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					fields,
					new BigInteger(4) / new BigInteger(2));

				env.UndeployAll();
			}
		}

		public class ExprCoreMathRulesLong : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select 10L/2L as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("c0"));

				var fields = "c0".SplitCsv();
				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 5L);

				env.UndeployAll();
			}
		}

		public class ExprCoreMathRulesFloat : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select 10f/2f as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				Assert.AreEqual(typeof(float?), env.Statement("s0").EventType.GetPropertyType("c0"));

				var fields = "c0".SplitCsv();
				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, 5f);

				env.UndeployAll();
			}
		}

		public class ExprCoreMathRulesDouble : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select 10d/0d as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "c0".SplitCsv();
				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null});

				env.UndeployAll();
			}
		}

		public class ExprCoreMathRulesInt : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select IntPrimitive/IntBoxed as result from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("result"));

				SendEvent(env, 100, 3);
				Assert.AreEqual(33, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

				SendEvent(env, 100, null);
				Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

				SendEvent(env, 100, 0);
				Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

				env.UndeployAll();
			}
		}

		private static void SendEvent(
			RegressionEnvironment env,
			int intPrimitive,
			int? intBoxed)
		{
			var bean = new SupportBean();
			bean.IntBoxed = intBoxed;
			bean.IntPrimitive = intPrimitive;
			env.SendEventBean(bean);
		}
	}
} // end of namespace
