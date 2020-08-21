///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.clazz
{
	public class ExprClassTypeUse
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprClassTypeUseEnum());
			executions.Add(new ExprClassTypeConst());
			executions.Add(new ExprClassTypeInnerClass());
			executions.Add(new ExprClassTypeNewKeyword());
			return executions;
		}

		private class ExprClassTypeNewKeyword : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "public class MyResult {\n" +
				              "  private final String id;\n" +
				              "  public MyResult(String id) {this.id = id;}\n" +
				              "  public String getId() {return id;}\n" +
				              "}";
				string epl = EscapeClass(text) +
				             "@Name('s0') select new MyResult(TheString) as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 0));
				object result = env.Listener("s0").AssertOneGetNewAndReset().Get("c0");
				try {
					Assert.That(result.GetType().GetProperty("Id").GetValue(result), Is.EqualTo("E1"));
				}
				catch (Exception ex) {
					Assert.Fail(ex.Message);
				}

				env.UndeployAll();
			}
		}

		private class ExprClassTypeInnerClass : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "public class MyConstants {\n" +
				              "  public static class MyInnerClass {" +
				              "    public final static String VALUE = \"abc\";\n" +
				              "  }" +
				              "}";
				string epl = EscapeClass(text) +
				             "@Name('s0') select MyConstants$MyInnerClass.VALUE as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				SendSBAssert(env, "E1", 0, "abc");

				env.UndeployAll();
			}
		}

		private class ExprClassTypeConst : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "public class MyConstants {\n" +
				              "  public final static String VALUE = \"test\";\n" +
				              "}";
				string epl = EscapeClass(text) +
				             "@Name('s0') select MyConstants.VALUE as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				SendSBAssert(env, "E1", 0, "test");

				env.UndeployAll();
			}
		}

		private class ExprClassTypeUseEnum : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "public enum MyLevel {\n" +
				              "  HIGH(3), MEDIUM(2), LOW(1);\n" +
				              "  final int levelCode;\n" +
				              "  MyLevel(int levelCode) {this.levelCode = levelCode;}\n" +
				              "  public int getLevelCode() {return levelCode;}\n" +
				              "}";
				string epl = EscapeClass(text) +
				             "@Name('s0') select MyLevel.MEDIUM.getLevelCode() as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				SendSBAssert(env, "E1", 0, 2);

				env.UndeployAll();
			}
		}

		private static string EscapeClass(string text)
		{
			return "inlined_class \"\"\"\n" + text + "\"\"\" \n";
		}

		private static void SendSBAssert(
			RegressionEnvironment env,
			string theString,
			int intPrimitive,
			object expected)
		{
			env.SendEventBean(new SupportBean(theString, intPrimitive));
			Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
		}
	}
} // end of namespace
