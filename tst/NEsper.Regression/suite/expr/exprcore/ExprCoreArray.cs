///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NEsper.Avro.Extensions;
using NEsper.Avro.Support;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static NEsper.Avro.Extensions.TypeBuilder;

using Array = System.Array;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreArray
    {
        // for use in testing a static method accepting array parameters
        private static int?[] _callbackInts;
        private static string[] _callbackStrings;
        private static object[] _callbackObjects;

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithMapResult(execs);
            WithCompile(execs);
            WithExpressionsOM(execs);
            WithComplexTypes(execs);
            WithAvroArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAvroArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreArrayAvroArray());
            return execs;
        }

        public static IList<RegressionExecution> WithComplexTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreArrayComplexTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionsOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreArrayExpressionsOM());
            return execs;
        }

        public static IList<RegressionExecution> WithCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreArrayCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithMapResult(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreArrayMapResult());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreArraySimple());
            return execs;
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
                            ClassicAssert.AreEqual(typeof(int[]), value.GetType());
                            EPAssertionUtil.AssertEqualsExactOrder(new int[] { 1, 2 }, value.Unwrap<int>());
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
                          nameof(SupportBean);
                env.CompileDeploy(epl).AddListener("s0");

                var bean = new SupportBean("a", 10);
                bean.LongPrimitive = 999;
                env.SendEventBean(bean);

                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("StringArray").Unwrap<string>(), new string[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("EmptyArray").Unwrap<object>(), Array.Empty<object>());
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("OneEleArray").Unwrap<int?>(), new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntArray").Unwrap<int?>(), new int?[] { 1, 2, 3 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntNullArray").Unwrap<int?>(), new int?[] { 1, null });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("LongArray").Unwrap<long?>(), new long?[] { 1L, 10L });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("MixedArray").Unwrap<object>(), new object[] { "a", 1, 1e20 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DoubleArray").Unwrap<double?>(), new double?[] { 1d, 1.1, 1e20 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntLongArray").Unwrap<long?>(), new long?[] { 5L, 6L });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("NullArray").Unwrap<object>(), new object[] { null });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("Func").Unwrap<string>(), new string[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("BoolArray").Unwrap<bool>(), new bool[] { true, false });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynIntArr").Unwrap<int?>(), new int?[] { 10 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynLongArr").Unwrap<long?>(), new long?[] { 10L, 999L });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynMixedArr").Unwrap<object>(), new object[] { 10, "a" });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynCalcArr").Unwrap<int?>(), new int?[] { 10, 20, 30 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("DynCalcArrNulls").Unwrap<object>(), new object[] { null, null, "aa" });
                    });

                // assert function parameters
                env.AssertThat(
                    () => {
                        EPAssertionUtil.AssertEqualsExactOrder(_callbackInts, new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder(_callbackStrings, new string[] { "a" });
                        EPAssertionUtil.AssertEqualsExactOrder(_callbackObjects, new object[] { 1, "d", null, true });
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreArrayCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select {\"a\",\"b\"} as StringArray, " +
                    "{} as EmptyArray, " +
                    "{1} as OneEleArray, " +
                    "{1,2,3} as IntArray " +
                    "from SupportBean";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                var bean = new SupportBean("a", 10);
                env.SendEventBean(bean);

                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("StringArray").Unwrap<string>(), new string[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("EmptyArray").Unwrap<object>(), Array.Empty<object>());
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("OneEleArray").Unwrap<int?>(), new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntArray").Unwrap<int?>(), new int?[] { 1, 2, 3 });
                    });

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
                            var arr = (object[])result;
                            ClassicAssert.AreSame(bean.ArrayProperty, arr[0]);
                            ClassicAssert.AreSame(bean.Nested, arr[1]);
                        });

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreArrayExpressionsOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select {\"a\",\"b\"} as StringArray, " +
                    "{} as EmptyArray, " +
                    "{1} as OneEleArray, " +
                    "{1,2,3} as IntArray " +
                    "from " +
                    nameof(SupportBean);
                var model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.Array().Add(Expressions.Constant("a")).Add(Expressions.Constant("b")), "StringArray")
                    .Add(Expressions.Array(), "EmptyArray")
                    .Add(Expressions.Array().Add(Expressions.Constant(1)), "OneEleArray")
                    .Add(Expressions.Array().Add(Expressions.Constant(1)).Add(2).Add(3), "IntArray");

                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                ClassicAssert.AreEqual(epl, model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                var bean = new SupportBean("a", 10);
                env.SendEventBean(bean);

                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("StringArray").Unwrap<string>(), new[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("EmptyArray").Unwrap<object>(), Array.Empty<object>());
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("OneEleArray").Unwrap<int?>(), new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("IntArray").Unwrap<int?>(), new int?[] { 1, 2, 3 });
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreArrayAvroArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Schema intArraySchema = SchemaBuilder.Array(IntType());
                Schema mixedArraySchema = SchemaBuilder.Array(Union(IntType(), StringType(), DoubleType()));
                Schema nullArraySchema = SchemaBuilder.Array(NullType());

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

                env.AssertEventNew(
                    "s0",
                    theEvent => {
                        SupportAvroUtil.AvroToJson(theEvent);

                        CompareColl(theEvent, "StringArray", new[] { "a", "b" });
                        CompareColl(theEvent, "EmptyArray", Array.Empty<object>());
                        CompareColl(theEvent, "OneEleArray", new int[] { 1 });
                        CompareColl(theEvent, "IntArray", new int[] { 1, 2, 3 });
                        CompareColl(theEvent, "IntNullArray", new int?[] { 1, null });
                        CompareColl(theEvent, "LongArray", new long[] { 1L, 10L });
                        CompareColl(theEvent, "MixedArray", new object[] { "a", 1, 1e20 });
                        CompareColl(theEvent, "DoubleArray", new double[] { 1d, 1.1, 1e20 });
                        CompareColl(theEvent, "IntLongArray", new long?[] { 5L, 6L });
                        CompareColl(theEvent, "NullArray", new object[] { null });
                        CompareColl(theEvent, "BoolArray", new bool[] { true, false });
                    });

                env.UndeployAll();
            }
        }

        // for testing EPL static method call
        private static void CompareColl<T>(
            EventBean @event,
            string property,
            T[] expected)
        {
            var rawValue = @event.Get(property);
            Assert.That(rawValue, Is.Not.Null);
            Assert.That(rawValue, Is.InstanceOf<ICollection<T>>().Or.InstanceOf<ICollection<object>>());

            if (rawValue is ICollection<T> typeCollection) {
                EPAssertionUtil.AssertEqualsExactOrder(typeCollection, expected);
            }
            else {
                var anyCollection = rawValue.Unwrap<object>(true);
                EPAssertionUtil.AssertEqualsExactOrder(anyCollection, expected.Unwrap<object>(true));
            }
        }

        public static string[] DoIt(
            string[] strings,
            int[] ints,
            object[] objects)
        {
            _callbackInts = ints.UnwrapIntoArray<int?>();
            _callbackStrings = strings;
            _callbackObjects = objects;
            return new string[] { "a", "b" };
        }
    }
} // end of namespace