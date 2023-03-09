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
			List<RegressionExecution> execs = new List<RegressionExecution>();
			WithUseEnum(execs);
			WithConst(execs);
			WithInnerClass(execs);
			WithNewKeyword(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithNewKeyword(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprClassTypeNewKeyword());
			return execs;
		}

		public static IList<RegressionExecution> WithInnerClass(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprClassTypeInnerClass());
			return execs;
		}

		public static IList<RegressionExecution> WithConst(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprClassTypeConst());
			return execs;
		}

		public static IList<RegressionExecution> WithUseEnum(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprClassTypeUseEnum());
			return execs;
		}

		private class ExprClassTypeNewKeyword : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "public class MyResult {\n" +
				              "  private readonly string _id;\n" +
				              "  public MyResult(string id) {this._id = id;}\n" +
				              "  public string Id => _id;\n" +
				              "}";
				string epl = EscapeClass(text) +
				             "@Name('s0') select new MyResult(TheString) as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 0));
				var result = env.Listener("s0").AssertOneGetNewAndReset().Get("c0");

				Assert.That(result.GetType().GetProperty("Id").GetValue(result), Is.EqualTo("E1"));

				env.UndeployAll();
			}
		}

		private class ExprClassTypeInnerClass : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "public class MyConstants {\n" +
				              "  public class MyInnerClass {" +
				              "    public static readonly string VALUE = \"abc\";\n" +
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
				              "  public static readonly string VALUE = \"test\";\n" +
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
				              "  HIGH, MEDIUM, LOW\n" +
				              "}\n" +
				              "public static class MyLevelExtensions {\n" +
				              "  public static int GetLevelCode(this MyLevel value) {\n" +
				              "    switch(value) {\n" +
				              "        case MyLevel.HIGH: return 3;\n" +
				              "        case MyLevel.MEDIUM: return 2;\n" +
				              "        case MyLevel.LOW: return 1;\n" +
				              "        default: throw new System.ArgumentException(nameof(value));\n" +
				              "    }\n" +
				              "  }\n" +
				              "}";
				string epl = EscapeClass(text) +
				             "@Name('s0') select MyLevel.MEDIUM.GetLevelCode() as c0 from SupportBean";
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
