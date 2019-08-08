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
            "${PREFIX} create expression double myscript(intvalue) [0];\n";

        private const string USER_EPL =
            "select 1 from MySchema;\n" +
            "select abc from SupportBean;\n" +
            "context MyContext select * from SupportBean;\n" +
            "on SupportBean update MyWindow set TheString = 'a';\n" +
            "into table MyTable select count(*) as c from SupportBean;\n" +
            "select MyExpr() from SupportBean;\n" +
            "select myscript(1) from SupportBean;\n";

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
            execs.Add(new ClientVisibilityAnnotationSendable());
            execs.Add(new ClientVisibilityAnnotationInvalid());
            execs.Add(new ClientVisibilityAmbiguousTwoPath());
            execs.Add(new ClientVisibilityDisambiguateWithUses());
            return execs;
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
                "Failed to validate select-clause expression 'MyExpr': Unknown single-row function, expression declaration, script or aggregation function named 'MyExpr' could not be resolved");
            TryInvalidCompile(
                env,
                path,
                "select myscript(1) from SupportBean",
                "Failed to validate select-clause expression 'myscript(1)': Unknown single-row function, aggregation function or mapped or indexed property named 'myscript' could not be resolved");
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
            env.CompileDeploy("uses b;\n @Name('s0') " + useEpl + "\n", path).AddListener("s0");

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
                args.Options.SetModuleName(ctx => "abc");
                compiledBoth = EPCompilerProvider.Compiler.Compile(epl, args);
            }
            catch (EPCompileException ex) {
                throw new EPException(ex);
            }

            var deployed = SupportCompileDeployUtil.Deploy(compiledBoth, env.Runtime);
            Assert.AreEqual("abc", deployed.ModuleName); // Option-provided module-name wins

            env.UndeployAll();
        }

        internal class ClientVisibilityDisambiguateWithUses : RegressionExecution
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
            }
        }

        internal class ClientVisibilityAnnotationInvalid : RegressionExecution
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

        internal class ClientVisibilityAnnotationSendable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "@Public @BusEventType create schema MyEvent(p0 string);\n" +
                        "@Name('s0') select * from MyEvent;\n")
                    .AddListener("s0");
                env.SendEventMap(Collections.EmptyDataMap, "MyEvent");
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.UndeployAll();

                env.Compile("@protected @BusEventType create schema MyEvent(p0 string)");

                var message = "Event type 'MyEvent' with bus-visibility requires protected or public access modifiers";
                TryInvalidCompile(env, "@Private @BusEventType create schema MyEvent(p0 string)", message);
                TryInvalidCompile(env, "@BusEventType create schema MyEvent(p0 string)", message);
            }
        }

        internal class ClientVisibilityModuleNameOption : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionOptionModuleName(env, "select 1 from SupportBean");
                RunAssertionOptionModuleName(env, "module x; select 1 from SupportBean");
            }
        }

        internal class ClientVisibilityAnnotationProtected : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "module a.b.c;\n" + CREATE_EPL.Replace("${PREFIX}", "@protected");
                var compiled = env.Compile(epl);
                TryInvalidNotVisible(env, compiled);

                var path = new RegressionPath();
                path.Add(compiled);
                env.Compile("module a.b.c;\n" + USER_EPL, path);

                TryInvalidCompile(env, path, "module a.b.d;\n" + USER_EPL, FIRST_MESSAGE);
            }
        }

        internal class ClientVisibilityAnnotationPublic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "module a.b.c;\n" + CREATE_EPL.Replace("${PREFIX}", "@public");
                var compiled = env.Compile(epl);

                var path = new RegressionPath();
                path.Add(compiled);
                env.Compile("module x;\n" + USER_EPL, path);
            }
        }

        internal class ClientVisibilityDefaultPrivate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = CREATE_EPL.Replace("${PREFIX}", "");
                var compiled = env.Compile(epl);
                TryInvalidNotVisible(env, compiled);

                epl = epl + USER_EPL;
                env.CompileDeploy(epl).UndeployAll();
            }
        }

        internal class ClientVisibilityAnnotationPrivate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = CREATE_EPL.Replace("${PREFIX}", "@private") + USER_EPL;
                var compiled = env.Compile(epl);
                TryInvalidNotVisible(env, compiled);
            }
        }

        internal class ClientVisibilityAmbiguousTwoPath : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var commonEPL = "create variable int abc;\n" +
                                "create schema MySchema();" +
                                "create context MyContext partition by TheString from SupportBean;\n" +
                                "create window MyWindow#keepall as SupportBean;\n" +
                                "create table MyTable as (c count(*));\n" +
                                "create expression MyExpr { 1 };\n" +
                                "create expression double myscript(stringvalue) [0];\n";

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
            }
        }

        internal class ClientVisibilityAmbiguousPathWithPreconfigured : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var args = new CompilerArguments(new Configuration());
                args.Options
                    .SetAccessModifierVariable(ctx => NameAccessModifier.PUBLIC)
                    .SetAccessModifierEventType(ctx => NameAccessModifier.PUBLIC);
                EPCompiled compiled;
                try {
                    compiled = EPCompilerProvider.Compiler.Compile(
                        "create variable int preconfigured_variable;\n" +
                        "create schema SupportBean_S1 as (p0 string);\n",
                        args);
                }
                catch (EPCompileException e) {
                    throw new EPException(e);
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

        internal class ClientVisibilityNamedWindowSimple : RegressionExecution
        {
            private readonly string[] fields = "c0,c1".SplitCsv();

            public void Run(RegressionEnvironment env)
            {
                var compiledCreate = env.Compile(
                    "create window MyWindow#length(2) as SupportBean",
                    options => options.AccessModifierNamedWindow = ctx => NameAccessModifier.PUBLIC);
                env.Deploy(compiledCreate);

                var compiledInsert = env.Compile(
                    "insert into MyWindow select * from SupportBean",
                    new CompilerArguments(new Configuration()).SetPath(new CompilerPath().Add(compiledCreate)));
                env.Deploy(compiledInsert);

                var compiledSelect = env.Compile(
                    "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1 from MyWindow;\n",
                    new CompilerArguments(new Configuration()).SetPath(new CompilerPath().Add(compiledCreate)));
                env.Deploy(compiledSelect).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10});

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 30});

                env.SendEventBean(new SupportBean("E3", 25));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 45});

                env.SendEventBean(new SupportBean("E4", 26));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 51});

                env.UndeployAll();
            }
        }
    }
} // end of namespace