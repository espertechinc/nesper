///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Avro.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
	public class ClientCompileModule
	{
		private static readonly string NEWLINE = Environment.NewLine;

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientCompileModuleWImports());
	        execs.Add(new ClientCompileModuleLineNumberAndComments());
	        execs.Add(new ClientCompileModuleTwoModules());
	        execs.Add(new ClientCompileModuleParse());
	        execs.Add(new ClientCompileModuleParseFail());
	        execs.Add(new ClientCompileModuleCommentTrailing());
	        execs.Add(new ClientCompileModuleEPLModuleText());
	        return execs;
	    }

	    private class ClientCompileModuleEPLModuleText : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            EPDeployment deployment;

	            var epl = "@name('s0') select * from SupportBean";
	            env.CompileDeploy(epl);
	            deployment = env.Deployment.GetDeployment(env.DeploymentId("s0"));
	            Assert.AreEqual(epl, deployment.ModuleProperties.Get(ModuleProperty.MODULETEXT));
	            env.UndeployAll();

	            env.EplToModelCompileDeploy(epl);
	            deployment = env.Deployment.GetDeployment(env.DeploymentId("s0"));
	            Assert.AreEqual(epl, deployment.ModuleProperties.Get(ModuleProperty.MODULETEXT));
	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS);
	        }
	    }

	    private class ClientCompileModuleCommentTrailing : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl =
	                "@public @buseventtype create map schema Fubar as (foo String, bar Double);" + NEWLINE
	                    + "/** comment after */";
	            env.CompileDeploy(epl).UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS);
	        }
	    }

	    private class ClientCompileModuleTwoModules : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var resource = "regression/test_module_12.epl";

	            var resourceManager = env.Container.ResourceManager();
	            var input = resourceManager.GetResourceAsStream(resource);
	            Assert.IsNotNull(input);

	            Module module;
	            try {
	                module = env.Compiler.ReadModule(input, resource);
	                module.Uri = "uri1";
	                module.ArchiveName = "archive1";
	                module.UserObjectCompileTime = "obj1";
	            } catch (Exception t) {
	                throw new EPRuntimeException(t);
	            }

	            var compiled = env.Compile(module);
	            EPDeployment deployed;
	            try {
	                deployed = env.Deployment.Deploy(compiled);
	            } catch (EPDeployException e) {
	                throw new EPRuntimeException(e);
	            }

	            var deplomentInfo = env.Deployment.GetDeployment(deployed.DeploymentId);
	            Assert.AreEqual("regression.test", deplomentInfo.ModuleName);
	            Assert.AreEqual(2, deplomentInfo.Statements.Length);
	            Assert.AreEqual("create schema MyType(col1 integer)", deplomentInfo.Statements[0].GetProperty(StatementProperty.EPL));

	            var moduleText = "module regression.test.two;" +
	                             "uses regression.test;" +
	                             "create schema MyTypeTwo(col1 integer, col2.col3 string);" +
	                             "select * from MyTypeTwo;";
	            var moduleTwo = env.ParseModule(moduleText);
	            moduleTwo.Uri = "uri2";
	            moduleTwo.ArchiveName = "archive2";
	            moduleTwo.UserObjectCompileTime = "obj2";
	            moduleTwo.Uses = new HashSet<string>(Arrays.AsList("a", "b"));
	            moduleTwo.Imports = new HashSet<Import>(Arrays.AsList(
		            new ImportNamespace("c"),
		            new ImportNamespace("d")));
	            var compiledTwo = env.Compile(moduleTwo);
	            env.Deploy(compiledTwo);

	            var deploymentIds = env.Deployment.Deployments;
	            Assert.AreEqual(2, deploymentIds.Length);

	            var infoList = deploymentIds
		            .Select(_ => env.Deployment.GetDeployment(_))
		            .OrderBy(_ => _.ModuleName)
		            .ToList();
	            
	            var infoOne = infoList[0];
	            var infoTwo = infoList[1];
	            Assert.AreEqual("regression.test", infoOne.ModuleName);
	            Assert.AreEqual("uri1", infoOne.ModuleProperties.Get(ModuleProperty.URI));
	            Assert.AreEqual("archive1", infoOne.ModuleProperties.Get(ModuleProperty.ARCHIVENAME));
	            Assert.AreEqual("obj1", infoOne.ModuleProperties.Get(ModuleProperty.USEROBJECT));
	            Assert.IsNull(infoOne.ModuleProperties.Get(ModuleProperty.USES));
	            Assert.IsNotNull(infoOne.ModuleProperties.Get(ModuleProperty.MODULETEXT));
	            Assert.IsNotNull(infoOne.LastUpdateDate);
	            Assert.AreEqual("regression.test.two", infoTwo.ModuleName);
	            Assert.AreEqual("uri2", infoTwo.ModuleProperties.Get(ModuleProperty.URI));
	            Assert.AreEqual("archive2", infoTwo.ModuleProperties.Get(ModuleProperty.ARCHIVENAME));
	            Assert.AreEqual("obj2", infoTwo.ModuleProperties.Get(ModuleProperty.USEROBJECT));
	            Assert.IsNotNull(infoOne.ModuleProperties.Get(ModuleProperty.MODULETEXT));
	            Assert.IsNotNull(infoTwo.LastUpdateDate);
	            EPAssertionUtil.AssertEqualsExactOrder("a,b".SplitCsv(), (string[]) infoTwo.ModuleProperties.Get(ModuleProperty.USES));
	            EPAssertionUtil.AssertEqualsExactOrder("c,d".SplitCsv(), (string[]) infoTwo.ModuleProperties.Get(ModuleProperty.IMPORTS));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS);
	        }
	    }

	    private class ClientCompileModuleLineNumberAndComments : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var moduleText = NEWLINE + NEWLINE + "select * from ABC;" +
	                             NEWLINE + "select * from DEF";

	            var module = env.ParseModule(moduleText);
	            Assert.AreEqual(2, module.Items.Count);
	            Assert.AreEqual(3, module.Items[0].LineNumber);
	            Assert.AreEqual(3, module.Items[0].LineNumberEnd);
	            Assert.AreEqual(4, module.Items[1].LineNumber);
	            Assert.AreEqual(4, module.Items[1].LineNumberEnd);

	            module = env.ParseModule("/* abc */");
	            var compiled = env.Compile(module);
	            EPDeployment resultOne;
	            try {
	                resultOne = env.Deployment.Deploy(compiled);
	            } catch (EPDeployException e) {
	                throw new EPRuntimeException(e);
	            }
	            Assert.AreEqual(0, resultOne.Statements.Length);

	            module = env.ParseModule("select * from SupportBean; \r\n/* abc */\r\n");
	            compiled = env.Compile(module);
	            EPDeployment resultTwo;
	            try {
	                resultTwo = env.Deployment.Deploy(compiled);
	            } catch (EPDeployException e) {
	                throw new EPRuntimeException(e);
	            }
	            Assert.AreEqual(1, resultTwo.Statements.Length);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS);
	        }
	    }

	    private class ClientCompileModuleWImports : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var module = MakeModule("com.testit", "@name('A') select SupportStaticMethodLib.PlusOne(intPrimitive) as val from SupportBean");
	            module.Imports.Add(new ImportNamespace(typeof(SupportStaticMethodLib)));

	            var compiled = CompileModule(env, module);
	            env.Deploy(compiled).AddListener("A");

	            env.SendEventBean(new SupportBean("E1", 4));
	            Assert.AreEqual(5, env.Listener("A").AssertOneGetNewAndReset().Get("val"));

	            env.UndeployAll();

	            var epl = "import " + typeof(SupportStaticMethodLib).FullName + ";\n" +
	                      "@name('A') select SupportStaticMethodLib.PlusOne(intPrimitive) as val from SupportBean;\n";
	            env.CompileDeploy(epl).AddListener("A");

	            env.SendEventBean(new SupportBean("E1", 6));
	            Assert.AreEqual(7, env.Listener("A").AssertOneGetNewAndReset().Get("val"));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS);
	        }
	    }

	    private class ClientCompileModuleParse : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var module = env.ReadModule("regression/test_module_4.epl");
	            AssertModuleNoLines(module, null, "abd", null, new string[]{
	                "select * from ABC",
	                "/* Final comment */"
	                });
	            AssertModuleLinesOnly(module, new ModuleItem(null, false, 3, -1, -1, 7, 3, 3),
	                    new ModuleItem(null, true, 8, -1, -1, 9, -1, -1));

	            module = env.ReadModule("regression/test_module_1.epl");
	            AssertModuleNoLines(module, "abc", "def,jlk", null, new string[]{
	                "select * from A",
	                "select * from B" + NEWLINE + "where C=d",
	                "/* Test ; Comment */" + NEWLINE + "update ';' where B=C",
	                "update D"
	                }
	            );

	            module = env.ReadModule("regression/test_module_2.epl");
	            AssertModuleNoLines(module, "abc.def.hij", "def.hik,jlk.aja", null, new string[]{
	                "// Note 4 white spaces after * and before from" + NEWLINE + "select * from A",
	                "select * from B",
	                "select *    " + NEWLINE + "    from C",
	                }
	            );

	            module = env.ReadModule("regression/test_module_3.epl");
	            AssertModuleNoLines(module, null, null, null, new string[]{
	                "create window ABC",
	                "select * from ABC"
	                }
	            );

	            module = env.ReadModule("regression/test_module_5.epl");
	            AssertModuleNoLines(module, "abd.def", null, null, Array.Empty<string>());

	            module = env.ReadModule("regression/test_module_6.epl");
	            AssertModuleNoLines(module, null, null, null, Array.Empty<string>());

	            module = env.ReadModule("regression/test_module_7.epl");
	            AssertModuleNoLines(module, null, null, null, Array.Empty<string>());

	            module = env.ReadModule("regression/test_module_8.epl");
	            AssertModuleNoLines(module, "def.jfk", null, null, Array.Empty<string>());

	            module = env.ParseModule("module mymodule; uses mymodule2; import abc; select * from MyEvent;");
	            AssertModuleNoLines(module, "mymodule", "mymodule2", "abc", new string[]{
	                "select * from MyEvent"
	            });

	            module = env.ReadModule("regression/test_module_11.epl");
	            AssertModuleNoLines(module, null, null, "com.mycompany.pck1", Array.Empty<string>());

	            module = env.ReadModule("regression/test_module_10.epl");
	            AssertModuleNoLines(module, "abd.def", "one.use,two.use", "com.mycompany.pck1,com.mycompany.*", new string[]{
	                "select * from A",
	                }
	            );

	            Assert.AreEqual("org.mycompany.events", env.ParseModule("module org.mycompany.events; select * from System.Object;").Name);
	            Assert.AreEqual("glob.update.me", env.ParseModule("module glob.update.me; select * from System.Object;").Name);
	            Assert.AreEqual("seconds.until.every.where", env.ParseModule("uses seconds.until.every.where; select * from System.Object;").Uses.ToArray()[0]);
	            Assert.AreEqual("seconds.until.every.where", env.ParseModule("import seconds.until.every.where; select * from System.Object;").Imports.ToArray()[0]);

	            // Test script square brackets
	            module = env.ReadModule("regression/test_module_13.epl");
	            Assert.AreEqual(1, module.Items.Count);

	            module = env.ReadModule("regression/test_module_14.epl");
	            Assert.AreEqual(4, module.Items.Count);

	            module = env.ReadModule("regression/test_module_15.epl");
	            Assert.AreEqual(1, module.Items.Count);
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS);
	        }
	    }

	    private class ClientCompileModuleParseFail : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            TryInvalidIO(env, "regression/dummy_not_there.epl",
	                "Failed to find resource 'regression/dummy_not_there.epl' in classpath");

	            TryInvalidParse(env, "regression/test_module_1_fail.epl",
	                "Keyword 'module' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_1_fail.epl'");

	            TryInvalidParse(env, "regression/test_module_2_fail.epl",
	                "Keyword 'uses' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_2_fail.epl'");

	            TryInvalidParse(env, "regression/test_module_3_fail.epl",
	                "Keyword 'module' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_3_fail.epl'");

	            TryInvalidParse(env, "regression/test_module_4_fail.epl",
	                "Keyword 'uses' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_4_fail.epl'");

	            TryInvalidParse(env, "regression/test_module_5_fail.epl",
	                "Keyword 'import' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_5_fail.epl'");

	            TryInvalidParse(env, "regression/test_module_6_fail.epl",
	                "The 'module' keyword must be the first declaration in the module file for resource 'regression/test_module_6_fail.epl'");

	            TryInvalidParse(env, "regression/test_module_7_fail.epl",
	                "Duplicate use of the 'module' keyword for resource 'regression/test_module_7_fail.epl'");

	            TryInvalidParse(env, "regression/test_module_8_fail.epl",
	                "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration");

	            TryInvalidParse(env, "regression/test_module_9_fail.epl",
	                "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration");

	            // try control chars
	            TryInvalidControlCharacters(env);
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.COMPILEROPS, RegressionFlag.INVALIDITY);
	        }
	    }

	    private static void TryInvalidControlCharacters(RegressionEnvironment env) {
	        var epl = "select * \u008F from SupportBean";
	        env.TryInvalidCompile(epl, "Failed to parse: Unrecognized control characters found in text, failed to parse text ");
	    }

	    private static void TryInvalidIO(RegressionEnvironment env, string resource, string message) {
	        try {
		        env.Compiler.ReadModule(resource, env.Container.ResourceManager()); 
	            Assert.Fail();
	        } catch (IOException ex) {
	            Assert.AreEqual(message, ex.Message);
	        } catch (ParseException ex) {
	            throw new EPRuntimeException(ex);
	        }
	    }

	    private static void TryInvalidParse(RegressionEnvironment env, string resource, string message) {
	        try {
	            env.Compiler.ReadModule(resource, env.Container.ResourceManager());
	            Assert.Fail();
	        } catch (IOException ex) {
	            throw new EPRuntimeException(ex);
	        } catch (ParseException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private static void AssertModuleNoLines(Module module, string name, string usesCSV, string importsCSV, string[] statements) {
	        Assert.AreEqual(name, module.Name);

	        var expectedUses = usesCSV == null ? Array.Empty<string>() : usesCSV.SplitCsv();
	        EPAssertionUtil.AssertEqualsExactOrder(expectedUses, module.Uses.ToArray());

	        var expectedImports = importsCSV == null ? Array.Empty<string>() : importsCSV.SplitCsv();
	        EPAssertionUtil.AssertEqualsExactOrder(expectedImports, module.Imports.ToArray());

	        var stmtsFound = new string[module.Items.Count];
	        for (var i = 0; i < module.Items.Count; i++) {
	            stmtsFound[i] = module.Items[i].Expression;
	        }
	        EPAssertionUtil.AssertEqualsExactOrder(statements, stmtsFound);
	    }

	    private static void AssertModuleLinesOnly(Module module, params ModuleItem[] expecteds) {
	        Assert.AreEqual(expecteds.Length, module.Items.Count);
	        for (var i = 0; i < expecteds.Length; i++) {
	            var expected = expecteds[i];
	            var actual = module.Items[i];
	            var message = "Failed to item#" + i;
	            Assert.AreEqual(expected.IsCommentOnly, actual.IsCommentOnly, message);
	            Assert.AreEqual(expected.LineNumber, actual.LineNumber, message);
	            Assert.AreEqual(expected.LineNumberEnd, actual.LineNumberEnd, message);
	            Assert.AreEqual(expected.LineNumberContent, actual.LineNumberContent, message);
	            Assert.AreEqual(expected.LineNumberContentEnd, actual.LineNumberContent, message);
	        }
	    }

	    private static EPCompiled CompileModule(RegressionEnvironment env, Module module) {
	        try {
	            return env.Compiler.Compile(module, new CompilerArguments(env.Configuration));
	        } catch (EPCompileException ex) {
	            throw new EPRuntimeException(ex);
	        }
	    }

	    private static Module MakeModule(string name, params string[] statements) {
	        var items = new ModuleItem[statements.Length];
	        for (var i = 0; i < statements.Length; i++) {
	            items[i] = new ModuleItem(statements[i], false, 0, 0, 0, 0, 0, 0);
	        }
	        return new Module(name, null, new HashSet<string>(), new HashSet<Import>(), Arrays.AsList(items), null);
	    }
	}
} // end of namespace
