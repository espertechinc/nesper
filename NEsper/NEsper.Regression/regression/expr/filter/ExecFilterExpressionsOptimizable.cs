///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportFilterItem;
// using static com.espertech.esper.supportregression.util.SupportFilterItem.GetBoolExprFilterItem;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.filter
{
    public class ExecFilterExpressionsOptimizable : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static EPLMethodInvocationContext _methodInvocationContextFilterOptimized;
    
        public override void Configure(Configuration configuration) {
            var func = new ConfigurationPlugInSingleRowFunction();
            func.FunctionClassName = GetType().FullName;
            func.FunctionMethodName = "MyCustomOkFunction";
            func.FilterOptimizable = FilterOptimizableEnum.ENABLED;
            func.IsRethrowExceptions = true;
            func.Name = "myCustomOkFunction";
            configuration.PlugInSingleRowFunctions.Add(func);
    
            configuration.AddEventType("SupportEvent", typeof(SupportTradeEvent));
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType(typeof(SupportBean_IntAlphabetic));
            configuration.AddEventType(typeof(SupportBean_StringAlphabetic));
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "libE1True", typeof(MyLib), "LibE1True", FilterOptimizableEnum.ENABLED);
    
            RunAssertionInAndNotInKeywordMultivalue(epService);
            RunAssertionOptimizablePerf(epService);
            RunAssertionOptimizableInspectFilter(epService);
            RunAssertionPatternUDFFilterOptimizable(epService);
            RunAssertionOrToInRewrite(epService);
            RunAssertionOrRewrite(epService);
            RunAssertionOrPerformance(epService);
        }
    
        private void RunAssertionInAndNotInKeywordMultivalue(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventInKeywordValue));
    
            TryInKeyword(epService, "ints", new MyEventInKeywordValue(new[]{1, 2}));
            TryInKeyword(epService, "mapOfIntKey", new MyEventInKeywordValue(CollectionUtil.TwoEntryMap(1, "x", 2, "y")));
            TryInKeyword(epService, "collOfInt", new MyEventInKeywordValue(Collections.List(1, 2)));
    
            TryNotInKeyword(epService, "ints", new MyEventInKeywordValue(new[]{1, 2}));
            TryNotInKeyword(epService, "mapOfIntKey", new MyEventInKeywordValue(CollectionUtil.TwoEntryMap(1, "x", 2, "y")));
            TryNotInKeyword(epService, "collOfInt", new MyEventInKeywordValue(Collections.List(1, 2)));
    
            TryInArrayContextProvided(epService);
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from pattern[every a=MyEventInKeywordValue -> SupportBean(IntPrimitive in (a.longs))]",
                    "Implicit conversion from datatype 'Int64' to 'Int32' for property 'IntPrimitive' is not allowed (strict filter type coercion)");
        }
    
        private void RunAssertionOptimizablePerf(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "libSplit", typeof(MyLib), "LibSplit", FilterOptimizableEnum.ENABLED);
    
            // create listeners
            int count = 10;
            var listeners = new SupportUpdateListener[count];
            for (int i = 0; i < count; i++) {
                listeners[i] = new SupportUpdateListener();
            }
    
            // Func(...) = value
            TryOptimizableEquals(epService, "select * from SupportBean(LibSplit(TheString) = !NUM!)", listeners);
    
            // Func(...) implied true
            TryOptimizableBoolean(epService, "select * from SupportBean(LibE1True(TheString))");
    
            // declared expression (...) = value
            epService.EPAdministrator.CreateEPL("create expression thesplit {TheString => LibSplit(TheString)}");
            TryOptimizableEquals(epService, "select * from SupportBean(thesplit(*) = !NUM!)", listeners);
    
            // declared expression (...) implied true
            epService.EPAdministrator.CreateEPL("create expression theE1Test {TheString => LibE1True(TheString)}");
            TryOptimizableBoolean(epService, "select * from SupportBean(theE1Test(*))");
    
            // typeof(e)
            TryOptimizableTypeOf(epService);
    
            // with context
            TryOptimizableMethodInvocationContext(epService);
    
            // with variable and separate thread
            TryOptimizableVariableAndSeparateThread(epService);
        }
    
        private void TryOptimizableVariableAndSeparateThread(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddVariable("myCheckServiceProvider", typeof(MyCheckServiceProvider), null);
            epService.EPRuntime.SetVariableValue("myCheckServiceProvider", new MyCheckServiceProvider());
    
            EPStatement epStatement = epService.EPAdministrator.CreateEPL("select * from SupportBean(myCheckServiceProvider.Check())");
            var listener = new SupportUpdateListener();
            epStatement.Events += listener.Update;
            var latch = new CountDownLatch(1);
    
            var executorService = Executors.NewSingleThreadExecutor();
            executorService.Submit(() => {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.IsTrue(listener.IsInvokedAndReset());
                latch.CountDown();
            });
            Assert.IsTrue(latch.Await(10, TimeUnit.SECONDS));
        }
    
        private void RunAssertionOptimizableInspectFilter(EPServiceProvider epService) {
    
            string epl;
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "funcOne", typeof(MyLib), "LibSplit", FilterOptimizableEnum.DISABLED);
            epl = "select * from SupportBean(funcOne(TheString) = 0)";
            AssertFilterSingle(epService, epl, FilterSpecCompiler.PROPERTY_NAME_BOOLEAN_EXPRESSION, FilterOperator.BOOLEAN_EXPRESSION);
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "funcOneWDefault", typeof(MyLib), "LibSplit");
            epl = "select * from SupportBean(funcOneWDefault(TheString) = 0)";
            AssertFilterSingle(epService, epl, "funcOneWDefault(TheString)", FilterOperator.EQUAL);
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "funcTwo", typeof(MyLib), "LibSplit", FilterOptimizableEnum.ENABLED);
            epl = "select * from SupportBean(funcTwo(TheString) = 0)";
            AssertFilterSingle(epService, epl, "funcTwo(TheString)", FilterOperator.EQUAL);
    
            epl = "select * from SupportBean(LibE1True(TheString))";
            AssertFilterSingle(epService, epl, "LibE1True(TheString)", FilterOperator.EQUAL);
    
            epl = "select * from SupportBean(funcTwo( TheString ) > 10)";
            AssertFilterSingle(epService, epl, "funcTwo(TheString)", FilterOperator.GREATER);
    
            epService.EPAdministrator.CreateEPL("create expression thesplit {TheString => funcOne(TheString)}");
    
            epl = "select * from SupportBean(thesplit(*) = 0)";
            AssertFilterSingle(epService, epl, "thesplit(*)", FilterOperator.EQUAL);
    
            epl = "select * from SupportBean(LibE1True(TheString))";
            AssertFilterSingle(epService, epl, "LibE1True(TheString)", FilterOperator.EQUAL);
    
            epl = "select * from SupportBean(thesplit(*) > 10)";
            AssertFilterSingle(epService, epl, "thesplit(*)", FilterOperator.GREATER);
    
            epl = "expression housenumber alias for {10} select * from SupportBean(IntPrimitive = housenumber)";
            AssertFilterSingle(epService, epl, "IntPrimitive", FilterOperator.EQUAL);
    
            epl = "expression housenumber alias for {IntPrimitive*10} select * from SupportBean(IntPrimitive = housenumber)";
            AssertFilterSingle(epService, epl, ".boolean_expression", FilterOperator.BOOLEAN_EXPRESSION);
    
            epl = "select * from SupportBean(typeof(e) = 'SupportBean') as e";
            AssertFilterSingle(epService, epl, "typeof(e)", FilterOperator.EQUAL);
        }
    
        private void RunAssertionPatternUDFFilterOptimizable(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "myCustomDecimalEquals", GetType(), "MyCustomDecimalEquals");
    
            string epl = "select * from pattern[a=SupportBean() -> b=SupportBean(myCustomDecimalEquals(a.DecimalBoxed, b.DecimalBoxed))]";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            var beanOne = new SupportBean("E1", 0);
            beanOne.DecimalBoxed = 13.0m;
            epService.EPRuntime.SendEvent(beanOne);
    
            var beanTwo = new SupportBean("E2", 0);
            beanTwo.DecimalBoxed = 13.0m;
            epService.EPRuntime.SendEvent(beanTwo);
    
            Assert.IsTrue(listener.IsInvoked);
        }
    
        private void RunAssertionOrToInRewrite(EPServiceProvider epService) {
            // test 'or' rewrite
            var filtersAB = new[]{
                    "TheString = 'a' or TheString = 'b'",
                    "TheString = 'a' or 'b' = TheString",
                    "'a' = TheString or 'b' = TheString",
                    "'a' = TheString or TheString = 'b'",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                AssertFilterSingle(epService, epl, "TheString", FilterOperator.IN_LIST_OF_VALUES);
                var listener = new SupportUpdateListener();
                epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
                epService.EPRuntime.SendEvent(new SupportBean("a", 0));
                Assert.IsTrue(listener.GetAndClearIsInvoked());
                epService.EPRuntime.SendEvent(new SupportBean("b", 0));
                Assert.IsTrue(listener.GetAndClearIsInvoked());
                epService.EPRuntime.SendEvent(new SupportBean("c", 0));
                Assert.IsFalse(listener.GetAndClearIsInvoked());
    
                epService.EPAdministrator.DestroyAllStatements();
            }
    
            string eplX = "select * from SupportBean(IntPrimitive = 1 and (TheString='a' or TheString='b'))";
            SupportFilterHelper.AssertFilterTwo(epService, eplX, "IntPrimitive", FilterOperator.EQUAL, "TheString", FilterOperator.IN_LIST_OF_VALUES);
        }
    
        private void RunAssertionOrRewrite(EPServiceProvider epService) {
            TryOrRewriteTwoOr(epService);
    
            TryOrRewriteOrRewriteThreeOr(epService);
    
            TryOrRewriteOrRewriteWithAnd(epService);
    
            TryOrRewriteOrRewriteThreeWithOverlap(epService);
    
            TryOrRewriteOrRewriteFourOr(epService);
    
            TryOrRewriteOrRewriteEightOr(epService);
    
            TryOrRewriteAndRewriteNotEquals(epService);
    
            TryOrRewriteAndRewriteInnerOr(epService);
    
            TryOrRewriteOrRewriteAndOrMulti(epService);
    
            TryOrRewriteBooleanExprSimple(epService);
    
            TryOrRewriteBooleanExprAnd(epService);
    
            TryOrRewriteSubquery(epService);
    
            TryOrRewriteHint(epService);
    
            TryOrRewriteContextPartitionedSegmented(epService);
    
            TryOrRewriteContextPartitionedHash(epService);
    
            TryOrRewriteContextPartitionedCategory(epService);
    
            TryOrRewriteContextPartitionedInitiatedSameEvent(epService);
    
            TryOrRewriteContextPartitionedInitiated(epService);
        }
    
        private void RunAssertionOrPerformance(EPServiceProvider epService) {
            foreach (var clazz in new[]{typeof(SupportBean)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            var listener = new SupportUpdateListener();
            for (int i = 0; i < 1000; i++) {
                string epl = "select * from SupportBean(TheString = '" + i + "' or IntPrimitive=" + i + ")";
                epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            }
    
            long start = PerformanceObserver.NanoTime;
            // Log.Info("Starting " + DateTime.Print(new Date()));
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("100", 1));
                Assert.IsTrue(listener.IsInvoked);
                listener.Reset();
            }
            // Log.Info("Ending " + DateTime.Print(new Date()));
            double delta = (PerformanceObserver.NanoTime - start) / 1000d / 1000d;
            // Log.Info("Delta=" + (delta + " msec"));
            Assert.IsTrue(delta < 500);
        }
    
        private void TryInKeyword(EPServiceProvider epService, string field, MyEventInKeywordValue prototype) {
            TryInKeywordPlain(epService, field, prototype);
            TryInKeywordPattern(epService, field, prototype);
        }
    
        private void TryNotInKeyword(EPServiceProvider epService, string field, MyEventInKeywordValue prototype) {
            TryNotInKeywordPlain(epService, field, prototype);
            TryNotInKeywordPattern(epService, field, prototype);
        }
    
        private void TryInKeywordPlain(EPServiceProvider epService, string field, MyEventInKeywordValue prototype) {
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyEventInKeywordValue#keepall where 1 in (" + field + ")");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SerializableObjectCopier.Copy(epService.Container, prototype));
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            stmt.Dispose();
        }
    
        private void TryNotInKeywordPlain(EPServiceProvider epService, string field, MyEventInKeywordValue prototype) {
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyEventInKeywordValue#keepall where 1 not in (" + field + ")");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SerializableObjectCopier.Copy(epService.Container, prototype));
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            stmt.Dispose();
        }
    
        private void TryInKeywordPattern(EPServiceProvider epService, string field, MyEventInKeywordValue prototype) {
    
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from pattern[every a=MyEventInKeywordValue -> SupportBean(IntPrimitive in (a." + field + "))]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            AssertInKeywordReceivedPattern(epService, listener, SerializableObjectCopier.Copy(epService.Container, prototype), 1, true);
            AssertInKeywordReceivedPattern(epService, listener, SerializableObjectCopier.Copy(epService.Container, prototype), 2, true);
    
            AssertInKeywordReceivedPattern(epService, listener, SerializableObjectCopier.Copy(epService.Container, prototype), 3, false);
            SupportFilterHelper.AssertFilterMulti(stmt, "SupportBean", new[]
            {
                new[]{new SupportFilterItem("IntPrimitive", FilterOperator.IN_LIST_OF_VALUES)},
            });
    
            stmt.Dispose();
        }
    
        private void TryNotInKeywordPattern(EPServiceProvider epService, string field, MyEventInKeywordValue prototype) {
    
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from pattern[every a=MyEventInKeywordValue -> SupportBean(IntPrimitive not in (a." + field + "))]");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            AssertInKeywordReceivedPattern(epService, listener, SerializableObjectCopier.Copy(epService.Container, prototype), 0, true);
            AssertInKeywordReceivedPattern(epService, listener, SerializableObjectCopier.Copy(epService.Container, prototype), 3, true);
    
            AssertInKeywordReceivedPattern(epService, listener, SerializableObjectCopier.Copy(epService.Container, prototype), 1, false);
            SupportFilterHelper.AssertFilterMulti(stmt, "SupportBean", new[]
            {
                new[]{new SupportFilterItem("IntPrimitive", FilterOperator.NOT_IN_LIST_OF_VALUES)},
            });
    
            stmt.Dispose();
        }
    
        private void AssertInKeywordReceivedPattern(EPServiceProvider epService, SupportUpdateListener listener, Object @event, int intPrimitive, bool expected) {
            epService.EPRuntime.SendEvent(@event);
            epService.EPRuntime.SendEvent(new SupportBean(null, intPrimitive));
            Assert.AreEqual(expected, listener.IsInvokedAndReset());
        }
    
        private void TryInArrayContextProvided(EPServiceProvider epService) {
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("create context MyContext initiated by MyEventInKeywordValue as mie terminated after 24 hours");
    
            EPStatement statementOne = epService.EPAdministrator.CreateEPL("context MyContext select * from SupportBean#keepall where IntPrimitive in (context.mie.ints)");
            statementOne.Events += listenerOne.Update;
    
            EPStatementSPI statementTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context MyContext select * from SupportBean(IntPrimitive in (context.mie.ints))");
            statementTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new MyEventInKeywordValue(new[]{1, 2}));
    
            AssertInKeywordReceivedContext(epService, listenerOne, listenerTwo);
    
            SupportFilterHelper.AssertFilterMulti(statementTwo, "SupportBean", new[]
            {
                new[]{new SupportFilterItem("IntPrimitive", FilterOperator.IN_LIST_OF_VALUES)},
            });
    
            statementOne.Dispose();
            statementTwo.Dispose();
        }
    
        private void AssertInKeywordReceivedContext(EPServiceProvider epService, SupportUpdateListener listenerOne, SupportUpdateListener listenerTwo) {
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(listenerOne.IsInvokedAndReset() && listenerTwo.IsInvokedAndReset());
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsTrue(listenerOne.IsInvokedAndReset() && listenerTwo.IsInvokedAndReset());
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listenerOne.IsInvokedAndReset() || listenerTwo.IsInvokedAndReset());
        }
    
        private void TryOrRewriteHint(EPServiceProvider epService) {
            string epl = "@Hint('MAX_FILTER_WIDTH=0') select * from SupportBean_IntAlphabetic((b=1 or c=1) and (d=1 or e=1))";
            AssertFilterSingle(epService, epl, ".boolean_expression", FilterOperator.BOOLEAN_EXPRESSION);
        }
    
        private void TryOrRewriteSubquery(EPServiceProvider epService) {
            string epl = "select (select * from SupportBean_IntAlphabetic(a=1 or b=1)#keepall) as c0 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBean_IntAlphabetic iaOne = IntEvent(1, 1);
            epService.EPRuntime.SendEvent(iaOne);
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(iaOne, listener.AssertOneGetNewAndReset().Get("c0"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOrRewriteContextPartitionedCategory(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext \n" +
                    "  group a=1 or b=1 as g1,\n" +
                    "  group c=1 as g1\n" +
                    "  from SupportBean_IntAlphabetic");
            string epl = "context MyContext select * from SupportBean_IntAlphabetic(d=1 or e=1)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendAssertEvents(epService, listener,
                    new object[]{IntEvent(1, 0, 0, 0, 1), IntEvent(0, 1, 0, 1, 0), IntEvent(0, 0, 1, 1, 1)},
                    new object[]{IntEvent(0, 0, 0, 1, 0), IntEvent(1, 0, 0, 0, 0), IntEvent(0, 0, 1, 0, 0)}
            );
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOrRewriteContextPartitionedHash(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext " +
                    "coalesce by Consistent_hash_crc32(a) from SupportBean_IntAlphabetic(b=1) granularity 16 preallocate");
            string epl = "context MyContext select * from SupportBean_IntAlphabetic(c=1 or d=1)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendAssertEvents(epService, listener,
                    new object[]{IntEvent(100, 1, 0, 1), IntEvent(100, 1, 1, 0)},
                    new object[]{IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0)}
            );
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOrRewriteContextPartitionedSegmented(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext partition by a from SupportBean_IntAlphabetic(b=1 or c=1)");
            string epl = "context MyContext select * from SupportBean_IntAlphabetic(d=1)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendAssertEvents(epService, listener,
                    new object[]{IntEvent(100, 1, 0, 1), IntEvent(100, 0, 1, 1)},
                    new object[]{IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0)}
            );
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOrRewriteBooleanExprAnd(EPServiceProvider epService) {
            var filters = new[]{
                    "(a='a' or a like 'A%') and (b='b' or b like 'B%')",
            };
            foreach (string filter in filters) {
                string epl = "select * from SupportBean_StringAlphabetic(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean_StringAlphabetic", new[]
                {
                    new[]{new SupportFilterItem("a", FilterOperator.EQUAL), new SupportFilterItem("b", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("a", FilterOperator.EQUAL), BoolExprFilterItem},
                    new[]{new SupportFilterItem("b", FilterOperator.EQUAL), BoolExprFilterItem},
                    new[]{BoolExprFilterItem},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new object[]{StringEvent("a", "b"), StringEvent("A1", "b"), StringEvent("a", "B1"), StringEvent("A1", "B1")},
                        new object[]{StringEvent("x", "b"), StringEvent("a", "x"), StringEvent("A1", "C"), StringEvent("C", "B1")}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteBooleanExprSimple(EPServiceProvider epService) {
            var filters = new[]{
                    "a like 'a%' and (b='b' or c='c')",
            };
            foreach (string filter in filters) {
                string epl = "select * from SupportBean_StringAlphabetic(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean_StringAlphabetic", new[]
                {
                    new[]{new SupportFilterItem("b", FilterOperator.EQUAL), BoolExprFilterItem},
                    new[]{new SupportFilterItem("c", FilterOperator.EQUAL), BoolExprFilterItem},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new object[]{StringEvent("a1", "b", null), StringEvent("a1", null, "c")},
                        new object[]{StringEvent("x", "b", null), StringEvent("a1", null, null), StringEvent("a1", null, "x")}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteAndRewriteNotEquals(EPServiceProvider epService) {
            TryOrRewriteAndRewriteNotEqualsOr(epService);
    
            TryOrRewriteAndRewriteNotEqualsConsolidate(epService);
    
            TryOrRewriteAndRewriteNotEqualsWithOrConsolidateSecond(epService);
        }
    
        private void TryOrRewriteAndRewriteNotEqualsWithOrConsolidateSecond(EPServiceProvider epService) {
            var filters = new[]{
                    "a!=1 and a!=2 and ((a!=3 and a!=4) or (a!=5 and a!=6))",
            };
            foreach (string filter in filters) {
                string epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean_IntAlphabetic", new[]
                {
                    new[]{new SupportFilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), BoolExprFilterItem},
                    new[]{new SupportFilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), BoolExprFilterItem},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new object[]{IntEvent(3), IntEvent(4), IntEvent(0)},
                        new object[]{IntEvent(2), IntEvent(1)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteAndRewriteNotEqualsConsolidate(EPServiceProvider epService) {
            var filters = new[]{
                    "a!=1 and a!=2 and (a!=3 or a!=4)",
            };
            foreach (string filter in filters) {
                string epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean_IntAlphabetic", new[]
                {
                    new[]{new SupportFilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new SupportFilterItem("a", FilterOperator.NOT_EQUAL)},
                    new[]{new SupportFilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new SupportFilterItem("a", FilterOperator.NOT_EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new object[]{IntEvent(3), IntEvent(4), IntEvent(0)},
                        new object[]{IntEvent(2), IntEvent(1)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteAndRewriteNotEqualsOr(EPServiceProvider epService) {
            var filters = new[]{
                    "a!=1 and a!=2 and (b=1 or c=1)",
            };
            foreach (string filter in filters) {
                string epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean_IntAlphabetic", new[]
                {
                    new[]{new SupportFilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new SupportFilterItem("b", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new SupportFilterItem("c", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new object[]{IntEvent(3, 1, 0), IntEvent(3, 0, 1), IntEvent(0, 1, 0)},
                        new object[]{IntEvent(2, 0, 0), IntEvent(1, 0, 0), IntEvent(3, 0, 0)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteAndRewriteInnerOr(EPServiceProvider epService) {
            var filtersAB = new[]{
                    "TheString='a' and (IntPrimitive=1 or LongPrimitive=10)",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean", new[]
                {
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL), new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL), new SupportFilterItem("LongPrimitive", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new[]{MakeEvent("a", 1, 0), MakeEvent("a", 0, 10), MakeEvent("a", 1, 10)},
                        new[]{MakeEvent("x", 0, 0), MakeEvent("a", 2, 20), MakeEvent("x", 1, 10)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteOrRewriteAndOrMulti(EPServiceProvider epService) {
            var filtersAB = new[]{
                    "a=1 and (b=1 or c=1) and (d=1 or e=1)",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean_IntAlphabetic(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean_IntAlphabetic", new[]
                {
                    new[]{new SupportFilterItem("a", FilterOperator.EQUAL), new SupportFilterItem("b", FilterOperator.EQUAL), new SupportFilterItem("d", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("a", FilterOperator.EQUAL), new SupportFilterItem("c", FilterOperator.EQUAL), new SupportFilterItem("d", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("a", FilterOperator.EQUAL), new SupportFilterItem("c", FilterOperator.EQUAL), new SupportFilterItem("e", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("a", FilterOperator.EQUAL), new SupportFilterItem("b", FilterOperator.EQUAL), new SupportFilterItem("e", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new object[]{IntEvent(1, 1, 0, 1, 0), IntEvent(1, 0, 1, 0, 1), IntEvent(1, 1, 0, 0, 1), IntEvent(1, 0, 1, 1, 0)},
                        new object[]{IntEvent(1, 0, 0, 1, 0), IntEvent(1, 0, 0, 1, 0), IntEvent(1, 1, 1, 0, 0), IntEvent(0, 1, 1, 1, 1)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteOrRewriteEightOr(EPServiceProvider epService) {
            var filtersAB = new[]{
                    "TheString = 'a' or IntPrimitive=1 or LongPrimitive=10 or DoublePrimitive=100 or BoolPrimitive=true or " +
                            "IntBoxed=2 or LongBoxed=20 or DoubleBoxed=200",
                    "LongBoxed=20 or TheString = 'a' or BoolPrimitive=true or IntBoxed=2 or LongPrimitive=10 or DoublePrimitive=100 or " +
                            "IntPrimitive=1 or DoubleBoxed=200",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean", new[]
                {
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("LongPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("DoublePrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("BoolPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("IntBoxed", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("LongBoxed", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("DoubleBoxed", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new[]{MakeEvent("a", 1, 10, 100, true, 2, 20, 200), MakeEvent("a", 0, 0, 0, true, 0, 0, 0),
                                MakeEvent("a", 0, 0, 0, true, 0, 20, 0), MakeEvent("x", 0, 0, 100, false, 0, 0, 0),
                                MakeEvent("x", 1, 0, 0, false, 0, 0, 200), MakeEvent("x", 0, 0, 0, false, 0, 0, 200),
                        },
                        new[]{MakeEvent("x", 0, 0, 0, false, 0, 0, 0)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteOrRewriteFourOr(EPServiceProvider epService) {
            var filtersAB = new[]{
                    "TheString = 'a' or IntPrimitive=1 or LongPrimitive=10 or DoublePrimitive=100",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean", new[]
                {
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("LongPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("DoublePrimitive", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new[]{MakeEvent("a", 1, 10, 100), MakeEvent("x", 0, 0, 100), MakeEvent("x", 0, 10, 100), MakeEvent("a", 0, 0, 0)},
                        new[]{MakeEvent("x", 0, 0, 0)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteOrRewriteThreeWithOverlap(EPServiceProvider epService) {
            var filtersAB = new[]{
                    "TheString = 'a' or TheString = 'b' or IntPrimitive=1",
                    "IntPrimitive = 1 or TheString = 'b' or TheString = 'a'",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean", new[]
                {
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new[]{MakeEvent("a", 1), MakeEvent("b", 0), MakeEvent("x", 1)},
                        new[]{MakeEvent("x", 0)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteOrRewriteWithAnd(EPServiceProvider epService) {
            var filtersAB = new[]{
                    "(TheString = 'a' and IntPrimitive = 1) or (TheString = 'b' and IntPrimitive = 2)",
                    "(IntPrimitive = 1 and TheString = 'a') or (IntPrimitive = 2 and TheString = 'b')",
                    "(TheString = 'b' and IntPrimitive = 2) or (TheString = 'a' and IntPrimitive = 1)",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean", new[]
                {
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL), new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL), new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new[]{MakeEvent("a", 1), MakeEvent("b", 2)},
                        new[]{MakeEvent("x", 0), MakeEvent("a", 0), MakeEvent("a", 2), MakeEvent("b", 1)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOrRewriteOrRewriteThreeOr(EPServiceProvider epService) {
            var filtersAB = new[]{
                    "TheString = 'a' or IntPrimitive = 1 or LongPrimitive = 2",
                    "2 = LongPrimitive or 1 = IntPrimitive or TheString = 'a'"
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean", new[]
                {
                    new[]{new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("LongPrimitive", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendAssertEvents(epService, listener,
                        new[]{MakeEvent("a", 0, 0), MakeEvent("b", 1, 0), MakeEvent("c", 0, 2), MakeEvent("c", 0, 2)},
                        new[]{MakeEvent("v", 0, 0), MakeEvent("c", 2, 1)}
                );
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void SendAssertEvents(EPServiceProvider epService, SupportUpdateListener listener, object[] matches, object[] nonMatches) {
            listener.Reset();
            foreach (Object match in matches) {
                epService.EPRuntime.SendEvent(match);
                Assert.AreSame(match, listener.AssertOneGetNewAndReset().Underlying);
            }
            listener.Reset();
            foreach (Object nonMatch in nonMatches) {
                epService.EPRuntime.SendEvent(nonMatch);
                Assert.IsFalse(listener.IsInvoked);
            }
        }
    
        private void TryOrRewriteTwoOr(EPServiceProvider epService) {
            // test 'or' rewrite
            var filtersAB = new[]{
                    "TheString = 'a' or IntPrimitive = 1",
                    "TheString = 'a' or 1 = IntPrimitive",
                    "'a' = TheString or 1 = IntPrimitive",
                    "'a' = TheString or IntPrimitive = 1",
            };
            foreach (string filter in filtersAB) {
                string epl = "select * from SupportBean(" + filter + ")";
                EPStatement stmt = SupportFilterHelper.AssertFilterMulti(epService, epl, "SupportBean", new[]
                {
                    new[]{new SupportFilterItem("IntPrimitive", FilterOperator.EQUAL)},
                    new[]{new SupportFilterItem("TheString", FilterOperator.EQUAL)},
                });
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                epService.EPRuntime.SendEvent(new SupportBean("a", 0));
                listener.AssertOneGetNewAndReset();
                epService.EPRuntime.SendEvent(new SupportBean("b", 1));
                listener.AssertOneGetNewAndReset();
                epService.EPRuntime.SendEvent(new SupportBean("c", 0));
                Assert.IsFalse(listener.GetAndClearIsInvoked());
    
                epService.EPAdministrator.DestroyAllStatements();
            }
        }
    
        private void TryOptimizableEquals(EPServiceProvider epService, string epl, SupportUpdateListener[] listeners) {
    
            // test function returns lookup value and "equals"
            for (int i = 0; i < listeners.Length; i++) {
                EPStatement stmt = epService.EPAdministrator.CreateEPL(epl.Replace("!NUM!", Convert.ToString(i)));
                stmt.Events += listeners[i].Update;
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            MyLib.ResetCountInvoked();
            int loops = 1000;
            for (int i = 0; i < loops; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E_" + i % listeners.Length, 0));
                SupportUpdateListener listener = listeners[i % listeners.Length];
                Assert.IsTrue(listener.GetAndClearIsInvoked());
            }
            long delta = DateTimeHelper.CurrentTimeMillis - startTime;
            Assert.AreEqual(loops, MyLib.GetCountInvoked());
    
            Log.Info("Equals delta=" + delta);
            Assert.IsTrue(delta < 1000, "Delta is " + delta);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOptimizableBoolean(EPServiceProvider epService, string epl) {
    
            // test function returns lookup value and "equals"
            int count = 10;
            var listener = new SupportUpdateListener();
            for (int i = 0; i < count; i++) {
                EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
                stmt.Events += listener.Update;
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            MyLib.ResetCountInvoked();
            int loops = 10000;
            for (int i = 0; i < loops; i++) {
                string key = "E_" + i % 100;
                epService.EPRuntime.SendEvent(new SupportBean(key, 0));
                if (key.Equals("E_1")) {
                    Assert.AreEqual(count, listener.NewDataList.Count);
                    listener.Reset();
                } else {
                    Assert.IsFalse(listener.IsInvoked);
                }
            }
            long delta = DateTimeHelper.CurrentTimeMillis - startTime;
            Assert.AreEqual(loops, MyLib.GetCountInvoked());
    
            Log.Info("bool? delta=" + delta);
            Assert.IsTrue(delta < 1000, "Delta is " + delta);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertFilterSingle(EPServiceProvider epService, string epl, string expression, FilterOperator op) {
            EPStatementSPI statementSPI = (EPStatementSPI) epService.EPAdministrator.CreateEPL(epl);
            if (((FilterServiceSPI) statementSPI.StatementContext.FilterService).IsSupportsTakeApply) {
                FilterValueSetParam param = SupportFilterHelper.GetFilterSingle(statementSPI);
                Assert.AreEqual(op, param.FilterOperator, "failed for '" + epl + "'");
                Assert.AreEqual(expression, param.Lookupable.Expression);
            }
        }
    
        private void TryOptimizableMethodInvocationContext(EPServiceProvider epService) {
            _methodInvocationContextFilterOptimized = null;
            epService.EPAdministrator.CreateEPL("select * from SupportBean e where myCustomOkFunction(e) = \"OK\"");
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual("default", _methodInvocationContextFilterOptimized.EngineURI);
            Assert.AreEqual("myCustomOkFunction", _methodInvocationContextFilterOptimized.FunctionName);
            Assert.IsNull(_methodInvocationContextFilterOptimized.StatementUserObject);
            Assert.IsNull(_methodInvocationContextFilterOptimized.StatementName);
            Assert.AreEqual(-1, _methodInvocationContextFilterOptimized.ContextPartitionId);
            _methodInvocationContextFilterOptimized = null;
        }
    
        private void TryOptimizableTypeOf(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportOverrideBase));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportOverrideBase(typeof(e) = 'SupportOverrideBase') as e");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportOverrideBase(""));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportOverrideOne("a", "b"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void TryOrRewriteContextPartitionedInitiated(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('ctx') create context MyContext initiated by SupportBean(TheString='A' or IntPrimitive=1) terminated after 24 hours");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('select') context MyContext select * from SupportBean").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            listener.AssertOneGetNewAndReset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOrRewriteContextPartitionedInitiatedSameEvent(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyContext initiated by SupportBean terminated after 24 hours");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("context MyContext select * from SupportBean(TheString='A' or IntPrimitive=1)").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            listener.AssertOneGetNewAndReset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive) {
            return MakeEvent(theString, intPrimitive, 0L);
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            return MakeEvent(theString, intPrimitive, longPrimitive, 0d);
        }
    
        private SupportBean_IntAlphabetic IntEvent(int a) {
            return new SupportBean_IntAlphabetic(a);
        }
    
        private SupportBean_IntAlphabetic IntEvent(int a, int b) {
            return new SupportBean_IntAlphabetic(a, b);
        }
    
        private SupportBean_IntAlphabetic IntEvent(int a, int b, int c, int d) {
            return new SupportBean_IntAlphabetic(a, b, c, d);
        }
    
        private SupportBean_StringAlphabetic StringEvent(string a, string b) {
            return new SupportBean_StringAlphabetic(a, b);
        }
    
        private SupportBean_StringAlphabetic StringEvent(string a, string b, string c) {
            return new SupportBean_StringAlphabetic(a, b, c);
        }
    
        private SupportBean_IntAlphabetic IntEvent(int a, int b, int c) {
            return new SupportBean_IntAlphabetic(a, b, c);
        }
    
        private SupportBean_IntAlphabetic IntEvent(int a, int b, int c, int d, int e) {
            return new SupportBean_IntAlphabetic(a, b, c, d, e);
        }
    
        private SupportBean MakeEvent(
            string theString, 
            int intPrimitive, 
            long longPrimitive,
            double doublePrimitive)
        {
            SupportBean @event = new SupportBean(theString, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            @event.DoublePrimitive = doublePrimitive;
            return @event;
        }
    
        private SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive, 
            double doublePrimitive,
            bool boolPrimitive, 
            int intBoxed, 
            long longBoxed, 
            double doubleBoxed)
        {
            SupportBean @event = new SupportBean(theString, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            @event.DoublePrimitive = doublePrimitive;
            @event.BoolPrimitive = boolPrimitive;
            @event.LongBoxed = longBoxed;
            @event.DoubleBoxed = doubleBoxed;
            @event.IntBoxed = intBoxed;
            return @event;
        }
    
        public static string MyCustomOkFunction(Object e, EPLMethodInvocationContext ctx) {
            _methodInvocationContextFilterOptimized = ctx;
            return "OK";
        }
    
        public static bool MyCustomDecimalEquals(decimal first, decimal second) {
            return first.CompareTo(second) == 0;
        }
    
        public class MyLib
        {
            private static int _countInvoked;
    
            public static int LibSplit(string theString) {
                string[] key = theString.Split('_');
                _countInvoked++;
                return int.Parse(key[1]);
            }
    
            public static bool LibE1True(string theString) {
                _countInvoked++;
                return theString.Equals("E_1");
            }
    
            public static int GetCountInvoked() {
                return _countInvoked;
            }
    
            public static void ResetCountInvoked() {
                _countInvoked = 0;
            }
        }

        [Serializable]
        public class MyEventInKeywordValue
        {
            private readonly int[] _ints;
            private readonly long[] _longs;
            private readonly IDictionary<int, string> _mapOfIntKey;
            private readonly ICollection<int> _collOfInt;

            public MyEventInKeywordValue(int[] ints)
            {
                _ints = ints;
            }

            public MyEventInKeywordValue(IDictionary<int, string> mapOfIntKey)
            {
                _mapOfIntKey = mapOfIntKey;
            }

            public MyEventInKeywordValue(ICollection<int> collOfInt)
            {
                _collOfInt = collOfInt;
            }

            public MyEventInKeywordValue(long[] longs)
            {
                _longs = longs;
            }

            public int[] Ints => _ints;

            public long[] Longs => _longs;

            public IDictionary<int, string> MapOfIntKey => _mapOfIntKey;

            public ICollection<int> CollOfInt => _collOfInt;
        }

        public class MyCheckServiceProvider
        {
            public bool Check()
            {
                return true;
            }
        }
    }
} // end of namespace
