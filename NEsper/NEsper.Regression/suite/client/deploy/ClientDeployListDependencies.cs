///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
	public class ClientDeployListDependencies
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientDeployListDependenciesObjectTypes());
			execs.Add(new ClientDeployListDependenciesWModuleName());
			execs.Add(new ClientDeployListDependenciesNoDependencies());
			execs.Add(new ClientDeployListDependencyStar());
			execs.Add(new ClientDeployListDependenciesInvalid());
			return execs;
		}

		private class ClientDeployListDependencyStar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@name('typea') @public create schema TypeA()", path);
				env.CompileDeploy("@name('typeb') @public create schema TypeB()", path);
				env.CompileDeploy("@name('typec') @public create schema TypeC(a TypeA, b TypeB)", path);
				env.CompileDeploy("@name('typed') @public create schema TypeD(c TypeC)", path);
				env.CompileDeploy("@name('typee') @public create schema TypeE(c TypeC)", path);

				var a = env.DeploymentId("typea");
				var b = env.DeploymentId("typeb");
				var c = env.DeploymentId("typec");
				var d = env.DeploymentId("typed");
				var e = env.DeploymentId("typee");

				AssertProvided(env, a, MakeProvided(EPObjectType.EVENTTYPE, "TypeA", c));
				AssertConsumed(env, a);

				AssertProvided(env, b, MakeProvided(EPObjectType.EVENTTYPE, "TypeB", c));
				AssertConsumed(env, b);

				AssertProvided(env, c, MakeProvided(EPObjectType.EVENTTYPE, "TypeC", d, e));
				AssertConsumed(
					env,
					c,
					new EPDeploymentDependencyConsumed.Item(a, EPObjectType.EVENTTYPE, "TypeA"),
					new EPDeploymentDependencyConsumed.Item(b, EPObjectType.EVENTTYPE, "TypeB"));

				AssertProvided(env, d);
				AssertConsumed(env, d, new EPDeploymentDependencyConsumed.Item(c, EPObjectType.EVENTTYPE, "TypeC"));

				AssertProvided(env, e);
				AssertConsumed(env, e, new EPDeploymentDependencyConsumed.Item(c, EPObjectType.EVENTTYPE, "TypeC"));

				env.UndeployAll();
			}
		}

		private class ClientDeployListDependenciesInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				Assert.IsNull(env.Deployment.GetDeploymentDependenciesConsumed("dummy"));
				Assert.IsNull(env.Deployment.GetDeploymentDependenciesProvided("dummy"));

				Assert.Throws<ArgumentException>(
					() => { Assert.IsNull(env.Deployment.GetDeploymentDependenciesConsumed(null)); });

				Assert.Throws<ArgumentException>(
					() => { Assert.IsNull(env.Deployment.GetDeploymentDependenciesProvided(null)); });
			}
		}

		private class ClientDeployListDependenciesNoDependencies : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy("@name('s0') select * from SupportBean");
				AssertNoneProvidedConsumed(env, "s0");
				env.CompileDeploy("module A;\n @name('table') create table MyTable(k string, v string)");
				AssertNoneProvidedConsumed(env, "table");
				env.UndeployAll();
			}
		}

		private class ClientDeployListDependenciesWModuleName : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var pathA = new RegressionPath();
				env.CompileDeploy("module A;\n @name('createA') @protected create window MyWindow#keepall as SupportBean", pathA);

				var pathB = new RegressionPath();
				env.CompileDeploy("module B;\n @name('createB') @protected create window MyWindow#keepall as SupportBean", pathB);

				env.CompileDeploy("@name('B1') select * from MyWindow", pathB);
				env.CompileDeploy("@name('A1') select * from MyWindow", pathA);
				env.CompileDeploy("@name('A2') select * from MyWindow", pathA);
				env.CompileDeploy("@name('B2') select * from MyWindow", pathB);

				AssertProvided(
					env,
					env.DeploymentId("createA"),
					MakeProvided(EPObjectType.NAMEDWINDOW, "MyWindow", env.DeploymentId("A1"), env.DeploymentId("A2")));
				AssertProvided(
					env,
					env.DeploymentId("createB"),
					MakeProvided(EPObjectType.NAMEDWINDOW, "MyWindow", env.DeploymentId("B1"), env.DeploymentId("B2")));
				foreach (var name in new string[] {"A1", "A2"}) {
					AssertConsumed(
						env,
						env.DeploymentId(name),
						new EPDeploymentDependencyConsumed.Item(env.DeploymentId("createA"), EPObjectType.NAMEDWINDOW, "MyWindow"));
				}

				foreach (var name in new string[] {"B1", "B2"}) {
					AssertConsumed(
						env,
						env.DeploymentId(name),
						new EPDeploymentDependencyConsumed.Item(env.DeploymentId("createB"), EPObjectType.NAMEDWINDOW, "MyWindow"));
				}

				env.UndeployAll();
			}
		}

		private class ClientDeployListDependenciesObjectTypes : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplProvide =
					"@name('provide') @public create window MyWindow#keepall as SupportBean;\n" +
					"@public create table MyTable(k string primary key, value string);\n" +
					"@public create variable int MyVariable = 0;\n" +
					"@public create context MyContext partition by theString from SupportBean;\n" +
					"@public create schema MyEventType();\n" +
					"@public create expression MyExpression { 0 };\n" +
					"@public create expression double MyScript(stringvalue) [0];\n" +
					"@public create index MyIndexA on MyWindow(intPrimitive);\n" +
					"@public create index MyIndexB on MyTable(value);\n" +
					"@public create inlined_class \"\"\" public class MyClass { public static String doIt() { return \"abc\"; } }\"\"\";\n";
				env.CompileDeploy(eplProvide, path);

				var eplConsume = "@name('consume') context MyContext select MyVariable, count(*), MyTable['a'].value from MyWindow;\n" +
				                 "select MyExpression(), MyScript('a'), MyClass.doIt() from MyEventType;\n" +
				                 "on SupportBean as sb merge MyWindow as mw where sb.intPrimitive=mw.intPrimitive when matched then delete;\n" +
				                 "on SupportBean as sb merge MyTable as mt where sb.theString=mt.value when matched then delete;\n";
				env.CompileDeploy(eplConsume, path);

				var deploymentIdProvide = env.DeploymentId("provide");
				var deploymentIdConsume = env.DeploymentId("consume");

				AssertProvided(
					env,
					deploymentIdProvide,
					MakeProvided(EPObjectType.NAMEDWINDOW, "MyWindow", deploymentIdConsume),
					MakeProvided(EPObjectType.TABLE, "MyTable", deploymentIdConsume),
					MakeProvided(EPObjectType.VARIABLE, "MyVariable", deploymentIdConsume),
					MakeProvided(EPObjectType.CONTEXT, "MyContext", deploymentIdConsume),
					MakeProvided(EPObjectType.EVENTTYPE, "MyEventType", deploymentIdConsume),
					MakeProvided(EPObjectType.EXPRESSION, "MyExpression", deploymentIdConsume),
					MakeProvided(EPObjectType.SCRIPT, "MyScript#1", deploymentIdConsume),
					MakeProvided(EPObjectType.INDEX, "MyIndexA on named-window MyWindow", deploymentIdConsume),
					MakeProvided(EPObjectType.INDEX, "MyIndexB on table MyTable", deploymentIdConsume),
					MakeProvided(EPObjectType.CLASSPROVIDED, "MyClass", deploymentIdConsume));

				AssertConsumed(
					env,
					deploymentIdConsume,
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.NAMEDWINDOW, "MyWindow"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.TABLE, "MyTable"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.VARIABLE, "MyVariable"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.CONTEXT, "MyContext"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.EVENTTYPE, "MyEventType"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.EXPRESSION, "MyExpression"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.SCRIPT, "MyScript#1"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.INDEX, "MyIndexA on named-window MyWindow"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.INDEX, "MyIndexB on table MyTable"),
					new EPDeploymentDependencyConsumed.Item(deploymentIdProvide, EPObjectType.CLASSPROVIDED, "MyClass"));

				AssertEqualsAnyOrder(new string[] {env.DeploymentId("provide")}, env.Deployment.GetDeployment(deploymentIdConsume).DeploymentIdDependencies);

				env.UndeployAll();
			}
		}

		private static EPDeploymentDependencyProvided.Item MakeProvided(
			EPObjectType objectType,
			string objectName,
			params string[] deploymentIds)
		{
			return new EPDeploymentDependencyProvided.Item(
				objectType,
				objectName,
				new HashSet<string>(deploymentIds));
		}

		private static void AssertConsumed(
			RegressionEnvironment env,
			string deploymentId,
			params EPDeploymentDependencyConsumed.Item[] expected)
		{
			var consumed = env.Deployment.GetDeploymentDependenciesConsumed(deploymentId);
			AssertEqualsAnyOrder(expected, consumed.Dependencies.ToArray());
		}

		private static void AssertProvided(
			RegressionEnvironment env,
			string deploymentId,
			params EPDeploymentDependencyProvided.Item[] expected)
		{
			var provided = env.Deployment.GetDeploymentDependenciesProvided(deploymentId);
			AssertEqualsAnyOrder(expected, provided.Dependencies.ToArray());
		}

		private static void AssertNoneProvidedConsumed(
			RegressionEnvironment env,
			string statementName)
		{
			var deploymentId = env.DeploymentId("s0");
			AssertProvided(env, deploymentId);
			AssertConsumed(env, deploymentId);
		}
	}
} // end of namespace
