///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using Avro;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

// using static org.apache.avro.SchemaBuilder.*;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprArrayExpression : RegressionExecution {
        // for use in testing a static method accepting array parameters
        private static int?[] _callbackInts;
        private static string[] _callbackStrings;
        private static object[] _callbackObjects;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionArrayMapResult(epService);
            RunAssertionArrayAvroResult(epService);
            RunAssertionArrayExpressions_Compile(epService);
            RunAssertionArrayExpressions_OM(epService);
            RunAssertionComplexTypes(epService);
        }
    
        private void RunAssertionComplexTypes(EPServiceProvider epService) {
            var stmtText = "select {arrayProperty, nested} as field" +
                    " from " + typeof(SupportBeanComplexProps).FullName;
    
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var bean = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            var arr = (object[]) theEvent.Get("field");
            Assert.AreSame(bean.ArrayProperty, arr[0]);
            Assert.AreSame(bean.Nested, arr[1]);
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayMapResult(EPServiceProvider epService) {
            var stmtText = "select {'a', 'b'} as stringArray," +
                    "{} as emptyArray," +
                    "{1} as oneEleArray," +
                    "{1,2,3} as intArray," +
                    "{1,null} as intNullArray," +
                    "{1L,10L} as longArray," +
                    "{'a',1, 1e20} as mixedArray," +
                    "{1, 1.1d, 1e20} as doubleArray," +
                    "{5, 6L} as intLongArray," +
                    "{null} as nullArray," +
                    typeof(ExecExprArrayExpression).FullName + ".DoIt({'a'}, {1}, {1, 'd', null, true}) as func," +
                    "{true, false} as boolArray," +
                    "{IntPrimitive} as dynIntArr," +
                    "{IntPrimitive, LongPrimitive} as dynLongArr," +
                    "{IntPrimitive, TheString} as dynMixedArr," +
                    "{IntPrimitive, IntPrimitive * 2, IntPrimitive * 3} as dynCalcArr," +
                    "{LongBoxed, DoubleBoxed * 2, TheString || 'a'} as dynCalcArrNulls" +
                    " from " + typeof(SupportBean).FullName;
    
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var bean = new SupportBean("a", 10);
            bean.LongPrimitive = 999;
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("stringArray"), new string[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("emptyArray"), new object[0]);
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[]{1, 2, 3});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intNullArray"), new int?[]{1, null});
            EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("longArray"), new long?[]{1L, 10L});
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("mixedArray"), new object[]{"a", 1, 1e20});
            EPAssertionUtil.AssertEqualsExactOrder((double?[]) theEvent.Get("doubleArray"), new double?[]{1d, 1.1, 1e20});
            EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("intLongArray"), new long?[]{5L, 6L});
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("nullArray"), new object[]{null});
            EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("func"), new string[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((bool?[]) theEvent.Get("boolArray"), new bool?[]{true, false});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("dynIntArr"), new int?[]{10});
            EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("dynLongArr"), new long?[]{10L, 999L});
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("dynMixedArr"), new object[]{10, "a"});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("dynCalcArr"), new int?[]{10, 20, 30});
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("dynCalcArrNulls"), new object[]{null, null, "aa"});
    
            // assert function parameters
            EPAssertionUtil.AssertEqualsExactOrder(_callbackInts, new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder(_callbackStrings, new string[]{"a"});
            EPAssertionUtil.AssertEqualsExactOrder(_callbackObjects, new object[]{1, "d", null, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayAvroResult(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            Schema intArraySchema = SchemaBuilder.Array(IntType());
            Schema mixedArraySchema = SchemaBuilder.Array(Union(IntType(), StringType(), DoubleType()));
            Schema nullArraySchema = SchemaBuilder.Array(NullType());
    
            var stmtText =
                    "@AvroSchemaField(Name='emptyArray', Schema='" + intArraySchema + "')" +
                    "@AvroSchemaField(Name='mixedArray', Schema='" + mixedArraySchema + "')" +
                    "@AvroSchemaField(Name='nullArray', Schema='" + nullArraySchema + "')" +
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
                    " from " + typeof(SupportBean).FullName;
    
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
    
            var theEvent = listener.AssertOneGetNewAndReset();
            SupportAvroUtil.AvroToJson(theEvent);
    
            CompareColl(theEvent, "stringArray", new object[]{"a", "b"});
            CompareColl(theEvent, "emptyArray", new object[0]);
            CompareColl(theEvent, "oneEleArray", new object[]{1});
            CompareColl(theEvent, "intArray", new object[]{1, 2, 3});
            CompareColl(theEvent, "intNullArray", new object[]{1, null});
            CompareColl(theEvent, "longArray", new object[]{1L, 10L});
            CompareColl(theEvent, "mixedArray", new object[]{"a", 1, 1e20});
            CompareColl(theEvent, "doubleArray", new object[]{1d, 1.1, 1e20});
            CompareColl(theEvent, "intLongArray", new object[]{5L, 6L});
            CompareColl(theEvent, "nullArray", new object[]{null});
            CompareColl(theEvent, "boolArray", new object[]{true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayExpressions_OM(EPServiceProvider epService) {
            var stmtText = "select {\"a\",\"b\"} as stringArray, " +
                    "{} as emptyArray, " +
                    "{1} as oneEleArray, " +
                    "{1,2,3} as intArray " +
                    "from " + typeof(SupportBean).FullName;
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.Array().Add(Expressions.Constant("a")).Add(Expressions.Constant("b")), "stringArray")
                .Add(Expressions.Array(), "emptyArray")
                .Add(Expressions.Array().Add(Expressions.Constant(1)), "oneEleArray")
                .Add(Expressions.Array().Add(Expressions.Constant(1)).Add(2).Add(3), "intArray");

            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            var stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var bean = new SupportBean("a", 10);
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("stringArray"), new string[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("emptyArray"), new object[0]);
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[]{1, 2, 3});
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayExpressions_Compile(EPServiceProvider epService) {
            var stmtText = "select {\"a\",\"b\"} as stringArray, " +
                    "{} as emptyArray, " +
                    "{1} as oneEleArray, " +
                    "{1,2,3} as intArray " +
                    "from " + typeof(SupportBean).FullName;
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            var stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var bean = new SupportBean("a", 10);
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder((string[]) theEvent.Get("stringArray"), new string[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("emptyArray"), new object[0]);
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[]{1, 2, 3});
    
            stmt.Dispose();
        }
    
        // for testing EPL static method call
        public static string[] DoIt(string[] strings, int?[] ints, object[] objects) {
            _callbackInts = ints;
            _callbackStrings = strings;
            _callbackObjects = objects;
            return new string[]{"a", "b"};
        }
    
        private static void CompareColl(EventBean @event, string property, object[] expected) {
            var col = @event.Get(property).UnwrapIntoArray<object>();
            EPAssertionUtil.AssertEqualsExactOrder(col, expected);
        }
    }
} // end of namespace
