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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendUDFInlinedClass
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInlinedLocalClass(execs);
            WithInlinedInvalid(execs);
            WithInlinedFAF(execs);
            WithCreateInlinedSameModule(execs);
            WithCreateInlinedOtherModule(execs);
            WithInlinedWOptions(execs);
            WithOverloaded(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOverloaded(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFOverloaded());
            return execs;
        }

        public static IList<RegressionExecution> WithInlinedWOptions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFInlinedWOptions());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateInlinedOtherModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFCreateInlinedOtherModule());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateInlinedSameModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFCreateInlinedSameModule());
            return execs;
        }

        public static IList<RegressionExecution> WithInlinedFAF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFInlinedFAF());
            return execs;
        }

        public static IList<RegressionExecution> WithInlinedInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFInlinedInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithInlinedLocalClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFInlinedLocalClass(false));
            execs.Add(new ClientExtendUDFInlinedLocalClass(true));
            return execs;
        }

        // Note: Janino does not support @Repeatable and does not support @Annos({@Anno, @Anno})
        private class ClientExtendUDFOverloaded : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') inlined_class \"\"\"\n" +
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
                          "select multiply(intPrimitive,intPrimitive) as c0, multiply(intPrimitive,intPrimitive,intPrimitive) as c1 \n" +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 4));
                env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { 16, 64 });

                env.UndeployAll();
            }
        }

        private class ClientExtendUDFInlinedWOptions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') inlined_class \"\"\"\n" +
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
                          "select multiply(intPrimitive,intPrimitive) as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertIntMultiply(env, 5, 25);

                env.UndeployAll();
            }
        }

        private class ClientExtendUDFCreateInlinedOtherModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplCreateInlined = "@name('clazz') @public create inlined_class \"\"\"\n" +
                                       "  @" +
                                       typeof(ExtensionSingleRowFunctionAttribute).FullName +
                                       "(name=\"multiply\", methodName=\"multiply\")\n" +
                                       "  public class MultiplyHelper {\n" +
                                       "    public static int multiply(int a, int b) {\n" +
                                       "      %BEHAVIOR%\n" +
                                       "    }\n" +
                                       "  }\n" +
                                       "\"\"\"\n;";
                var path = new RegressionPath();
                env.Compile(eplCreateInlined.Replace("%BEHAVIOR%", "return -1;"), path);

                var eplSelect = "@name('s0') select multiply(intPrimitive,intPrimitive) as c0 from SupportBean";
                var compiledSelect = env.Compile(eplSelect, path);

                env.CompileDeploy(eplCreateInlined.Replace("%BEHAVIOR%", "return a*b;"));
                env.Deploy(compiledSelect).AddListener("s0");

                SendAssertIntMultiply(env, 3, 9);

                env.Milestone(0);

                SendAssertIntMultiply(env, 13, 13 * 13);

                // assert dependencies
                SupportDeploymentDependencies.AssertSingle(
                    env,
                    "s0",
                    "clazz",
                    EPObjectType.CLASSPROVIDED,
                    "MultiplyHelper");

                env.UndeployAll();
            }
        }

        private class ClientExtendUDFCreateInlinedSameModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create inlined_class \"\"\"\n" +
                          "  @" +
                          typeof(ExtensionSingleRowFunctionAttribute).FullName +
                          "(name=\"multiply\", methodName=\"multiply\")\n" +
                          "  public class MultiplyHelper {\n" +
                          "    public static int multiply(int a, int b) {\n" +
                          "      return a*b;\n" +
                          "    }\n" +
                          "  }\n" +
                          "\"\"\"\n;" +
                          "@name('s0') select multiply(intPrimitive,intPrimitive) as c0 from SupportBean;\n";
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
                var path = new RegressionPath();
                var eplWindow = "@public create window MyWindow#keepall as (theString string);\n" +
                                "on SupportBean merge MyWindow insert select theString;\n";
                env.CompileDeploy(eplWindow, path);

                env.SendEventBean(new SupportBean("E1", 1));

                var eplFAF = "inlined_class \"\"\"\n" +
                             "  @" +
                             typeof(ExtensionSingleRowFunctionAttribute).FullName +
                             "(name=\"appendDelimiters\", methodName=\"doIt\")\n" +
                             "  public class MyClass {\n" +
                             "    public static String doIt(String parameter) {\n" +
                             "      return '>' + parameter + '<';\n" +
                             "    }\n" +
                             "  }\n" +
                             "\"\"\"\n select appendDelimiters(theString) as c0 from MyWindow";
                env.AssertThat(
                    () => {
                        var result = env.CompileExecuteFAF(eplFAF, path);
                        Assert.AreEqual(">E1<", result.Array[0].Get("c0"));
                    });

                env.Milestone(0);

                env.AssertThat(
                    () => {
                        var result = env.CompileExecuteFAF(eplFAF, path);
                        Assert.AreEqual(">E1<", result.Array[0].Get("c0"));
                    });

                env.UndeployAll();
            }
        }

        private class ClientExtendUDFInlinedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') inlined_class \"\"\"\n" +
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
                          "select multiply(intPrimitive,intPrimitive) as c0 from SupportBean";
                env.TryInvalidCompile(
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
                var epl = "@name('s0') inlined_class \"\"\"\n" +
                          "  @" +
                          typeof(ExtensionSingleRowFunctionAttribute).FullName +
                          "(name=\"multiply\", methodName=\"multiply\")\n" +
                          "  public class MultiplyHelper {\n" +
                          "    public static int multiply(int a, int b) {\n" +
                          "      return a*b;\n" +
                          "    }\n" +
                          "  }\n" +
                          "\"\"\" " +
                          "select multiply(intPrimitive,intPrimitive) as c0 from SupportBean";
                env.CompileDeploy(soda, epl).AddListener("s0");

                SendAssertIntMultiply(env, 5, 25);

                env.Milestone(0);

                SendAssertIntMultiply(env, 6, 36);

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private static void SendAssertIntMultiply(
            RegressionEnvironment env,
            int intPrimitive,
            int expected)
        {
            env.SendEventBean(new SupportBean("E1", intPrimitive));
            env.AssertEqualsNew("s0", "c0", expected);
        }
    }
} // end of namespace