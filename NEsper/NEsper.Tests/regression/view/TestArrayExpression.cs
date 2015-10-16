///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestArrayExpression  {
        // for use in testing a static method accepting array parameters
        private static int?[] _callbackInts;
        private static String[] _callbackStrings;
        private static Object[] _callbackObjects;
    
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestArrayExpressions_OM() {
            String stmtText = "select {\"a\",\"b\"} as stringArray, " +
                    "{} as emptyArray, " +
                    "{1} as oneEleArray, " +
                    "{1,2,3} as intArray " +
                    "from " + typeof(SupportBean).FullName;
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.Array().Add(Expressions.Constant("a")).Add(Expressions.Constant("b")), "stringArray")
                .Add(Expressions.Array(), "emptyArray")
                .Add(Expressions.Array().Add(Expressions.Constant(1)), "oneEleArray")
                .Add(Expressions.Array().Add(Expressions.Constant(1)).Add(2).Add(3), "intArray");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBean bean = new SupportBean("a", 10);
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder((String[]) theEvent.Get("stringArray"), new String[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) theEvent.Get("emptyArray"), new Object[0]);
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[]{1, 2, 3});
        }
    
        [Test]
        public void TestArrayExpressions_Compile() {
            String stmtText = "select {\"a\",\"b\"} as stringArray, " +
                    "{} as emptyArray, " +
                    "{1} as oneEleArray, " +
                    "{1,2,3} as intArray " +
                    "from " + typeof(SupportBean).FullName;
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBean bean = new SupportBean("a", 10);
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder((String[]) theEvent.Get("stringArray"), new String[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) theEvent.Get("emptyArray"), new Object[0]);
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[]{1, 2, 3});
        }
    
        [Test]
        public void TestArrayExpressions() {
            String stmtText = "select {'a', 'b'} as stringArray," +
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
                    "{IntPrimitive} as dynIntArr," +
                    "{IntPrimitive, LongPrimitive} as dynLongArr," +
                    "{IntPrimitive, TheString} as dynMixedArr," +
                    "{IntPrimitive, IntPrimitive * 2, IntPrimitive * 3} as dynCalcArr," +
                    "{LongBoxed, DoubleBoxed * 2, TheString || 'a'} as dynCalcArrNulls" +
                    " from " + typeof(SupportBean).FullName;
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBean bean = new SupportBean("a", 10);
            bean.LongPrimitive = 999;
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsExactOrder((String[]) theEvent.Get("stringArray"), new String[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) theEvent.Get("emptyArray"), new Object[0]);
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("oneEleArray"), new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intArray"), new int?[]{1, 2, 3});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("intNullArray"), new int?[]{1, null});
            EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("longArray"), new long?[]{1L, 10L});
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) theEvent.Get("mixedArray"), new Object[]{"a", 1, 1e20});
            EPAssertionUtil.AssertEqualsExactOrder((double?[]) theEvent.Get("doubleArray"), new double?[]{1d, 1.1, 1e20});
            EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("intLongArray"), new long?[]{5L, 6L});
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) theEvent.Get("nullArray"), new Object[]{null});
            EPAssertionUtil.AssertEqualsExactOrder((String[]) theEvent.Get("func"), new String[]{"a", "b"});
            EPAssertionUtil.AssertEqualsExactOrder((bool?[]) theEvent.Get("boolArray"), new bool?[]{true, false});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("dynIntArr"), new int?[]{10});
            EPAssertionUtil.AssertEqualsExactOrder((long?[]) theEvent.Get("dynLongArr"), new long?[]{10L, 999L});
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) theEvent.Get("dynMixedArr"), new Object[]{10, "a"});
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("dynCalcArr"), new int?[]{10, 20, 30});
            EPAssertionUtil.AssertEqualsExactOrder((Object[]) theEvent.Get("dynCalcArrNulls"), new Object[]{null, null, "aa"});
    
            // assert function parameters
            EPAssertionUtil.AssertEqualsExactOrder(_callbackInts, new int?[]{1});
            EPAssertionUtil.AssertEqualsExactOrder(_callbackStrings, new String[]{"a"});
            EPAssertionUtil.AssertEqualsExactOrder(_callbackObjects, new Object[]{1, "d", null, true});
        }
    
        [Test]
        public void TestComplexTypes() {
            String stmtText = "select {ArrayProperty, nested} as field" +
                    " from " + typeof(SupportBeanComplexProps).FullName;
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBeanComplexProps bean = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(bean);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Object[] arr = (Object[]) theEvent.Get("field");
            Assert.AreSame(bean.ArrayProperty, arr[0]);
            Assert.AreSame(bean.Nested, arr[1]);
        }
    
        // for testing EPL static method call
        public static String[] DoIt(String[] strings, int?[] ints, Object[] objects) {
            _callbackInts = ints;
            _callbackStrings = strings;
            _callbackObjects = objects;
            return new String[]{"a", "b"};
        }
    }
}
