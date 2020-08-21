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
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.clazz
{
	public class ExprClassStaticMethod
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprClassStaticMethodLocal(false));
			executions.Add(new ExprClassStaticMethodLocal(true));
			executions.Add(new ExprClassStaticMethodCreate(false));
			executions.Add(new ExprClassStaticMethodCreate(true));
			executions.Add(new ExprClassStaticMethodCreateCompileVsRuntime());
			executions.Add(new ExprClassStaticMethodLocalFAFQuery());
			executions.Add(new ExprClassStaticMethodLocalFAFQuery());
			executions.Add(new ExprClassStaticMethodCreateFAFQuery());
			executions.Add(new ExprClassStaticMethodLocalWithPackageName());
			executions.Add(new ExprClassStaticMethodCreateClassWithPackageName());
			executions.Add(new ExprClassStaticMethodLocalAndCreateClassTogether());
			executions.Add(new ExprClassInvalidCompile());
			executions.Add(new ExprClassDocSamples());
			return executions;
		}

		private class ExprClassDocSamples : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "inlined_class \"\"\"\n" +
				             "  public class MyUtility {\n" +
				             "    public static double fib(int n) {\n" +
				             "      if (n <= 1)\n" +
				             "        return n;\n" +
				             "      return fib(n-1) + fib(n-2);\n" +
				             "    }\n" +
				             "  }\n" +
				             "\"\"\"\n" +
				             "select MyUtility.fib(IntPrimitive) from SupportBean";
				env.Compile(epl);

				RegressionPath path = new RegressionPath();
				string eplCreate = "create inlined_class \"\"\" \n" +
				                   "  public class MyUtility {\n" +
				                   "    public static double midPrice(double buy, double sell) {\n" +
				                   "      return (buy + sell) / 2;\n" +
				                   "    }\n" +
				                   "  }\n" +
				                   "\"\"\"";
				env.Compile(eplCreate, path);
				env.Compile("select MyUtility.midPrice(DoublePrimitive, DoubleBoxed) from SupportBean", path);
			}
		}

		private class ExprClassStaticMethodLocalAndCreateClassTogether : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplClass = "inlined_class \"\"\"\n" +
				                  "    public class MyUtil {\n" +
				                  "        public static String returnBubba() {\n" +
				                  "            return \"bubba\";\n" +
				                  "        }\n" +
				                  "    }\n" +
				                  "\"\"\" \n" +
				                  "@public create inlined_class \"\"\"\n" +
				                  "    public class MyClass {\n" +
				                  "        public static String doIt() {\n" +
				                  "            return \"|\" + MyUtil.returnBubba() + \"|\";\n" +
				                  "        }\n" +
				                  "    }\n" +
				                  "\"\"\"\n";
				env.CompileDeploy(eplClass, path);

				string epl = "@Name('s0') select MyClass.doIt() as c0 from SupportBean\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				SendSBAssert(env, "E1", 1, "|bubba|");

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodLocalWithPackageName : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') inlined_class \"\"\"\n" +
				             "    package mypackage;" +
				             "    public class MyUtil {\n" +
				             "        public static String doIt() {\n" +
				             "            return \"test\";\n" +
				             "        }\n" +
				             "    }\n" +
				             "\"\"\" \n" +
				             "select mypackage.MyUtil.doIt() as c0 from SupportBean\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendSBAssert(env, "E1", 1, "test");

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodCreateClassWithPackageName : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl =
					"create inlined_class \"\"\"\n" +
					"    package mypackage;" +
					"    public class MyUtil {\n" +
					"        public static String doIt(String TheString, int IntPrimitive) {\n" +
					"            return TheString + Convert.ToString(IntPrimitive);\n" +
					"        }\n" +
					"    }\n" +
					"\"\"\";\n" +
					"@Name('s0') select mypackage.MyUtil.doIt(TheString, IntPrimitive) as c0 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendSBAssert(env, "E1", 1, "E11");

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodCreateFAFQuery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplWindow =
					"@public create inlined_class \"\"\"\n" +
					"    public class MyClass {\n" +
					"        public static String doIt(String parameter) {\n" +
					"            return \"abc\";\n" +
					"        }\n" +
					"    }\n" +
					"\"\"\";\n" +
					"create window MyWindow#keepall as (TheString string);\n" +
					"on SupportBean merge MyWindow insert select TheString;\n";
				env.CompileDeploy(eplWindow, path);

				env.SendEventBean(new SupportBean("E1", 1));

				string eplFAF = "select MyClass.doIt(TheString) as c0 from MyWindow";
				EPFireAndForgetQueryResult result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual("abc", result.Array[0].Get("c0"));

				env.Milestone(0);

				result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual("abc", result.Array[0].Get("c0"));

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodLocalFAFQuery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplWindow = "create window MyWindow#keepall as (TheString string);\n" +
				                   "on SupportBean merge MyWindow insert select TheString;\n";
				env.CompileDeploy(eplWindow, path);

				env.SendEventBean(new SupportBean("E1", 1));

				string eplFAF = "inlined_class \"\"\"\n" +
				                "    public class MyClass {\n" +
				                "        public static String doIt(String parameter) {\n" +
				                "            return '>' + parameter + '<';\n" +
				                "        }\n" +
				                "    }\n" +
				                "\"\"\"\n select MyClass.doIt(TheString) as c0 from MyWindow";
				EPFireAndForgetQueryResult result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual(">E1<", result.Array[0].Get("c0"));

				env.Milestone(0);

				result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual(">E1<", result.Array[0].Get("c0"));

				env.UndeployAll();
			}
		}

		private class ExprClassInvalidCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// we allow empty class text
				env.Compile("inlined_class \"\"\" \"\"\" select * from SupportBean");

				// invalid class
				TryInvalidCompile(
					env,
					"inlined_class \"\"\" x \"\"\" select * from SupportBean",
					"Failed to compile class: Line 1, Column 2: One of 'class enum interface @' expected instead of 'x' for class [\"\"\" x \"\"\"]");

				// invalid already deployed
				RegressionPath path = new RegressionPath();
				string createClassEPL = "create inlined_class \"\"\" public class MyClass {}\"\"\"";
				env.Compile(createClassEPL, path);
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					path,
					createClassEPL,
					"Class 'MyClass' has already been declared");

				// duplicate local class
				string eplDuplLocal =
					"inlined_class \"\"\" class MyDuplicate{} \"\"\" inlined_class \"\"\" class MyDuplicate{} \"\"\" select * from SupportBean";
				TryInvalidCompile(env, eplDuplLocal, "Duplicate class by name 'MyDuplicate'");

				// duplicate local class and create-class class
				string eplDuplLocalAndCreate = "inlined_class \"\"\" class MyDuplicate{} \"\"\" create inlined_class \"\"\" class MyDuplicate{} \"\"\"";
				TryInvalidCompile(env, eplDuplLocalAndCreate, "Duplicate class by name 'MyDuplicate'");

				// duplicate create-class class
				string eplDuplCreate = "create inlined_class \"\"\" public class MyDuplicate{} \"\"\";\n" +
				                       "create inlined_class \"\"\" public class MyDuplicate{} \"\"\";\n";
				TryInvalidCompile(env, eplDuplCreate, "Class 'MyDuplicate' has already been declared");
			}
		}

		private class ExprClassStaticMethodCreateCompileVsRuntime : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplTemplate = "@public create inlined_class \"\"\"\n" +
				                     "    public class MyClass {\n" +
				                     "        public static int doIt(int parameter) {\n" +
				                     "            return %REPLACE%;\n" +
				                     "        }\n" +
				                     "    }\n" +
				                     "\"\"\"\n";
				EPCompiled compiledReturnZero = env.Compile(eplTemplate.Replace("%REPLACE%", "0"));
				EPCompiled compiledReturnPlusOne = env.Compile(eplTemplate.Replace("%REPLACE%", "parameter+1"));

				RegressionPath path = new RegressionPath();
				path.Add(compiledReturnZero);
				EPCompiled compiledQuery = env.Compile("@Name('s0') select MyClass.doIt(IntPrimitive) as c0 from SupportBean;\n", path);
				env.Deploy(compiledReturnPlusOne);
				env.Deploy(compiledQuery).AddListener("s0");

				SendSBAssert(env, "E1", 10, 11);

				env.Milestone(0);

				SendSBAssert(env, "E2", 20, 21);

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodCreate : RegressionExecution
		{
			private readonly bool _soda;

			public ExprClassStaticMethodCreate(bool soda)
			{
				this._soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplClass = "@public create inlined_class \"\"\"\n" +
				                  "    public class MyClass {\n" +
				                  "        public static String doIt(String parameter) {\n" +
				                  "            return \"|\" + parameter + \"|\";\n" +
				                  "        }\n" +
				                  "    }\n" +
				                  "\"\"\"\n";
				env.CompileDeploy(_soda, eplClass, path);
				env.CompileDeploy(_soda, "@Name('s0') select MyClass.doIt(TheString) as c0 from SupportBean", path);
				env.AddListener("s0");

				SendSBAssert(env, "E1", 0, "|E1|");

				env.Milestone(0);

				SendSBAssert(env, "E2", 0, "|E2|");

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodLocal : RegressionExecution
		{
			private readonly bool _soda;

			public ExprClassStaticMethodLocal(bool soda)
			{
				this._soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') inlined_class \"\"\"\n" +
				             "    public class MyClass {\n" +
				             "        public static String doIt(String parameter) {\n" +
				             "            return \"|\" + parameter + \"|\";\n" +
				             "        }\n" +
				             "    }\n" +
				             "\"\"\" " +
				             "select MyClass.doIt(TheString) as c0 from SupportBean\n";
				env.CompileDeploy(_soda, epl).AddListener("s0");

				SendSBAssert(env, "E1", 0, "|E1|");

				env.Milestone(0);

				SendSBAssert(env, "E2", 0, "|E2|");

				env.UndeployAll();
			}
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

		private class MyClass
		{
			public string DoIt(string parameter)
			{
				return "|" + parameter + "|";
			}
		}
	}
} // end of namespace
