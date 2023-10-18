///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client.option;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.clazz
{
	public class ExprClassStaticMethod
	{

		public static ICollection<RegressionExecution> Executions()
		{
			IList<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprClassStaticMethodLocal(false));
			executions.Add(new ExprClassStaticMethodLocal(true));
			executions.Add(new ExprClassStaticMethodCreate(false));
			executions.Add(new ExprClassStaticMethodCreate(true));
			executions.Add(new ExprClassStaticMethodCreateCompileVsRuntime());
			executions.Add(new ExprClassStaticMethodLocalFAFQuery());
			executions.Add(new ExprClassStaticMethodCreateFAFQuery());
			executions.Add(new ExprClassStaticMethodLocalAndCreateClassTogether());
			executions.Add(new ExprClassDocSamples());
			executions.Add(new ExprClassInvalidCompile());
			executions.Add(new ExprClassStaticMethodCreateClassWithPackageName());
			executions.Add(new ExprClassCompilerInlinedClassInspectionOption());
			executions.Add(new ExprClassStaticMethodLocalWithPackageName());
			return executions;
		}

		private class ExprClassCompilerInlinedClassInspectionOption : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "inlined_class \"\"\"\n" +
				          "  import java.io.File;" +
				          "  import java.util.Arrays;" +
				          "  public class MyUtility {\n" +
				          "    public static void Fib(int n) {\n" +
				          "      Console.WriteLine(Arrays.AsList(new File(\".\").list()));\n" +
				          "    }\n" +
				          "  }\n" +
				          "\"\"\"\n" +
				          "@name('s0') select MyUtility.Fib(intPrimitive) from SupportBean";

				var support = new MySupportInlinedClassInspection();
				env.Compile(epl, compilerOptions => compilerOptions.InlinedClassInspection = support);

				env.AssertThat(
					() => {
						Assert.AreEqual(1, support.Contexts.Count);
						var ctx = support.Contexts[0];
						Assert.AreEqual("MyUtility", ctx.GetClassFiles()[0]); // .ThisClassName
					});
			}
		}

		private class ExprClassDocSamples : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "inlined_class \"\"\"\n" +
				          "  public class MyUtilityA {\n" +
				          "    public static double Fib(int n) {\n" +
				          "      if (n <= 1)\n" +
				          "        return n;\n" +
				          "      return Fib(n-1) + Fib(n-2);\n" +
				          "    }\n" +
				          "  }\n" +
				          "\"\"\"\n" +
				          "select MyUtilityA.Fib(IntPrimitive) from SupportBean";
				env.Compile(epl);

				var path = new RegressionPath();
				var eplCreate = "@public create inlined_class \"\"\" \n" +
				                "  public class MyUtilityB {\n" +
				                "    public static double MidPrice(double buy, double sell) {\n" +
				                "      return (buy + sell) / 2;\n" +
				                "    }\n" +
				                "  }\n" +
				                "\"\"\"";
				env.Compile(eplCreate, path);
				env.Compile("select MyUtilityB.MidPrice(DoublePrimitive, DoubleBoxed) from SupportBean", path);
			}
		}

		private class ExprClassStaticMethodLocalAndCreateClassTogether : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplClass = "inlined_class \"\"\"\n" +
				               "    public class MyUtil {\n" +
				               "        public static string ReturnBubba() {\n" +
				               "            return \"bubba\";\n" +
				               "        }\n" +
				               "    }\n" +
				               "\"\"\" \n" +
				               "@public create inlined_class \"\"\"\n" +
				               "    public class MyClass {\n" +
				               "        public static string DoIt() {\n" +
				               "            return \"|\" + MyUtil.ReturnBubba() + \"|\";\n" +
				               "        }\n" +
				               "    }\n" +
				               "\"\"\"\n";
				env.CompileDeploy(eplClass, path);

				var epl = "@name('s0') select MyClass.DoIt() as c0 from SupportBean\n";
				env.CompileDeploy(epl, path).AddListener("s0");

				SendSBAssert(env, "E1", 1, "|bubba|");

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodLocalWithPackageName : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') inlined_class \"\"\"\n" +
				          "namespace mypackage {" +
				          "    public class MyUtil {\n" +
				          "        public static string DoIt() {\n" +
				          "            return \"test\";\n" +
				          "        }\n" +
				          "    }\n" +
				          "}\n" +
				          "\"\"\" \n" +
				          "select mypackage.MyUtil.DoIt() as c0 from SupportBean\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendSBAssert(env, "E1", 1, "test");

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodCreateClassWithPackageName : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"create inlined_class \"\"\"\n" +
					"namespace mypackage {" +
					"    public class MyUtil {\n" +
					"        public static string DoIt(string TheString, int IntPrimitive) {\n" +
					"            return TheString + System.Convert.ToString(IntPrimitive);\n" +
					"        }\n" +
					"        }\n" +
					"    }\n" +
					"\"\"\";\n" +
					"@name('s0') select mypackage.MyUtil.DoIt(TheString, IntPrimitive) as c0 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendSBAssert(env, "E1", 1, "E11");

				env.UndeployAll();
			}
		}

		private class ExprClassStaticMethodCreateFAFQuery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplWindow =
					"@public create inlined_class \"\"\"\n" +
					"    public class MyClass {\n" +
					"        public static string DoIt(string parameter) {\n" +
					"            return \"abc\";\n" +
					"        }\n" +
					"    }\n" +
					"\"\"\";\n" +
					"@public create window MyWindow#keepall as (TheString string);\n" +
					"on SupportBean merge MyWindow insert select TheString;\n";
				env.CompileDeploy(eplWindow, path);

				env.SendEventBean(new SupportBean("E1", 1));

				var eplFAF = "select MyClass.DoIt(TheString) as c0 from MyWindow";
				var result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual("abc", result.Array[0].Get("c0"));

				env.Milestone(0);

				result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual("abc", result.Array[0].Get("c0"));

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.FIREANDFORGET);
			}
		}

		private class ExprClassStaticMethodLocalFAFQuery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplWindow = "@public create window MyWindow#keepall as (TheString string);\n" +
				                "on SupportBean merge MyWindow insert select TheString;\n";
				env.CompileDeploy(eplWindow, path);
				env.SendEventBean(new SupportBean("E1", 1));

				var eplFAF = "inlined_class \"\"\"\n" +
				             "namespace ${NAMESPACE} {\n" +
				             "    public class MyClass {\n" +
				             "        public static string DoIt(string parameter) {\n" +
				             "            return '>' + parameter + '<';\n" +
				             "        }\n" +
				             "    }\n" +
				             "}\n" +
				             "\"\"\"\n select ${NAMESPACE}.MyClass.DoIt(TheString) as c0 from MyWindow";
				var result = env.CompileExecuteFAF(eplFAF.Replace("${NAMESPACE}", NamespaceGenerator.Create()), path);
				Assert.AreEqual(">E1<", result.Array[0].Get("c0"));

				env.Milestone(0);

				result = env.CompileExecuteFAF(eplFAF.Replace("${NAMESPACE}", NamespaceGenerator.Create()), path);
				Assert.AreEqual(">E1<", result.Array[0].Get("c0"));

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.FIREANDFORGET);
			}
		}

		private class ExprClassInvalidCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// we allow empty class text
				env.Compile("inlined_class \"\"\" \"\"\" select * from SupportBean");

				// invalid class
				env.TryInvalidCompile(
					"inlined_class \"\"\" x \"\"\" select * from SupportBean",
					"Failed to compile an inlined-class: Line 1, Column 2: One of 'class enum interface @' expected instead of 'x' for class [\"\"\" x \"\"\"]");

				// invalid already deployed
				var path = new RegressionPath();
				var createClassEPL = "@public create inlined_class \"\"\" public class MyClass {}\"\"\"";
				env.Compile(createClassEPL, path);
				env.TryInvalidCompile(
					path,
					createClassEPL,
					"Class 'MyClass' has already been declared");

				// duplicate local class
				var eplDuplLocal =
					"inlined_class \"\"\" class MyDuplicate{} \"\"\" inlined_class \"\"\" class MyDuplicate{} \"\"\" select * from SupportBean";
				env.TryInvalidCompile(
					eplDuplLocal,
					"Failed to compile an inlined-class: Duplicate class by name 'MyDuplicate'");

				// duplicate local class and create-class class
				var eplDuplLocalAndCreate =
					"inlined_class \"\"\" class MyDuplicate{} \"\"\" create inlined_class \"\"\" class MyDuplicate{} \"\"\"";
				env.TryInvalidCompile(
					eplDuplLocalAndCreate,
					"Failed to compile an inlined-class: Duplicate class by name 'MyDuplicate'");

				// duplicate create-class class
				var eplDuplCreate = "create inlined_class \"\"\" public class MyDuplicate{} \"\"\";\n" +
				                    "create inlined_class \"\"\" public class MyDuplicate{} \"\"\";\n";
				env.TryInvalidCompile(eplDuplCreate, "Class 'MyDuplicate' has already been declared");
			}
		}

		private class ExprClassStaticMethodCreateCompileVsRuntime : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var eplTemplate = "@public create inlined_class \"\"\"\n" +
				                  "namespace ${NAMESPACE} {\n" +
				                  "    public class MyClass {\n" +
				                  "        public static int DoIt(int parameter) {\n" +
				                  "            return ${REPLACE};\n" +
				                  "        }\n" +
				                  "    }\n" +
				                  "}\n" +
				                  "\"\"\"\n";

				var ns1 = NamespaceGenerator.Create();
				var ns2 = NamespaceGenerator.Create();
				var compiledReturnZero = env.Compile(
					eplTemplate
						.Replace("${REPLACE}", "0")
						.Replace("${NAMESPACE}", ns1));
				var compiledReturnPlusOne = env.Compile(
					eplTemplate
						.Replace("${REPLACE}", "parameter+1")
						.Replace("${NAMESPACE}", ns2));

				var path = new RegressionPath();
				path.Add(compiledReturnZero);

				var compiledQuery = env.Compile(
					$"@nName('s0') select {ns2}.MyClass.DoIt(IntPrimitive) as c0 from SupportBean;\n",
					path);
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
				var path = new RegressionPath();
				var namespc = NamespaceGenerator.Create();
				var eplClass = "@public create inlined_class \"\"\"\n" +
				               $"namespace {namespc} {{\n" +
				               "    public class MyClass {\n" +
				               "        public static string DoIt(string parameter) {\n" +
				               "            return \"|\" + parameter + \"|\";\n" +
				               "        }\n" +
				               "    }\n" +
				               "}\n" +
				               "\"\"\"\n";
				env.CompileDeploy(_soda, eplClass, path);
				env.CompileDeploy(
					_soda,
					$"@name('s0') select {namespc}.MyClass.DoIt(TheString) as c0 from SupportBean",
					path);
				env.AddListener("s0");

				SendSBAssert(env, "E1", 0, "|E1|");

				env.Milestone(0);

				SendSBAssert(env, "E2", 0, "|E2|");

				env.UndeployAll();
			}

			public string Name()
			{
				return this.GetType().Name +
				       "{" +
				       "soda=" +
				       _soda +
				       '}';
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
				var nsp = NamespaceGenerator.Create();
				var epl = "@Name('s0') inlined_class \"\"\"\n" +
				          $"namespace {nsp} {{\n" +
				          "    public class MyClass {\n" +
				          "        public static string DoIt(string parameter) {\n" +
				          "            return \"|\" + parameter + \"|\";\n" +
				          "        }\n" +
				          "    }\n" +
				          "}\n" +
				          "\"\"\" " +
				          $"select {nsp}.MyClass.DoIt(TheString) as c0 from SupportBean\n";
				env.CompileDeploy(_soda, epl).AddListener("s0");

				SendSBAssert(env, "E1", 0, "|E1|");

				env.Milestone(0);

				SendSBAssert(env, "E2", 0, "|E2|");

				env.UndeployAll();
			}

			public string Name()
			{
				return $"{this.GetType().Name}{{soda={_soda}}}";
			}
		}

		private static void SendSBAssert(
			RegressionEnvironment env,
			string theString,
			int intPrimitive,
			object expected)
		{
			env.SendEventBean(new SupportBean(theString, intPrimitive));
			env.AssertEqualsNew("s0", "c0", expected);
		}

		private class MyClass
		{
			public string DoIt(string parameter)
			{
				return "|" + parameter + "|";
			}
		}

		private class MySupportInlinedClassInspection : InlinedClassInspectionOption
		{
			private IList<InlinedClassInspectionContext> contexts = new List<InlinedClassInspectionContext>();

			public IList<InlinedClassInspectionContext> Contexts => contexts;

			public void Visit(InlinedClassInspectionContext env)
			{
				contexts.Add(env);
			}
		}
	}
} // end of namespace
