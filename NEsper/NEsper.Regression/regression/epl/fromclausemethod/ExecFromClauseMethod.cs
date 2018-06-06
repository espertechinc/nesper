///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using static com.espertech.esper.util.TypeHelper;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.fromclausemethod
{
    public class ExecFromClauseMethod : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionUDFAndScriptReturningEvents(epService);
            RunAssertionEventBeanArray(epService);
            RunAssertionOverloaded(epService);
            RunAssertion2StreamMaxAggregation(epService);
            RunAssertion2JoinHistoricalSubordinateOuterMultiField(epService);
            RunAssertion2JoinHistoricalSubordinateOuter(epService);
            RunAssertion2JoinHistoricalIndependentOuter(epService);
            RunAssertion2JoinHistoricalOnlyDependent(epService);
            RunAssertion2JoinHistoricalOnlyIndependent(epService);
            RunAssertionNoJoinIterateVariables(epService);
            RunAssertionDifferentReturnTypes(epService);
            RunAssertionArrayNoArg(epService);
            RunAssertionArrayWithArg(epService);
            RunAssertionObjectNoArg(epService);
            RunAssertionObjectWithArg(epService);
            RunAssertionInvocationTargetEx(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionUDFAndScriptReturningEvents(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema ItemEvent(id string)");
    
            var entry = new ConfigurationPlugInSingleRowFunction();
            entry.Name = "myItemProducerUDF";
            entry.FunctionClassName = GetType().FullName;
            entry.FunctionMethodName = "MyItemProducerUDF";
            entry.EventTypeName = "ItemEvent";
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(entry);

            string script = "create expression EventBean[] @Type(ItemEvent) jscript:myItemProducerScript() [\n" +
                            "  function myItemProducerScript() {" +
                            "    var eventBean = host.resolveType('com.espertech.esper.client.EventBean');\n" +
                            "    var events = host.newArr(eventBean, 2);\n" +
                            "    events[0] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"id\", \"id1\"), \"ItemEvent\");\n" +
                            "    events[1] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"id\", \"id3\"), \"ItemEvent\");\n" +
                            "    return events;\n" +
                            "  }" +
                            "  return myItemProducerScript();" +
                            "]";
            epService.EPAdministrator.CreateEPL(script);
    
            TryAssertionUDFAndScriptReturningEvents(epService, "MyItemProducerUDF");
            TryAssertionUDFAndScriptReturningEvents(epService, "myItemProducerScript");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventBeanArray(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema MyItemEvent(p0 string)");
    
            TryAssertionEventBeanArray(epService, "EventBeanArrayForString", false);
            TryAssertionEventBeanArray(epService, "EventBeanArrayForString", true);
            TryAssertionEventBeanArray(epService, "EventBeanCollectionForString", false);
            TryAssertionEventBeanArray(epService, "EventBeanIteratorForString", false);
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".FetchResult12(0) @Type(ItemEvent)",
                    "Error starting statement: The @type annotation is only allowed when the invocation target returns EventBean instances");
        }
    
        private void RunAssertionOverloaded(EPServiceProvider epService) {
            TryAssertionOverloaded(epService, "", "A", "B");
            TryAssertionOverloaded(epService, "10", "10", "B");
            TryAssertionOverloaded(epService, "10, 20", "10", "20");
            TryAssertionOverloaded(epService, "'x'", "x", "B");
            TryAssertionOverloaded(epService, "'x', 50", "x", "50");
        }
    
        private void TryAssertionOverloaded(EPServiceProvider epService, string @params, string expectedFirst, string expectedSecond) {
            string epl = "select col1, col2 from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".OverloadedMethodForJoin(" + @params + ")";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "col1,col2".Split(','), new object[]{expectedFirst, expectedSecond});
            stmt.Dispose();
        }
    
        private void RunAssertion2StreamMaxAggregation(EPServiceProvider epService) {
            string className = typeof(SupportStaticMethodLib).FullName;
            string stmtText;
    
            // ESPER 556
            stmtText = "select max(col1) as maxcol1 from SupportBean#unique(TheString), method:" + className + ".FetchResult100() ";
    
            string[] fields = "maxcol1".Split(',');
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {9}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {9}});
    
            stmt.Dispose();
        }
    
        private void RunAssertion2JoinHistoricalSubordinateOuterMultiField(EPServiceProvider epService) {
            string className = typeof(SupportStaticMethodLib).FullName;
            string stmtText;
    
            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
            stmtText = "select IntPrimitive,IntBoxed,col1,col2 from SupportBean#keepall " +
                    "left outer join " +
                    "method:" + className + ".FetchResult100() " +
                    "on IntPrimitive = col1 and IntBoxed = col2";
    
            string[] fields = "IntPrimitive,IntBoxed,col1,col2".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportBeanEvent(epService, 2, 4);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {2, 4, 2, 4}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {2, 4, 2, 4}});
    
            stmt.Dispose();
        }
    
        private void RunAssertion2JoinHistoricalSubordinateOuter(EPServiceProvider epService) {
            string className = typeof(SupportStaticMethodLib).FullName;
            string stmtText;
    
            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".FetchResult12(0) as s0 " +
                    "left outer join " +
                    "method:" + className + ".FetchResult23(s0.value) as s1 on s0.value = s1.value";
            AssertJoinHistoricalSubordinateOuter(epService, stmtText);
    
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                    "method:" + className + ".FetchResult23(s0.value) as s1 " +
                    "right outer join " +
                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
            AssertJoinHistoricalSubordinateOuter(epService, stmtText);
    
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                    "method:" + className + ".FetchResult23(s0.value) as s1 " +
                    "full outer join " +
                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
            AssertJoinHistoricalSubordinateOuter(epService, stmtText);
    
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                    "method:" + className + ".FetchResult12(0) as s0 " +
                    "full outer join " +
                    "method:" + className + ".FetchResult23(s0.value) as s1 on s0.value = s1.value";
            AssertJoinHistoricalSubordinateOuter(epService, stmtText);
        }
    
        private void RunAssertion2JoinHistoricalIndependentOuter(EPServiceProvider epService) {
            string[] fields = "valueOne,valueTwo".Split(',');
            string className = typeof(SupportStaticMethodLib).FullName;
            string stmtText;
    
            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".FetchResult12(0) as s0 " +
                    "left outer join " +
                    "method:" + className + ".FetchResult23(0) as s1 on s0.value = s1.value";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, null}, new object[] {2, 2}});
            stmt.Dispose();
    
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                    "method:" + className + ".FetchResult23(0) as s1 " +
                    "right outer join " +
                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, null}, new object[] {2, 2}});
            stmt.Dispose();
    
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                    "method:" + className + ".FetchResult23(0) as s1 " +
                    "full outer join " +
                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, null}, new object[] {2, 2}, new object[] {null, 3}});
            stmt.Dispose();
    
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                    "method:" + className + ".FetchResult12(0) as s0 " +
                    "full outer join " +
                    "method:" + className + ".FetchResult23(0) as s1 on s0.value = s1.value";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, null}, new object[] {2, 2}, new object[] {null, 3}});
            stmt.Dispose();
        }
    
        private void AssertJoinHistoricalSubordinateOuter(EPServiceProvider epService, string expression) {
            string[] fields = "valueOne,valueTwo".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, null}, new object[] {2, 2}});
            stmt.Dispose();
        }
    
        private void RunAssertion2JoinHistoricalOnlyDependent(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create variable int lower");
            epService.EPAdministrator.CreateEPL("create variable int upper");
            EPStatement setStmt = epService.EPAdministrator.CreateEPL("on SupportBean set lower=IntPrimitive,upper=IntBoxed");
            Assert.AreEqual(StatementType.ON_SET, ((EPStatementSPI) setStmt).StatementMetadata.StatementType);
    
            string className = typeof(SupportStaticMethodLib).FullName;
            string stmtText;
    
            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
            stmtText = "select value,result from method:" + className + ".FetchBetween(lower, upper), " +
                    "method:" + className + ".FetchIdDelimited(value)";
            AssertJoinHistoricalOnlyDependent(epService, stmtText);
    
            stmtText = "select value,result from " +
                    "method:" + className + ".FetchIdDelimited(value), " +
                    "method:" + className + ".FetchBetween(lower, upper)";
            AssertJoinHistoricalOnlyDependent(epService, stmtText);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertion2JoinHistoricalOnlyIndependent(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create variable int lower");
            epService.EPAdministrator.CreateEPL("create variable int upper");
            epService.EPAdministrator.CreateEPL("on SupportBean set lower=IntPrimitive,upper=IntBoxed");
    
            string className = typeof(SupportStaticMethodLib).FullName;
            string stmtText;
    
            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".FetchBetween(lower, upper) as s0, " +
                    "method:" + className + ".FetchBetweenString(lower, upper) as s1";
            AssertJoinHistoricalOnlyIndependent(epService, stmtText);
    
            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                    "method:" + className + ".FetchBetweenString(lower, upper) as s1, " +
                    "method:" + className + ".FetchBetween(lower, upper) as s0 ";
            AssertJoinHistoricalOnlyIndependent(epService, stmtText);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertJoinHistoricalOnlyIndependent(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "valueOne,valueTwo".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, 5, 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {5, "5"}});
    
            SendSupportBeanEvent(epService, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, "1"}, new object[] {1, "2"}, new object[] {2, "1"}, new object[] {2, "2"}});
    
            SendSupportBeanEvent(epService, 0, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            stmt.Dispose();
            SendSupportBeanEvent(epService, 0, -1);
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void AssertJoinHistoricalOnlyDependent(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "value,result".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, 5, 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {5, "|5|"}});
    
            SendSupportBeanEvent(epService, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, "|1|"}, new object[] {2, "|2|"}});
    
            SendSupportBeanEvent(epService, 0, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, 4, 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {4, "|4|"}, new object[] {5, "|5|"}, new object[] {6, "|6|"}});
    
            stmt.Dispose();
            SendSupportBeanEvent(epService, 0, -1);
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void RunAssertionNoJoinIterateVariables(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create variable int lower");
            epService.EPAdministrator.CreateEPL("create variable int upper");
            epService.EPAdministrator.CreateEPL("on SupportBean set lower=IntPrimitive,upper=IntBoxed");
    
            // Test int and singlerow
            string className = typeof(SupportStaticMethodLib).FullName;
            string stmtText = "select value from method:" + className + ".FetchBetween(lower, upper)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, null);
    
            SendSupportBeanEvent(epService, 5, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, new object[][]{new object[] {5}, new object[] {6}, new object[] {7}, new object[] {8}, new object[] {9}, new object[] {10}});
    
            SendSupportBeanEvent(epService, 10, 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, null);
    
            SendSupportBeanEvent(epService, 4, 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, new object[][]{new object[] {4}});
    
            Assert.IsFalse(listener.IsInvoked);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionReturnTypeMultipleRow(EPServiceProvider epService, string method) {
            string epl = "select TheString, IntPrimitive, mapstring, mapint from " +
                    typeof(SupportBean).FullName + "#keepall as s1, " +
                    "method:" + typeof(SupportStaticMethodLib).FullName + "." + method;
            string[] fields = "TheString,IntPrimitive,mapstring,mapint".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendBeanEvent(epService, "E1", 0);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendBeanEvent(epService, "E2", -1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendBeanEvent(epService, "E3", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 1, "|E3_0|", 100});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3", 1, "|E3_0|", 100}});
    
            SendBeanEvent(epService, "E4", 2);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"E4", 2, "|E4_0|", 100}, new object[] {"E4", 2, "|E4_1|", 101}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3", 1, "|E3_0|", 100}, new object[] {"E4", 2, "|E4_0|", 100}, new object[] {"E4", 2, "|E4_1|", 101}});
    
            SendBeanEvent(epService, "E5", 3);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"E5", 3, "|E5_0|", 100}, new object[] {"E5", 3, "|E5_1|", 101}, new object[] {"E5", 3, "|E5_2|", 102}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3", 1, "|E3_0|", 100},
                new object[] {"E4", 2, "|E4_0|", 100},
                new object[] {"E4", 2, "|E4_1|", 101},
                new object[] {"E5", 3, "|E5_0|", 100},
                new object[] {"E5", 3, "|E5_1|", 101},
                new object[] {"E5", 3, "|E5_2|", 102}});
    
            listener.Reset();
            stmt.Dispose();
        }
    
        private void RunAssertionDifferentReturnTypes(EPServiceProvider epService) {
            TryAssertionSingleRowFetch(epService, "FetchMap(TheString, IntPrimitive)");
            TryAssertionSingleRowFetch(epService, "FetchMapEventBean(s1, 'TheString', 'IntPrimitive')");
            TryAssertionSingleRowFetch(epService, "FetchObjectArrayEventBean(TheString, IntPrimitive)");
            TryAssertionSingleRowFetch(epService, "FetchPonoArray(TheString, IntPrimitive)");
            TryAssertionSingleRowFetch(epService, "FetchPonoCollection(TheString, IntPrimitive)");
            TryAssertionSingleRowFetch(epService, "FetchPonoIterator(TheString, IntPrimitive)");
    
            TryAssertionReturnTypeMultipleRow(epService, "FetchMapArrayMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchOAArrayMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchPonoArrayMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchMapCollectionMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchOACollectionMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchPonoCollectionMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchMapIteratorMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchOAIteratorMR(TheString, IntPrimitive)");
            TryAssertionReturnTypeMultipleRow(epService, "FetchPonoIteratorMR(TheString, IntPrimitive)");
        }
    
        private void TryAssertionSingleRowFetch(EPServiceProvider epService, string method) {
            string epl = "select TheString, IntPrimitive, mapstring, mapint from " +
                    typeof(SupportBean).FullName + " as s1, " +
                    "method:" + typeof(SupportStaticMethodLib).FullName + "." + method;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new string[]{"TheString", "IntPrimitive", "mapstring", "mapint"};
    
            SendBeanEvent(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1, "|E1|", 2});
    
            SendBeanEvent(epService, "E2", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 3, "|E2|", 4});
    
            SendBeanEvent(epService, "E3", 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 0, null, null});
    
            SendBeanEvent(epService, "E4", -1);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayNoArg(EPServiceProvider epService) {
            string joinStatement = "select id, TheString from " +
                    typeof(SupportBean).FullName + "#length(3) as s1, " +
                    "method:" + typeof(SupportStaticMethodLib).FullName + ".FetchArrayNoArg";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            TryArrayNoArg(epService, stmt);
    
            joinStatement = "select id, TheString from " +
                    typeof(SupportBean).FullName + "#length(3) as s1, " +
                    "method:" + typeof(SupportStaticMethodLib).FullName + ".FetchArrayNoArg()";
            stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            TryArrayNoArg(epService, stmt);
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(joinStatement);
            Assert.AreEqual(joinStatement, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            TryArrayNoArg(epService, stmt);
    
            model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("id", "TheString");
            model.FromClause = FromClause.Create()
                    .Add(FilterStream.Create(typeof(SupportBean).FullName, "s1").AddView("length", Expressions.Constant(3)))
                    .Add(MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "FetchArrayNoArg"));
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(joinStatement, model.ToEPL());
    
            TryArrayNoArg(epService, stmt);
        }
    
        private void TryArrayNoArg(EPServiceProvider epService, EPStatement stmt) {
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new string[]{"id", "TheString"};
    
            SendBeanEvent(epService, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"1", "E1"});
    
            SendBeanEvent(epService, "E2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"1", "E2"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionArrayWithArg(EPServiceProvider epService) {
            string joinStatement = "select irstream id, TheString from " +
                    typeof(SupportBean).FullName + "()#length(3) as s1, " +
                    " method:" + typeof(SupportStaticMethodLib).FullName + ".FetchArrayGen(IntPrimitive)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            TryArrayWithArg(epService, stmt);
    
            joinStatement = "select irstream id, TheString from " +
                    "method:" + typeof(SupportStaticMethodLib).FullName + ".FetchArrayGen(IntPrimitive) as s0, " +
                    typeof(SupportBean).FullName + "#length(3)";
            stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            TryArrayWithArg(epService, stmt);
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(joinStatement);
            Assert.AreEqual(joinStatement, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            TryArrayWithArg(epService, stmt);
    
            model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("id", "TheString")
                .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH);
            model.FromClause = FromClause.Create()
                .Add(MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "FetchArrayGen", "s0")
                        .AddParameter(Expressions.Property("IntPrimitive")))
                .Add(FilterStream.Create(typeof(SupportBean).FullName).AddView("length", Expressions.Constant(3)));
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(joinStatement, model.ToEPL());
    
            TryArrayWithArg(epService, stmt);
        }
    
        private void TryArrayWithArg(EPServiceProvider epService, EPStatement stmt) {
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new string[]{"id", "TheString"};
    
            SendBeanEvent(epService, "E1", -1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, "E2", 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, "E3", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", "E3"});
    
            SendBeanEvent(epService, "E4", 2);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"A", "E4"}, new object[] {"B", "E4"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendBeanEvent(epService, "E5", 3);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"A", "E5"}, new object[] {"B", "E5"}, new object[] {"C", "E5"}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendBeanEvent(epService, "E6", 1);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"A", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields, new object[][]{new object[] {"A", "E3"}});
            listener.Reset();
    
            SendBeanEvent(epService, "E7", 1);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"A", "E7"}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields, new object[][]{new object[] {"A", "E4"}, new object[] {"B", "E4"}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionObjectNoArg(EPServiceProvider epService) {
            string joinStatement = "select id, TheString from " +
                    typeof(SupportBean).FullName + "()#length(3) as s1, " +
                    " method:" + typeof(SupportStaticMethodLib).FullName + ".FetchObjectNoArg()";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new string[]{"id", "TheString"};
    
            SendBeanEvent(epService, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"2", "E1"});
    
            SendBeanEvent(epService, "E2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"2", "E2"});
        }
    
        private void RunAssertionObjectWithArg(EPServiceProvider epService) {
            string joinStatement = "select id, TheString from " +
                    typeof(SupportBean).FullName + "()#length(3) as s1, " +
                    " method:" + typeof(SupportStaticMethodLib).FullName + ".FetchObject(TheString)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new string[]{"id", "TheString"};
    
            SendBeanEvent(epService, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"|E1|", "E1"});
    
            SendBeanEvent(epService, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, "E2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"|E2|", "E2"});
        }
    
        private void RunAssertionInvocationTargetEx(EPServiceProvider epService) {
            string joinStatement = "select s1.TheString from " +
                    typeof(SupportBean).FullName + "()#length(3) as s1, " +
                    " method:" + typeof(SupportStaticMethodLib).FullName + ".ThrowExceptionBeanReturn()";
    
            epService.EPAdministrator.CreateEPL(joinStatement);
    
            try {
                SendBeanEvent(epService, "E1");
                Assert.Fail(); // default test configuration rethrows this exception
            } catch (EPException) {
                // fine
            }
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".FetchArrayGen()",
                    "Error starting statement: Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find static method named 'FetchArrayGen' in class '" + typeof(SupportStaticMethodLib).FullName + "' taking no parameters (nearest match found was 'FetchArrayGen' taking type(s) 'System.Int32') [");
    
            TryInvalid(epService, "select * from SupportBean, method:.abc where 1=2",
                    "Incorrect syntax near '.' at line 1 column 34, please check the method invocation join within the from clause [select * from SupportBean, method:.abc where 1=2]");
    
            TryInvalid(epService, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".FetchObjectAndSleep(1)",
                    string.Format("Error starting statement: Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed '{0}': Could not find static method named 'FetchObjectAndSleep' in class '{1}' with matching parameter number and expected parameter type(s) '{2}' (nearest match found was 'FetchObjectAndSleep' taking type(s) 'System.String, System.Int32, System.Int64') [", 
                        GetCleanName<int>(), typeof(SupportStaticMethodLib).FullName, 
                        GetCleanName<int>()));
    
            TryInvalid(epService, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".Sleep(100) where 1=2",
                    string.Format("Error starting statement: Invalid return type for static method 'Sleep' of class '{0}', expecting a class [select * from SupportBean, method:{0}.Sleep(100) where 1=2]", 
                        typeof(SupportStaticMethodLib).FullName));
    
            TryInvalid(epService, "select * from SupportBean, method:AClass. where 1=2",
                    "Incorrect syntax near 'where' (a reserved keyword) expecting an identifier but found 'where' at line 1 column 42, please check the view specifications within the from clause [select * from SupportBean, method:AClass. where 1=2]");
    
            TryInvalid(epService, "select * from SupportBean, method:Dummy.abc where 1=2",
                    "Error starting statement: Could not load class by name 'Dummy', please check imports [select * from SupportBean, method:Dummy.abc where 1=2]");
    
            TryInvalid(epService, "select * from SupportBean, method:Math where 1=2",
                    "Error starting statement: A function named 'Math' is not defined");
    
            TryInvalid(epService, "select * from SupportBean, method:Dummy.Dummy()#length(100) where 1=2",
                    "Error starting statement: Method data joins do not allow views onto the data, view 'length' is not valid in this context [select * from SupportBean, method:Dummy.Dummy()#length(100) where 1=2]");
    
            TryInvalid(epService, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).GetCleanName() + ".dummy where 1=2",
                    string.Format("Error starting statement: Could not find public static method named 'dummy' in class '{0}' [", 
                        typeof(SupportStaticMethodLib).GetCleanName()));
    
            TryInvalid(epService, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).GetCleanName() + ".MinusOne(10) where 1=2",
                    string.Format("Error starting statement: Invalid return type for static method 'MinusOne' of class '{0}', expecting a class [", 
                        typeof(SupportStaticMethodLib).GetCleanName()));
    
            TryInvalid(epService, "select * from SupportBean, xyz:" + typeof(SupportStaticMethodLib).GetCleanName() + ".FetchArrayNoArg() where 1=2",
                    string.Format("Expecting keyword 'method', found 'xyz' [select * from SupportBean, xyz:{0}.FetchArrayNoArg() where 1=2]", 
                        typeof(SupportStaticMethodLib).GetCleanName()));
    
            TryInvalid(epService, "select * from method:" + typeof(SupportStaticMethodLib).GetCleanName() + ".FetchBetween(s1.value, s1.value) as s0, method:" + typeof(SupportStaticMethodLib).GetCleanName() + ".FetchBetween(s0.value, s0.value) as s1",
                    "Error starting statement: Circular dependency detected between historical streams [");
    
            TryInvalid(epService, "select * from method:" + typeof(SupportStaticMethodLib).GetCleanName() + ".FetchBetween(s0.value, s0.value) as s0, method:" + typeof(SupportStaticMethodLib).GetCleanName() + ".FetchBetween(s0.value, s0.value) as s1",
                    "Error starting statement: Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");
    
            TryInvalid(epService, "select * from method:" + typeof(SupportStaticMethodLib).GetCleanName() + ".FetchBetween(s0.value, s0.value) as s0",
                    "Error starting statement: Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");
    
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportMethodInvocationJoinInvalid));
            TryInvalid(epService, "select * from method:SupportMethodInvocationJoinInvalid.ReadRowNoMetadata()",
                    "Error starting statement: Could not find getter method for method invocation, expected a method by name 'ReadRowNoMetadataMetadata' accepting no parameters [select * from method:SupportMethodInvocationJoinInvalid.ReadRowNoMetadata()]");
    
            TryInvalid(epService, "select * from method:SupportMethodInvocationJoinInvalid.ReadRowWrongMetadata()",
                    string.Format("Error starting statement: Getter method 'ReadRowWrongMetadataMetadata' does not return {0} [select * from method:SupportMethodInvocationJoinInvalid.ReadRowWrongMetadata()]", 
                        typeof(IDictionary<string, object>).GetCleanName()));
    
            TryInvalid(epService, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".InvalidOverloadForJoin(null)",
                    string.Format("Error starting statement: Method by name 'InvalidOverloadForJoin' is overloaded in class '{0}' and overloaded methods do not return the same type", 
                        typeof(SupportStaticMethodLib).GetCleanName()));
        }
    
        private void TryAssertionUDFAndScriptReturningEvents(EPServiceProvider epService, string methodName) {
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select id from SupportBean, method:" + methodName);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "id".Split(','), new object[][] {
                new object[] {"id1"}, new object[] {"id3"}
            });
    
            stmtSelect.Dispose();
        }
    
        private void TryAssertionEventBeanArray(EPServiceProvider epService, string methodName, bool soda) {
            string epl = "select p0 from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + "." + methodName + "(TheString) @Type(MyItemEvent)";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("a,b", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "p0".Split(','), new object[][] {
                new object[] {"a"}, new object[] {"b"}
            });
    
            stmt.Dispose();
        }
    
        private void SendBeanEvent(EPServiceProvider epService, string theString) {
            var bean = new SupportBean();
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendBeanEvent(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int intPrimitive, int intBoxed) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        public static EventBean[] MyItemProducerUDF(EPLMethodInvocationContext context) {
            var events = new EventBean[2];
            int count = 0;
            foreach (string id in "id1,id3".Split(',')) {
                events[count++] = context.EventBeanService.AdapterForMap(Collections.SingletonDataMap("id", id), "ItemEvent");
            }
            return events;
        }
    }
} // end of namespace
