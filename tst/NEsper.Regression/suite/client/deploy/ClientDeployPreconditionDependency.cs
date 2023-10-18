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
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
	public class ClientDeployPreconditionDependency
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientVisibilityDeployDepClass());
			execs.Add(new ClientVisibilityDeployDepScript());
			execs.Add(new ClientVisibilityDeployDepVariablePreconfig());
			execs.Add(new ClientVisibilityDeployDepVariablePath());
			execs.Add(new ClientVisibilityDeployDepEventTypePreconfig());
			execs.Add(new ClientVisibilityDeployDepEventTypePath());
			execs.Add(new ClientVisibilityDeployDepNamedWindow());
			execs.Add(new ClientVisibilityDeployDepTable());
			execs.Add(new ClientVisibilityDeployDepExprDecl());
			execs.Add(new ClientVisibilityDeployDepContext());
			execs.Add(new ClientVisibilityDeployDepNamedWindowOfNamedModule());
			execs.Add(new ClientVisibilityDeployDepIndex());
			return execs;
		}

		public class ClientVisibilityDeployDepClass : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create inlined_class \"\"\" public class MyClass { public static String doIt() { return \"def\"; } }\"\"\";\n",
					path); // Note: not deploying, just adding to path

				var text = "dependency application-inlined class 'MyClass'";
				TryInvalidDeploy(env, path, "select MyClass.doIt() from SupportBean", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepEventTypePreconfig : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var configuration = env.Runtime.ConfigurationDeepCopy;
				configuration.Common.AddEventType(typeof(SomeEvent));

				EPCompiled compiled;
				try {
					compiled = env.Compiler.Compile("select * from SomeEvent", new CompilerArguments(configuration));
				}
				catch (EPCompileException e) {
					throw new EPRuntimeException(e);
				}

				TryInvalidDeploy(env, compiled, "pre-configured event type 'SomeEvent'");
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepVariablePreconfig : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var configuration = env.Runtime.ConfigurationDeepCopy;
				configuration.Common.AddVariable("mypublicvariable", typeof(string), null, true);
				configuration.Common.AddEventType(typeof(SupportBean));

				EPCompiled compiled;
				try {
					compiled = env.Compiler.Compile(
						"select mypublicvariable from SupportBean",
						new CompilerArguments(configuration));
				}
				catch (EPCompileException e) {
					throw new EPRuntimeException(e);
				}

				TryInvalidDeploy(env, compiled, "pre-configured variable 'mypublicvariable'");
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}

		}

		public class ClientVisibilityDeployDepVariablePath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create variable string somevariable = 'a'",
					path); // Note: not deploying, just adding to path

				var text = "dependency variable 'somevariable'";
				TryInvalidDeploy(env, path, "select somevariable from SupportBean", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepNamedWindow : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create window SimpleWindow#keepall as SupportBean",
					path); // Note: not deploying, just adding to path

				var text = "dependency named window 'SimpleWindow'";
				TryInvalidDeploy(env, path, "select * from SimpleWindow", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepTable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create table SimpleTable(col1 string)",
					path); // Note: not deploying, just adding to path

				var text = "dependency table 'SimpleTable'";
				TryInvalidDeploy(env, path, "select * from SimpleTable", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepExprDecl : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create expression someexpression { 0 }",
					path); // Note: not deploying, just adding to path

				var text = "dependency declared-expression 'someexpression'";
				TryInvalidDeploy(env, path, "select someexpression() from SupportBean", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepScript : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create expression double myscript(stringvalue) [0]",
					path); // Note: not deploying, just adding to path

				var text = "dependency script 'myscript'";
				TryInvalidDeploy(env, path, "select myscript('abc') from SupportBean", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepContext : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create context MyContext partition by theString from SupportBean",
					path); // Note: not deploying, just adding to path

				var text = "dependency context 'MyContext'";
				TryInvalidDeploy(env, path, "context MyContext select * from SupportBean", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepIndex : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				string text;

				// Table
				env.CompileDeploy(
					"@name('infra') @public create table MyTable(col1 string primary key, col2 string)",
					path);
				env.Compile(
					"@name('infra') create index MyIndexForTable on MyTable(col2)",
					path); // Note: not deploying, just adding to path

				text = "dependency index 'MyIndexForTable'";
				TryInvalidDeploy(
					env,
					path,
					"select * from SupportBean as sb, MyTable as mt where mt.col2 = sb.theString",
					text);
				TryInvalidDeploy(
					env,
					path,
					"select * from SupportBean as sb where exists (select * from MyTable as mt where mt.col2 = sb.theString)",
					text);

				// Named Window
				env.CompileDeploy("@name('infra') @public create window MyWindow#keepall as SupportBean", path);
				env.Compile(
					"@name('infra') create index MyIndexForNW on MyWindow(intPrimitive)",
					path); // Note: not deploying, just adding to path

				text = "dependency index 'MyIndexForNW'";
				TryInvalidDeploy(
					env,
					path,
					"on SupportBean_S0 as sb update MyWindow as mw set theString='a' where sb.id = mw.intPrimitive",
					text);

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		public class ClientVisibilityDeployDepEventTypePath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.Compile(
					"@name('infra') @public create schema MySchema(col1 string)",
					path); // Note: not deploying, just adding to path

				var text = "dependency event type 'MySchema'";
				TryInvalidDeploy(env, path, "insert into MySchema select 'a' as col1 from SupportBean", text);
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		private class ClientVisibilityDeployDepNamedWindowOfNamedModule : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var windowABC = env.Compile("module ABC; create window MyWindow#keepall as SupportBean", path);
				path.Clear();

				env.Compile("module DEF; @public create window MyWindow#keepall as SupportBean", path);
				var insertDEF = env.Compile("select * from MyWindow", path);
				env.Deploy(windowABC);

				TryInvalidDeploy(env, insertDEF, "dependency named window 'MyWindow' module 'DEF'");

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		private static void TryInvalidDeploy(
			RegressionEnvironment env,
			RegressionPath path,
			string epl,
			string text)
		{
			var compiled = env.Compile(epl, path);
			TryInvalidDeploy(env, compiled, text);
		}

		private static void TryInvalidDeploy(
			RegressionEnvironment env,
			EPCompiled compiled,
			string text)
		{
			var message = "A precondition is not satisfied: Required " + text + " cannot be found";
			try {
				env.Runtime.DeploymentService.Deploy(compiled);
				Assert.Fail();
			}
			catch (EPDeployPreconditionException ex) {
				Assert.AreEqual(-1, ex.RolloutItemNumber);
				if (!message.Equals("skip")) {
					SupportMessageAssertUtil.AssertMessage(ex.Message, message);
				}
			}
			catch (EPDeployException ex) {
				Console.WriteLine(ex.StackTrace);
				Assert.Fail();
			}
		}

		[Serializable]
		public class SomeEvent
		{
		}
	}
} // end of namespace
