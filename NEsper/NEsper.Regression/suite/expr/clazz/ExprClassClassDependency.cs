///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.clazz
{
	public class ExprClassClassDependency
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprClassClassDependencyAllLocal());
			executions.Add(new ExprClassClassDependencyInvalid());
			executions.Add(new ExprClassClassDependencyClasspath());
			return executions;
		}

		private class ExprClassClassDependencyClasspath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string eplNoImport = "@Name('s0') " +
				                     "inlined_class \"\"\"\n" +
				                     "    public class MyUtil {\n" +
				                     "        public static String DoIt(String parameter) {\n" +
				                     "            return " + typeof(ExprClassClassDependency).FullName + ".SupportQuoteString(parameter);\n" +
				                     "        }\n" +
				                     "    }\n" +
				                     "\"\"\" \n" +
				                     "select MyUtil.DoIt(TheString) as c0 from SupportBean\n";
				RunAssertion(env, eplNoImport);

				string eplImport = "@Name('s0') " +
				                   "inlined_class \"\"\"\n" +
				                   "    import " +
				                   typeof(ExprClassClassDependency).FullName +
				                   ";" +
				                   "    public class MyUtil {\n" +
				                   "        public static String DoIt(String parameter) {\n" +
				                   "            return " + typeof(ExprClassClassDependency).Name + ".SupportQuoteString(parameter);\n" +
				                   "        }\n" +
				                   "    }\n" +
				                   "\"\"\" \n" +
				                   "select MyUtil.DoIt(TheString) as c0 from SupportBean\n";
				RunAssertion(env, eplImport);
			}

			private void RunAssertion(
				RegressionEnvironment env,
				string epl)
			{
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				Assert.AreEqual("'E1'", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		private class ExprClassClassDependencyInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// Class depending on create-class class
				RegressionPath path = new RegressionPath();
				string epl = "@public create inlined_class \"\"\"\n" +
				             "    public class MyUtil {\n" +
				             "        public static String SomeFunction(String parameter) {\n" +
				             "            return \"|\" + parameter + \"|\";\n" +
				             "        }\n" +
				             "    }\n" +
				             "\"\"\"";
				EPCompiled compiled = env.Compile(epl);
				path.Add(compiled);

				string eplInvalid = "inlined_class \"\"\"\n" +
				                    "    public class MyClass {\n" +
				                    "        public static string DoIt(String parameter) {\n" +
				                    "            return MyUtil.SomeFunction(\">\" + parameter + \"<\");\n" +
				                    "        }\n" +
				                    "    }\n" +
				                    "\"\"\" \n" +
				                    "select MyClass.DoIt(TheString) as c0 from SupportBean\n";
				TryInvalidCompile(env, path, eplInvalid, "Failed to compile class: Line 4, Column 27: Unknown variable or type \"MyUtil\" for class");

				// create-class depending on create-class
				eplInvalid = "create inlined_class \"\"\"\n" +
				             "    public class MyClass {\n" +
				             "        public static string DoIt(String parameter) {\n" +
				             "            return MyUtil.SomeFunction(\">\" + parameter + \"<\");\n" +
				             "        }\n" +
				             "    }\n" +
				             "\"\"\"";
				TryInvalidCompile(env, path, eplInvalid, "Failed to compile class: Line 4, Column 27: Unknown variable or type \"MyUtil\" for class");
			}
		}

		private class ExprClassClassDependencyAllLocal : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0') " +
				             "inlined_class \"\"\"\n" +
				             "    public class MyUtil {\n" +
				             "        public static string SomeFunction(String parameter) {\n" +
				             "            return \"|\" + parameter + \"|\";\n" +
				             "        }\n" +
				             "    }\n" +
				             "\"\"\" \n" +
				             "inlined_class \"\"\"\n" +
				             "    public class MyClass {\n" +
				             "        public static String DoIt(String parameter) {\n" +
				             "            return MyUtil.SomeFunction(\">\" + parameter + \"<\");\n" +
				             "        }\n" +
				             "    }\n" +
				             "\"\"\" \n" +
				             "select MyClass.DoIt(TheString) as c0 from SupportBean\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				Assert.AreEqual("|>E1<|", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		public static string SupportQuoteString(string s)
		{
			return "'" + s + "'";
		}
	}
} // end of namespace
