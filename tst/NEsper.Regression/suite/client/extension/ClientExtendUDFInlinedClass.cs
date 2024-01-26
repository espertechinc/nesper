///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.singlerowfunc;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;
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

        private class ClientExtendUDFOverloaded : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var @namespace = NamespaceGenerator.Create();
                var epl = "@name('s0') inlined_class \"\"\"\n" +
                          "  using com.espertech.esper.common.client.hook.singlerowfunc;\n" +
                          "  namespace " +
                          @namespace +
                          " {\n" +
                          "    [ExtensionSingleRowFunction(Name=\"multiply\", MethodName=\"MultiplyIt\")]\n" +
                          "    public class MultiplyHelper {\n" +
                          "      public static int MultiplyIt(int a, int b) {\n" +
                          "        return a*b;\n" +
                          "      }\n" +
                          "      public static int MultiplyIt(int a, int b, int c) {\n" +
                          "        return a*b*c;\n" +
                          "      }\n" +
                          "    }\n" +
                          "  }\n" +
                          "\"\"\" " +
                          "select multiply(IntPrimitive,IntPrimitive) as c0, multiply(IntPrimitive,IntPrimitive,IntPrimitive) as c1 \n" +
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
                var @namespace = NamespaceGenerator.Create();
                var epl = 
                      "@Name('s0') inlined_class \"\"\"\n" +
                      " namespace " +
                      @namespace +
                      " {\n" +
                      "  [" + typeof(ExtensionSingleRowFunctionAttribute).FullName + "(\n" +
                      "      Name=\"multiply\", MethodName=\"MultiplyIfPositive\",\n" +
                      "      ValueCache=" + typeof(ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum).FullName.CodeInclusionTypeName() + ".DISABLED,\n" +
                      "      FilterOptimizable=" + typeof(ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum).FullName.CodeInclusionTypeName() + ".DISABLED,\n" +
                      "      RethrowExceptions=false,\n" +
                      "      EventTypeName=\"abc\"\n" +
                      "    )]\n" +
                      "    public class MultiplyHelper {\n" +
                      "      public static int MultiplyIfPositive(int a, int b) {\n" +
                      "        return a*b;\n" +
                      "      }\n" +
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
                var eplCreateInlined =
                    "@name('clazz') @public create inlined_class \"\"\"\n" +
                    " namespace %NAMESPACE% {\n" +
                    "   [" + typeof(ExtensionSingleRowFunctionAttribute).FullName + "(Name=\"multiply\", MethodName=\"Multiply\")]\n" +
                    "   public class MultiplyHelper {\n" +
                    "     public static int Multiply(int a, int b) {\n" +
                    "        %BEHAVIOR%\n" +
                    "      }\n" +
                    "    }\n" +
                    " }\n" +
                    "\"\"\"\n;";
                var path = new RegressionPath();
                var ns1 = NamespaceGenerator.Create();
                env.Compile(
                    eplCreateInlined
                        .Replace("%NAMESPACE%", ns1)
                        .Replace("%BEHAVIOR%", "return -1;"),
                    path);

                var eplSelect = "@Name('s0') select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean";
                var compiledSelect = env.Compile(eplSelect, path);

                var ns2 = NamespaceGenerator.Create();
                env.CompileDeploy(
                    eplCreateInlined
                        .Replace("%NAMESPACE%", ns2)
                        .Replace("%BEHAVIOR%", "return a*b;"));
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
                var @namespace = NamespaceGenerator.Create();
                var epl =
                    "create inlined_class \"\"\"\n" +
                    " namespace " +
                    @namespace +
                    " {\n" +
                    "   [" +
                    typeof(ExtensionSingleRowFunctionAttribute).FullName +
                    "(Name=\"multiply\", MethodName=\"Multiply\")]\n" +
                    "  public class MultiplyHelper {\n" +
                    "     public static int Multiply(int a, int b) {\n" +
                    "      return a*b;\n" +
                    "     }\n" +
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
                var path = new RegressionPath();
                var eplWindow = 
                    "@public create window MyWindow#keepall as (TheString string);\n" +
                    "on SupportBean merge MyWindow insert select TheString;\n";
                env.CompileDeploy(eplWindow, path);

                env.SendEventBean(new SupportBean("E1", 1));

                var eplFAF =
                    "inlined_class \"\"\"\n" +
                    " namespace %NAMESPACE% {\n" +
                    "   [" +
                    typeof(ExtensionSingleRowFunctionAttribute).FullName +
                    "(Name=\"appendDelimiters\", MethodName=\"DoIt\")]\n" +
                    "  public class MyClass {\n" +
                    "     public static string DoIt(string parameter) {\n" +
                    "      return '>' + parameter + '<';\n" +
                    "    }\n" +
                    "  }\n" +
                    " }\n" +
                    "\"\"\"\n select appendDelimiters(TheString) as c0 from MyWindow";

                env.AssertThat(
                    () => {
                        var ns1 = NamespaceGenerator.Create();
                        var result = env.CompileExecuteFAF(eplFAF.Replace("%NAMESPACE%", ns1), path);
                        Assert.AreEqual(">E1<", result.Array[0].Get("c0"));
                    });

                env.Milestone(0);

                env.AssertThat(
                    () => {
                        var ns2 = NamespaceGenerator.Create();
                        var result = env.CompileExecuteFAF(eplFAF.Replace("%NAMESPACE%", ns2), path);
                        Assert.AreEqual(">E1<", result.Array[0].Get("c0"));
                    });

                env.UndeployAll();
            }
        }

        private class ClientExtendUDFInlinedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var @namespace = NamespaceGenerator.Create();
                var epl =
                    "@Name('s0') inlined_class \"\"\"\n" +
                    " namespace " +
                    @namespace +
                    " {\n" +
                    "   [" +
                    typeof(ExtensionSingleRowFunctionAttribute).FullName +
                    "(Name=\"multiply\", MethodName=\"Multiply\")]\n" +
                    "  public class MultiplyHelperOne {\n" +
                    "     public static int Multiply(int a, int b) { return 0; }\n" +
                    "  }\n" +
                    "   [" +
                    typeof(ExtensionSingleRowFunctionAttribute).FullName +
                    "(Name=\"multiply\", MethodName=\"Multiply\")]\n" +
                    "  public class MultiplyHelperTwo {\n" +
                    "     public static int Multiply(int a, int b, int c) { return 0; }\n" +
                    "   }\n" +
                    "  }\n" +
                    "\"\"\" " +
                    "select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean";
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
                var @namespace = NamespaceGenerator.Create();
                var epl = "@Name('s0') inlined_class \"\"\"\n" +
                          " namespace " +
                          @namespace +
                          " {\n" +
                          "   [" +
                          typeof(ExtensionSingleRowFunctionAttribute).FullName +
                          "(Name=\"multiply\", MethodName=\"Multiply\")]\n" +
                          "   public class MultiplyHelper {\n" +
                          "     public static int Multiply(int a, int b) {\n" +
                          "       return a*b;\n" +
                          "     }\n" +
                          "   }\n" +
					      " }\n" +
                          "\"\"\" " +
                          "select multiply(IntPrimitive,IntPrimitive) as c0 from SupportBean";
                env.CompileDeploy(soda, epl).AddListener("s0");

                SendAssertIntMultiply(env, 5, 25);

                env.Milestone(0);

                SendAssertIntMultiply(env, 6, 36);

                env.UndeployAll();
            }

            public string Name()
            {
                return $"{this.GetType().Name}{{soda={soda}}}";
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