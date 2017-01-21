///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestCaseExpr
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddImport(typeof (CompatExtensions));

            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        private void RunCaseSyntax1Sum()
        {
            SendMarketDataEvent("DELL", 10000, 50);
            EventBean theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(50.0, theEvent.Get("p1"));

            SendMarketDataEvent("DELL", 10000, 50);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(100.0, theEvent.Get("p1"));

            SendMarketDataEvent("CSCO", 4000, 5);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("p1"));

            SendMarketDataEvent("GE", 20, 30);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(20.0, theEvent.Get("p1"));
        }

        private void RunCaseSyntax1WithElse()
        {
            SendMarketDataEvent("CSCO", 4000, 0);
            EventBean theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(4000l, theEvent.Get("p1"));

            SendMarketDataEvent("DELL", 20, 0);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(3*20L, theEvent.Get("p1"));
        }

        public void RunCaseSyntax2WithNull()
        {
            SendSupportBeanEvent(4);
            Assert.AreEqual(2.0, _testListener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(1);
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(2);
            Assert.AreEqual(1.0, _testListener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(3);
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));
        }

        private void SendSupportBeanEvent(bool boolPrimitive,
                                          bool? boolBoxed,
                                          int intPrimitive,
                                          int? intBoxed,
                                          long longPrimitive,
                                          long? longBoxed,
                                          char charPrimitive,
                                          char? charBoxed,
                                          short shortPrimitive,
                                          short? shortBoxed,
                                          byte bytePrimitive,
                                          byte? byteBoxed,
                                          float floatPrimitive,
                                          float? floatBoxed,
                                          double doublePrimitive,
                                          double? doubleBoxed,
                                          string str,
                                          SupportEnum @enum)
        {
            var theEvent = new SupportBean
            {
                BoolPrimitive = boolPrimitive,
                BoolBoxed = boolBoxed,
                IntPrimitive = intPrimitive,
                IntBoxed = intBoxed,
                LongPrimitive = longPrimitive,
                LongBoxed = longBoxed,
                CharPrimitive = charPrimitive,
                CharBoxed = charBoxed,
                ShortPrimitive = shortPrimitive,
                ShortBoxed = shortBoxed,
                BytePrimitive = bytePrimitive,
                ByteBoxed = byteBoxed,
                FloatPrimitive = floatPrimitive,
                FloatBoxed = floatBoxed,
                DoublePrimitive = doublePrimitive,
                DoubleBoxed = doubleBoxed,
                TheString = str,
                EnumValue = @enum
            };
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendSupportBeanEvent(int intPrimitive,
                                          long longPrimitive,
                                          float floatPrimitive,
                                          double doublePrimitive)
        {
            var theEvent = new SupportBean
            {
                IntPrimitive = intPrimitive,
                LongPrimitive = longPrimitive,
                FloatPrimitive = floatPrimitive,
                DoublePrimitive = doublePrimitive
            };
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendSupportBeanEvent(int intPrimitive)
        {
            var theEvent = new SupportBean
            {
                IntPrimitive = intPrimitive
            };
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendSupportBeanEvent(String stringValue)
        {
            var theEvent = new SupportBean
            {
                TheString = stringValue
            };
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendSupportBeanEvent(bool boolBoxed)
        {
            var theEvent = new SupportBean
            {
                BoolBoxed = boolBoxed
            };
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendSupportBeanEvent(String stringValue,
                                          SupportEnum supportEnum)
        {
            var theEvent = new SupportBeanWithEnum(stringValue, supportEnum);
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendMarketDataEvent(String symbol,
                                         long volume,
                                         double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            _epService.EPRuntime.SendEvent(bean);
        }

        [Test]
        public void TestCaseSyntax1Branches3()
        {
            // Same test but the where clause doesn't match any of the condition of the case expresssion
            String caseExpr = "select case " +
                              " when (Symbol='GE') then Volume " +
                              " when (Symbol='DELL') then Volume / 2.0 " +
                              " when (Symbol='MSFT') then Volume / 3.0 " +
                              " end as p1 from " + typeof(SupportMarketDataBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            SendMarketDataEvent("DELL", 10000, 0);
            EventBean theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(10000/2.0, theEvent.Get("p1"));

            SendMarketDataEvent("MSFT", 10000, 0);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(10000/3.0, theEvent.Get("p1"));

            SendMarketDataEvent("GE", 10000, 0);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(10000.0, theEvent.Get("p1"));
        }

        [Test]
        public void TestCaseSyntax1Sum()
        {
            // Testing the two forms of the case expression
            // Furthermore the test checks the different when clauses and actions related.
            String caseExpr = "select case " +
                              " when Symbol='GE' then Volume " +
                              " when Symbol='DELL' then sum(Price) " +
                              "end as p1 from " + typeof(SupportMarketDataBean).FullName + ".win:length(10)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax1Sum();
        }

        [Test]
        public void TestCaseSyntax1Sum_Compile()
        {
            String caseExpr = "select case" +
                              " when Symbol=\"GE\" then Volume" +
                              " when Symbol=\"DELL\" then sum(Price) " +
                              "end as p1 from " + typeof(SupportMarketDataBean).FullName + ".win:length(10)";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(caseExpr);

            Assert.AreEqual(caseExpr, model.ToEPL());
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax1Sum();
        }

        [Test]
        public void TestCaseSyntax1Sum_OM()
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CaseWhenThen()
                .Add(Expressions.Eq("Symbol", "GE"),
                    Expressions.Property("Volume"))
                .Add(Expressions.Eq("Symbol", "DELL"),
                    Expressions.Sum("Price")), "p1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("win", "length",
                Expressions.
                    Constant(10)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

            String caseExpr = "select case" +
                              " when Symbol=\"GE\" then Volume" +
                              " when Symbol=\"DELL\" then sum(Price) " +
                              "end as p1 from " + typeof(SupportMarketDataBean).FullName + ".win:length(10)";

            Assert.AreEqual(caseExpr, model.ToEPL());
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax1Sum();
        }

        [Test]
        public void TestCaseSyntax1WithElse()
        {
            // Adding to the EPL statement an else expression
            // when a CSCO ticker is sent the property for the else expression is selected
            String caseExpr = "select case " +
                              " when Symbol='DELL' then 3 * Volume " +
                              " else Volume " +
                              "end as p1 from " + typeof(SupportMarketDataBean).FullName + ".win:length(3)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax1WithElse();
        }

        [Test]
        public void TestCaseSyntax1WithElse_Compile()
        {
            String caseExpr = "select case " +
                              "when Symbol=\"DELL\" then Volume*3 " +
                              "else Volume " +
                              "end as p1 from " + typeof(SupportMarketDataBean).FullName + ".win:length(10)";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(caseExpr);
            Assert.AreEqual(caseExpr, model.ToEPL());

            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax1WithElse();
        }

        [Test]
        public void TestCaseSyntax1WithElse_OM()
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CaseWhenThen()
                .SetElse(Expressions.Property("Volume"))
                .Add(Expressions.Eq("Symbol", "DELL"),
                    Expressions.Multiply(
                        Expressions.Property("Volume"),
                        Expressions.Constant(3))), "p1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("win", "length",
                Expressions.
                    Constant(10)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

            String caseExpr = "select case " +
                              "when Symbol=\"DELL\" then Volume*3 " +
                              "else Volume " +
                              "end as p1 from " + typeof(SupportMarketDataBean).FullName + ".win:length(10)";
            Assert.AreEqual(caseExpr, model.ToEPL());

            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax1WithElse();
        }

        [Test]
        public void TestCaseSyntax1WithNull()
        {
            String caseExpr = "select case " +
                              " when TheString is null then true " +
                              " when TheString = '' then false end as p1" +
                              " from " + typeof(SupportBean).FullName + ".win:length(100)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent("x");
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));

            SendSupportBeanEvent("null");
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));

            SendSupportBeanEvent(null);
            Assert.AreEqual(true, _testListener.AssertOneGetNewAndReset().Get("p1"));

            SendSupportBeanEvent("");
            Assert.AreEqual(false, _testListener.AssertOneGetNewAndReset().Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2()
        {
            String caseExpr = "select case IntPrimitive " +
                              " when LongPrimitive then (IntPrimitive + LongPrimitive) " +
                              " when DoublePrimitive then IntPrimitive * DoublePrimitive" +
                              " when FloatPrimitive then FloatPrimitive / DoublePrimitive " +
                              " else (IntPrimitive + LongPrimitive + FloatPrimitive + DoublePrimitive) end as p1 " +
                              " from " + typeof(SupportBean).FullName + ".win:length(10)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            // intPrimitive = longPrimitive
            // case result is intPrimitive + longPrimitive
            SendSupportBeanEvent(2, 2L, 1.0f, 1.0);
            EventBean theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(4.0, theEvent.Get("p1"));
            // intPrimitive = doublePrimitive
            // case result is intPrimitive * doublePrimitive
            SendSupportBeanEvent(5, 1L, 1.0f, 5.0);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(25.0, theEvent.Get("p1"));
            // intPrimitive = floatPrimitive
            // case result is floatPrimitive / doublePrimitive
            SendSupportBeanEvent(12, 1L, 12.0f, 4.0);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(3.0, theEvent.Get("p1"));
            // all the properties of the event are different
            // The else part is computed: 1+2+3+4 = 10
            SendSupportBeanEvent(1, 2L, 3.0f, 4.0);
            theEvent = _testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(10.0, theEvent.Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2EnumChecks()
        {
            String caseExpr = "select case supportEnum " +
                              " when com.espertech.esper.support.bean.SupportEnumHelper.GetValueForEnum(0) then 1 " +
                              " when com.espertech.esper.support.bean.SupportEnumHelper.GetValueForEnum(1) then 2 " +
                              " end as p1 " +
                              " from " + typeof(SupportBeanWithEnum).FullName + ".win:length(10)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(int?), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent("a", SupportEnum.ENUM_VALUE_1);
            EventBean theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(1, theEvent.Get("p1"));

            SendSupportBeanEvent("b", SupportEnum.ENUM_VALUE_2);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(2, theEvent.Get("p1"));

            SendSupportBeanEvent("c", SupportEnum.ENUM_VALUE_3);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(null, theEvent.Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2EnumResult()
        {
            String caseExpr = "select case IntPrimitive * 2 " +
                              " when 2 then com.espertech.esper.support.bean.SupportEnumHelper.GetValueForEnum(0) " +
                              " when 4 then com.espertech.esper.support.bean.SupportEnumHelper.GetValueForEnum(1) " +
                              " else com.espertech.esper.support.bean.SupportEnumHelper.GetValueForEnum(2) " +
                              " end as p1 " +
                              " from " + typeof(SupportBean).FullName + ".win:length(10)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(SupportEnum?), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent(1);
            EventBean theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_1, theEvent.Get("p1"));

            SendSupportBeanEvent(2);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_2, theEvent.Get("p1"));

            SendSupportBeanEvent(3);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_3, theEvent.Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2NoAsName()
        {
            String caseSubExpr = "case IntPrimitive when 1 then 0 end";
            String caseExpr = "select " + caseSubExpr +
                              " from " + typeof(SupportBean).FullName + ".win:length(10)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(int?), selectTestCase.EventType.GetPropertyType(caseSubExpr));

            SendSupportBeanEvent(1);
            EventBean theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(0, theEvent.Get(caseSubExpr));
        }

        [Test]
        public void TestCaseSyntax2NoElseWithNull()
        {
            String caseExpr = "select case TheString " +
                              " when null then true " +
                              " when '' then false end as p1" +
                              " from " + typeof(SupportBean).FullName + ".win:length(100)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent("x");
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));

            SendSupportBeanEvent("null");
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));

            SendSupportBeanEvent(null);
            Assert.AreEqual(true, _testListener.AssertOneGetNewAndReset().Get("p1"));

            SendSupportBeanEvent("");
            Assert.AreEqual(false, _testListener.AssertOneGetNewAndReset().Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2StringsNBranches()
        {
            // Test of the various coercion user cases.
            String caseExpr = "select case IntPrimitive" +
                              " when 1 then CompatExtensions.Render(BoolPrimitive) " +
                              " when 2 then CompatExtensions.Render(BoolBoxed) " +
                              " when 3 then CompatExtensions.Render(IntPrimitive) " +
                              " when 4 then CompatExtensions.Render(IntBoxed)" +
                              " when 5 then CompatExtensions.Render(LongPrimitive) " +
                              " when 6 then CompatExtensions.Render(LongBoxed) " +
                              " when 7 then CompatExtensions.Render(CharPrimitive) " +
                              " when 8 then CompatExtensions.Render(CharBoxed) " +
                              " when 9 then CompatExtensions.Render(ShortPrimitive) " +
                              " when 10 then CompatExtensions.Render(ShortBoxed) " +
                              " when 11 then CompatExtensions.Render(BytePrimitive) " +
                              " when 12 then CompatExtensions.Render(ByteBoxed) " +
                              " when 13 then CompatExtensions.Render(FloatPrimitive) " +
                              " when 14 then CompatExtensions.Render(FloatBoxed) " +
                              " when 15 then CompatExtensions.Render(DoublePrimitive) " +
                              " when 16 then CompatExtensions.Render(DoubleBoxed) " +
                              " when 17 then TheString " +
                              " else 'x' end as p1 " +
                              " from " + typeof(SupportBean).FullName + ".win:length(1)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent(true, false, 1, 0, 0L, 0L, '0', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            EventBean theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("True", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 2, 0, 0L, 0L, '0', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("False", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 3, 0, 0L, 0L, '0', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("3", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 4, 4, 0L, 0L, '0', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("4", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 5, 0, 5L, 0L, '0', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("5", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 6, 0, 0L, 6L, '0', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("6", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 7, 0, 0L, 0L, 'A', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("A", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 8, 0, 0L, 0L, 'A', 'a', 0, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("a", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 9, 0, 0L, 0L, 'A', 'a', 9, 0, 0, (0), 0.0f, 0f, 0.0, 0.0, null, SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("9", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 10, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("10", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 11, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("11", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 12, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("12", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 13, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("13.0", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 14, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("14.0", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 15, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("15.0", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 16, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("16.0", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, 17, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("testCoercion", theEvent.Get("p1"));

            SendSupportBeanEvent(true, false, -1, 0, 0L, 0L, 'A', 'a', 9, 10, 11, (12), 13.0f, 14f, 15.0, 16.0, "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("x", theEvent.Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2Sum()
        {
            String caseExpr = "select case IntPrimitive when 1 then sum(LongPrimitive) " +
                              " when 2 then sum(FloatPrimitive) " +
                              " else sum(IntPrimitive) end as p1 " +
                              " from " + typeof(SupportBean).FullName + ".win:length(10)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent(1, 10L, 3.0f, 4.0);
            EventBean theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(10d, theEvent.Get("p1"));

            SendSupportBeanEvent(1, 15L, 3.0f, 4.0);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(25d, theEvent.Get("p1"));

            SendSupportBeanEvent(2, 1L, 3.0f, 4.0);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(9d, theEvent.Get("p1"));

            SendSupportBeanEvent(2, 1L, 3.0f, 4.0);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(12.0d, theEvent.Get("p1"));

            SendSupportBeanEvent(5, 1L, 1.0f, 1.0);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(11.0d, theEvent.Get("p1"));

            SendSupportBeanEvent(5, 1L, 1.0f, 1.0);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(16d, theEvent.Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2WithCoercion()
        {
            String caseExpr = "select case IntPrimitive " +
                              " when 1.0 then null " +
                              " when 4/2.0 then 'x'" +
                              " end as p1 from " + typeof(SupportBean).FullName + ".win:length(100)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent(1);
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(2);
            Assert.AreEqual("x", _testListener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(3);
            Assert.AreEqual(null, _testListener.AssertOneGetNewAndReset().Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2WithNull()
        {
            String caseExpr = "select case IntPrimitive " +
                              " when 1 then null " +
                              " when 2 then 1.0" +
                              " when 3 then null " +
                              " else 2 " +
                              " end as p1 from " + typeof(SupportBean).FullName + ".win:length(100)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax2WithNull();
        }

        [Test]
        public void TestCaseSyntax2WithNullBool()
        {
            String caseExpr = "select case BoolBoxed " +
                              " when null then 1 " +
                              " when true then 2l" +
                              " when false then 3 " +
                              " end as p1 from " + typeof(SupportBean).FullName + ".win:length(100)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent(null);
            Assert.AreEqual(1L, _testListener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(false);
            Assert.AreEqual(3L, _testListener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(true);
            Assert.AreEqual(2L, _testListener.AssertOneGetNewAndReset().Get("p1"));
        }

        [Test]
        public void TestCaseSyntax2WithNull_OM()
        {
            String caseExpr = "select case IntPrimitive " +
                              "when 1 then null " +
                              "when 2 then 1.0d " +
                              "when 3 then null " +
                              "else 2 " +
                              "end as p1 from " + typeof(SupportBean).FullName + ".win:length(100)";

            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CaseSwitch("IntPrimitive")
                .SetElse(Expressions.Constant(2))
                .Add(Expressions.Constant(1), Expressions.Constant(null))
                .Add(Expressions.Constant(2), Expressions.Constant(1.0))
                .Add(Expressions.Constant(3), Expressions.Constant(null)),
                "p1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView("win", "length",
                Expressions.Constant(100)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

            Assert.AreEqual(caseExpr, model.ToEPL());
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax2WithNull();
        }

        [Test]
        public void TestCaseSyntax2WithNull_compile()
        {
            String caseExpr = "select case IntPrimitive " +
                              "when 1 then null " +
                              "when 2 then 1.0d " +
                              "when 3 then null " +
                              "else 2 " +
                              "end as p1 from " + typeof(SupportBean).FullName + ".win:length(100)";

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(caseExpr);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(caseExpr, model.ToEPL());

            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("p1"));

            RunCaseSyntax2WithNull();
        }

        [Test]
        public void TestCaseSyntax2WithinExpression()
        {
            String caseExpr = "select 2 * (case " +
                              " IntPrimitive when 1 then 2 " +
                              " when 2 then 3 " +
                              " else 10 end) as p1 " +
                              " from " + typeof(SupportBean).FullName + ".win:length(1)";

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _testListener.Update;
            Assert.AreEqual(typeof(int?), selectTestCase.EventType.GetPropertyType("p1"));

            SendSupportBeanEvent(1);
            EventBean theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(4, theEvent.Get("p1"));

            SendSupportBeanEvent(2);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(6, theEvent.Get("p1"));

            SendSupportBeanEvent(3);
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(20, theEvent.Get("p1"));
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
