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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreArray
    {
        // for use in testing a static method accepting array parameters
        private static int?[] callbackInts;
        private static string[] callbackStrings;
        private static object[] callbackObjects;

        public static IList<RegressionExecution> Executions()
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
            int?[] ints,
            object[] objects)
        {
            callbackInts = ints;
            callbackStrings = strings;
            callbackObjects = objects;
            return new[] {"a", "b"};
        }

        internal class ExprCoreArraySimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select {1, 2} as c0 from SupportBean";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean());
                var result = env.Listener("s0").AssertOneGetNewAndReset().Get("c0");
                Assert.AreEqual(typeof(int?[]), result.GetType());
                EPAssertionUtil.AssertEqualsExactOrder(
                    new int?[] {1, 2},
                    (int?[]) result);

                env.UndeployAll();
            }
        }

        internal class ExprCoreArrayMapResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select {'a', 'b'} as stringArray," +
                          "{} as emptyArray," +
                          "{1} as oneEleArray," +
                          "{1,2,3} as intArray," +
                          "{1,null} as intNullArray," +
                          "{1L,10L} as longArray," +
                          "{'a',1, 1e20} as mixedArray," +
                          "{1, 1.1d, 1e20} as doubleArray," +
                          "{5, 6L} as intLongArray," +
                          "{null} as nullArray," +
                          typeof(ExprCoreArray).Name +
                          ".doIt({'a'}, new object[] {1}, new object[] {1, 'd', null, true}) as func," +
                          "{true, false} as boolArray," +
                          "{intPrimitive} as dynIntArr," +
                          "{intPrimitive, longPrimitive} as dynLongArr," +
                          "{intPrimitive, theString} as dynMixedArr," +
                          "{intPrimitive, intPrimitive * 2, IntPrimitive * 3} as dynCalcArr," +
                          "{longBoxed, doubleBoxed * 2, theString || 'a'} as dynCalcArrNulls" +
                          " from " +
                          typeof(SupportBean).Name;
                env.CompileDeploy(epl).AddListener("s0");

                var bean = new SupportBean("a", 10);
                bean.LongPrimitive = 999;
                env.SendEventBean(bean);

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("stringArray"), new[] {"a", "b"});
                EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("emptyArray"), new object[0]);
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[] {1});
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[] {1, 2, 3});
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intNullArray"), new int?[] {1, null});
                EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("longArray"), new long?[] {1L, 10L});
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("mixedArray"),
                    new object[] {"a", 1, 1e20});
                EPAssertionUtil.AssertEqualsExactOrder(
                    (double?[]) theEvent.Get("doubleArray"),
                    new double?[] {1d, 1.1, 1e20});
                EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("intLongArray"), new long?[] {5L, 6L});
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("nullArray"),
                    new object[] {null});
                EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("func"), new[] {"a", "b"});
                EPAssertionUtil.AssertEqualsExactOrder((bool?[]) theEvent.Get("boolArray"), new bool?[] {true, false});
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("dynIntArr"), new int?[] {10});
                EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("dynLongArr"), new long?[] {10L, 999L});
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("dynMixedArr"),
                    new object[] {10, "a"});
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("dynCalcArr"), new int?[] {10, 20, 30});
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("dynCalcArrNulls"),
                    new object[] {null, null, "aa"});

                // assert function parameters
                EPAssertionUtil.AssertEqualsExactOrder(callbackInts, new int?[] {1});
                EPAssertionUtil.AssertEqualsExactOrder(callbackStrings, new[] {"a"});
                EPAssertionUtil.AssertEqualsExactOrder(
                    callbackObjects,
                    new object[] {1, "d", null, true});

                env.UndeployAll();
            }
        }

        internal class ExprCoreArrayCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select {\"a\",\"b\"} as stringArray, " +
                          "{} as emptyArray, " +
                          "{1} as oneEleArray, " +
                          "{1,2,3} as intArray " +
                          "from SupportBean";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                var bean = new SupportBean("a", 10);
                env.SendEventBean(bean);

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("stringArray"), new[] {"a", "b"});
                EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("emptyArray"), new object[0]);
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[] {1});
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[] {1, 2, 3});

                env.UndeployAll();
            }
        }

        internal class ExprCoreArrayComplexTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select {arrayProperty, nested} as field from " +
                          typeof(SupportBeanComplexProps).Name;
                env.CompileDeploy(epl).AddListener("s0");

                var bean = SupportBeanComplexProps.MakeDefaultBean();
                env.SendEventBean(bean);

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                var arr = (object[]) theEvent.Get("field");
                Assert.AreSame(bean.ArrayProperty, arr[0]);
                Assert.AreSame(bean.Nested, arr[1]);

                env.UndeployAll();
            }
        }

        internal class ExprCoreArrayExpressionsOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select {\"a\",\"b\"} as stringArray, " +
                          "{} as emptyArray, " +
                          "{1} as oneEleArray, " +
                          "{1,2,3} as intArray " +
                          "from " +
                          typeof(SupportBean).Name;
                var model = new EPStatementObjectModel();
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                model.SelectClause = SelectClause.Create()
                    .Add(
                        Expressions.Array().Add(Expressions.Constant("a")).Add(Expressions.Constant("b")),
                        "stringArray")
                    .Add(Expressions.Array(), "emptyArray")
                    .Add(Expressions.Array().Add(Expressions.Constant(1)), "oneEleArray")
                    .Add(Expressions.Array().Add(Expressions.Constant(1)).Add(2).Add(3), "intArray");

                model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).Name));
                Assert.AreEqual(epl, model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                var bean = new SupportBean("a", 10);
                env.SendEventBean(bean);

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("stringArray"), new[] {"a", "b"});
                EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("emptyArray"), new object[0]);
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[] {1});
                EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[] {1, 2, 3});

                env.UndeployAll();
            }
        }

        internal class ExprCoreArrayAvroArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var intArraySchema = SchemaBuilder.Array(TypeBuilder.IntType());
                var mixedArraySchema = SchemaBuilder.Array(
                    TypeBuilder.Union(
                        TypeBuilder.IntType(),
                        TypeBuilder.StringType(),
                        TypeBuilder.DoubleType()));
                var nullArraySchema = SchemaBuilder.Array(TypeBuilder.NullType());

                var stmtText =
                    "@Name('s0') @AvroSchemaField(name='emptyArray', schema='" +
                    intArraySchema +
                    "')" +
                    "@AvroSchemaField(name='mixedArray', schema='" +
                    mixedArraySchema +
                    "')" +
                    "@AvroSchemaField(name='nullArray', schema='" +
                    nullArraySchema +
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

                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                SupportAvroUtil.AvroToJson(theEvent);

                CompareColl(theEvent, "StringArray", new[] {"a", "b"});
                CompareColl(theEvent, "EmptyArray", new object[0]);
                CompareColl(theEvent, "OneEleArray", new int?[] {1});
                CompareColl(theEvent, "IntArray", new int?[] {1, 2, 3});
                CompareColl(theEvent, "IntNullArray", new int?[] {1, null});
                CompareColl(theEvent, "LongArray", new long?[] {1L, 10L});
                CompareColl(
                    theEvent,
                    "MixedArray",
                    new object[] {"a", 1, 1e20});
                CompareColl(theEvent, "DoubleArray", new double?[] {1d, 1.1, 1e20});
                CompareColl(theEvent, "IntLongArray", new long?[] {5L, 6L});
                CompareColl(
                    theEvent,
                    "NullArray",
                    new object[] {null});
                CompareColl(theEvent, "BoolArray", new bool?[] {true, false});

                env.UndeployAll();
            }
        }
    }
} // end of namespace