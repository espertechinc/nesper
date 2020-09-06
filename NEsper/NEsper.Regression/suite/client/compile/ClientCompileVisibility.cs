///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
	public class ClientCompileVisibility
	{
		private const string FIRST_MESSAGE = "Failed to resolve event type, named window or table by name 'MySchema'";

		private const string CREATE_EPL =
			"${PREFIX} create schema MySchema();" +
			"${PREFIX} create variable int abc;\n" +
			"${PREFIX} create context MyContext partition by TheString from SupportBean;\n" +
			"${PREFIX} create window MyWindow#keepall as SupportBean;\n" +
			"${PREFIX} create table MyTable as (c count(*));\n" +
			"${PREFIX} create expression MyExpr { 1 };\n" +
			"${PREFIX} create expression double myscript(intvalue) [0];\n" +
			"${PREFIX} create inlined_class \"\"\" namespace ${NAMESPACE} { public class MyClass { public static string DoIt() { return \"def\"; } } }\"\"\";\n";

		private const string USER_EPL =
			"select 1 from MySchema;\n" +
			"select abc from SupportBean;\n" +
			"context MyContext select * from SupportBean;\n" +
			"on SupportBean update MyWindow set TheString = 'a';\n" +
			"into table MyTable select count(*) as c from SupportBean;\n" +
			"select MyExpr() from SupportBean;\n" +
			"select myscript(1) from SupportBean;\n" +
			"select ${NAMESPACE}.MyClass.DoIt() from SupportBean;\n";

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientVisibilityNamedWindowSimple());
			execs.Add(new ClientVisibilityAmbiguousPathWithPreconfigured());
			execs.Add(new ClientVisibilityDefaultPrivate());
			execs.Add(new ClientVisibilityAnnotationPrivate());
			execs.Add(new ClientVisibilityAnnotationProtected());
			execs.Add(new ClientVisibilityAnnotationPublic());
			execs.Add(new ClientVisibilityModuleNameOption());
			execs.Add(new ClientVisibilityAnnotationBusEventType());
			execs.Add(new ClientVisibilityAnnotationInvalid());
			execs.Add(new ClientVisibilityAmbiguousTwoPath());
			execs.Add(new ClientVisibilityDisambiguateWithUses());
			execs.Add(new ClientVisibilityBusRequiresPublic());
			return execs;
		}

		private class ClientVisibilityBusRequiresPublic : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var message = "Event type 'ABC' with bus-visibility requires the public access modifier for the event type";
				TryInvalidCompile(env, "@Private @BusEventType create schema ABC()", message);
				TryInvalidCompile(env, "@BusEventType create schema ABC()", message);

				TryInvalidCompileWConfigure(
					env,
					config => config.Compiler.ByteCode.BusModifierEventType = EventTypeBusModifier.BUS,
					"@private create schema ABC()",
					message);
				TryInvalidCompileWConfigure(
					env,
					config => config.Compiler.ByteCode.BusModifierEventType = EventTypeBusModifier.BUS,
					"@protected create schema ABC()",
					message);

				foreach (var modifier in new [] {
					NameAccessModifier.INTERNAL,
					NameAccessModifier.PRIVATE
				}) {
					TryInvalidCompileWConfigure(
						env,
						config => {
							config.Compiler.ByteCode.BusModifierEventType = EventTypeBusModifier.BUS;
							config.Compiler.ByteCode.AccessModifierEventType = modifier;
						},
						"create schema ABC()",
						message);
				}
			}
		}

		private class ClientVisibilityDisambiguateWithUses : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RunAssertionDisambiguate(
					env,
					"create variable int var_named = 1",
					"create variable String var_named = 'x'",
					"select var_named from SupportBean",
					() => {
						env.SendEventBean(new SupportBean());
						Assert.AreEqual("x", env.Listener("s0").AssertOneGetNewAndReset().Get("var_named"));
					});

				RunAssertionDisambiguate(
					env,
					"create context MyContext partition by TheString from SupportBean;",
					"create context MyContext partition by Id from SupportBean_S0;",
					"context MyContext select P00 from SupportBean_S0",
					() => { });

				RunAssertionDisambiguate(
					env,
					"create window MyWindow#keepall as SupportBean",
					"create window MyWindow#keepall as SupportBean_S0",
					"select P00 from MyWindow",
					() => { });

				RunAssertionDisambiguate(
					env,
					"create table MyTable(c0 string)",
					"create table MyTable(c1 int)",
					"select c1 from MyTable",
					() => { });

				RunAssertionDisambiguate(
					env,
					"create expression MyExpr {1}",
					"create expression MyExpr {'y'}",
					"select MyExpr() as c0 from SupportBean",
					() => {
						env.SendEventBean(new SupportBean());
						Assert.AreEqual("y", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
					});

				RunAssertionDisambiguate(
					env,
					"create expression double myscript() [0];",
					"create expression string myscript() ['z'];",
					"select myscript() as c0 from SupportBean",
					() => {
						env.SendEventBean(new SupportBean());
						Assert.AreEqual("z", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
					});

				RunAssertionDisambiguate(
					env,
					"create schema MySchema as (p0 int);",
					"create schema MySchema as (p1 string);",
					"select p1 from MySchema",
					() => { });

				RunAssertionDisambiguate(
					env,
					"create inlined_class \"\"\" public class MyClass { " +
					"public static string DoIt() { return \"abc\"; } }\"\"\";\n",
					"create inlined_class \"\"\" public class MyClass { " +
					"public static string DoIt() { return \"def\"; } }\"\"\";\n",
					"select MyClass.DoIt() as c0 from SupportBean",
					() => {
						env.SendEventBean(new SupportBean());
						Assert.AreEqual("def", env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
					});
			}
		}

		private class ClientVisibilityAnnotationInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryInvalidCompile(
					env,
					"@protected @private create schema abc()",
					"Encountered both the @private and the @protected annotation");
				TryInvalidCompile(
					env,
					"@public @private create schema abc()",
					"Encountered both the @private and the @public annotation");
				TryInvalidCompile(
					env,
					"@public @protected create schema abc()",
					"Encountered both the @protected and the @public annotation");
			}
		}

		private class ClientVisibilityAnnotationBusEventType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@public @BusEventType create schema MyEvent(p0 string);\n" +
						"@Name('s0') select * from MyEvent;\n")
					.AddListener("s0");
				env.SendEventMap(EmptyDictionary<string, object>.Instance, "MyEvent");
				Assert.IsTrue(env.Listener("s0").IsInvoked);
				env.UndeployAll();
			}
		}

		private class ClientVisibilityModuleNameOption : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RunAssertionOptionModuleName(env, "select 1 from SupportBean");
				RunAssertionOptionModuleName(env, "module x; select 1 from SupportBean");
			}
		}

		private class ClientVisibilityAnnotationProtected : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var namespc = NamespaceGenerator.Create();
				var createEPL = CREATE_EPL
					.Replace("${PREFIX}", "@protected")
					.Replace("${NAMESPACE}", namespc);
				
				var epl = "module a.b.c;\n" + createEPL;
				var compiled = env.Compile(epl);
				TryInvalidNotVisible(env, compiled);

				var userEPL = USER_EPL.Replace("${NAMESPACE}", namespc);
				var path = new RegressionPath();
				path.Add(compiled);
				env.Compile("module a.b.c;\n" + userEPL, path);

				TryInvalidCompile(env, path, "module a.b.d;\n" + userEPL, FIRST_MESSAGE);
			}
		}

		private class ClientVisibilityAnnotationPublic : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var namespc = NamespaceGenerator.Create();
				var createEPL = CREATE_EPL
					.Replace("${PREFIX}", "@public")
					.Replace("${NAMESPACE}", namespc);

				var epl = "module a.b.c;\n" + createEPL.Replace("${PREFIX}", "@public");
				var compiled = env.Compile(epl);

				var userEPL = USER_EPL.Replace("${NAMESPACE}", namespc);
				var path = new RegressionPath();
				path.Add(compiled);
				env.Compile("module x;\n" + userEPL, path);
			}
		}

		private class ClientVisibilityDefaultPrivate : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var namespc = NamespaceGenerator.Create();
				var createEPL = CREATE_EPL
					.Replace("${PREFIX}", "")
					.Replace("${NAMESPACE}", namespc);

				var epl = createEPL;
				var compiled = env.Compile(epl);
				TryInvalidNotVisible(env, compiled);

				namespc = NamespaceGenerator.Create();
				epl = CREATE_EPL
					.Replace("${PREFIX}", "")
					.Replace("${NAMESPACE}", namespc);
				var userEPL = USER_EPL.Replace("${NAMESPACE}", namespc);
				epl = epl + userEPL;
				env.CompileDeploy(epl).UndeployAll();
			}
		}

		private class ClientVisibilityAnnotationPrivate : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var namespc = NamespaceGenerator.Create();
				var createEPL = CREATE_EPL
					.Replace("${PREFIX}", "@private")
					.Replace("${NAMESPACE}", namespc);
				var userEPL = USER_EPL.Replace("${NAMESPACE}", namespc);

				var epl = createEPL + userEPL;
				var compiled = env.Compile(epl);
				TryInvalidNotVisible(env, compiled);
			}
		}

		private class ClientVisibilityAmbiguousTwoPath : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var commonEPL = "create variable int abc;\n" +
				                "create schema MySchema();" +
				                "create context MyContext partition by TheString from SupportBean;\n" +
				                "create window MyWindow#keepall as SupportBean;\n" +
				                "create table MyTable as (c count(*));\n" +
				                "create expression MyExpr { 1 };\n" +
				                "create expression double myscript(stringvalue) [0];\n" +
				                "create inlined_class \"\"\" public class MyClass { public static string DoIt() { return \"def\"; } }\"\"\";\n";

				var modOne = env.Compile("module one;\n " + commonEPL, new RegressionPath());
				var modTwo = env.Compile("module two;\n " + commonEPL, new RegressionPath());

				var path = new RegressionPath();
				path.Add(modOne);
				path.Add(modTwo);
				TryInvalidCompile(
					env,
					path,
					"select abc from SupportBean",
					"The variable by name 'abc' is ambiguous as it exists for multiple modules");
				TryInvalidCompile(
					env,
					path,
					"select 1 from MySchema",
					"The event type by name 'MySchema' is ambiguous as it exists for multiple modules");
				TryInvalidCompile(
					env,
					path,
					"context MyContext select * from SupportBean",
					"The context by name 'MyContext' is ambiguous as it exists for multiple modules");
				TryInvalidCompile(
					env,
					path,
					"select * from MyWindow",
					"The named window by name 'MyWindow' is ambiguous as it exists for multiple modules");
				TryInvalidCompile(
					env,
					path,
					"select * from MyTable",
					"The table by name 'MyTable' is ambiguous as it exists for multiple modules");
				TryInvalidCompile(
					env,
					path,
					"select MyExpr() from SupportBean",
					"The declared-expression by name 'MyExpr' is ambiguous as it exists for multiple modules");
				TryInvalidCompile(
					env,
					path,
					"select myscript('a') from SupportBean",
					"The script by name 'myscript' is ambiguous as it exists for multiple modules: A script by name 'myscript (1 parameters)' is exported by multiple modules");
				TryInvalidCompile(
					env,
					path,
					"select MyClass.DoIt() from SupportBean",
					"Failed to validate select-clause expression 'MyClass.DoIt()': The application-inlined class by name 'MyClass' is ambiguous as it exists for multiple modules: An application-inlined class by name 'MyClass' is exported by multiple modules");
			}
		}

		private class ClientVisibilityAmbiguousPathWithPreconfigured : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var args = new CompilerArguments(env.MinimalConfiguration());
				args.Options
					.SetAccessModifierVariable(_ => NameAccessModifier.PUBLIC)
					.SetAccessModifierEventType(_ => NameAccessModifier.PUBLIC);
				EPCompiled compiled;
				try {
					compiled = env.Compiler.Compile(
						"create variable int preconfigured_variable;\n" +
						"create schema SupportBean_S1 as (p0 string);\n",
						args);
				}
				catch (EPCompileException e) {
					throw new EPRuntimeException(e);
				}

				path.Add(compiled);

				TryInvalidCompile(
					env,
					path,
					"select preconfigured_variable from SupportBean",
					"The variable by name 'preconfigured_variable' is ambiguous as it exists in both the path space and the preconfigured space");
				TryInvalidCompile(
					env,
					path,
					"select 'test' from SupportBean_S1",
					"The event type by name 'SupportBean_S1' is ambiguous as it exists in both the path space and the preconfigured space");
			}
		}

		private class ClientVisibilityNamedWindowSimple : RegressionExecution
		{
			string[] fields = "c0,c1".SplitCsv();

			public void Run(RegressionEnvironment env)
			{
				var compiledCreate = env.Compile(
					"create window MyWindow#length(2) as SupportBean",
					options => options.AccessModifierNamedWindow = ctx => NameAccessModifier.PUBLIC);
				env.Deploy(compiledCreate);

				var compiledInsert = env.Compile(
					"insert into MyWindow select * from SupportBean",
					new CompilerArguments(env.MinimalConfiguration()).SetPath(new CompilerPath().Add(compiledCreate)));
				env.Deploy(compiledInsert);

				var compiledSelect = env.Compile(
					"@Name('s0') select TheString as c0, sum(IntPrimitive) as c1 from MyWindow;\n",
					new CompilerArguments(env.MinimalConfiguration()).SetPath(new CompilerPath().Add(compiledCreate)));
				env.Deploy(compiledSelect).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 10));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E1", 10});

				env.SendEventBean(new SupportBean("E2", 20));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E2", 30});

				env.SendEventBean(new SupportBean("E3", 25));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E3", 45});

				env.SendEventBean(new SupportBean("E4", 26));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E4", 51});

				env.UndeployAll();
			}
		}

		private static void TryInvalidNotVisible(
			RegressionEnvironment env,
			EPCompiled compiled)
		{
			var path = new RegressionPath();
			path.Add(compiled);
			TryInvalidCompile(
				env,
				path,
				"select 1 from MySchema",
				"Failed to resolve event type, named window or table by name 'MySchema'");
			TryInvalidCompile(
				env,
				path,
				"select abc from SupportBean",
				"Failed to validate select-clause expression 'abc': Property named 'abc' is not valid in any stream");
			TryInvalidCompile(
				env,
				path,
				"context MyContext select * from SupportBean",
				"Context by name 'MyContext' could not be found");
			TryInvalidCompile(
				env,
				path,
				"on SupportBean update MyWindow set TheString = 'a'",
				"A named window or table 'MyWindow' has not been declared");
			TryInvalidCompile(
				env,
				path,
				"into table MyTable select count(*) as c from SupportBean",
				"Invalid into-table clause: Failed to find table by name 'MyTable'");
			TryInvalidCompile(
				env,
				path,
				"select MyExpr() from SupportBean",
				"Failed to validate select-clause expression 'MyExpr()': Unknown single-row function, expression declaration, script or aggregation function named 'MyExpr' could not be resolved");
			TryInvalidCompile(
				env,
				path,
				"select myscript(1) from SupportBean",
				"Failed to validate select-clause expression 'myscript(1)': Unknown single-row function, aggregation function or mapped or indexed property named 'myscript' could not be resolved");
			TryInvalidCompile(
				env,
				path,
				"select MyClassX.DoIt() from SupportBean",
				"Failed to validate select-clause expression 'MyClassX.DoIt()': Failed to resolve 'MyClassX.DoIt' to a property, single-row function, aggregation function, script, stream or class name");
		}

		private static void RunAssertionDisambiguate(
			RegressionEnvironment env,
			string firstEpl,
			string secondEpl,
			string useEpl,
			Runnable assertion)
		{
			var first = env.Compile("module a;\n @public " + firstEpl + "\n");
			var second = env.Compile("module b;\n @public " + secondEpl + "\n");
			env.Deploy(first);
			env.Deploy(second);

			var path = new RegressionPath();
			path.Add(first);
			path.Add(second);
			env.CompileDeploy("uses b;\n @name('s0') " + useEpl + "\n", path).AddListener("s0");

			assertion.Invoke();

			env.UndeployAll();
		}

		private static void RunAssertionOptionModuleName(
			RegressionEnvironment env,
			string epl)
		{
			EPCompiled compiledBoth;
			try {
				var args = new CompilerArguments(env.Configuration);
				args.Options.ModuleName = ctx => "abc";
				compiledBoth = env.Compiler.Compile(epl, args);
			}
			catch (EPCompileException ex) {
				throw new EPRuntimeException(ex);
			}

			var deployed = SupportCompileDeployUtil.Deploy(compiledBoth, env.Runtime);
			Assert.AreEqual("abc", deployed.ModuleName); // Option-provided module-name wins

			env.UndeployAll();
		}

		private static void TryInvalidCompileWConfigure(
			RegressionEnvironment env,
			Consumer<Configuration> configurer,
			string epl,
			string message)
		{
			try {
				var configuration = env.MinimalConfiguration();
				configurer.Invoke(configuration);
				var args = new CompilerArguments(configuration);
				env.Compiler.Compile(epl, args);
			}
			catch (EPCompileException ex) {
				SupportMessageAssertUtil.AssertMessage(ex, message);
			}
		}
	}
} // end of namespace
