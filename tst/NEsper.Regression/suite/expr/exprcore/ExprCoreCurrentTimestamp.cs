///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreCurrentTimestamp
	{
		public static ICollection<RegressionExecution> Executions()
		{
			IList<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreCurrentTimestampGet());
			executions.Add(new ExprCoreCurrentTimestampOM());
			executions.Add(new ExprCoreCurrentTimestampCompile());
			return executions;
		}

		private class ExprCoreCurrentTimestampGet : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimer(env, 0);
				var stmtText = "@Name('s0') select current_timestamp(), " +
				               " current_timestamp as t0, " +
				               " current_timestamp() as t1, " +
				               " current_timestamp + 1 as t2 " +
				               " from SupportBean";
				env.CompileDeploy(stmtText).AddListener("s0");

				var type = env.Statement("s0").EventType;
				Assert.AreEqual(typeof(long?), type.GetPropertyType("current_timestamp()"));
				Assert.AreEqual(typeof(long?), type.GetPropertyType("t0"));
				Assert.AreEqual(typeof(long?), type.GetPropertyType("t1"));
				Assert.AreEqual(typeof(long?), type.GetPropertyType("t2"));

				SendTimer(env, 100);
				env.SendEventBean(new SupportBean());
				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				AssertResults(theEvent, new object[] {100L, 100L, 101L});

				SendTimer(env, 999);
				env.SendEventBean(new SupportBean());
				theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				AssertResults(theEvent, new object[] {999L, 999L, 1000L});
				Assert.AreEqual(theEvent.Get("current_timestamp()"), theEvent.Get("t0"));

				env.UndeployAll();
			}
		}

		private class ExprCoreCurrentTimestampOM : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimer(env, 0);
				var stmtText = "select current_timestamp() as t0 from SupportBean";

				var model = new EPStatementObjectModel();
				model.SelectClause = SelectClause.Create().Add(Expressions.CurrentTimestamp(), "t0");
				model.FromClause = FromClause.Create().Add(FilterStream.Create(nameof(SupportBean)));
				model = SerializableObjectCopier.GetInstance(env.Container).Copy(model);
				Assert.AreEqual(stmtText, model.ToEPL());

				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				env.CompileDeploy(model).AddListener("s0").Milestone(0);

				Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("t0"));

				SendTimer(env, 777);
				env.SendEventBean(new SupportBean());
				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				AssertResults(theEvent, new object[] {777L});

				env.UndeployAll();
			}
		}

		private class ExprCoreCurrentTimestampCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimer(env, 0);
				var stmtText = "@Name('s0') select current_timestamp() as t0 from SupportBean";
				env.EplToModelCompileDeploy(stmtText).AddListener("s0").Milestone(0);

				Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("t0"));

				SendTimer(env, 777);
				env.SendEventBean(new SupportBean());
				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				AssertResults(theEvent, new object[] {777L});

				env.UndeployAll();
			}
		}

		private static void SendTimer(
			RegressionEnvironment env,
			long timeInMSec)
		{
			env.AdvanceTime(timeInMSec);
		}

		private static void AssertResults(
			EventBean theEvent,
			object[] result)
		{
			for (var i = 0; i < result.Length; i++) {
				Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
			}
		}
	}
} // end of namespace
