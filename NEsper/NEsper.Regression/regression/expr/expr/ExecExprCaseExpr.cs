///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprCaseExpr : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            base.Configure(configuration);
            configuration.AddImport(typeof(CompatExtensions));
        }

        public override void Run(EPServiceProvider epService) {
            RunAssertionCaseSyntax1Sum(epService);
            RunAssertionCaseSyntax1Sum_OM(epService);
            RunAssertionCaseSyntax1Sum_Compile(epService);
            RunAssertionCaseSyntax1WithElse(epService);
            RunAssertionCaseSyntax1WithElse_OM(epService);
            RunAssertionCaseSyntax1WithElse_Compile(epService);
            RunAssertionCaseSyntax1Branches3(epService);
            RunAssertionCaseSyntax2(epService);
            RunAssertionCaseSyntax2StringsNBranches(epService);
            RunAssertionCaseSyntax2NoElseWithNull(epService);
            RunAssertionCaseSyntax1WithNull(epService);
            RunAssertionCaseSyntax2WithNull_OM(epService);
            RunAssertionCaseSyntax2WithNull_compile(epService);
            RunAssertionCaseSyntax2WithNull(epService);
            RunAssertionCaseSyntax2WithNullBool(epService);
            RunAssertionCaseSyntax2WithCoercion(epService);
            RunAssertionCaseSyntax2WithinExpression(epService);
            RunAssertionCaseSyntax2Sum(epService);
            RunAssertionCaseSyntax2EnumChecks(epService);
            RunAssertionCaseSyntax2EnumResult(epService);
            RunAssertionCaseSyntax2NoAsName(epService);
        }
    
        private void RunAssertionCaseSyntax1Sum(EPServiceProvider epService) {
            // Testing the two forms of the case expression
            // Furthermore the test checks the different when clauses and actions related.
            string caseExpr = "select case " +
                    " when symbol='GE' then volume " +
                    " when symbol='DELL' then sum(price) " +
                    "end as p1 from " + typeof(SupportMarketDataBean).FullName + "#length(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax1Sum(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax1Sum_OM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CaseWhenThen()
                    .Add(Expressions.Eq("symbol", "GE"), Expressions.Property("volume"))
                    .Add(Expressions.Eq("symbol", "DELL"), Expressions.Sum("price")), "p1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("win", "length", Expressions.Constant(10)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string caseExpr = "select case" +
                    " when symbol=\"GE\" then volume" +
                    " when symbol=\"DELL\" then sum(price) " +
                    "end as p1 from " + typeof(SupportMarketDataBean).FullName + ".win:length(10)";
    
            Assert.AreEqual(caseExpr, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax1Sum(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax1Sum_Compile(EPServiceProvider epService) {
            string caseExpr = "select case" +
                    " when symbol=\"GE\" then volume" +
                    " when symbol=\"DELL\" then sum(price) " +
                    "end as p1 from " + typeof(SupportMarketDataBean).FullName + "#length(10)";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(caseExpr);
    
            Assert.AreEqual(caseExpr, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax1Sum(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunCaseSyntax1Sum(EPServiceProvider epService, SupportUpdateListener listener) {
            SendMarketDataEvent(epService, "DELL", 10000, 50);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(50.0, theEvent.Get("p1"));
    
            SendMarketDataEvent(epService, "DELL", 10000, 50);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(100.0, theEvent.Get("p1"));
    
            SendMarketDataEvent(epService, "CSCO", 4000, 5);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("p1"));
    
            SendMarketDataEvent(epService, "GE", 20, 30);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(20.0, theEvent.Get("p1"));
        }
    
        private void RunAssertionCaseSyntax1WithElse(EPServiceProvider epService) {
            // Adding to the EPL statement an else expression
            // when a CSCO ticker is sent the property for the else expression is selected
            string caseExpr = "select case " +
                    " when symbol='DELL' then 3 * volume " +
                    " else volume " +
                    "end as p1 from " + typeof(SupportMarketDataBean).FullName + "#length(3)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax1WithElse(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax1WithElse_OM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CaseWhenThen()
                    .SetElse(Expressions.Property("volume"))
                    .Add(Expressions.Eq("symbol", "DELL"), Expressions.Multiply(Expressions.Property("volume"), Expressions.Constant(3))), "p1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("length", Expressions.Constant(10)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string caseExpr = "select case " +
                    "when symbol=\"DELL\" then volume*3 " +
                    "else volume " +
                    "end as p1 from " + typeof(SupportMarketDataBean).FullName + "#length(10)";
            Assert.AreEqual(caseExpr, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax1WithElse(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax1WithElse_Compile(EPServiceProvider epService) {
            string caseExpr = "select case " +
                    "when symbol=\"DELL\" then volume*3 " +
                    "else volume " +
                    "end as p1 from " + typeof(SupportMarketDataBean).FullName + "#length(10)";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(caseExpr);
            Assert.AreEqual(caseExpr, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax1WithElse(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunCaseSyntax1WithElse(EPServiceProvider epService, SupportUpdateListener listener) {
            SendMarketDataEvent(epService, "CSCO", 4000, 0);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(4000L, theEvent.Get("p1"));
    
            SendMarketDataEvent(epService, "DELL", 20, 0);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(3 * 20L, theEvent.Get("p1"));
        }
    
        private void RunAssertionCaseSyntax1Branches3(EPServiceProvider epService) {
            // Same test but the where clause doesn't match any of the condition of the case expresssion
            string caseExpr = "select case " +
                    " when (symbol='GE') then volume " +
                    " when (symbol='DELL') then volume / 2.0 " +
                    " when (symbol='MSFT') then volume / 3.0 " +
                    " end as p1 from " + typeof(SupportMarketDataBean).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            SendMarketDataEvent(epService, "DELL", 10000, 0);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(10000 / 2.0, theEvent.Get("p1"));
    
            SendMarketDataEvent(epService, "MSFT", 10000, 0);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(10000 / 3.0, theEvent.Get("p1"));
    
            SendMarketDataEvent(epService, "GE", 10000, 0);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(10000.0, theEvent.Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2(EPServiceProvider epService) {
            string caseExpr = "select case IntPrimitive " +
                    " when LongPrimitive then (IntPrimitive + LongPrimitive) " +
                    " when DoublePrimitive then IntPrimitive * DoublePrimitive" +
                    " when FloatPrimitive then FloatPrimitive / DoublePrimitive " +
                    " else (IntPrimitive + LongPrimitive + FloatPrimitive + DoublePrimitive) end as p1 " +
                    " from " + typeof(SupportBean).FullName + "#length(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            // intPrimitive = longPrimitive
            // case result is intPrimitive + longPrimitive
            SendSupportBeanEvent(epService, 2, 2L, 1.0f, 1.0);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(4.0, theEvent.Get("p1"));
            // intPrimitive = doublePrimitive
            // case result is intPrimitive * doublePrimitive
            SendSupportBeanEvent(epService, 5, 1L, 1.0f, 5.0);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(25.0, theEvent.Get("p1"));
            // intPrimitive = floatPrimitive
            // case result is floatPrimitive / doublePrimitive
            SendSupportBeanEvent(epService, 12, 1L, 12.0f, 4.0);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(3.0, theEvent.Get("p1"));
            // all the properties of the event are different
            // The else part is computed: 1+2+3+4 = 10
            SendSupportBeanEvent(epService, 1, 2L, 3.0f, 4.0);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(10.0, theEvent.Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2StringsNBranches(EPServiceProvider epService) {
            // Test of the various coercion user cases.
            string caseExpr = "select case IntPrimitive" +
                    " when 1 then CompatExtensions.RenderAny(BoolPrimitive) " +
                    " when 2 then CompatExtensions.RenderAny(BoolBoxed) " +
                    " when 3 then CompatExtensions.RenderAny(IntPrimitive) " +
                    " when 4 then CompatExtensions.RenderAny(IntBoxed)" +
                    " when 5 then CompatExtensions.RenderAny(LongPrimitive) " +
                    " when 6 then CompatExtensions.RenderAny(LongBoxed) " +
                    " when 7 then CompatExtensions.RenderAny(charPrimitive) " +
                    " when 8 then CompatExtensions.RenderAny(charBoxed) " +
                    " when 9 then CompatExtensions.RenderAny(ShortPrimitive) " +
                    " when 10 then CompatExtensions.RenderAny(ShortBoxed) " +
                    " when 11 then CompatExtensions.RenderAny(bytePrimitive) " +
                    " when 12 then CompatExtensions.RenderAny(byteBoxed) " +
                    " when 13 then CompatExtensions.RenderAny(FloatPrimitive) " +
                    " when 14 then CompatExtensions.RenderAny(FloatBoxed) " +
                    " when 15 then CompatExtensions.RenderAny(DoublePrimitive) " +
                    " when 16 then CompatExtensions.RenderAny(DoubleBoxed) " +
                    " when 17 then TheString " +
                    " else 'x' end as p1 " +
                    " from " + typeof(SupportBean).FullName + "#length(1)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 1, new int?(0), 0L, 0L, '0', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("True", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 2, new int?(0), 0L, 0L, '0', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("False", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 3, new int?(0), 0L, 0L, '0', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("3", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 4, new int?(4), 0L, 0L, '0', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("4", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 5, new int?(0), 5L, 0L, '0', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("5L", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 6, new int?(0), 0L, 6L, '0', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("6L", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 7, new int?(0), 0L, 0L, 'A', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("A", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 8, new int?(0), 0L, 0L, 'A', 'a', (short) 0, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("a", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 9, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 0, (byte) 0, (byte) 0, 0.0f, (float) 0, 0.0, new double?(0.0), null, SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("9", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 10, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("10", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 11, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("11", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 12, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("12", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 13, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("13.0f", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 14, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("14.0f", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 15, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("15.0", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 16, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("16.0", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), 17, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("testCoercion", theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, true, new bool?(false), -1, new int?(0), 0L, 0L, 'A', 'a', (short) 9, (short) 10, (byte) 11, (byte) 12, 13.0f, (float) 14, 15.0, new double?(16.0), "testCoercion", SupportEnum.ENUM_VALUE_1);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual("x", theEvent.Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2NoElseWithNull(EPServiceProvider epService) {
            string caseExpr = "select case TheString " +
                    " when null then true " +
                    " when '' then false end as p1" +
                    " from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, "x");
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
    
            SendSupportBeanEvent(epService, "null");
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
    
            SendSupportBeanEvent(epService, null);
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("p1"));
    
            SendSupportBeanEvent(epService, "");
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax1WithNull(EPServiceProvider epService) {
            string caseExpr = "select case " +
                    " when TheString is null then true " +
                    " when TheString = '' then false end as p1" +
                    " from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, "x");
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
    
            SendSupportBeanEvent(epService, "null");
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
    
            SendSupportBeanEvent(epService, null);
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("p1"));
    
            SendSupportBeanEvent(epService, "");
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2WithNull_OM(EPServiceProvider epService) {
            string caseExpr = "select case IntPrimitive " +
                    "when 1 then null " +
                    "when 2 then 1.0d " +
                    "when 3 then null " +
                    "else 2 " +
                    "end as p1 from " + typeof(SupportBean).FullName + "#length(100)";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.CaseSwitch("IntPrimitive")
                    .SetElse(Expressions.Constant(2))
                    .Add(Expressions.Constant(1), Expressions.Constant(null))
                    .Add(Expressions.Constant(2), Expressions.Constant(1.0))
                    .Add(Expressions.Constant(3), Expressions.Constant(null)), "p1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName)
                .AddView("length", Expressions.Constant(100)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            Assert.AreEqual(caseExpr, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax2WithNull(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2WithNull_compile(EPServiceProvider epService) {
            string caseExpr = "select case IntPrimitive " +
                    "when 1 then null " +
                    "when 2 then 1.0d " +
                    "when 3 then null " +
                    "else 2 " +
                    "end as p1 from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(caseExpr);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(caseExpr, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax2WithNull(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2WithNull(EPServiceProvider epService) {
            string caseExpr = "select case IntPrimitive " +
                    " when 1 then null " +
                    " when 2 then 1.0" +
                    " when 3 then null " +
                    " else 2 " +
                    " end as p1 from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            RunCaseSyntax2WithNull(epService, listener);
    
            stmt.Dispose();
        }
    
        public void RunCaseSyntax2WithNull(EPServiceProvider epService, SupportUpdateListener listener) {
            SendSupportBeanEvent(epService, 4);
            Assert.AreEqual(2.0, listener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(epService, 1);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(epService, 2);
            Assert.AreEqual(1.0, listener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(epService, 3);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
        }
    
        private void RunAssertionCaseSyntax2WithNullBool(EPServiceProvider epService) {
            string caseExpr = "select case BoolBoxed " +
                    " when null then 1 " +
                    " when true then 2l" +
                    " when false then 3 " +
                    " end as p1 from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, null);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(epService, false);
            Assert.AreEqual(3L, listener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(epService, true);
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2WithCoercion(EPServiceProvider epService) {
            string caseExpr = "select case IntPrimitive " +
                    " when 1.0 then null " +
                    " when 4/2.0 then 'x'" +
                    " end as p1 from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, 1);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(epService, 2);
            Assert.AreEqual("x", listener.AssertOneGetNewAndReset().Get("p1"));
            SendSupportBeanEvent(epService, 3);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2WithinExpression(EPServiceProvider epService) {
            string caseExpr = "select 2 * (case " +
                    " IntPrimitive when 1 then 2 " +
                    " when 2 then 3 " +
                    " else 10 end) as p1 " +
                    " from " + typeof(SupportBean).FullName + "#length(1)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, 1);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(4, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 2);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(6, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 3);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(20, theEvent.Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2Sum(EPServiceProvider epService) {
            string caseExpr = "select case IntPrimitive when 1 then sum(LongPrimitive) " +
                    " when 2 then sum(FloatPrimitive) " +
                    " else sum(IntPrimitive) end as p1 " +
                    " from " + typeof(SupportBean).FullName + "#length(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, 1, 10L, 3.0f, 4.0);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(10d, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 1, 15L, 3.0f, 4.0);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(25d, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 2, 1L, 3.0f, 4.0);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(9d, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 2, 1L, 3.0f, 4.0);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(12.0d, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 5, 1L, 1.0f, 1.0);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(11.0d, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 5, 1L, 1.0f, 1.0);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(16d, theEvent.Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2EnumChecks(EPServiceProvider epService) {
            string caseExpr = "select case supportEnum " +
                    " when " + Name.Of<SupportEnumHelper>() + ".GetValueForEnum(0) then 1 " +
                    " when " + Name.Of<SupportEnumHelper>() + ".GetValueForEnum(1) then 2 " +
                    " end as p1 " +
                    " from " + typeof(SupportBeanWithEnum).FullName + "#length(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, "a", SupportEnum.ENUM_VALUE_1);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(1, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, "b", SupportEnum.ENUM_VALUE_2);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(2, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, "c", SupportEnum.ENUM_VALUE_3);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(null, theEvent.Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2EnumResult(EPServiceProvider epService) {
            string caseExpr = "select case IntPrimitive * 2 " +
                    " when 2 then " + Name.Of<SupportEnumHelper>() + ".GetValueForEnum(0) " +
                    " when 4 then " + Name.Of<SupportEnumHelper>() + ".GetValueForEnum(1) " +
                    " else " + Name.Of<SupportEnumHelper>() + ".GetValueForEnum(2) " +
                    " end as p1 " +
                    " from " + typeof(SupportBean).FullName + "#length(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(SupportEnum?), stmt.EventType.GetPropertyType("p1"));
    
            SendSupportBeanEvent(epService, 1);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_1, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 2);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_2, theEvent.Get("p1"));
    
            SendSupportBeanEvent(epService, 3);
            theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_3, theEvent.Get("p1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCaseSyntax2NoAsName(EPServiceProvider epService) {
            string caseSubExpr = "case IntPrimitive when 1 then 0 end";
            string caseExpr = "select " + caseSubExpr +
                    " from " + typeof(SupportBean).FullName + "#length(10)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(caseSubExpr));
    
            SendSupportBeanEvent(epService, 1);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(0, theEvent.Get(caseSubExpr));
    
            stmt.Dispose();
        }
    
        private void SendSupportBeanEvent(
            EPServiceProvider epService,
            bool b,
            bool? boolBoxed,
            int i,
            int? intBoxed,
            long l, 
            long longBoxed,
            char c, 
            char? charBoxed,
            short s,
            short? shortBoxed,
            byte by,
            byte? byteBoxed,
            float f, 
            float? floatBoxed, 
            double d, 
            double? doubleBoxed, 
            string str,
            SupportEnum enumval)
        {
            var theEvent = new SupportBean();
            theEvent.BoolPrimitive = b;
            theEvent.BoolBoxed = boolBoxed;
            theEvent.IntPrimitive = i;
            theEvent.IntBoxed = intBoxed;
            theEvent.LongPrimitive = l;
            theEvent.LongBoxed = longBoxed;
            theEvent.CharPrimitive = c;
            theEvent.CharBoxed = charBoxed;
            theEvent.ShortPrimitive = s;
            theEvent.ShortBoxed = shortBoxed;
            theEvent.BytePrimitive = by;
            theEvent.ByteBoxed = byteBoxed;
            theEvent.FloatPrimitive = f;
            theEvent.FloatBoxed = floatBoxed;
            theEvent.DoublePrimitive = d;
            theEvent.DoubleBoxed = doubleBoxed;
            theEvent.TheString = str;
            theEvent.EnumValue = enumval;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(
            EPServiceProvider epService, 
            int intPrimitive, 
            long longPrimitive,
            float floatPrimitive,
            double doublePrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.LongPrimitive = longPrimitive;
            theEvent.FloatPrimitive = floatPrimitive;
            theEvent.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int intPrimitive) {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, string theString) {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, bool boolBoxed) {
            var theEvent = new SupportBean();
            theEvent.BoolBoxed = boolBoxed;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, string theString, SupportEnum supportEnum) {
            var theEvent = new SupportBeanWithEnum(theString, supportEnum);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendMarketDataEvent(EPServiceProvider epService, string symbol, long volume, double price) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
