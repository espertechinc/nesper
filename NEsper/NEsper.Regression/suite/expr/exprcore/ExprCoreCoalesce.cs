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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // TryInvalidCompile

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreCoalesce
	{

		public static ICollection<RegressionExecution> Executions()
		{
			var executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreCoalesceBeans());
			executions.Add(new ExprCoreCoalesceLong());
			executions.Add(new ExprCoreCoalesceLongOM());
			executions.Add(new ExprCoreCoalesceLongCompile());
			executions.Add(new ExprCoreCoalesceDouble());
			executions.Add(new ExprCoreCoalesceNull());
			executions.Add(new ExprCoreCoalesceInvalid());
			return executions;
		}

		private class ExprCoreCoalesceBeans : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select coalesce(a.TheString, b.TheString) as myString, coalesce(a, b) as myBean" +
				          " from pattern [every (a=SupportBean(TheString='s0') or b=SupportBean(TheString='s1'))]";
				env.CompileDeploy(epl).AddListener("s0");

				var theEvent = SendEvent(env, "s0");
				var eventReceived = env.Listener("s0").AssertOneGetNewAndReset();
				Assert.AreEqual("s0", eventReceived.Get("myString"));
				Assert.AreSame(theEvent, eventReceived.Get("myBean"));

				theEvent = SendEvent(env, "s1");
				eventReceived = env.Listener("s0").AssertOneGetNewAndReset();
				Assert.AreEqual("s1", eventReceived.Get("myString"));
				Assert.AreSame(theEvent, eventReceived.Get("myBean"));

				env.UndeployAll();
			}
		}

		private class ExprCoreCoalesceLong : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0')  select coalesce(LongBoxed, IntBoxed, ShortBoxed) as result from SupportBean").AddListener("s0");

				Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("result"));

				TryCoalesceLong(env);

				env.UndeployAll();
			}
		}

		private class ExprCoreCoalesceLongOM : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "select coalesce(LongBoxed,IntBoxed,ShortBoxed) as result" +
				          " from SupportBean#length(1000)";

				var model = new EPStatementObjectModel();
				model.SelectClause = SelectClause.Create()
					.Add(
						Expressions.Coalesce(
							"LongBoxed",
							"IntBoxed",
							"ShortBoxed"),
						"result");
				model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).Name).AddView("length", Expressions.Constant(1000)));
				model = SerializableObjectCopier.GetInstance(env.Container).Copy(model);
				Assert.AreEqual(epl, model.ToEPL());

				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				env.CompileDeploy(model).AddListener("s0");
				Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("result"));

				TryCoalesceLong(env);

				env.UndeployAll();
			}
		}

		private class ExprCoreCoalesceLongCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select coalesce(LongBoxed,IntBoxed,ShortBoxed) as result" +
				          " from SupportBean#length(1000)";

				env.EplToModelCompileDeploy(epl).AddListener("s0");
				Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("result"));

				TryCoalesceLong(env);

				env.UndeployAll();
			}
		}

		private class ExprCoreCoalesceDouble : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpressions(fields, "coalesce(null, ByteBoxed, ShortBoxed, IntBoxed, LongBoxed, FloatBoxed, DoubleBoxed)")
					.WithStatementConsumer(stmt => Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("c0")));

				builder.WithAssertion(MakeEventWithDouble(env, null, null, null, null, null, null)).Expect(fields, new object[] {null});

				builder.WithAssertion(MakeEventWithDouble(env, null, short.Parse("2"), null, null, null, 1d)).Expect(fields, 2d);

				builder.WithAssertion(MakeEventWithDouble(env, null, null, null, null, null, 100d)).Expect(fields, 100d);

				builder.WithAssertion(MakeEventWithDouble(env, null, null, null, null, 10f, 100d)).Expect(fields, 10d);

				builder.WithAssertion(MakeEventWithDouble(env, null, null, 1, 5L, 10f, 100d)).Expect(fields, 1d);

				builder.WithAssertion(MakeEventWithDouble(env, byte.Parse("3"), null, null, null, null, null)).Expect(fields, 3d);

				builder.WithAssertion(MakeEventWithDouble(env, null, null, null, 5L, 10f, 100d)).Expect(fields, 5d);

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreCoalesceInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryCoalesceInvalid(env, "coalesce(IntPrimitive)");
				TryCoalesceInvalid(env, "coalesce(IntPrimitive, string)");
				TryCoalesceInvalid(env, "coalesce(IntPrimitive, xxx)");
				TryCoalesceInvalid(env, "coalesce(IntPrimitive, booleanBoxed)");
				TryCoalesceInvalid(env, "coalesce(CharPrimitive, LongBoxed)");
				TryCoalesceInvalid(env, "coalesce(CharPrimitive, string, string)");
				TryCoalesceInvalid(env, "coalesce(string, LongBoxed)");
				TryCoalesceInvalid(env, "coalesce(null, LongBoxed, string)");
				TryCoalesceInvalid(env, "coalesce(null, null, BoolBoxed, 1l)");
			}
		}

		private class ExprCoreCoalesceNull : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpressions(fields, "coalesce(null, null)")
					.WithStatementConsumer(stmt => Assert.AreEqual(null, stmt.EventType.GetPropertyType("result")));

				builder.WithAssertion(new SupportBean()).Expect(fields, new object[] {null});

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private static void TryCoalesceInvalid(
			RegressionEnvironment env,
			string coalesceExpr)
		{
			var epl = "select " + coalesceExpr + " as result from SupportBean";
			TryInvalidCompile(env, epl, "skip");
		}

		private static void TryCoalesceLong(RegressionEnvironment env)
		{
			SendEvent(env, 1L, 2, 3);
			Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

			SendBoxedEvent(env, null, 2, null);
			Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

			SendBoxedEvent(env, null, null, short.Parse("3"));
			Assert.AreEqual(3L, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));

			SendBoxedEvent(env, null, null, null);
			Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("result"));
		}

		private static SupportBean SendEvent(
			RegressionEnvironment env,
			string theString)
		{
			var bean = new SupportBean();
			bean.TheString = theString;
			env.SendEventBean(bean);
			return bean;
		}

		private static void SendEvent(
			RegressionEnvironment env,
			long longBoxed,
			int intBoxed,
			short shortBoxed)
		{
			SendBoxedEvent(env, longBoxed, intBoxed, shortBoxed);
		}

		private static void SendBoxedEvent(
			RegressionEnvironment env,
			long? longBoxed,
			int? intBoxed,
			short? shortBoxed)
		{
			var bean = new SupportBean();
			bean.LongBoxed = longBoxed;
			bean.IntBoxed = intBoxed;
			bean.ShortBoxed = shortBoxed;
			env.SendEventBean(bean);
		}

		private static SupportBean MakeEventWithDouble(
			RegressionEnvironment env,
			byte? byteBoxed,
			short? shortBoxed,
			int? intBoxed,
			long? longBoxed,
			float? floatBoxed,
			double? doubleBoxed)
		{
			var bean = new SupportBean();
			bean.ByteBoxed = byteBoxed;
			bean.ShortBoxed = shortBoxed;
			bean.IntBoxed = intBoxed;
			bean.LongBoxed = longBoxed;
			bean.FloatBoxed = floatBoxed;
			bean.DoubleBoxed = doubleBoxed;
			return bean;
		}
	}
} // end of namespace
