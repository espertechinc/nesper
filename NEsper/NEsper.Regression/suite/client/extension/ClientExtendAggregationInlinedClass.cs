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
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendAggregationInlinedClass
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientExtendAggregationInlinedLocalClass());
			execs.Add(new ClientExtendAggregationInlinedFAF());
			execs.Add(new ClientExtendAggregationInlinedSameModule());
			execs.Add(new ClientExtendAggregationInlinedOtherModule());
			execs.Add(new ClientExtendAggregationInlinedInvalid());
			execs.Add(new ClientExtendAggregationInlinedMultiModuleUses());
			return execs;
		}

		const string INLINEDCLASS_CONCAT = "inlined_class \"\"\"\n" +
		                                   "using System;\n" +
		                                   "using System.IO;\n" +
		                                   "using com.espertech.esper.common.client.hook.aggfunc;\n" +
		                                   "using com.espertech.esper.common.internal.epl.expression.core;\n" +
		                                   "using com.espertech.esper.common.client.hook.forgeinject;\n" +
		                                   "using com.espertech.esper.common.client.serde;\n" +
		                                   "[ExtensionAggregationFunction(Name=\"concat\")]\n" +
		                                   "public class ConcatAggForge : AggregationFunctionForge {\n" +
		                                   "  public void Validate(AggregationFunctionValidationContext validationContext) {\n" +
		                                   "    var paramType = validationContext.GetParameterTypes()[0];\n" +
		                                   "    if (paramType != String.class) {\n" +
		                                   "      throw new ExprValidationException(\"Invalid parameter type '\" + paramType.getSimpleName() + \"'\");\n" +
		                                   "    }\n" +
		                                   "  }\n" +
		                                   "\n" +
		                                   "  public Class getValueType() {\n" +
		                                   "    return String.class;\n" +
		                                   "  }\n" +
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
		                                   "  public class ConcatAggFunction implements AggregationFunction {\n" +
		                                   "    private final static String DELIMITER = \",\";\n" +
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

		private class ClientExtendAggregationInlinedLocalClass : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('s0')\n" +
				             INLINEDCLASS_CONCAT +
				             "select concat(TheString) as c0 from SupportBean";
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
				string eplTwiceLocal = INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeOne") +
				                       INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeTwo") +
				                       "select concat(TheString) from SupportBean";
				TryInvalidCompile(
					env,
					eplTwiceLocal,
					"The plug-in aggregation function 'concat' occurs multiple times");

				string eplTwiceCreate = "create " +
				                        INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeOne") +
				                        ";\n" +
				                        "create " +
				                        INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeTwo") +
				                        ";\n" +
				                        "select concat(TheString) from SupportBean";
				TryInvalidCompile(
					env,
					eplTwiceCreate,
					"The plug-in aggregation function 'concat' occurs multiple times");

				RegressionPath path = new RegressionPath();
				env.Compile("@public create " + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeOne"), path);
				env.Compile("@public create " + INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForgeTwo"), path);
				string eplTwiceInPath = "select concat(TheString) from SupportBean";
				TryInvalidCompile(
					env,
					path,
					eplTwiceInPath,
					"The plug-in aggregation function 'concat' occurs multiple times");
			}
		}

		private class ClientExtendAggregationInlinedFAF : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string eplWindow = "create window MyWindow#keepall as (TheString string);\n" +
				                   "on SupportBean merge MyWindow insert select TheString;\n";
				env.CompileDeploy(eplWindow, path);

				env.SendEventBean(new SupportBean("E1", 1));
				env.SendEventBean(new SupportBean("E2", 1));

				string eplFAF = INLINEDCLASS_CONCAT +
				                "select concat(TheString) as c0 from MyWindow";
				EPFireAndForgetQueryResult result = env.CompileExecuteFAF(eplFAF, path);
				Assert.AreEqual("E1,E2", result.Array[0].Get("c0"));

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationInlinedSameModule : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create " +
				             INLINEDCLASS_CONCAT +
				             ";\n" +
				             "@Name('s0') select concat(TheString) as c0 from SupportBean;\n";
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
				string eplCreateInlined = "@Name('clazz') @public create " + INLINEDCLASS_CONCAT + ";\n";
				RegressionPath path = new RegressionPath();
				env.Compile(eplCreateInlined.Replace("builder.toString()", "null"), path);

				string eplSelect = "@Name('s0') select concat(TheString) as c0 from SupportBean";
				EPCompiled compiledSelect = env.Compile(eplSelect, path);

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

		private class ClientExtendAggregationInlinedMultiModuleUses : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();

				foreach (string module in new string[] {"XXX", "YYY", "ZZZ"}) {
					string epl = "module " +
					             module +
					             "; @public create " +
					             INLINEDCLASS_CONCAT.Replace("ConcatAggForge", "ConcatAggForge" + module).Replace("builder.toString()", "\"" + module + "\"");
					env.CompileDeploy(epl, path);
				}

				string eplSelect = "uses YYY; @name('s0') select concat(TheString) as c0 from SupportBean";
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
			Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
		}
	}
} // end of namespace
