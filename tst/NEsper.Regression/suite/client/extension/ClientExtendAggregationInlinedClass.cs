///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendAggregationInlinedClass {
	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientExtendAggregationInlinedLocalClass());
	        execs.Add(new ClientExtendAggregationInlinedFAF());
	        execs.Add(new ClientExtendAggregationInlinedSameModule());
	        execs.Add(new ClientExtendAggregationInlinedOtherModule());
	        execs.Add(new ClientExtendAggregationInlinedInvalid());
	        execs.Add(new ClientExtendAggregationInlinedMultiModuleUses());
	        return execs;
	    }

	    const string INLINEDCLASS_CONCAT = "inlined_class \"\"\"\n" +
	        "import com.espertech.esper.common.client.hook.aggfunc.*;\n" +
	        "import com.espertech.esper.common.internal.epl.expression.core.*;\n" +
	        "import com.espertech.esper.common.client.hook.forgeinject.*;\n" +
	        "import com.espertech.esper.common.client.serde.*;\n" +
	        "import com.espertech.esper.common.client.type.*;\n" +
	        "import java.io.*;\n" +
	        "@ExtensionAggregationFunction(name=\"concat\")\n" +
	        "public class ConcatAggForge implements AggregationFunctionForge {\n" +
	        "  public void validate(AggregationFunctionValidationContext validationContext) throws ExprValidationException {\n" +
	        "    EPType paramType = validationContext.getParameterTypes()[0];\n" +
	        "    if (paramType == null || paramType != typeof(string)) {\n" +
	        "      throw new ExprValidationException(\"Invalid parameter type '\" + paramType + \"'\");\n" +
	        "    }\n" +
	        "  }\n" +
	        "\n" +
	        "  public Type ValueType => typeof(string);\n" +
	        "\n" +
	        "  public AggregationFunctionMode getAggregationFunctionMode() {\n" +
	        "    AggregationFunctionModeManaged mode = new AggregationFunctionModeManaged();\n" +
	        "    mode.setHasHA(true);\n" +
	        "    mode.setSerde(ConcatAggSerde.class);\n" +
	        "    mode.setInjectionStrategyAggregationFunctionFactory(new InjectionStrategyClassNewInstance(ConcatAggFactory.class.getName()));\n" +
	        "    return mode;\n" +
	        "  }\n" +
	        "\n" +
	        "  public class ConcatAggFactory implements AggregationFunctionFactory {\n" +
	        "    public AggregationFunction newAggregator(AggregationFunctionFactoryContext ctx) {\n" +
	        "      return new ConcatAggFunction();\n" +
	        "    }\n" +
	        "  }\n" +
	        "\n" +
	        "  public class ConcatAggFunction : AggregationFunction {\n" +
	        "    private readonly static String DELIMITER = \",\";\n" +
	        "    private StringBuilder builder;\n" +
	        "    private String delimiter;\n" +
	        "\n" +
	        "    public ConcatAggFunction() {\n" +
	        "      super();\n" +
	        "      builder = new StringBuilder();\n" +
	        "      delimiter = \"\";\n" +
	        "    }\n" +
	        "\n" +
	        "    public void enter(Object value) {\n" +
	        "      if (value != null) {\n" +
	        "        builder.append(delimiter);\n" +
	        "        builder.append(value.toString());\n" +
	        "        delimiter = DELIMITER;\n" +
	        "      }\n" +
	        "    }\n" +
	        "\n" +
	        "    public void leave(Object value) {\n" +
	        "      if (value != null) {\n" +
	        "        builder.delete(0, value.toString().length() + 1);\n" +
	        "      }\n" +
	        "    }\n" +
	        "  \n" +
	        "    public String getValue() {\n" +
	        "      return builder.toString();\n" +
	        "    }\n" +
	        "  \n" +
	        "    public void clear() {\n" +
	        "      builder = new StringBuilder();\n" +
	        "      delimiter = \"\";\n" +
	        "    }\n" +
	        "  }\n" +
	        "  public class ConcatAggSerde {\n" +
	        "    public static void write(DataOutput output, AggregationFunction value) throws IOException {\n" +
	        "      ConcatAggFunction agg = (ConcatAggFunction) value;\n" +
	        "      output.writeUTF(agg.getValue());\n" +
	        "    }\n" +
	        "\n" +
	        "    public static AggregationFunction read(DataInput input) throws IOException {\n" +
	        "      ConcatAggFunction concatAggFunction = new ConcatAggFunction();\n" +
	        "      String current = input.readUTF();\n" +
	        "      if (!current.isEmpty()) {\n" +
	        "        concatAggFunction.enter(current);\n" +
	        "      }\n" +
	        "      return concatAggFunction;\n" +
	        "    }\n" +
	        "  }\n" +
	        "}\n" +
	        "\"\"\"\n";

	    private class ClientExtendAggregationInlinedLocalClass : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0')\n" +
	                      INLINEDCLASS_CONCAT +
	                      "select concat(theString) as c0 from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendAssertConcat(env, "A", "A");
	            SendAssertConcat(env, "B", "A,B");

	            env.Milestone(0);

	            SendAssertConcat(env, "C", "A,B,C");

	            env.UndeployAll();
	        }
	    }

	    private class ClientExtendAggregationInlinedInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var eplTwiceLocal = INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeOne") + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeTwo") +
	                                "select concat(theString) from SupportBean";
	            env.TryInvalidCompile(eplTwiceLocal,
	                "The plug-in aggregation function 'concat' occurs multiple times");

	            var eplTwiceCreate = "create " + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeOne") + ";\n" +
	                                 "create " + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeTwo") + ";\n" +
	                                 "select concat(theString) from SupportBean";
	            env.TryInvalidCompile(eplTwiceCreate,
	                "The plug-in aggregation function 'concat' occurs multiple times");

	            var path = new RegressionPath();
	            env.Compile("@public create " + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeOne"), path);
	            env.Compile("@public create " + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeTwo"), path);
	            var eplTwiceInPath = "select concat(theString) from SupportBean";
	            env.TryInvalidCompile(path, eplTwiceInPath,
	                "The plug-in aggregation function 'concat' occurs multiple times");
	        }
	    }

	    private class ClientExtendAggregationInlinedFAF : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplWindow = "@public create window MyWindow#keepall as (theString string);\n" +
	                            "on SupportBean merge MyWindow insert select theString;\n";
	            env.CompileDeploy(eplWindow, path);

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean("E2", 1));

	            env.AssertThat(() => {
	                var eplFAF = INLINEDCLASS_CONCAT +
	                             "select concat(theString) as c0 from MyWindow";
	                var result = env.CompileExecuteFAF(eplFAF, path);
	                Assert.AreEqual("E1,E2", result.Array[0].Get("c0"));
	            });

	            env.UndeployAll();
	        }
	    }

	    private class ClientExtendAggregationInlinedSameModule : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "create " + INLINEDCLASS_CONCAT + ";\n" +
	                      "@name('s0') select concat(theString) as c0 from SupportBean;\n";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendAssertConcat(env, "A", "A");

	            env.Milestone(0);

	            SendAssertConcat(env, "B", "A,B");

	            SupportDeploymentDependencies.AssertEmpty(env, "s0");

	            env.UndeployAll();
	        }
	    }

	    private class ClientExtendAggregationInlinedOtherModule : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var eplCreateInlined = "@name('clazz') @public create " + INLINEDCLASS_CONCAT + ";\n";
	            var path = new RegressionPath();
	            env.Compile(eplCreateInlined.Replace("builder.toString()", "null"), path);

	            var eplSelect = "@name('s0') select concat(theString) as c0 from SupportBean";
	            var compiledSelect = env.Compile(eplSelect, path);

	            env.CompileDeploy(eplCreateInlined);
	            env.Deploy(compiledSelect).AddListener("s0");

	            SendAssertConcat(env, "A", "A");

	            env.Milestone(0);

	            SendAssertConcat(env, "B", "A,B");

	            // assert dependencies
	            SupportDeploymentDependencies.AssertSingle(env, "s0", "clazz", EPObjectType.CLASSPROVIDED, "ConcatAggForge");

	            env.UndeployAll();
	        }
	    }

	    private class ClientExtendAggregationInlinedMultiModuleUses : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();

	            foreach (var module in new string[]{"XXX", "YYY", "ZZZ"}) {
	                var epl = "module " + module + "; @public create " + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForge" + module).Replace("builder.toString()", "\"" + module + "\"");
	                env.CompileDeploy(epl, path);
	            }

	            var eplSelect = "uses YYY; @name('s0') select concat(theString) as c0 from SupportBean";
	            env.CompileDeploy(eplSelect, path).AddListener("s0");

	            SendAssertConcat(env, "A", "YYY");

	            env.UndeployAll();
	        }
	    }

	    private static void SendAssertConcat(RegressionEnvironment env, string theString, string expected) {
	        env.SendEventBean(new SupportBean(theString, 0));
	        env.AssertEqualsNew("s0", "c0", expected);
	    }
	}
} // end of namespace
