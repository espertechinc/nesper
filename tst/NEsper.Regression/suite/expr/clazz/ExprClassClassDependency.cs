///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
			var execs = new List<RegressionExecution>();
			WithAllLocal(execs);
			WithInvalid(execs);
			WithClasspath(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithClasspath(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprClassClassDependencyClasspath());
			return execs;
		}

		public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprClassClassDependencyInvalid());
			return execs;
		}

		public static IList<RegressionExecution> WithAllLocal(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprClassClassDependencyAllLocal());
			return execs;
		}

		private class ExprClassClassDependencyClasspath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var exprClassClassDependency = typeof(ExprClassClassDependency);
				
				var eplNoImport = "@name('s0') " +
				                  "inlined_class \"\"\"\n" +
				                  "    public class MyUtilX {\n" +
				                  "        public static string DoIt(string parameter) {\n" +
				                  $"            return {exprClassClassDependency.FullName}.SupportQuoteString(parameter);\n" +
				                  "        }\n" +
				                  "    }\n" +
				                  "\"\"\" \n" +
				                  "select MyUtilX.DoIt(TheString) as c0 from SupportBean\n";
				RunAssertion(env, eplNoImport);

				var eplImport = "@name('s0') " +
				                "inlined_class \"\"\"\n" +
				                $"    using {exprClassClassDependency.Namespace};\n" +
				                "    public class MyUtilY {\n" +
				                "        public static string DoIt(string parameter) {\n" +
				                $"            return {exprClassClassDependency}.SupportQuoteString(parameter);\n" +
				                "        }\n" +
				                "    }\n" +
				                "\"\"\" \n" +
				                "select MyUtilY.DoIt(TheString) as c0 from SupportBean\n";
				RunAssertion(env, eplImport);
			}

			private void RunAssertion(
				RegressionEnvironment env,
				string epl)
			{
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				env.AssertEqualsNew("s0", "c0", "'E1'");

				env.UndeployAll();
			}
		}

		private class ExprClassClassDependencyInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// Class depending on create-class class
				var path = new RegressionPath();
				var epl = "@public create inlined_class \"\"\"\n" +
				          "    public class MyUtilX {\n" +
				          "        public static string SomeFunction(string parameter) {\n" +
				          "            return \"|\" + parameter + \"|\";\n" +
				          "        }\n" +
				          "    }\n" +
				          "\"\"\"";
				var compiled = env.Compile(epl);
				path.Add(compiled);

				var eplInvalid = "inlined_class \"\"\"\n" +
				                 "    public class MyClassY {\n" +
				                 "        public static string DoIt(string parameter) {\n" +
				                 "            return MyUtil.SomeFunction(\">\" + parameter + \"<\");\n" +
				                 "        }\n" +
				                 "    }\n" +
				                 "\"\"\" \n" +
				                 "select MyClassY.DoIt(TheString) as c0 from SupportBean\n";
				env.TryInvalidCompile(
					path,
					eplInvalid,
					"Exception processing statement: " +
					"Failure during module compilation: " +
					"[(4,20): error CS0103: The name 'MyUtil' does not exist in the current context]");

				// create-class depending on create-class
				eplInvalid = "create inlined_class \"\"\"\n" +
				             "    public class MyClassZ {\n" +
				             "        public static string DoIt(string parameter) {\n" +
				             "            return MyUtil.SomeFunction(\">\" + parameter + \"<\");\n" +
				             "        }\n" +
				             "    }\n" +
				             "\"\"\"";
				env.TryInvalidCompile(
					path,
					eplInvalid,
					"Exception processing statement: " +
					"Failure during module compilation: " +
					"[(4,20): error CS0103: The name 'MyUtil' does not exist in the current context]");
			}
		}

		private class ExprClassClassDependencyAllLocal : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') " +
				          "inlined_class \"\"\"\n" +
				          "    public class MyUtil {\n" +
				          "        public static string SomeFunction(string parameter) {\n" +
				          "            return \"|\" + parameter + \"|\";\n" +
				          "        }\n" +
				          "    }\n" +
				          "\"\"\" \n" +
				          "inlined_class \"\"\"\n" +
				          "    public class MyClass {\n" +
				          "        public static string DoIt(string parameter) {\n" +
				          "            return MyUtil.SomeFunction(\">\" + parameter + \"<\");\n" +
				          "        }\n" +
				          "    }\n" +
				          "\"\"\" \n" +
				          "select MyClass.DoIt(TheString) as c0 from SupportBean\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				env.AssertEqualsNew("s0", "c0", "|>E1<|");

				env.UndeployAll();
			}
		}

		public static string SupportQuoteString(string s)
		{
			return "'" + s + "'";
		}
	}
} // end of namespace
