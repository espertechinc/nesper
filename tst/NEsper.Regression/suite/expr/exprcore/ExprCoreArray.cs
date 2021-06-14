///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreArray
	{
		// for use in testing a static method accepting array parameters
		private static int[] _callbackInts;
		private static string[] _callbackStrings;
		private static object[] _callbackObjects;

		public static ICollection<RegressionExecution> Executions()
		{
			var executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreArraySimple());
			executions.Add(new ExprCoreArrayMapResult());
			executions.Add(new ExprCoreArrayCompile());
			executions.Add(new ExprCoreArrayExpressionsOM());
			executions.Add(new ExprCoreArrayComplexTypes());
			executions.Add(new ExprCoreArrayAvroArray());
			return executions;
		}

		private class ExprCoreArraySimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpressions(fields, "{1, 2}");
				builder.WithAssertion(new SupportBean())
					.Verify(
						"c0",
						value => {
							Assert.AreEqual(typeof(int[]), value.GetType());
							EPAssertionUtil.AssertEqualsExactOrder(new[] {1, 2}, value.Unwrap<int>());
						});

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreArrayMapResult : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select " +
				          "{'a', 'b'} as StringArray," +
				          "{} as EmptyArray," +
				          "{1} as OneEleArray," +
				          "{1,2,3} as IntArray," +
				          "{1,null} as IntNullArray," +
				          "{1L,10L} as LongArray," +
				          "{'a',1, 1e20} as MixedArray," +
				          "{1, 1.1d, 1e20} as DoubleArray," +
				          "{5, 6L} as IntLongArray," +
				          "{null} as NullArray," +
				          typeof(ExprCoreArray).FullName + ".DoIt({'a'}, {1}, {1, 'd', null, true}) as Func," +
				          "{true, false} as BoolArray," +
				          "{IntPrimitive} as DynIntArr," +
				          "{IntPrimitive, LongPrimitive} as DynLongArr," +
				          "{IntPrimitive, TheString} as DynMixedArr," +
				          "{IntPrimitive, IntPrimitive * 2, IntPrimitive * 3} as DynCalcArr," +
				          "{LongBoxed, DoubleBoxed * 2, TheString || 'a'} as DynCalcArrNulls" +
				          " from " +
				          typeof(SupportBean).Name;
				env.CompileDeploy(epl).AddListener("s0");

				var bean = new SupportBean("a", 10);
				bean.LongPrimitive = 999;
				env.SendEventBean(bean);

				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("StringArray").Unwrap<string>(), new[] {"a", "b"});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("EmptyArray").Unwrap<object>(), new object[0]);
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("OneEleArray").Unwrap<int>(), new int[] {1});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntArray").Unwrap<int>(), new int[] {1, 2, 3});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntNullArray").Unwrap<int?>(), new int?[] {1, null});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("LongArray").Unwrap<long>(), new long[] {1L, 10L});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("MixedArray").Unwrap<object>(), new object[] {"a", 1, 1e20});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DoubleArray").Unwrap<double>(), new double[] {1d, 1.1, 1e20});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntLongArray").Unwrap<long>(), new long[] {5L, 6L});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("NullArray").Unwrap<object>(), new object[] {null});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("Func").Unwrap<string>(), new[] {"a", "b"});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("BoolArray").Unwrap<bool>(), new bool[] {true, false});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynIntArr").Unwrap<int>(), new int[] {10});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynLongArr").Unwrap<long>(), new long[] {10L, 999L});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynMixedArr").Unwrap<object>(), new object[] {10, "a"});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynCalcArr").Unwrap<int>(), new int[] {10, 20, 30});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynCalcArrNulls").Unwrap<object>(), new object[] {null, null, "aa"});

				// assert function parameters
				EPAssertionUtil.AssertEqualsExactOrder(_callbackInts, new int?[] {1});
				EPAssertionUtil.AssertEqualsExactOrder(_callbackStrings, new[] {"a"});
				EPAssertionUtil.AssertEqualsExactOrder(_callbackObjects, new object[] {1, "d", null, true});

				env.UndeployAll();
			}
		}

		private class ExprCoreArrayCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select {\"a\",\"b\"} as StringArray, " +
				          "{} as EmptyArray, " +
				          "{1} as OneEleArray, " +
				          "{1,2,3} as IntArray " +
				          "from SupportBean";
				env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

				var bean = new SupportBean("a", 10);
				env.SendEventBean(bean);

				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("StringArray").Unwrap<string>(), new[] {"a", "b"});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("EmptyArray").Unwrap<object>(), new object[0]);
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("OneEleArray").Unwrap<int>(), new int[] {1});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntArray").Unwrap<int>(), new int[] {1, 2, 3});

				env.UndeployAll();
			}
		}

		private class ExprCoreArrayComplexTypes : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBeanComplexProps")
					.WithExpressions(fields, "{ArrayProperty, Nested}");

				var bean = SupportBeanComplexProps.MakeDefaultBean();
				builder.WithAssertion(bean)
					.Verify(
						"c0",
						result => {
							var arr = (object[]) result;
							Assert.AreSame(bean.ArrayProperty, arr[0]);
							Assert.AreSame(bean.Nested, arr[1]);
						});

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreArrayExpressionsOM : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@Name('s0') select {\"a\",\"b\"} as StringArray, " +
				          "{} as EmptyArray, " +
				          "{1} as OneEleArray, " +
				          "{1,2,3} as IntArray " +
				          "from " +
				          typeof(SupportBean).Name;
				var model = new EPStatementObjectModel();
				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				model.SelectClause = SelectClause.Create()
					.Add(Expressions.Array().Add(Expressions.Constant("a")).Add(Expressions.Constant("b")), "StringArray")
					.Add(Expressions.Array(), "EmptyArray")
					.Add(Expressions.Array().Add(Expressions.Constant(1)), "OneEleArray")
					.Add(Expressions.Array().Add(Expressions.Constant(1)).Add(2).Add(3), "IntArray");

				model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).Name));
				Assert.AreEqual(epl, model.ToEPL());
				env.CompileDeploy(model).AddListener("s0");

				var bean = new SupportBean("a", 10);
				env.SendEventBean(bean);

				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("StringArray").Unwrap<string>(), new[] {"a", "b"});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("EmptyArray").Unwrap<object>(), new object[0]);
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("OneEleArray").Unwrap<int>(), new int[] {1});
				EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntArray").Unwrap<int>(), new int[] {1, 2, 3});

				env.UndeployAll();
			}
		}

		private class ExprCoreArrayAvroArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var intArraySchema = SchemaBuilder.Array(IntType());
				var mixedArraySchema = SchemaBuilder.Array(Union(IntType(), StringType(), DoubleType()));
				var nullArraySchema = SchemaBuilder.Array(NullType());

				var stmtText =
					"@Name('s0') " +
					"@AvroSchemaField(Name='EmptyArray', Schema='" + intArraySchema + "')" +
					"@AvroSchemaField(Name='MixedArray', Schema='" + mixedArraySchema + "')" +
					"@AvroSchemaField(Name='NullArray', Schema='" + nullArraySchema + "')" +
					EventRepresentationChoice.AVRO.GetAnnotationText() +
					"select {'a', 'b'} as StringArray," +
					"{} as EmptyArray," +
					"{1} as OneEleArray," +
					"{1,2,3} as IntArray," +
					"{1,null} as IntNullArray," +
					"{1L,10L} as LongArray," +
					"{'a',1, 1e20} as MixedArray," +
					"{1, 1.1d, 1e20} as DoubleArray," +
					"{5, 6L} as IntLongArray," +
					"{null} as NullArray," +
					"{true, false} as BoolArray" +
					" from SupportBean";
				env.CompileDeploy(stmtText).AddListener("s0");

				env.SendEventBean(new SupportBean());

				var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
				SupportAvroUtil.AvroToJson(theEvent);

				CompareColl(theEvent, "StringArray", new[] {"a", "b"});
				CompareColl(theEvent, "EmptyArray", new object[0]);
				CompareColl(theEvent, "OneEleArray", new int?[] {1});
				CompareColl(theEvent, "IntArray", new int?[] {1, 2, 3});
				CompareColl(theEvent, "IntNullArray", new int?[] {1, null});
				CompareColl(theEvent, "LongArray", new long?[] {1L, 10L});
				CompareColl(theEvent, "MixedArray", new object[] {"a", 1, 1e20});
				CompareColl(theEvent, "DoubleArray", new double?[] {1d, 1.1, 1e20});
				CompareColl(theEvent, "IntLongArray", new long?[] {5L, 6L});
				CompareColl(theEvent, "NullArray", new object[] {null});
				CompareColl(theEvent, "BoolArray", new bool?[] {true, false});

				env.UndeployAll();
			}
		}

		// for testing EPL static method call
		private static void CompareColl<T>(
			EventBean @event,
			string property,
			T[] expected)
		{
			var col = @event.Get(property).UnwrapIntoArray<T>();
			EPAssertionUtil.AssertEqualsExactOrder(col, expected);
		}

		public static string[] DoIt(
			string[] strings,
			int[] ints,
			object[] objects)
		{
			_callbackInts = ints;
			_callbackStrings = strings;
			_callbackObjects = objects;
			return new[] {"a", "b"};
		}
	}
} // end of namespace
