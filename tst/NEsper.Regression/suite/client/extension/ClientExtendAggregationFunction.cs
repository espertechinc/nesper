///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.aggfunc;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;


namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendAggregationFunction
	{

		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
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

		private class ClientExtendAggregationTable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var epl = "@public create table MyTable(col1 concatstring(string));\n" +
				          "into table MyTable select concatstring(theString) as col1 from SupportBean;\n";
				env.CompileDeploy(epl, path);

				env.SendEventBean(new SupportBean("E1", 0));

				env.Milestone(0);

				env.SendEventBean(new SupportBean("E2", 0));
				env.AssertThat(
					() => Assert.AreEqual(
						"E1 E2",
						env.CompileExecuteFAF("select col1 from MyTable", path).Array[0].Get("col1")));

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationCodegeneratedCount : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var text = "@name('s0') select concatWCodegen(theString) as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 0));
				env.AssertEqualsNew("s0", "val", "E1");

				env.SendEventBean(new SupportBean("E2", 0));
				env.AssertEqualsNew("s0", "val", "E1E2");

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationMultiParamSingleArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var text = "@name('s0') select irstream countback({1,2,intPrimitive}) as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean());
				AssertPairSingleRow(env, new object[] { -1 }, new object[] { 0 });

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationManagedWindow : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var text = "@name('s0') select irstream concatstring(theString) as val from SupportBean#length(2)";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean("a", -1));
				AssertPairSingleRow(env, new object[] { "a" }, new object[] { "" });

				env.SendEventBean(new SupportBean("b", -1));
				AssertPairSingleRow(env, new object[] { "a b" }, new object[] { "a" });

				env.Milestone(0);

				env.SendEventBean(new SupportBean("c", -1));
				AssertPairSingleRow(env, new object[] { "b c" }, new object[] { "a b" });

				env.SendEventBean(new SupportBean("d", -1));
				AssertPairSingleRow(env, new object[] { "c d" }, new object[] { "b c" });

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationManagedGrouped : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var milestone = new AtomicLong();
				var textOne =
					"@name('s0') select irstream CONCATSTRING(theString) as val from SupportBean#length(10) group by intPrimitive";
				TryGrouped(env, textOne, null, milestone);

				var textTwo =
					"@name('s0') select irstream concatstring(theString) as val from SupportBean#win:length(10) group by intPrimitive";
				TryGrouped(env, textTwo, null, milestone);

				var textThree =
					"@name('s0') select irstream concatstring(theString) as val from SupportBean#length(10) group by intPrimitive";
				var model = env.EplToModel(textThree);
				env.CopyMayFail(model);
				Assert.AreEqual(textThree, model.ToEPL());
				TryGrouped(env, null, model, milestone);

				var textFour =
					"select irstream concatstring(theString) as val from SupportBean#length(10) group by intPrimitive";
				var modelTwo = new EPStatementObjectModel();
				modelTwo.SelectClause = SelectClause.Create()
					.SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
					.Add(Expressions.PlugInAggregation("concatstring", Expressions.Property("theString")), "val");
				modelTwo.FromClause = FromClause.Create(
					FilterStream.Create("SupportBean").AddView(null, "length", Expressions.Constant(10)));
				modelTwo.GroupByClause = GroupByClause.Create("intPrimitive");
				Assert.AreEqual(textFour, modelTwo.ToEPL());
				env.CopyMayFail(modelTwo);
				modelTwo.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				TryGrouped(env, null, modelTwo, milestone);

				env.UndeployAll();
			}

			private void TryGrouped(
				RegressionEnvironment env,
				string text,
				EPStatementObjectModel model,
				AtomicLong milestone)
			{
				if (model != null) {
					env.CompileDeploy(model);
				}
				else {
					env.CompileDeploy(text);
				}

				env.AddListener("s0");

				env.SendEventBean(new SupportBean("a", 1));
				AssertPairSingleRow(env, new object[] { "a" }, new object[] { "" });

				env.SendEventBean(new SupportBean("b", 2));
				AssertPairSingleRow(env, new object[] { "b" }, new object[] { "" });

				env.SendEventBean(new SupportBean("c", 1));
				AssertPairSingleRow(env, new object[] { "a c" }, new object[] { "a" });

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("d", 2));
				AssertPairSingleRow(env, new object[] { "b d" }, new object[] { "b" });

				env.SendEventBean(new SupportBean("e", 1));
				AssertPairSingleRow(env, new object[] { "a c e" }, new object[] { "a c" });

				env.SendEventBean(new SupportBean("f", 2));
				AssertPairSingleRow(env, new object[] { "b d f" }, new object[] { "b d" });

				env.UndeployModuleContaining("s0");
			}
		}

		private class ClientExtendAggregationManagedDistinctAndStarParam : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var milestone = new AtomicLong();

				// test *-parameter
				var textTwo = "@name('s0') select concatstring(*) as val from SupportBean";
				env.CompileDeploy(textTwo).AddListener("s0");

				env.SendEventBean(new SupportBean("d", -1));
				env.AssertPropsNew("s0", "val".SplitCsv(), new object[] { "SupportBean(d, -1)" });

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("e", 2));
				env.AssertPropsNew("s0", "val".SplitCsv(), new object[] { "SupportBean(d, -1) SupportBean(e, 2)" });

				env.TryInvalidCompile(
					"select concatstring(*) as val from SupportBean#lastevent, SupportBean unidirectional",
					"Failed to validate select-clause expression 'concatstring(*)': The 'concatstring' aggregation function requires that in joins or subqueries the stream-wildcard (stream-alias.*) syntax is used instead");
				env.UndeployAll();

				// test distinct
				var text = "@name('s0') select irstream concatstring(distinct theString) as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean("a", -1));
				AssertPairSingleRow(env, new object[] { "a" }, new object[] { "" });

				env.SendEventBean(new SupportBean("b", -1));
				AssertPairSingleRow(env, new object[] { "a b" }, new object[] { "a" });

				env.SendEventBean(new SupportBean("b", -1));
				AssertPairSingleRow(env, new object[] { "a b" }, new object[] { "a b" });

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("c", -1));
				AssertPairSingleRow(env, new object[] { "a b c" }, new object[] { "a b" });

				env.SendEventBean(new SupportBean("a", -1));
				AssertPairSingleRow(env, new object[] { "a b c" }, new object[] { "a b c" });

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationManagedDotMethod : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// test dot-method
				SupportSupportBeanAggregationFunctionFactory.InstanceCount = 0;
				var fields = "val0,val1".SplitCsv();
				env.CompileDeploy(
						"@name('s0') select (myagg(id)).getTheString() as val0, (myagg(id)).getIntPrimitive() as val1 from SupportBean_A")
					.AddListener("s0");

				env.SendEventBean(new SupportBean_A("A1"));
				env.AssertPropsNew("s0", fields, new object[] { "XX", 1 });

				env.SendEventBean(new SupportBean_A("A2"));
				env.AssertPropsNew("s0", fields, new object[] { "XX", 2 });

				env.AssertThat(() => Assert.AreEqual(1, SupportSupportBeanAggregationFunctionFactory.InstanceCount));

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationMultiParamMulti : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var milestone = new AtomicLong();
				TryAssertionMultipleParams(env, false, milestone);
				TryAssertionMultipleParams(env, true, milestone);
			}

			private void TryAssertionMultipleParams(
				RegressionEnvironment env,
				bool soda,
				AtomicLong milestone)
			{

				var text = "@name('s0') select irstream countboundary(1,10,intPrimitive,*) as val from SupportBean";
				env.CompileDeploy(soda, text).AddListener("s0");

				env.AssertThat(
					() => {
						var validContext =
							SupportLowerUpperCompareAggregationFunctionForge.Contexts[0];
						EPAssertionUtil.AssertEqualsExactOrder(
							new Type[] { typeof(int), typeof(int), typeof(int?), typeof(SupportBean) },
							validContext.ParameterTypes);
						EPAssertionUtil.AssertEqualsExactOrder(
							new object[] { 1, 10, null, null },
							validContext.ConstantValues);
						EPAssertionUtil.AssertEqualsExactOrder(
							new bool[] { true, true, false, false },
							validContext.IsConstantValue);
					});

				var e1 = new SupportBean("E1", 5);
				env.SendEventBean(e1);
				AssertPairSingleRow(env, new object[] { 1 }, new object[] { 0 });
				env.AssertThat(
					() => EPAssertionUtil.AssertEqualsExactOrder(
						new object[] { 1, 10, 5, e1 },
						SupportLowerUpperCompareAggregationFunction.LastEnterParameters));

				env.SendEventBean(new SupportBean("E1", 0));
				AssertPairSingleRow(env, new object[] { 1 }, new object[] { 1 });

				env.SendEventBean(new SupportBean("E1", 11));
				AssertPairSingleRow(env, new object[] { 1 }, new object[] { 1 });

				env.SendEventBean(new SupportBean("E1", 1));
				AssertPairSingleRow(env, new object[] { 2 }, new object[] { 1 });

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationMultiParamNoParam : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var text = "@name('s0') select irstream countback() as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");

				env.SendEventBean(new SupportBean());
				AssertPairSingleRow(env, new object[] { -1 }, new object[] { 0 });

				env.SendEventBean(new SupportBean());
				AssertPairSingleRow(env, new object[] { -2 }, new object[] { -1 });

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationManagedMappedPropertyLookAlike : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var text = "@name('s0') select irstream concatstring('a') as val from SupportBean";
				env.CompileDeploy(text).AddListener("s0");
				env.AssertStatement(
					"s0",
					statement => Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("val")));

				env.SendEventBean(new SupportBean());
				AssertPairSingleRow(env, new object[] { "a" }, new object[] { "" });

				env.SendEventBean(new SupportBean());
				AssertPairSingleRow(env, new object[] { "a a" }, new object[] { "a" });

				env.Milestone(0);

				env.SendEventBean(new SupportBean());
				AssertPairSingleRow(env, new object[] { "a a a" }, new object[] { "a a" });

				env.UndeployAll();
			}
		}

		private class ClientExtendAggregationFailedValidation : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.TryInvalidCompile(
					"select concatstring(1) from SupportBean",
					"Failed to validate select-clause expression 'concatstring(1)': Plug-in aggregation function 'concatstring' failed validation: Invalid parameter type '");
			}
		}

		private class ClientExtendAggregationInvalidUse : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.TryInvalidCompile(
					"select * from SupportBean group by invalidAggFuncForge(1)",
					"Error resolving aggregation: Class by name 'System.String' does not implement the AggregationFunctionForge interface");

				env.TryInvalidCompile(
					"select * from SupportBean group by nonExistAggFuncForge(1)",
					"Error resolving aggregation: Could not load aggregation factory class by name 'com.NoSuchClass'");
			}
		}

		private class ClientExtendAggregationInvalidCannotResolve : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.TryInvalidCompile(
					"select zzz(theString) from SupportBean",
					"Failed to validate select-clause expression 'zzz(theString)': Unknown single-row function, aggregation function or mapped or indexed property named 'zzz' could not be resolved");
			}
		}

		private static void AssertPairSingleRow(
			RegressionEnvironment env,
			object[] expectedNew,
			object[] expectedOld)
		{
			var fields = "val".SplitCsv();
			env.AssertPropsPerRowIRPair("s0", fields, new object[][] { expectedNew }, new object[][] { expectedOld });
		}
	}
} // end of namespace
