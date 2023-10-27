///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NEsper.Avro.Extensions;
using NEsper.Avro.Support;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using Array = System.Array;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps; // assertEquals

// assertSame

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreArray
    {
        // for use in testing a static method accepting array parameters
        private static int?[] callbackInts;
        private static string[] callbackStrings;
        private static object[] callbackObjects;

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
                            Assert.AreEqual(typeof(int?[]), value.GetType());
                            EPAssertionUtil.AssertEqualsExactOrder(new int?[] { 1, 2 }, (int?[])value);
                        });

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreArrayMapResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select {'a', 'b'} as stringArray," +
                          "{} as emptyArray," +
                          "{1} as oneEleArray," +
                          "{1,2,3} as intArray," +
                          "{1,null} as intNullArray," +
                          "{1L,10L} as longArray," +
                          "{'a',1, 1e20} as mixedArray," +
                          "{1, 1.1d, 1e20} as doubleArray," +
                          "{5, 6L} as intLongArray," +
                          "{null} as nullArray," +
                          typeof(ExprCoreArray).FullName +
                          ".doIt({'a'}, new object[] {1}, new object[] {1, 'd', null, true}) as func," +
                          "{true, false} as boolArray," +
                          "{IntPrimitive} as dynIntArr," +
                          "{IntPrimitive, LongPrimitive} as dynLongArr," +
                          "{IntPrimitive, TheString} as dynMixedArr," +
                          "{IntPrimitive, IntPrimitive * 2, IntPrimitive * 3} as dynCalcArr," +
                          "{LongBoxed, DoubleBoxed * 2, TheString || 'a'} as dynCalcArrNulls" +
                          " from " +
                          nameof(SupportBean);
                env.CompileDeploy(epl).AddListener("s0");

                var bean = new SupportBean("a", 10);
                bean.LongPrimitive = 999;
                env.SendEventBean(bean);

                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (string[])@event.Get("stringArray"),
                            new string[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])@event.Get("emptyArray"),
                            Array.Empty<object>());
                        EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("oneEleArray"), new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("intArray"), new int?[] { 1, 2, 3 });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (int?[])@event.Get("intNullArray"),
                            new int?[] { 1, null });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (long?[])@event.Get("longArray"),
                            new long?[] { 1L, 10L });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])@event.Get("mixedArray"),
                            new object[] { "a", 1, 1e20 });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (double?[])@event.Get("doubleArray"),
                            new double?[] { 1d, 1.1, 1e20 });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (long?[])@event.Get("intLongArray"),
                            new long?[] { 5L, 6L });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])@event.Get("nullArray"),
                            new object[] { null });
                        EPAssertionUtil.AssertEqualsExactOrder((string[])@event.Get("func"), new string[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (bool[])@event.Get("boolArray"),
                            new bool[] { true, false });
                        EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("dynIntArr"), new int?[] { 10 });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (long?[])@event.Get("dynLongArr"),
                            new long?[] { 10L, 999L });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])@event.Get("dynMixedArr"),
                            new object[] { 10, "a" });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (int?[])@event.Get("dynCalcArr"),
                            new int?[] { 10, 20, 30 });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])@event.Get("dynCalcArrNulls"),
                            new object[] { null, null, "aa" });
                    });

                // assert function parameters
                env.AssertThat(
                    () => {
                        EPAssertionUtil.AssertEqualsExactOrder(callbackInts, new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder(callbackStrings, new string[] { "a" });
                        EPAssertionUtil.AssertEqualsExactOrder(callbackObjects, new object[] { 1, "d", null, true });
                    });

                env.UndeployAll();
            }
        }

        private class ExprCoreArrayCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select {\"a\",\"b\"} as stringArray, " +
                          "{} as emptyArray, " +
                          "{1} as oneEleArray, " +
                          "{1,2,3} as intArray " +
                          "from SupportBean";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                var bean = new SupportBean("a", 10);
                env.SendEventBean(bean);

                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (string[])@event.Get("stringArray"),
                            new string[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])@event.Get("emptyArray"),
                            Array.Empty<object>());
                        EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("oneEleArray"), new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("intArray"), new int?[] { 1, 2, 3 });
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
                    .WithExpressions(fields, "{ArrayProperty, nested}");

                var bean = SupportBeanComplexProps.MakeDefaultBean();
                builder.WithAssertion(bean)
                    .Verify(
                        "c0",
                        result => {
                            var arr = (object[])result;
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
                var epl = "@name('s0') select {\"a\",\"b\"} as stringArray, " +
                          "{} as emptyArray, " +
                          "{1} as oneEleArray, " +
                          "{1,2,3} as intArray " +
                          "from " +
                          nameof(SupportBean);
                var model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.Array().Add(Expressions.Constant("a")).Add(Expressions.Constant("b")),
                        "stringArray")
                    .Add(Expressions.Array(), "emptyArray")
                    .Add(Expressions.Array().Add(Expressions.Constant(1)), "oneEleArray")
                    .Add(Expressions.Array().Add(Expressions.Constant(1)).Add(2).Add(3), "intArray");

                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                Assert.AreEqual(epl, model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                var bean = new SupportBean("a", 10);
                env.SendEventBean(bean);

                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (string[])@event.Get("stringArray"),
                            new string[] { "a", "b" });
                        EPAssertionUtil.AssertEqualsExactOrder(
                            (object[])@event.Get("emptyArray"),
                            Array.Empty<object>());
                        EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("oneEleArray"), new int?[] { 1 });
                        EPAssertionUtil.AssertEqualsExactOrder((int?[])@event.Get("intArray"), new int?[] { 1, 2, 3 });
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
                    "@name('s0') @AvroSchemaField(name='emptyArray', schema='" +
                    intArraySchema.ToString() +
                    "')" +
                    "@AvroSchemaField(name='mixedArray', schema='" +
                    mixedArraySchema.ToString() +
                    "')" +
                    "@AvroSchemaField(name='nullArray', schema='" +
                    nullArraySchema.ToString() +
                    "')" +
                    EventRepresentationChoice.AVRO.GetAnnotationText() +
                    "select {'a', 'b'} as stringArray," +
                    "{} as emptyArray," +
                    "{1} as oneEleArray," +
                    "{1,2,3} as intArray," +
                    "{1,null} as intNullArray," +
                    "{1L,10L} as longArray," +
                    "{'a',1, 1e20} as mixedArray," +
                    "{1, 1.1d, 1e20} as doubleArray," +
                    "{5, 6L} as intLongArray," +
                    "{null} as nullArray," +
                    "{true, false} as boolArray" +
                    " from SupportBean";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(new SupportBean());

                env.AssertEventNew(
                    "s0",
                    @event => {
                        SupportAvroUtil.AvroToJson(@event);

                        CompareColl(@event, "stringArray", new string[] { "a", "b" });
                        CompareColl(@event, "emptyArray", Array.Empty<object>());
                        CompareColl(@event, "oneEleArray", new int?[] { 1 });
                        CompareColl(@event, "intArray", new int?[] { 1, 2, 3 });
                        CompareColl(@event, "intNullArray", new int?[] { 1, null });
                        CompareColl(@event, "longArray", new long?[] { 1L, 10L });
                        CompareColl(@event, "mixedArray", new object[] { "a", 1, 1e20 });
                        CompareColl(@event, "doubleArray", new double?[] { 1d, 1.1, 1e20 });
                        CompareColl(@event, "intLongArray", new long?[] { 5L, 6L });
                        CompareColl(@event, "nullArray", new object[] { null });
                        CompareColl(@event, "boolArray", new bool[] { true, false });
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
                var anyCollection = rawValue.Unwrap<object>();
                EPAssertionUtil.AssertEqualsExactOrder(anyCollection, expected.Unwrap<object>());
            }
        }

        public static string[] DoIt(
            string[] strings,
            int?[] ints,
            object[] objects)
        {
            callbackInts = ints;
            callbackStrings = strings;
            callbackObjects = objects;
            return new string[] { "a", "b" };
        }
    }
} // end of namespace