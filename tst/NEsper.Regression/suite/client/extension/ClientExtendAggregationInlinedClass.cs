///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendAggregationInlinedClass
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithLocalClass(execs);
            WithFAF(execs);
            WithSameModule(execs);
            WithOtherModule(execs);
            WithInvalid(execs);
            WithMultiModuleUses(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMultiModuleUses(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationInlinedMultiModuleUses());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationInlinedInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithOtherModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationInlinedOtherModule());
            return execs;
        }

        public static IList<RegressionExecution> WithSameModule(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationInlinedSameModule());
            return execs;
        }

        public static IList<RegressionExecution> WithFAF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationInlinedFAF());
            return execs;
        }

        public static IList<RegressionExecution> WithLocalClass(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendAggregationInlinedLocalClass());
            return execs;
        }

        const string INLINEDCLASS_CONCAT = (
            "inlined_class \"\"\"\n" +
            "using System;\n" +
            "using System.IO;\n" +
            "using System.Text;\n" +
            "\n" +
            "using com.espertech.esper.common.client.hook.aggfunc;\n" +
            "using com.espertech.esper.common.@internal.epl.expression.core;\n" +
            "using com.espertech.esper.common.client.hook.forgeinject;\n" +
            "using com.espertech.esper.common.client.serde;\n" +
            "using com.espertech.esper.compat.collections;\n" +
            "using com.espertech.esper.compat.io;\n" +
            "\n" +
            "namespace ${NAMESPACE} {\n" +
            "    [ExtensionAggregationFunction(Name=\"concat\")]\n" +
            "    public class ConcatAggForge : AggregationFunctionForge {\n" +
            "      public string FunctionName {\n" +
            "          set { }\n" +
            "      }\n" +
            "\n" +
            "      public void Validate(AggregationFunctionValidationContext validationContext) {\n" +
            "        var paramType = validationContext.ParameterTypes[0];\n" +
            "        if (paramType != typeof(string)) {\n" +
            "          throw new ExprValidationException(\"Invalid parameter type '\" + paramType.Name + \"'\");\n" +
            "    }\n" +
            "  }\n" +
            "\n" +
            "  public Type ValueType => typeof(string);\n" +
            "\n" +
            "      public AggregationFunctionMode AggregationFunctionMode {\n" +
            "        get {\n" +
            "    AggregationFunctionModeManaged mode = new AggregationFunctionModeManaged();\n" +
            "          mode.SetHasHA(true);\n" +
            "          mode.SetSerde(typeof(ConcatAggSerde));\n" +
            "          mode.SetInjectionStrategyAggregationFunctionFactory(new InjectionStrategyClassNewInstance(typeof(ConcatAggFactory)));\n" +
            "    return mode;\n" +
            "        }\n" +
            "  }\n" +
            "\n" +
            "      public class ConcatAggFactory : AggregationFunctionFactory {\n" +
            "        public AggregationFunction NewAggregator(AggregationFunctionFactoryContext ctx) {\n" +
            "      return new ConcatAggFunction();\n" +
            "    }\n" +
            "  }\n" +
            "\n" +
            "  public class ConcatAggFunction : AggregationFunction {\n" +
            "        private const string DELIMITER = \",\";\n" +
            "    private StringBuilder builder;\n" +
            "        private string delimiter;\n" +
            "\n" +
            "        public ConcatAggFunction() : base() {\n" +
            "      builder = new StringBuilder();\n" +
            "      delimiter = \"\";\n" +
            "    }\n" +
            "\n" +
            "        public void Enter(object value) {\n" +
            "      if (value != null) {\n" +
            "            builder.Append(delimiter);\n" +
            "            builder.Append(value.ToString());\n" +
            "        delimiter = DELIMITER;\n" +
            "      }\n" +
            "    }\n" +
            "\n" +
            "        public void Leave(object value) {\n" +
            "      if (value != null) {\n" +
            "            builder.Remove(0, value.ToString().Length);\n" +
            "      }\n" +
            "    }\n" +
            "  \n" +
            "        public object Value {\n" +
            "          get {\n" +
            "            return builder.ToString();\n" +
            "          }\n" +
            "    }\n" +
            "  \n" +
            "        public void Clear() {\n" +
            "      builder = new StringBuilder();\n" +
            "      delimiter = \"\";\n" +
            "    }\n" +
            "  }\n" +
            "  public class ConcatAggSerde {\n" +
            "        public static void Write(DataOutput output, AggregationFunction value) {\n" +
            "      ConcatAggFunction agg = (ConcatAggFunction) value;\n" +
            "          output.WriteUTF((string) agg.Value);\n" +
            "    }\n" +
            "\n" +
            "        public static AggregationFunction Read(DataInput input) {\n" +
            "      ConcatAggFunction concatAggFunction = new ConcatAggFunction();\n" +
            "          string current = input.ReadUTF();\n" +
            "          if (!string.IsNullOrEmpty(current)) {\n" +
            "            concatAggFunction.Enter(current);\n" +
            "      }\n" +
            "      return concatAggFunction;\n" +
            "    }\n" +
            "  }\n" +
            "}\n" +
            "}\n" +
            "\"\"\"\n"
        );

        private class ClientExtendAggregationInlinedLocalClass : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var ns = NamespaceGenerator.Create();
                var inlined = INLINEDCLASS_CONCAT.Replace("${NAMESPACE}", ns);
                var epl = "@Name('s0')\n" + inlined + "select concat(TheString) as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertConcat(env, "A", "A");
                SendAssertConcat(env, "B", "A,B");

                env.Milestone(0);

                SendAssertConcat(env, "C", "A,B,C");

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationInlinedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var ns = NamespaceGenerator.Create();
                var inlined = INLINEDCLASS_CONCAT.Replace("${NAMESPACE}", ns);

                var eplTwiceLocal =
                    inlined.Replace("ConcatAggForge", "ConcatAggForgeOne") +
                    inlined.Replace("ConcatAggForge", "ConcatAggForgeTwo") +
                    "select concat(TheString) from SupportBean";
                env.TryInvalidCompile(
                    eplTwiceLocal,
                    "The plug-in aggregation function 'concat' occurs multiple times");

                var eplTwiceCreate = "create " +
                                     inlined.Replace("ConcatAggForge", "ConcatAggForgeOne") +
                                     ";\n" +
                                     "create " +
                                     inlined.Replace("ConcatAggForge", "ConcatAggForgeTwo") +
                                     ";\n" +
                                     "select concat(TheString) from SupportBean";
                env.TryInvalidCompile(
                    eplTwiceCreate,
                    "The plug-in aggregation function 'concat' occurs multiple times");

                var path = new RegressionPath();
                env.Compile("@public create " + inlined.Replace("ConcatAggForge", "ConcatAggForgeOne"), path);
                env.Compile("@public create " + inlined.Replace("ConcatAggForge", "ConcatAggForgeTwo"), path);
                var eplTwiceInPath = "select concat(TheString) from SupportBean";
                env.TryInvalidCompile(
                    path,
                    eplTwiceInPath,
                    "The plug-in aggregation function 'concat' occurs multiple times");
            }
        }

        private class ClientExtendAggregationInlinedFAF : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var ns = NamespaceGenerator.Create();
                var inlined = INLINEDCLASS_CONCAT.Replace("${NAMESPACE}", ns);

                var path = new RegressionPath();
                var eplWindow = "@public create window MyWindow#keepall as (TheString string);\n" +
                                "on SupportBean merge MyWindow insert select TheString;\n";
                env.CompileDeploy(eplWindow, path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));

                env.AssertThat(
                    () => {
                        var eplFAF = inlined + "select concat(TheString) as c0 from MyWindow";
                        var result = env.CompileExecuteFAF(eplFAF, path);
                        ClassicAssert.AreEqual("E1,E2", result.Array[0].Get("c0"));
                    });

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationInlinedSameModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var ns = NamespaceGenerator.Create();
                var inlined = INLINEDCLASS_CONCAT.Replace("${NAMESPACE}", ns);

                var epl =
                    "create " +
                    inlined +
                    ";\n" +
                    "@name('s0') select concat(TheString) as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertConcat(env, "A", "A");

                env.Milestone(0);

                SendAssertConcat(env, "B", "A,B");

                SupportDeploymentDependencies.AssertEmpty(env, "s0");

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationInlinedOtherModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var ns = NamespaceGenerator.Create();
                var inlined = INLINEDCLASS_CONCAT.Replace("${NAMESPACE}", ns);

                var eplCreateInlined = "@Name('clazz') @public create " + inlined + ";\n";
                var path = new RegressionPath();
                env.Compile(eplCreateInlined.Replace("builder.ToString()", "null"), path);

                var eplSelect = "@name('s0') select concat(TheString) as c0 from SupportBean";
                var compiledSelect = env.Compile(eplSelect, path);

                env.CompileDeploy(eplCreateInlined);
                env.Deploy(compiledSelect).AddListener("s0");

                SendAssertConcat(env, "A", "A");

                env.Milestone(0);

                SendAssertConcat(env, "B", "A,B");

                // assert dependencies
                SupportDeploymentDependencies.AssertSingle(
                    env,
                    "s0",
                    "clazz",
                    EPObjectType.CLASSPROVIDED,
                    "ConcatAggForge");

                env.UndeployAll();
            }
        }

        private class ClientExtendAggregationInlinedMultiModuleUses : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var ns = NamespaceGenerator.Create();
                var inlined = INLINEDCLASS_CONCAT.Replace("${NAMESPACE}", ns);

                foreach (var module in new string[] { "XXX", "YYY", "ZZZ" }) {
                    var epl = "module " +
                              module +
                              "; " +
                              "@public create " +
                              inlined
                                  .Replace("ConcatAggForge", $"ConcatAggForge{module}")
                                  .Replace("builder.ToString()", $"\"{module}\"");
                    env.CompileDeploy(epl, path);
                }

                var eplSelect = "uses YYY; @name('s0') select concat(TheString) as c0 from SupportBean";
                env.CompileDeploy(eplSelect, path).AddListener("s0");

                SendAssertConcat(env, "A", "YYY");

                env.UndeployAll();
            }
        }

        private static void SendAssertConcat(
            RegressionEnvironment env,
            string theString,
            string expected)
        {
            env.SendEventBean(new SupportBean(theString, 0));
            env.AssertEqualsNew("s0", "c0", expected);
        }
    }
} // end of namespace