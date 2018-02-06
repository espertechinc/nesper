///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using Avro;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

namespace com.espertech.esper.regression.expr
{
    public class TestArrayExpression
    {
        // for use in testing a static method accepting array parameters
        private static int?[] _callbackInts;
        private static string[] _callbackStrings;
        private static object[] _callbackObjects;
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);
            }
        }
    
        protected void TearDown() {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            }
        }
    
        [Test]
        public void TestArrayExpressions() {
            RunAssertionArrayMapResult();
            RunAssertionArrayAvroResult();
            RunAssertionArrayExpressions_Compile();
            RunAssertionArrayExpressions_OM();
            RunAssertionComplexTypes();
        }
    
        private void RunAssertionComplexTypes() {
            string stmtText = "select {arrayProperty, nested} as field" +
                              " from " + Name.Of<SupportBeanComplexProps>();
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SupportBeanComplexProps bean = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            object[] arr = (object[]) theEvent.Get("field");
            Assert.AreSame(bean.ArrayProperty, arr[0]);
            Assert.AreSame(bean.Nested, arr[1]);
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayMapResult() {
            string stmtText = "select {'a', 'b'} as stringArray," +
                              "{} as emptyArray," +
                              "{1} as oneEleArray," +
                              "{1,2,3} as intArray," +
                              "{1,null} as intNullArray," +
                              "{1L,10L} as longArray," +
                              "{'a',1, 1e20} as mixedArray," +
                              "{1, 1.1d, 1e20} as doubleArray," +
                              "{5, 6L} as intLongArray," +
                              "{null} as nullArray," +
                              typeof(TestArrayExpression).FullName + ".DoIt({'a'}, {1}, {1, 'd', null, true}) as func," +
                              "{true, false} as boolArray," +
                              "{intPrimitive} as dynIntArr," +
                              "{intPrimitive, longPrimitive} as dynLongArr," +
                              "{intPrimitive, theString} as dynMixedArr," +
                              "{intPrimitive, intPrimitive * 2, intPrimitive * 3} as dynCalcArr," +
                              "{longBoxed, doubleBoxed * 2, theString || 'a'} as dynCalcArrNulls" +
                              " from " + typeof(SupportBean).FullName;
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var bean = new SupportBean("a", 10);
            bean.LongPrimitive = 999;
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("stringArray").UnwrapIntoArray<string>(), new string[] {"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("emptyArray").UnwrapIntoArray<object>(), new object[0]);
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("oneEleArray").UnwrapIntoArray<int?>(), new int?[] {1});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("intArray").UnwrapIntoArray<int?>(), new int?[] {1, 2, 3});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("intNullArray").UnwrapIntoArray<int?>(), new int?[] {1, null});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("longArray").UnwrapIntoArray<long?>(), new long?[] {1L, 10L});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("mixedArray").UnwrapIntoArray<object>(), new object[] {"a", 1, 1e20});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("doubleArray").UnwrapIntoArray<double?>(), new double?[] {1d, 1.1, 1e20});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("intLongArray").UnwrapIntoArray<long?>(), new long?[] {5L, 6L});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("nullArray").UnwrapIntoArray<object>(), new object[] {null});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("func").UnwrapIntoArray<string>(), new string[] {"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("boolArray").UnwrapIntoArray<bool?>(), new bool?[] {true, false});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("dynIntArr").UnwrapIntoArray<int?>(), new int?[] {10});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("dynLongArr").UnwrapIntoArray<long?>(), new long?[] {10L, 999L});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("dynMixedArr").UnwrapIntoArray<object>(), new object[] {10, "a"});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("dynCalcArr").UnwrapIntoArray<int?>(), new int?[] {10, 20, 30});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("dynCalcArrNulls").UnwrapIntoArray<object>(), new object[] {null, null, "aa"});
    
            // assert function parameters
            EPAssertionUtil.AssertEqualsExactOrder(_callbackInts, new int?[] {1});
            EPAssertionUtil.AssertEqualsExactOrder(_callbackStrings, new string[] {"a"});
            EPAssertionUtil.AssertEqualsExactOrder(_callbackObjects, new object[] {1, "d", null, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayAvroResult() {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));

            Schema intArraySchema = SchemaBuilder.Array(TypeBuilder.Int());
            Schema mixedArraySchema = SchemaBuilder.Array(TypeBuilder.Union(TypeBuilder.Int(), TypeBuilder.String(), TypeBuilder.Double()));
            Schema nullArraySchema = SchemaBuilder.Array(TypeBuilder.Null());
    
            string stmtText =
                "@AvroSchemaField(Name='emptyArray', Schema='" + intArraySchema.ToString() + "')" +
                "@AvroSchemaField(Name='mixedArray', Schema='" + mixedArraySchema.ToString() + "')" +
                "@AvroSchemaField(Name='nullArray', Schema='" + nullArraySchema.ToString() + "')" +
                EventRepresentationChoice.AVRO.GetAnnotationText() +
                " " +
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
                " from " + typeof(SupportBean).Name;
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            _epService.EPRuntime.SendEvent(new SupportBean());
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            SupportAvroUtil.AvroToJson(theEvent);
    
            CompareColl(theEvent, "stringArray", new string[] {"a", "b"});
            CompareColl(theEvent, "emptyArray", new object[0]);
            CompareColl(theEvent, "oneEleArray", new int[] {1});
            CompareColl(theEvent, "intArray", new int[] {1, 2, 3});
            CompareColl(theEvent, "intNullArray", new int?[] {1, null});
            CompareColl(theEvent, "longArray", new long[] {1L, 10L});
            CompareColl(theEvent, "mixedArray", new object[] {"a", 1, 1e20});
            CompareColl(theEvent, "doubleArray", new double[] {1d, 1.1, 1e20});
            CompareColl(theEvent, "intLongArray", new long[] {5L, 6L});
            CompareColl(theEvent, "nullArray", new object[] {null});
            CompareColl(theEvent, "boolArray", new bool[] {true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayExpressions_OM() {
            string stmtText = "select {\"a\",\"b\"} as stringArray, " +
                              "{} as emptyArray, " +
                              "{1} as oneEleArray, " +
                              "{1,2,3} as intArray " +
                              "from " + Name.Of<SupportBean>();
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.Array().Add(Expressions.Constant("a")).Add(Expressions.Constant("b")), "stringArray")
                .Add(Expressions.Array(), "emptyArray")
                .Add(Expressions.Array().Add(Expressions.Constant(1)), "oneEleArray")
                .Add(Expressions.Array().Add(Expressions.Constant(1)).Add(2).Add(3), "intArray");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var bean = new SupportBean("a", 10);
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("stringArray").UnwrapIntoArray<string>(), new string[] {"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("emptyArray").UnwrapIntoArray<object>(), new object[0]);
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("oneEleArray").UnwrapIntoArray<int>(), new int[] {1});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("intArray").UnwrapIntoArray<int>(), new int[] {1, 2, 3});
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayExpressions_Compile() {
            string stmtText = "select {\"a\",\"b\"} as stringArray, " +
                              "{} as emptyArray, " +
                              "{1} as oneEleArray, " +
                              "{1,2,3} as intArray " +
                              "from " + typeof(SupportBean).Name;
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var bean = new SupportBean("a", 10);
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("stringArray").UnwrapIntoArray<string>(), new string[] {"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("emptyArray").UnwrapIntoArray<object>(), new object[0]);
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("oneEleArray").UnwrapIntoArray<int>(), new int[] {1});
            EPAssertionUtil.AssertEqualsExactOrder(theEvent.Get("intArray").UnwrapIntoArray<int>(), new int[] {1, 2, 3});
    
            stmt.Dispose();
        }
    
        // for testing EPL static method call
        public static string[] DoIt(string[] strings, int?[] ints, object[] objects) {
            _callbackInts = ints;
            _callbackStrings = strings;
            _callbackObjects = objects;
            return new string[] {"a", "b"};
        }
    
        private static void CompareColl<T>(EventBean @event, string property, T[] expected) {
            var col = @event.Get(property).Unwrap<T>(true);
            EPAssertionUtil.AssertEqualsExactOrder(col, expected);
        }
    }
} // end of namespace
