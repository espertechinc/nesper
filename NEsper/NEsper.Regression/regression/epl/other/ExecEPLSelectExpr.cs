///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    using DescriptionAttribute = com.espertech.esper.client.annotation.DescriptionAttribute;

    public class ExecEPLSelectExpr : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPrecedenceNoColumnName(epService);
            RunAssertionGraphSelect(epService);
            RunAssertionKeywordsAllowed(epService);
            RunAssertionEscapeString(epService);
            RunAssertionGetEventType(epService);
            RunAssertionWindowStats(epService);
        }
    
        private void RunAssertionPrecedenceNoColumnName(EPServiceProvider epService) {
            TryPrecedenceNoColumnName(epService, "3*2+1", "3*2+1", 7);
            TryPrecedenceNoColumnName(epService, "(3*2)+1", "3*2+1", 7);
            TryPrecedenceNoColumnName(epService, "3*(2+1)", "3*(2+1)", 9);
        }
    
        private void TryPrecedenceNoColumnName(EPServiceProvider epService, string selectColumn, string expectedColumn, Object value) {
            string epl = "select " + selectColumn + " from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
            if (!stmt.EventType.PropertyNames[0].Equals(expectedColumn)) {
                Assert.Fail("Expected '" + expectedColumn + "' but was " + stmt.EventType.PropertyNames[0]);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean @event = testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(value, @event.Get(expectedColumn));
            stmt.Dispose();
        }
    
        private void RunAssertionGraphSelect(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("insert into MyStream select nested from " + typeof(SupportBeanComplexProps).FullName);
    
            string epl = "select nested.nestedValue, nested.nestedNested.nestedNestedValue from MyStream";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            Assert.IsNotNull(testListener.AssertOneGetNewAndReset());
    
            stmt.Dispose();
        }
    
        private void RunAssertionKeywordsAllowed(EPServiceProvider epService) {
            string fields = "count,escape,every,sum,avg,max,min,coalesce,median,stddev,avedev,events,first,last,unidirectional,pattern,sql,metadatasql,prev,prior,weekday,lastweekday,cast,snapshot,variable,window,left,right,full,outer,join";
            epService.EPAdministrator.Configuration.AddEventType("Keywords", typeof(SupportBeanKeywords));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select " + fields + " from Keywords");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
            epService.EPRuntime.SendEvent(new SupportBeanKeywords());
            EPAssertionUtil.AssertEqualsExactOrder(stmt.EventType.PropertyNames, fields.Split(','));
    
            EventBean theEvent = testListener.AssertOneGetNewAndReset();
    
            string[] fieldsArr = fields.Split(',');
            foreach (string aFieldsArr in fieldsArr) {
                Assert.AreEqual(1, theEvent.Get(aFieldsArr));
            }
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select escape as stddev, count(*) as count, last from Keywords");
            stmt.Events += testListener.Update;
            epService.EPRuntime.SendEvent(new SupportBeanKeywords());
    
            theEvent = testListener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, theEvent.Get("stddev"));
            Assert.AreEqual(1L, theEvent.Get("count"));
            Assert.AreEqual(1, theEvent.Get("last"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionEscapeString(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // The following EPL syntax compiles but fails to match a string "A'B", we are looking into:
            // EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean(string='A\\\'B')");
    
            TryEscapeMatch(epService, "A'B", "\"A'B\"");       // opposite quotes
            TryEscapeMatch(epService, "A'B", "'A\\'B'");      // escape '
            TryEscapeMatch(epService, "A'B", "'A\\u0027B'");   // unicode
    
            TryEscapeMatch(epService, "A\"B", "'A\"B'");       // opposite quotes
            TryEscapeMatch(epService, "A\"B", "'A\\\"B'");      // escape "
            TryEscapeMatch(epService, "A\"B", "'A\\u0022B'");   // unicode
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('A\\\'B') @Description(\"A\\\"B\") select * from SupportBean");
            Assert.AreEqual("A\'B", stmt.Name);
            var desc = (DescriptionAttribute) stmt.Annotations.Skip(1).First();
            Assert.AreEqual("A\"B", desc.Value);
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select 'volume' as field1, \"sleep\" as field2, \"\\u0041\" as unicodeA from SupportBean");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(testListener.AssertOneGetNewAndReset(), new string[]{"field1", "field2", "unicodeA"}, new object[]{"volume", "sleep", "A"});
            stmt.Dispose();
    
            TryStatementMatch(epService, "John's", "select * from SupportBean(TheString='John\\'s')");
            TryStatementMatch(epService, "John's", "select * from SupportBean(TheString='John\\u0027s')");
            TryStatementMatch(epService, "Quote \"Hello\"", "select * from SupportBean(TheString like \"Quote \\\"Hello\\\"\")");
            TryStatementMatch(epService, "Quote \"Hello\"", "select * from SupportBean(TheString like \"Quote \\u0022Hello\\u0022\")");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryEscapeMatch(EPServiceProvider epService, string property, string escaped) {
            string epl = "select * from SupportBean(TheString=" + escaped + ")";
            string text = "trying >" + escaped + "< (" + escaped.Length + " chars) EPL " + epl;
            Log.Info("tryEscapeMatch for " + text);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
            epService.EPRuntime.SendEvent(new SupportBean(property, 1));
            Assert.AreEqual(testListener.AssertOneGetNewAndReset().Get("IntPrimitive"), 1);
            stmt.Dispose();
        }
    
        private void TryStatementMatch(EPServiceProvider epService, string property, string epl) {
            string text = "trying EPL " + epl;
            Log.Info("tryEscapeMatch for " + text);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
            epService.EPRuntime.SendEvent(new SupportBean(property, 1));
            Assert.AreEqual(testListener.AssertOneGetNewAndReset().Get("IntPrimitive"), 1);
            stmt.Dispose();
        }
    
        private void RunAssertionGetEventType(EPServiceProvider epService) {
            string epl = "select TheString, BoolBoxed aBool, 3*IntPrimitive, FloatBoxed+FloatPrimitive result" +
                    " from " + typeof(SupportBean).FullName + "#length(3) " +
                    " where BoolBoxed = true";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            EventType type = stmt.EventType;
            Log.Debug(".testGetEventType properties=" + CompatExtensions.Render(type.PropertyNames));
            EPAssertionUtil.AssertEqualsAnyOrder(type.PropertyNames, new string[]{"3*IntPrimitive", "TheString", "result", "aBool"});
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(bool?), type.GetPropertyType("aBool"));
            Assert.AreEqual(typeof(float?), type.GetPropertyType("result"));
            Assert.AreEqual(typeof(int?), type.GetPropertyType("3*IntPrimitive"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWindowStats(EPServiceProvider epService) {
            string epl = "select TheString, BoolBoxed as aBool, 3*IntPrimitive, FloatBoxed+FloatPrimitive as result" +
                    " from " + typeof(SupportBean).FullName + "#length(3) " +
                    " where BoolBoxed = true";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            SendEvent(epService, "a", false, 0, 0, 0);
            SendEvent(epService, "b", false, 0, 0, 0);
            Assert.IsTrue(testListener.LastNewData == null);
            SendEvent(epService, "c", true, 3, 10, 20);
    
            EventBean received = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("c", received.Get("TheString"));
            Assert.AreEqual(true, received.Get("aBool"));
            Assert.AreEqual(30f, received.Get("result"));
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string s, bool b, int i, float f1, float f2) {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.BoolBoxed = b;
            bean.IntPrimitive = i;
            bean.FloatPrimitive = f1;
            bean.FloatBoxed = f2;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
