///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.hook.singlerowfunc;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendUDFInlinedClass
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientExtendUDFInlinedLocalClass(false));
			execs.Add(new ClientExtendUDFInlinedLocalClass(true));
			execs.Add(new ClientExtendUDFInlinedInvalid());
			execs.Add(new ClientExtendUDFInlinedFAF());
			execs.Add(new ClientExtendUDFCreateInlinedSameModule());
			execs.Add(new ClientExtendUDFCreateInlinedOtherModule());
			execs.Add(new ClientExtendUDFInlinedWOptions());
			execs.Add(new ClientExtendUDFOverloaded());
			return execs;
		}

		private class ClientExtendUDFOverloaded : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') inlined_class \"\"\"\n" +
				             "  import com.espertech.esper.common.client.hook.singlerowfunc.*;\n" +
				             "  @ExtensionSingleRowFunction(name=\"multiply\", methodName=\"multiplyIt\")\n" +
				             "  public class MultiplyHelper {\n" +
				             "    public static int multiplyIt(int a, int b) {\n" +
				             "      return a*b;\n" +
				             "    }\n" +
				             "    public static int multiplyIt(int a, int b, int c) {\n" +
				             "      return a*b*c;\n" +
				             "    }\n" +
				             "  }\n" +
				             "\"\"\" " +
				             "select multiply(IntPrimitive,IntPrimitive) as c0, multiply(IntPrimitive,IntPrimitive,IntPrimitive) as c1 \n" +
				             " from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 4));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0,c1".SplitCsv(), new object[] {16, 64});

				env.UndeployAll();
			}
		}

		private class ClientExtendUDFInlinedWOptions : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') inlined_class \"\"\"\n" +
				             "  @" +
				             typeof(ExtensionSingleRowFunctionAttribute).FullName +
				             "(" +
				             "      name=\"multiply\", methodName=\"multiplyIfPositive\",\n" +
				             "      valueCache=" +
				             typeof(ConfigurationCompilerPlugInSingleRowFunction).FullName +
				             ".ValueCache.DISABLED,\n" +
				             "      filterOptimizable=" +
				             typeof(ConfigurationCompilerPlugInSingleRowFunction).FullName +
				             ".FilterOptimizable.DISABLED,\n" +
				             "      rethrowExceptions=false,\n" +
				             "      eventTypeName=\"abc\"\n" +
				             "      )\n" +
				             "  public class MultiplyHelper {\n" +
				             "    public static int multiplyIfPositive(int a, int b) {\n" +
				             "      return a*b;\n" +
				             "    }\n" +
				             "  }\n" +
				             "\"\"\" " +
				             "select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssertIntMultiply(env, 5, 25);

				env.UndeployAll();
			}
		}

		private class ClientExtendUDFCreateInlinedOtherModule : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplCreateInlined = "@Name('clazz') @public create inlined_class \"\"\"\n" +
				                          "  @" +
				                          typeof(ExtensionSingleRowFunctionAttribute).FullName +
				                          "(name=\"multiply\", methodName=\"multiply\")\n" +
				                          "  public class MultiplyHelper {\n" +
				                          "    public static int multiply(int a, int b) {\n" +
				                          "      %BEHAVIOR%\n" +
				                          "    }\n" +
				                          "  }\n" +
				                          "\"\"\"\n;";
				RegressionPath path = new RegressionPath();
				env.Compile(eplCreateInlined.Replace("%BEHAVIOR%", "return -1;"), path);

				string eplSelect = "@Name('s0') select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean";
				EPCompiled compiledSelect = env.Compile(eplSelect, path);

				env.CompileDeploy(eplCreateInlined.Replace("%BEHAVIOR%", "return a*b;"));
				env.Deploy(compiledSelect).AddListener("s0");

				SendAssertIntMultiply(env, 3, 9);

				env.Milestone(0);

				SendAssertIntMultiply(env, 13, 13 * 13);

				// assert dependencies
				SupportDeploymentDependencies.AssertSingle(env, "s0", "clazz", EPObjectType.CLASSPROVIDED, "MultiplyHelper");

				env.UndeployAll();
			}
		}

		private class ClientExtendUDFCreateInlinedSameModule : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create inlined_class \"\"\"\n" +
				             "  @" +
				             typeof(ExtensionSingleRowFunctionAttribute).FullName +
				             "(name=\"multiply\", methodName=\"multiply\")\n" +
				             "  public class MultiplyHelper {\n" +
				             "    public static int multiply(int a, int b) {\n" +
				             "      return a*b;\n" +
				             "    }\n" +
				             "  }\n" +
				             "\"\"\"\n;" +
				             "@Name('s0') select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssertIntMultiply(env, 5, 25);

				env.Milestone(0);

				SendAssertIntMultiply(env, 6, 36);

				SupportDeploymentDependencies.AssertEmpty(env, "s0");

				env.UndeployAll();
			}
		}

		private class ClientExtendUDFInlinedFAF : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplWindow = "create window MyWindow#keepall as (TheString string);\n" +
				                   "on SupportBean merge MyWindow insert select TheString;\n";
				env.CompileDeploy(eplWindow, path);

				env.SendEventBean(new SupportBean("E1", 1));

				string eplFAF = "inlined_class \"\"\"\n" +
				                "  @" +
				                typeof(ExtensionSingleRowFunctionAttribute).FullName +
				                "(name=\"appendDelimiters\", methodName=\"doIt\")\n" +
				                "  public class MyClass {\n" +
				                "    public static String doIt(String parameter) {\n" +
				                "      return '>' + parameter + '<';\n" +
				                "    }\n" +
				                "  }\n" +
				                "\"\"\"\n select appendDelimiters(TheString) as c0 from MyWindow";
				EPFireAndForgetQueryResult result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual(">E1<", result.Array[0].Get("c0"));

				env.Milestone(0);

				result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual(">E1<", result.Array[0].Get("c0"));

				env.UndeployAll();
			}
		}

		private class ClientExtendUDFInlinedInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') inlined_class \"\"\"\n" +
				             "  @" +
				             typeof(ExtensionSingleRowFunctionAttribute).FullName +
				             "(name=\"multiply\", methodName=\"multiply\")\n" +
				             "  public class MultiplyHelperOne {\n" +
				             "    public static int multiply(int a, int b) { return 0; }\n" +
				             "  }\n" +
				             "  @" +
				             typeof(ExtensionSingleRowFunctionAttribute).FullName +
				             "(name=\"multiply\", methodName=\"multiply\")\n" +
				             "  public class MultiplyHelperTwo {\n" +
				             "    public static int multiply(int a, int b, int c) { return 0; }\n" +
				             "  }\n" +
				             "\"\"\" " +
				             "select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean";
				TryInvalidCompile(
					env,
					epl,
					"The plug-in single-row function 'multiply' occurs multiple times");
			}
		}

		private class ClientExtendUDFInlinedLocalClass : RegressionExecution
		{
			private readonly bool soda;

			public ClientExtendUDFInlinedLocalClass(bool soda)
			{
				this.soda = soda;
			}

			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') inlined_class \"\"\"\n" +
				             "  @" +
				             typeof(ExtensionSingleRowFunctionAttribute).FullName +
				             "(name=\"multiply\", methodName=\"multiply\")\n" +
				             "  public class MultiplyHelper {\n" +
				             "    public static int multiply(int a, int b) {\n" +
				             "      return a*b;\n" +
				             "    }\n" +
				             "  }\n" +
				             "\"\"\" " +
				             "select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean";
				env.CompileDeploy(soda, epl).AddListener("s0");

				SendAssertIntMultiply(env, 5, 25);

				env.Milestone(0);

				SendAssertIntMultiply(env, 6, 36);

				env.UndeployAll();
			}
		}

		private static void SendAssertIntMultiply(
			RegressionEnvironment env,
			int intPrimitive,
			int expected)
		{
			env.SendEventBean(new SupportBean("E1", intPrimitive));
			Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
		}
	}
} // end of namespace
