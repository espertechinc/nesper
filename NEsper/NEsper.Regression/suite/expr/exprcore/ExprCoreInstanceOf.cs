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
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreInstanceOf
	{

		public static ICollection<RegressionExecution> Executions()
		{
			IList<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreInstanceofSimple());
			executions.Add(new ExprCoreInstanceofStringAndNullOM());
			executions.Add(new ExprCoreInstanceofStringAndNullCompile());
			executions.Add(new ExprCoreDynamicPropertyJavaTypes());
			executions.Add(new ExprCoreDynamicSuperTypeAndInterface());
			return executions;
		}

		private class ExprCoreInstanceofSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpression(fields[0], "instanceof(TheString, string)")
					.WithExpression(fields[1], "instanceof(IntBoxed, int)")
					.WithExpression(fields[2], "instanceof(FloatBoxed, System.Single)")
					.WithExpression(fields[3], "instanceof(TheString, System.Single, char, byte)")
					.WithExpression(fields[4], "instanceof(IntPrimitive, System.Int32)")
					.WithExpression(fields[5], "instanceof(IntPrimitive, long)")
					.WithExpression(fields[6], "instanceof(IntPrimitive, long, long, System.Object)")
					.WithExpression(fields[7], "instanceof(FloatBoxed, long, float)");

				builder.WithStatementConsumer(
					stmt => {
						for (var i = 0; i < fields.Length; i++) {
							Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType(fields[i]));
						}
					});

				var bean = new SupportBean("abc", 100);
				bean.FloatBoxed = 100F;
				builder.WithAssertion(bean).Expect(fields, true, false, true, false, true, false, true, true);

				bean = new SupportBean(null, 100);
				bean.FloatBoxed = null;
				builder.WithAssertion(bean).Expect(fields, false, false, false, false, true, false, true, false);

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreInstanceofStringAndNullOM : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "select " + 
				               "instanceof(TheString,string) as t0, " +
				               "instanceof(TheString,float,string,int) as t1 " +
				               "from SupportBean";

				var model = new EPStatementObjectModel();
				model.SelectClause = SelectClause.Create()
					.Add(Expressions.InstanceOf("TheString", "string"), "t0")
					.Add(Expressions.InstanceOf(Expressions.Property("TheString"), "float", "string", "int"), "t1");
				model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).Name));
				model = SerializableObjectCopier.GetInstance(env.Container).Copy(model);
				Assert.AreEqual(stmtText, model.ToEPL());

				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				env.CompileDeploy(model).AddListener("s0").Milestone(0);

				env.SendEventBean(new SupportBean("abc", 100));
				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				Assert.IsTrue((Boolean) theEvent.Get("t0"));
				Assert.IsTrue((Boolean) theEvent.Get("t1"));

				env.SendEventBean(new SupportBean(null, 100));
				theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				Assert.IsFalse((Boolean) theEvent.Get("t0"));
				Assert.IsFalse((Boolean) theEvent.Get("t1"));

				env.UndeployAll();
			}
		}

		private class ExprCoreInstanceofStringAndNullCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select instanceof(TheString,string) as t0, " +
				          "instanceof(TheString,float,string,int) as t1 " +
				          "from SupportBean";
				env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

				env.SendEventBean(new SupportBean("abc", 100));
				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				Assert.IsTrue((Boolean) theEvent.Get("t0"));
				Assert.IsTrue((Boolean) theEvent.Get("t1"));

				env.SendEventBean(new SupportBean(null, 100));
				theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				Assert.IsFalse((Boolean) theEvent.Get("t0"));
				Assert.IsFalse((Boolean) theEvent.Get("t1"));

				env.UndeployAll();
			}
		}

		private class ExprCoreDynamicPropertyJavaTypes : RegressionExecution
		{

			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select instanceof(item?, string) as t0, " +
				          " instanceof(item?, int) as t1, " +
				          " instanceof(item?, System.Single) as t2, " +
				          " instanceof(item?, System.Single, char, byte) as t3, " +
				          " instanceof(item?, System.Int32) as t4, " +
				          " instanceof(item?, long) as t5, " +
				          " instanceof(item?, long, long, System.Object) as t6, " +
				          " instanceof(item?, long, float) as t7 " +
				          " from SupportBeanDynRoot";

				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBeanDynRoot("abc"));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {true, false, false, false, false, false, false, false});

				env.SendEventBean(new SupportBeanDynRoot(100f));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, false, true, true, false, false, true, true});

				env.SendEventBean(new SupportBeanDynRoot(null));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, false, false, false, false, false, false, false});

				env.SendEventBean(new SupportBeanDynRoot(10));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, true, false, false, true, false, true, false});

				env.SendEventBean(new SupportBeanDynRoot(99L));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, false, false, false, false, true, true, true});

				env.UndeployAll();
			}
		}

		private class ExprCoreDynamicSuperTypeAndInterface : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select" +
				          " instanceof(item?, " + typeof(SupportMarkerInterface).Name + ") as t0, " +
				          " instanceof(item?, " + typeof(ISupportA).Name + ") as t1, " +
				          " instanceof(item?, " + typeof(ISupportBaseAB).Name + ") as t2, " +
				          " instanceof(item?, " + typeof(ISupportBaseABImpl).Name + ") as t3, " +
				          " instanceof(item?, " + typeof(ISupportA).Name + ", " + typeof(ISupportB).Name +") as t4, " +
				          " instanceof(item?, " + typeof(ISupportBaseAB).Name + ", " + typeof(ISupportB).Name + ") as t5, " +
				          " instanceof(item?, " + typeof(ISupportAImplSuperG).Name + ", " + typeof(ISupportB).Name + ") as t6, " +
				          " instanceof(item?, " + typeof(ISupportAImplSuperGImplPlus).Name + ", " + typeof(SupportBeanAtoFBase).Name + ") as t7 " +
				          " from SupportBeanDynRoot";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBeanDynRoot(new SupportBeanDynRoot("abc")));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {true, false, false, false, false, false, false, false});

				env.SendEventBean(new SupportBeanDynRoot(new ISupportAImplSuperGImplPlus()));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, true, true, false, true, true, true, true});

				env.SendEventBean(new SupportBeanDynRoot(new ISupportAImplSuperGImpl("", "", "")));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, true, true, false, true, true, true, false});

				env.SendEventBean(new SupportBeanDynRoot(new ISupportBaseABImpl("")));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, false, true, true, false, true, false, false});

				env.SendEventBean(new SupportBeanDynRoot(new ISupportBImpl("", "")));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, false, true, false, true, true, true, false});

				env.SendEventBean(new SupportBeanDynRoot(new ISupportAImpl("", "")));
				AssertResults(env.Listener("s0").AssertOneGetNewAndReset(), new[] {false, true, true, false, true, true, false, false});

				env.UndeployAll();
			}
		}

		private static void AssertResults(
			EventBean theEvent,
			bool[] result)
		{
			for (var i = 0; i < result.Length; i++) {
				Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
			}
		}
	}
} // end of namespace
