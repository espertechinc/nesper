///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.extend.aggfunc;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendAggregationFunction
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ClientExtendAggregationManagedWindow());
			execs.Add(new ClientExtendAggregationManagedGrouped());
			execs.Add(new ClientExtendAggregationManagedDistinctAndStarParam());
			execs.Add(new ClientExtendAggregationManagedDotMethod());
			execs.Add(new ClientExtendAggregationManagedMappedPropertyLookAlike());
			execs.Add(new ClientExtendAggregationMultiParamMulti());
			execs.Add(new ClientExtendAggregationMultiParamNoParam());
			execs.Add(new ClientExtendAggregationMultiParamSingleArray());
			execs.Add(new ClientExtendAggregationCodegeneratedCount());
			execs.Add(new ClientExtendAggregationFailedValidation());
			execs.Add(new ClientExtendAggregationInvalidUse());
			execs.Add(new ClientExtendAggregationInvalidCannotResolve());
			execs.Add(new ClientExtendAggregationTable());
			return execs;
		}

		internal class ClientExtendAggregationTable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string epl = "create table MyTable(col1 concatstring(string));\n" +
				             "into table MyTable select concatstring(TheString) as col1 from SupportBean;\n";
				env.CompileDeploy(epl, path);

				env.SendEventBean(new SupportBean("E1", 0));
				env.SendEventBean(new SupportBean("E2", 0));
				Assert.AreEqual("E1 E2", env.CompileExecuteFAF("select col1 from MyTable", path).Array[0].Get("col1"));

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationCodegeneratedCount : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "@Name('s0') select concatWCodegen(TheString) as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 0));
				Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

				env.SendEventBean(new SupportBean("E2", 0));
				Assert.AreEqual("E1E2", env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationMultiParamSingleArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "@Name('s0') select irstream countback({1,2,IntPrimitive}) as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {-1}, new object[] {0});

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationManagedWindow : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "@Name('s0') select irstream concatstring(TheString) as val from SupportBean#length(2)";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean("a", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a"}, new object[] {""});

				env.SendEventBean(new SupportBean("b", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a b"}, new object[] {"a"});

				env.SendEventBean(new SupportBean("c", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"b c"}, new object[] {"a b"});

				env.SendEventBean(new SupportBean("d", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"c d"}, new object[] {"b c"});

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationManagedGrouped : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string textOne = "@Name('s0') select irstream CONCATSTRING(TheString) as val from SupportBean#length(10) group by IntPrimitive";
				TryGrouped(env, textOne, null);

				string textTwo = "@Name('s0') select irstream concatstring(TheString) as val from SupportBean#win:length(10) group by IntPrimitive";
				TryGrouped(env, textTwo, null);

				string textThree = "@Name('s0') select irstream concatstring(TheString) as val from SupportBean#length(10) group by IntPrimitive";
				EPStatementObjectModel model = env.EplToModel(textThree);
				SerializableObjectCopier.CopyMayFail(env.Container, model);
				Assert.AreEqual(textThree, model.ToEPL());
				TryGrouped(env, null, model);

				string textFour = "select irstream concatstring(TheString) as val from SupportBean#length(10) group by IntPrimitive";
				EPStatementObjectModel modelTwo = new EPStatementObjectModel();
				modelTwo.SelectClause = SelectClause
					.Create(StreamSelector.RSTREAM_ISTREAM_BOTH)
					.Add(Expressions.PlugInAggregation("concatstring", Expressions.Property("TheString")), "val");
				modelTwo.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView(null, "length", Expressions.Constant(10)));
				modelTwo.GroupByClause = GroupByClause.Create("IntPrimitive");
				Assert.AreEqual(textFour, modelTwo.ToEPL());
				SerializableObjectCopier.CopyMayFail(env.Container, modelTwo);
				modelTwo.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				TryGrouped(env, null, modelTwo);

				env.UndeployAll();
			}

			private void TryGrouped(
				RegressionEnvironment env,
				string text,
				EPStatementObjectModel model)
			{
				if (model != null) {
					env.CompileDeploy(model);
				}
				else {
					env.CompileDeploy(text);
				}

				env.AddListener("s0");

				env.SendEventBean(new SupportBean("a", 1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a"}, new object[] {""});

				env.SendEventBean(new SupportBean("b", 2));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"b"}, new object[] {""});

				env.SendEventBean(new SupportBean("c", 1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a c"}, new object[] {"a"});

				env.SendEventBean(new SupportBean("d", 2));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"b d"}, new object[] {"b"});

				env.SendEventBean(new SupportBean("e", 1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a c e"}, new object[] {"a c"});

				env.SendEventBean(new SupportBean("f", 2));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"b d f"}, new object[] {"b d"});

				env.Listener("s0").Reset();
				env.UndeployModuleContaining("s0");
			}
		}

		internal class ClientExtendAggregationManagedDistinctAndStarParam : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// test *-parameter
				string textTwo = "@Name('s0') select concatstring(*) as val from SupportBean";
				env.CompileDeploy(textTwo).AddListener("s0");

				env.SendEventBean(new SupportBean("d", -1));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "val".SplitCsv(), new object[] {"SupportBean(\"d\", -1)"});

				env.SendEventBean(new SupportBean("e", 2));
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					"val".SplitCsv(),
					new object[] {"SupportBean(\"d\", -1) SupportBean(\"e\", 2)"});

				TryInvalidCompile(
					env,
					"select concatstring(*) as val from SupportBean#lastevent, SupportBean unidirectional",
					"Failed to validate select-clause expression 'concatstring(*)': The 'concatstring' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead");
				env.UndeployAll();

				// test distinct
				string text = "@Name('s0') select irstream concatstring(distinct TheString) as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean("a", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a"}, new object[] {""});

				env.SendEventBean(new SupportBean("b", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a b"}, new object[] {"a"});

				env.SendEventBean(new SupportBean("b", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a b"}, new object[] {"a b"});

				env.SendEventBean(new SupportBean("c", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a b c"}, new object[] {"a b"});

				env.SendEventBean(new SupportBean("a", -1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a b c"}, new object[] {"a b c"});

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationManagedDotMethod : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// test dot-method
				SupportSupportBeanAggregationFunctionFactory.InstanceCount = 0;
				string[] fields = "val0,val1".SplitCsv();
				env.CompileDeploy("@Name('s0') select (myagg(Id)).GetTheString() as val0, (myagg(Id)).GetIntPrimitive() as val1 from SupportBean_A")
					.AddListener("s0");

				env.SendEventBean(new SupportBean_A("A1"));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"XX", 1});

				env.SendEventBean(new SupportBean_A("A2"));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"XX", 2});

				Assert.AreEqual(1, SupportSupportBeanAggregationFunctionFactory.InstanceCount);

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationMultiParamMulti : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryAssertionMultipleParams(env, false);
				TryAssertionMultipleParams(env, true);
			}

			private void TryAssertionMultipleParams(
				RegressionEnvironment env,
				bool soda)
			{

				string text = "@Name('s0') select irstream countboundary(1,10,IntPrimitive,*) as val from SupportBean";
				env.CompileDeploy(soda, text).AddListener("s0");

				var validContext = SupportLowerUpperCompareAggregationFunctionForge.Contexts[0];
				EPAssertionUtil.AssertEqualsExactOrder(new Type[] {typeof(int), typeof(int), typeof(int?), typeof(SupportBean)}, validContext.ParameterTypes);
				EPAssertionUtil.AssertEqualsExactOrder(new object[] {1, 10, null, null}, validContext.ConstantValues);
				EPAssertionUtil.AssertEqualsExactOrder(new bool[] {true, true, false, false}, validContext.IsConstantValue);

				SupportBean e1 = new SupportBean("E1", 5);
				env.SendEventBean(e1);
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {1}, new object[] {0});
				EPAssertionUtil.AssertEqualsExactOrder(new object[] {1, 10, 5, e1}, SupportLowerUpperCompareAggregationFunction.LastEnterParameters);

				env.SendEventBean(new SupportBean("E1", 0));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {1}, new object[] {1});

				env.SendEventBean(new SupportBean("E1", 11));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {1}, new object[] {1});

				env.SendEventBean(new SupportBean("E1", 1));
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {2}, new object[] {1});

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationMultiParamNoParam : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "@Name('s0') select irstream countback() as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {-1}, new object[] {0});

				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {-2}, new object[] {-1});

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationManagedMappedPropertyLookAlike : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string text = "@Name('s0') select irstream concatstring('a') as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");
				Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("val"));

				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a"}, new object[] {""});

				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a a"}, new object[] {"a"});

				env.SendEventBean(new SupportBean());
				EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").AssertInvokedAndReset(), "val", new object[] {"a a a"}, new object[] {"a a"});

				env.UndeployAll();
			}
		}

		internal class ClientExtendAggregationFailedValidation : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryInvalidCompile(
					env,
					"select concatstring(1) from SupportBean",
					"Failed to validate select-clause expression 'concatstring(1)': Plug-in aggregation function 'concatstring' failed validation: Invalid parameter type '");
			}
		}

		internal class ClientExtendAggregationInvalidUse : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryInvalidCompile(
					env,
					"select * from SupportBean group by invalidAggFuncForge(1)",
					"Error resolving aggregation: Class by name 'System.TimeSpan' does not implement the AggregationFunctionForge interface");

				TryInvalidCompile(
					env,
					"select * from SupportBean group by nonExistAggFuncForge(1)",
					"Error resolving aggregation: Could not load aggregation factory class by name 'com.NoSuchClass'");
			}
		}

		internal class ClientExtendAggregationInvalidCannotResolve : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryInvalidCompile(
					env,
					"select zzz(TheString) from SupportBean",
					"Failed to validate select-clause expression 'zzz(TheString)': Unknown single-row function, aggregation function or mapped or indexed property named 'zzz' could not be resolved");
			}
		}
	}
} // end of namespace