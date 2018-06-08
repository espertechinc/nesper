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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.filter
{
    public class ExecFilterExpressions : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportEvent", typeof(SupportTradeEvent));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionPromoteIndexToSetNotIn(epService);
            RunAssertionFilterInstanceMethodWWildcard(epService);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionShortCircuitEvalAndOverspecified(epService);
            }
            RunAssertionRelationalOpConstantFirst(epService);
            RunAssertionInSet(epService);
            RunAssertionRewriteWhere(epService);
            RunAssertionNullBooleanExpr(epService);
            RunAssertionFilterOverInClause(epService);
            RunAssertionConstant(epService);
            RunAssertionEnumSyntaxOne(epService);
            RunAssertionEnumSyntaxTwo(epService);
            RunAssertionNotEqualsConsolidate(epService);
            RunAssertionEqualsSemanticFilter(epService);
            RunAssertionEqualsSemanticExpr(epService);
            RunAssertionNotEqualsNull(epService);
            RunAssertionPatternFunc3Stream(epService);
            RunAssertionPatternFunc(epService);
            RunAssertionIn3ValuesAndNull(epService);
            RunAssertionFilterStaticFunc(epService);
            RunAssertionFilterRelationalOpRange(epService);
            RunAssertionFilterBooleanExpr(epService);
            RunAssertionFilterWithEqualsSameCompare(epService);
            RunAssertionPatternWithExpr(epService);
            RunAssertionMathExpression(epService);
            RunAssertionShortAndByteArithmetic(epService);
            RunAssertionExpressionReversed(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionPromoteIndexToSetNotIn(EPServiceProvider epService) {
            var listenerOne = new SupportUpdateListener();
            var listenerTwo = new SupportUpdateListener();
            string eplOne = "select * from SupportBean(TheString != 'x' and TheString != 'y' and DoubleBoxed is not null)";
            string eplTwo = "select * from SupportBean(TheString != 'x' and TheString != 'y' and LongBoxed is not null)";
    
            EPStatement stmeOne = epService.EPAdministrator.CreateEPL(eplOne);
            stmeOne.Events += listenerOne.Update;
            EPStatement stmeTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            stmeTwo.Events += listenerTwo.Update;
    
            var bean = new SupportBean("E1", 0);
            bean.DoubleBoxed = 1d;
            bean.LongBoxed = 1L;
            epService.EPRuntime.SendEvent(bean);
    
            listenerOne.AssertOneGetNewAndReset();
            listenerTwo.AssertOneGetNewAndReset();
    
            stmeOne.Dispose();
            stmeTwo.Dispose();
        }
    
        private void RunAssertionFilterInstanceMethodWWildcard(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
    
            TryFilterInstanceMethod(epService, "select * from TestEvent(s0.MyInstanceMethodAlwaysTrue()) as s0", new[]{true, true, true});
            TryFilterInstanceMethod(epService, "select * from TestEvent(s0.MyInstanceMethodEventBean(s0, 'x', 1)) as s0", new[]{false, true, false});
            TryFilterInstanceMethod(epService, "select * from TestEvent(s0.MyInstanceMethodEventBean(*, 'x', 1)) as s0", new[]{false, true, false});
        }
    
        private void TryFilterInstanceMethod(EPServiceProvider epService, string epl, bool[] expected) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            for (int i = 0; i < 3; i++) {
                epService.EPRuntime.SendEvent(new TestEvent(i));
                Assert.AreEqual(expected[i], listener.GetAndClearIsInvoked());
            }
            stmt.Dispose();
        }
    
        private void RunAssertionShortCircuitEvalAndOverspecified(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEvent));
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyEvent(MyEvent.property2 = '4' and MyEvent.property1 = '1')");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new MyEvent());
            Assert.IsFalse(listener.IsInvoked, "Subscriber should not have received result(s)");
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString='A' and TheString='B')");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionRelationalOpConstantFirst(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(TestEvent));
            var listener = new SupportUpdateListener();
    
            epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 < x").Events += listener.Update;
            AssertSendReceive(epService, listener, new[]{3, 4, 5}, new[]{false, false, true});
    
            epService.EPAdministrator.GetStatement("A").Dispose();
            epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 <= x").Events += listener.Update;
            AssertSendReceive(epService, listener, new[]{3, 4, 5}, new[]{false, true, true});
    
            epService.EPAdministrator.GetStatement("A").Dispose();
            epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 > x").Events += listener.Update;
            AssertSendReceive(epService, listener, new[]{3, 4, 5}, new[]{true, false, false});
    
            epService.EPAdministrator.GetStatement("A").Dispose();
            epService.EPAdministrator.CreateEPL("@Name('A') select * from TestEvent where 4 >= x").Events += listener.Update;
            AssertSendReceive(epService, listener, new[]{3, 4, 5}, new[]{true, true, false});
    
            epService.EPAdministrator.GetStatement("A").Dispose();
        }
    
        private void RunAssertionInSet(EPServiceProvider epService) {
    
            // Esper-484
            var startLoadType = new Dictionary<string, object>();
            startLoadType.Put("versions", typeof(ICollection<string>));
            epService.EPAdministrator.Configuration.AddEventType("StartLoad", startLoadType);
    
            var singleLoadType = new Dictionary<string, object>();
            singleLoadType.Put("ver", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("SingleLoad", singleLoadType);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select * from \n" +
                            "pattern [ \n" +
                            " every start_load=StartLoad \n" +
                            " -> \n" +
                            " single_load=SingleLoad(ver in (start_load.versions)) \n" +
                            "]"
            );
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var versions = new HashSet<string>();
            versions.Add("Version1");
            versions.Add("Version2");
    
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("versions", versions), "StartLoad");
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("ver", "Version1"), "SingleLoad");
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionRewriteWhere(EPServiceProvider epService) {
            TryRewriteWhere(epService, "");
            TryRewriteWhere(epService, "@Hint('DISABLE_WHEREEXPR_MOVETO_FILTER')");
            TryRewriteWhereNamedWindow(epService);
        }
    
        private void TryRewriteWhereNamedWindow(EPServiceProvider epService) {
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL("create window NamedWindowA#length(1) as SupportBean");
            EPStatement stmtWithMethod = epService.EPAdministrator.CreateEPL("select * from NamedWindowA mywindow WHERE (mywindow.TheString.Trim() is 'abc')");
            stmtWindow.Dispose();
            stmtWithMethod.Dispose();
        }
    
        private void TryRewriteWhere(EPServiceProvider epService, string prefix) {
            string epl = prefix + " select * from SupportBean as A0 where A0.IntPrimitive = 3";
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            statement.Dispose();
        }
    
        private void RunAssertionNullBooleanExpr(EPServiceProvider epService) {
            string stmtOneText = "every event1=SupportEvent(userId like '123%')";
            EPStatement statement = epService.EPAdministrator.CreatePattern(stmtOneText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportTradeEvent(1, null, 1001));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportTradeEvent(2, "1234", 1001));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("event1.id"));
    
            statement.Dispose();
        }
    
        private void RunAssertionFilterOverInClause(EPServiceProvider epService) {
            // Test for Esper-159
            string stmtOneText = "every event1=SupportEvent(userId in ('100','101'),amount>=1000)";
            EPStatement statement = epService.EPAdministrator.CreatePattern(stmtOneText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportTradeEvent(1, "100", 1001));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("event1.id"));
    
            string stmtTwoText = "every event1=SupportEvent(userId in ('100','101'))";
            epService.EPAdministrator.CreatePattern(stmtTwoText);
    
            epService.EPRuntime.SendEvent(new SupportTradeEvent(2, "100", 1001));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("event1.id"));
    
            statement.Dispose();
        }
    
        private void RunAssertionConstant(EPServiceProvider epService) {
            string text = "select * from pattern [" +
                    typeof(SupportBean).FullName + "(IntPrimitive=" + typeof(ISupportAConst).FullName + ".VALUE_1)]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var theEvent = new SupportBean("e1", 2);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBean("e1", 1);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionEnumSyntaxOne(EPServiceProvider epService) {
            string text = "select * from pattern [" +
                    typeof(SupportBeanWithEnum).FullName + "(supportEnum=" + typeof(SupportEnumHelper).FullName + ".GetEnumFor('ENUM_VALUE_1'))]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_1);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionEnumSyntaxTwo(EPServiceProvider epService) {
            string text = "select * from pattern [" +
                    typeof(SupportBeanWithEnum).FullName + "(supportEnum=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_2)]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
    
            // test where clause
            text = "select * from " + typeof(SupportBeanWithEnum).FullName + " where supportEnum=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_2";
            stmt = epService.EPAdministrator.CreateEPL(text);
            stmt.Events += listener.Update;
    
            theEvent = new SupportBeanWithEnum("e1", SupportEnum.ENUM_VALUE_2);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            theEvent = new SupportBeanWithEnum("e2", SupportEnum.ENUM_VALUE_1);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotEqualsConsolidate(EPServiceProvider epService) {
            TryNotEqualsConsolidate(epService, "IntPrimitive not in (1, 2)");
            TryNotEqualsConsolidate(epService, "IntPrimitive != 1, IntPrimitive != 2");
            TryNotEqualsConsolidate(epService, "IntPrimitive != 1 and IntPrimitive != 2");
        }
    
        private void TryNotEqualsConsolidate(EPServiceProvider epService, string filter) {
            string text = "select * from " + typeof(SupportBean).FullName + "(" + filter + ")";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 5; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("", i));
    
                if ((i == 1) || (i == 2)) {
                    Assert.IsFalse(listener.IsInvoked, "incorrect:" + i);
                } else {
                    Assert.IsTrue(listener.IsInvoked, "incorrect:" + i);
                }
                listener.Reset();
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionEqualsSemanticFilter(EPServiceProvider epService) {
            // Test for Esper-114
            string text = "select * from " + typeof(SupportBeanComplexProps).FullName + "(nested=nested)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBeanComplexProps eventOne = SupportBeanComplexProps.MakeDefaultBean();
            eventOne.SimpleProperty = "1";
    
            epService.EPRuntime.SendEvent(eventOne);
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionEqualsSemanticExpr(EPServiceProvider epService) {
            // Test for Esper-114
            string text = "select * from " + typeof(SupportBeanComplexProps).FullName + "(simpleProperty='1')#keepall as s0" +
                    ", " + typeof(SupportBeanComplexProps).FullName + "(simpleProperty='2')#keepall as s1" +
                    " where s0.nested = s1.nested";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBeanComplexProps eventOne = SupportBeanComplexProps.MakeDefaultBean();
            eventOne.SimpleProperty = "1";
    
            SupportBeanComplexProps eventTwo = SupportBeanComplexProps.MakeDefaultBean();
            eventTwo.SimpleProperty = "2";
    
            Assert.AreEqual(eventOne.Nested, eventTwo.Nested);
    
            epService.EPRuntime.SendEvent(eventOne);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(eventTwo);
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotEqualsNull(EPServiceProvider epService) {
            var listeners = new SupportUpdateListener[6];
            for (int i = 0; i < listeners.Length; i++) {
                listeners[i] = new SupportUpdateListener();
            }
    
            // test equals&where-clause (can be optimized into filter)
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString != 'A'").Events += listeners[0].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString != 'A' or IntPrimitive != 0").Events += listeners[1].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString = 'A'").Events += listeners[2].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString = 'A' or IntPrimitive != 0").Events += listeners[3].Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            AssertListeners(listeners, new[]{false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            AssertListeners(listeners, new[]{false, true, false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            AssertListeners(listeners, new[]{false, false, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            AssertListeners(listeners, new[]{false, true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 0));
            AssertListeners(listeners, new[]{true, true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            AssertListeners(listeners, new[]{true, true, false, true});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test equals&selection
            string[] fields = "val0,val1,val2,val3,val4,val5".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select " +
                    "TheString != 'A' as val0, " +
                    "TheString != 'A' or IntPrimitive != 0 as val1, " +
                    "TheString != 'A' and IntPrimitive != 0 as val2, " +
                    "TheString = 'A' as val3," +
                    "TheString = 'A' or IntPrimitive != 0 as val4, " +
                    "TheString = 'A' and IntPrimitive != 0 as val5 from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, false, null, null, false});
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, true, null, null, true, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false, false, true, true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true, false, true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true, true, false, true, false});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test is-and-isnot&where-clause
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is null").Events += listeners[0].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is null or IntPrimitive != 0").Events += listeners[1].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is not null").Events += listeners[2].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString is not null or IntPrimitive != 0").Events += listeners[3].Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            AssertListeners(listeners, new[]{true, true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            AssertListeners(listeners, new[]{true, true, false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            AssertListeners(listeners, new[]{false, false, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            AssertListeners(listeners, new[]{false, true, true, true});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // test is-and-isnot&selection
            epService.EPAdministrator.CreateEPL("select " +
                    "TheString is null as val0, " +
                    "TheString is null or IntPrimitive != 0 as val1, " +
                    "TheString is null and IntPrimitive != 0 as val2, " +
                    "TheString is not null as val3," +
                    "TheString is not null or IntPrimitive != 0 as val4, " +
                    "TheString is not null and IntPrimitive != 0 as val5 " +
                    "from SupportBean").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true, true, false, true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false, false, true, true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true, false, true, true, true});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // filter expression
            epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString is null)").Events += listeners[0].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString = null").Events += listeners[1].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString = null)").Events += listeners[2].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString is not null)").Events += listeners[3].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString != null").Events += listeners[4].Update;
            epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString != null)").Events += listeners[5].Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 0));
            AssertListeners(listeners, new[]{true, false, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 0));
            AssertListeners(listeners, new[]{false, false, false, true, false, false});
    
            epService.EPAdministrator.DestroyAllStatements();
    
            // select constants
            fields = "val0,val1,val2,val3".Split(',');
            epService.EPAdministrator.CreateEPL("select " +
                    "2 != null as val0," +
                    "null = null as val1," +
                    "2 != null or 1 = 2 as val2," +
                    "2 != null and 2 = 2 as val3 " +
                    "from SupportBean").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});
    
            // test SODA
            string epl = "select IntBoxed is null, IntBoxed is not null, IntBoxed=1, IntBoxed!=1 from SupportBean";
            stmt = SupportModelHelper.CompileCreate(epService, epl);
            EPAssertionUtil.AssertEqualsExactOrder(new[]{"IntBoxed is null", "IntBoxed is not null",
                    "IntBoxed=1", "IntBoxed!=1"}, stmt.EventType.PropertyNames);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPatternFunc3Stream(EPServiceProvider epService) {
            string text;
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed, IntBoxed=b.IntBoxed and IntBoxed != null)]";
            TryPattern3Stream(epService, text, new int?[]{null, 2, 1, null, 8, 1, 2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 4, -2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 5, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{false, false, false, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed is a.IntBoxed, IntBoxed is b.IntBoxed and IntBoxed is not null)]";
            TryPattern3Stream(epService, text, new int?[]{null, 2, 1, null, 8, 1, 2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 4, -2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 5, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{false, false, true, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed or IntBoxed=b.IntBoxed)]";
            TryPattern3Stream(epService, text, new int?[]{null, 2, 1, null, 8, 1, 2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 4, -2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 5, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{false, true, true, true, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed, IntBoxed=b.IntBoxed)]";
            TryPattern3Stream(epService, text, new int?[]{null, 2, 1, null, 8, 1, 2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 4, -2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 5, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{false, false, true, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed!=a.IntBoxed, IntBoxed!=b.IntBoxed)]";
            TryPattern3Stream(epService, text, new int?[]{null, 2, 1, null, 8, 1, 2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 4, -2}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, 3, 1, 8, null, 5, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{false, false, false, false, false, true, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed!=a.IntBoxed)]";
            TryPattern3Stream(epService, text, new int?[]{2, 8, null, 2, 1, null, 1}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{-2, null, null, 3, 1, 8, 4}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, null, null, 3, 1, 8, 5}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{false, false, false, true, false, false, true});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed is not a.IntBoxed)]";
            TryPattern3Stream(epService, text, new int?[]{2, 8, null, 2, 1, null, 1}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{-2, null, null, 3, 1, 8, 4}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{null, null, null, 3, 1, 8, 5}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{true, true, false, true, false, true, true});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed=a.IntBoxed, DoubleBoxed=b.DoubleBoxed)]";
            TryPattern3Stream(epService, text, new int?[]{2, 2, 1, 2, 1, 7, 1}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{0, 0, 0, 0, 0, 0, 0}, new double?[]{1d, 2d, 0d, 2d, 0d, 1d, 0d},
                    new int?[]{2, 2, 3, 2, 1, 7, 5}, new double?[]{1d, 1d, 1d, 2d, 1d, 1d, 1d},
                    new[]{true, false, false, true, false, true, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed in (a.IntBoxed, b.IntBoxed))]";
            TryPattern3Stream(epService, text, new int?[]{2, 1, 1, null, 1, null, 1}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{1, 2, 1, null, null, 2, 0}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{2, 2, 3, null, 1, null, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{true, true, false, false, true, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed in [a.IntBoxed:b.IntBoxed])]";
            TryPattern3Stream(epService, text, new int?[]{2, 1, 1, null, 1, null, 1}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{1, 2, 1, null, null, 2, 0}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{2, 1, 3, null, 1, null, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{true, true, false, false, false, false, false});
    
            text = "select * from pattern [" +
                    "a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportBean).FullName + " -> " +
                    "c=" + typeof(SupportBean).FullName + "(IntBoxed not in [a.IntBoxed:b.IntBoxed])]";
            TryPattern3Stream(epService, text, new int?[]{2, 1, 1, null, 1, null, 1}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{1, 2, 1, null, null, 2, 0}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new int?[]{2, 1, 3, null, 1, null, null}, new double?[]{0d, 0d, 0d, 0d, 0d, 0d, 0d},
                    new[]{false, false, true, false, false, false, false});
        }
    
        private void RunAssertionPatternFunc(EPServiceProvider epService) {
            string text;
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(IntBoxed = a.IntBoxed and DoubleBoxed = a.DoubleBoxed)]";
            TryPattern(epService, text, new int?[]{null, 2, 1, null, 8, 1, 2}, new double?[]{2d, 2d, 2d, 1d, 5d, 6d, 7d},
                    new int?[]{null, 3, 1, 8, null, 1, 2}, new double?[]{2d, 3d, 2d, 1d, 5d, 6d, 8d},
                    new[]{false, false, true, false, false, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(IntBoxed is a.IntBoxed and DoubleBoxed = a.DoubleBoxed)]";
            TryPattern(epService, text, new int?[]{null, 2, 1, null, 8, 1, 2}, new double?[]{2d, 2d, 2d, 1d, 5d, 6d, 7d},
                    new int?[]{null, 3, 1, 8, null, 1, 2}, new double?[]{2d, 3d, 2d, 1d, 5d, 6d, 8d},
                    new[]{true, false, true, false, false, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed = DoubleBoxed)]";
            TryPattern(epService, text, new int?[]{0, 0}, new double?[]{2d, 2d},
                    new int?[]{0, 0}, new double?[]{2d, 3d},
                    new[]{true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed = b.DoubleBoxed)]";
            TryPattern(epService, text, new int?[]{0, 0}, new double?[]{2d, 2d},
                    new int?[]{0, 0}, new double?[]{2d, 3d},
                    new[]{true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed != DoubleBoxed)]";
            TryPattern(epService, text, new int?[]{0, 0}, new double?[]{2d, 2d},
                    new int?[]{0, 0}, new double?[]{2d, 3d},
                    new[]{false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(a.DoubleBoxed != b.DoubleBoxed)]";
            TryPattern(epService, text, new int?[]{0, 0}, new double?[]{2d, 2d},
                    new int?[]{0, 0}, new double?[]{2d, 3d},
                    new[]{false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in [a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{false, true, true, true, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in (a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{false, false, true, true, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(b.DoubleBoxed in (a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{false, false, true, true, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in [a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{false, true, true, true, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in [a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{true, false, false, false, false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in (a.DoubleBoxed:a.IntBoxed])]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{true, true, false, false, false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(b.DoubleBoxed not in (a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{true, true, false, false, true, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in [a.DoubleBoxed:a.IntBoxed))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{true, false, false, false, true, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in (a.DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{true, false, true, false, false, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed in (a.DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{false, true, false, true, true, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(b.DoubleBoxed in (DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{true, true, true, true, true, true});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed not in (DoubleBoxed, a.IntBoxed, 9))]";
            TryPattern(epService, text, new int?[]{1, 1, 1, 1, 1, 1}, new double?[]{10d, 10d, 10d, 10d, 10d, 10d},
                    new int?[]{0, 0, 0, 0, 0, 0}, new double?[]{0d, 1d, 2d, 9d, 10d, 11d},
                    new[]{false, false, false, false, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed = " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(a.DoubleBoxed))]";
            TryPattern(epService, text, new int?[]{0, 0, 0}, new double?[]{10d, 10d, 10d},
                    new int?[]{0, 0, 0}, new double?[]{9d, 10d, 11d},
                    new[]{true, false, false});
    
            text = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                    typeof(SupportBean).FullName + "(DoubleBoxed = " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(a.DoubleBoxed) or " +
                    "DoubleBoxed = " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(a.IntBoxed))]";
            TryPattern(epService, text, new int?[]{0, 0, 12}, new double?[]{10d, 10d, 10d},
                    new int?[]{0, 0, 0}, new double?[]{9d, 10d, 11d},
                    new[]{true, false, true});
        }
    
        private void TryPattern(EPServiceProvider epService, string text,
                                int?[] intBoxedA,
                                double?[] doubleBoxedA,
                                int?[] intBoxedB,
                                double?[] doubleBoxedB,
                                bool[] expected) {
            Assert.AreEqual(intBoxedA.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedB.Length);
            Assert.AreEqual(expected.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedA.Length, doubleBoxedB.Length);
    
            for (int i = 0; i < intBoxedA.Length; i++) {
                EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendBeanIntDouble(epService, intBoxedA[i], doubleBoxedA[i]);
                SendBeanIntDouble(epService, intBoxedB[i], doubleBoxedB[i]);
                Assert.AreEqual(expected[i], listener.GetAndClearIsInvoked(), "failed at index " + i);
                stmt.Stop();
            }
        }
    
        private void TryPattern3Stream(EPServiceProvider epService, string text,
                                       int?[] intBoxedA,
                                       double?[] doubleBoxedA,
                                       int?[] intBoxedB,
                                       double?[] doubleBoxedB,
                                       int?[] intBoxedC,
                                       double?[] doubleBoxedC,
                                       bool[] expected) {
            Assert.AreEqual(intBoxedA.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedB.Length);
            Assert.AreEqual(expected.Length, doubleBoxedA.Length);
            Assert.AreEqual(intBoxedA.Length, doubleBoxedB.Length);
            Assert.AreEqual(intBoxedC.Length, doubleBoxedC.Length);
            Assert.AreEqual(intBoxedB.Length, doubleBoxedC.Length);
    
            for (int i = 0; i < intBoxedA.Length; i++) {
                EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
                var listener = new SupportUpdateListener();
                stmt.Events += listener.Update;
    
                SendBeanIntDouble(epService, intBoxedA[i], doubleBoxedA[i]);
                SendBeanIntDouble(epService, intBoxedB[i], doubleBoxedB[i]);
                SendBeanIntDouble(epService, intBoxedC[i], doubleBoxedC[i]);
                Assert.AreEqual(expected[i], listener.GetAndClearIsInvoked(), "failed at index " + i);
                stmt.Stop();
            }
        }
    
        private void RunAssertionIn3ValuesAndNull(EPServiceProvider epService) {
            string text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (IntBoxed, DoubleBoxed))";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{0, 1, 0}, new double?[]{2d, 2d, 1d}, new[]{false, true, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (IntBoxed, " +
                    typeof(SupportStaticMethodLib).FullName + ".MinusOne(DoubleBoxed)))";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{0, 1, 0}, new double?[]{2d, 2d, 1d}, new[]{true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntPrimitive not in (IntBoxed, DoubleBoxed))";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{0, 1, 0}, new double?[]{2d, 2d, 1d}, new[]{true, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed = DoubleBoxed)";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{null, 1, null}, new double?[]{null, null, 1d}, new[]{false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (DoubleBoxed))";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{null, 1, null}, new double?[]{null, null, 1d}, new[]{false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (DoubleBoxed))";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{null, 1, null}, new double?[]{null, null, 1d}, new[]{false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [DoubleBoxed:10))";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{null, 1, 2}, new double?[]{null, null, 1d}, new[]{false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [DoubleBoxed:10))";
            Try3Fields(epService, text, new[]{1, 1, 1}, new int?[]{null, 1, 2}, new double?[]{null, null, 1d}, new[]{false, true, false});
        }
    
        private void RunAssertionFilterStaticFunc(EPServiceProvider epService) {
            string text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(" +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('b', TheString))";
            TryFilter(epService, text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(" +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('bx', TheString || 'x'))";
            TryFilter(epService, text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "('b'=TheString," +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('bx', TheString || 'x'))";
            TryFilter(epService, text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "('b'=TheString, TheString='b', TheString != 'a')";
            TryFilter(epService, text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString != 'a', TheString != 'c')";
            TryFilter(epService, text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString = 'b', TheString != 'c')";
            TryFilter(epService, text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString != 'a' and TheString != 'c')";
            TryFilter(epService, text, true);
    
            text = "select * from " + typeof(SupportBean).FullName + "(TheString = 'a' and TheString = 'c' and " +
                    typeof(SupportStaticMethodLib).FullName + ".IsStringEquals('bx', TheString || 'x'))";
            TryFilter(epService, text, false);
        }
    
        private void RunAssertionFilterRelationalOpRange(EPServiceProvider epService) {
            string text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [2:3])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [2:3] and IntBoxed in [2:3])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [2:3] and IntBoxed in [2:2])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, true, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [1:10] and IntBoxed in [3:2])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [3:3] and IntBoxed in [1:3])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, false, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [3:3] and IntBoxed in [1:3] and IntBoxed in [4:5])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [3:3] and IntBoxed not in [1:3])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (2:4) and IntBoxed not in (1:3))";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{true, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [2:4) and IntBoxed not in [1:3))";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (2:4] and IntBoxed not in (1:3])";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{true, false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in (2:4)";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{true, true, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in [2:4]";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{true, false, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in [2:4)";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{true, false, false, true});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed not in (2:4]";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{true, true, false, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in (2:4)";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, false, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in [2:4]";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, true, true, true});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in [2:4)";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, true, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + " where IntBoxed in (2:4]";
            TryFilterRelationalOpRange(epService, text, new[]{1, 2, 3, 4}, new[]{false, false, true, true});
        }
    
        private void TryFilterRelationalOpRange(EPServiceProvider epService, string text, int[] testData, bool[] isReceived) {
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            Assert.AreEqual(testData.Length, isReceived.Length);
            for (int i = 0; i < testData.Length; i++) {
                SendBeanIntDouble(epService, testData[i], 0D);
                Assert.AreEqual(isReceived[i], listener.GetAndClearIsInvoked(), "failed testing index " + i);
            }

            stmt.Events -= listener.Update;
            stmt.Dispose();
        }
    
        private void TryFilter(EPServiceProvider epService, string text, bool isReceived) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBeanString(epService, "a");
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendBeanString(epService, "b");
            Assert.AreEqual(isReceived, listener.GetAndClearIsInvoked());
            SendBeanString(epService, "c");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            stmt.Events -= listener.Update;
            stmt.Dispose();
        }
    
        private void Try3Fields(EPServiceProvider epService, string text,
                                int[] intPrimitive,
                                int?[] intBoxed,
                                double?[] doubleBoxed,
                                bool[] expected) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(intPrimitive.Length, doubleBoxed.Length);
            Assert.AreEqual(intBoxed.Length, doubleBoxed.Length);
            Assert.AreEqual(expected.Length, doubleBoxed.Length);
            for (int i = 0; i < intBoxed.Length; i++) {
                SendBeanIntIntDouble(epService, intPrimitive[i], intBoxed[i], doubleBoxed[i]);
                Assert.AreEqual(expected[i], listener.GetAndClearIsInvoked(), "failed at index " + i);
            }
    
            stmt.Stop();
        }
    
        private void RunAssertionFilterBooleanExpr(EPServiceProvider epService) {
            string text = "select * from " + typeof(SupportBean).FullName + "(2*IntBoxed=DoubleBoxed)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBeanIntDouble(epService, 20, 50d);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendBeanIntDouble(epService, 25, 50d);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            text = "select * from " + typeof(SupportBean).FullName + "(2*IntBoxed=DoubleBoxed, TheString='s')";
            stmt = epService.EPAdministrator.CreateEPL(text);
            var listenerTwo = new SupportUpdateListener();
            stmt.Events += listenerTwo.Update;
    
            SendBeanIntDoubleString(epService, 25, 50d, "s");
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
            SendBeanIntDoubleString(epService, 25, 50d, "x");
            Assert.IsFalse(listenerTwo.GetAndClearIsInvoked());
            epService.EPAdministrator.DestroyAllStatements();
    
            // test priority of equals and boolean
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib));
            epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 1 or IntPrimitive = 2)");
            epService.EPAdministrator.CreateEPL("select * from SupportBean(IntPrimitive = 3, SupportStaticMethodLib.AlwaysTrue({}))");
    
            SupportStaticMethodLib.Invocations.Clear();
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(SupportStaticMethodLib.Invocations.IsEmpty());
        }
    
        private void RunAssertionFilterWithEqualsSameCompare(EPServiceProvider epService) {
            string text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed=DoubleBoxed)";
            TryFilterWithEqualsSameCompare(epService, text, new[]{1, 1}, new double[]{1, 10}, new[]{true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed=IntBoxed and DoubleBoxed=DoubleBoxed)";
            TryFilterWithEqualsSameCompare(epService, text, new[]{1, 1}, new double[]{1, 10}, new[]{true, true});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed=IntBoxed)";
            TryFilterWithEqualsSameCompare(epService, text, new[]{1, 1}, new double[]{1, 10}, new[]{true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed in (IntBoxed))";
            TryFilterWithEqualsSameCompare(epService, text, new[]{1, 1}, new double[]{1, 10}, new[]{true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (DoubleBoxed))";
            TryFilterWithEqualsSameCompare(epService, text, new[]{1, 1}, new double[]{1, 10}, new[]{true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed not in (10, IntBoxed))";
            TryFilterWithEqualsSameCompare(epService, text, new[]{1, 1, 1}, new double[]{1, 5, 10}, new[]{false, true, false});
    
            text = "select * from " + typeof(SupportBean).FullName + "(DoubleBoxed in (IntBoxed:20))";
            TryFilterWithEqualsSameCompare(epService, text, new[]{0, 1, 2}, new double[]{1, 1, 1}, new[]{true, false, false});
        }
    
        private void TryFilterWithEqualsSameCompare(EPServiceProvider epService, string text, int[] intBoxed, double[] doubleBoxed, bool[] expected) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(intBoxed.Length, doubleBoxed.Length);
            Assert.AreEqual(expected.Length, doubleBoxed.Length);
            for (int i = 0; i < intBoxed.Length; i++) {
                SendBeanIntDouble(epService, intBoxed[i], doubleBoxed[i]);
                Assert.AreEqual(expected[i], listener.GetAndClearIsInvoked(), "failed at index " + i);
            }
    
            stmt.Stop();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select * from pattern [every a=" + typeof(SupportBean).FullName + " -> " +
                            "b=" + typeof(SupportMarketDataBean).FullName + "(sum(a.LongBoxed) = 2)]",
                    "Aggregation functions not allowed within filters [");
    
            TryInvalid(epService, "select * from pattern [every a=" + typeof(SupportBean).FullName + "(prior(1, a.LongBoxed))]",
                    "Failed to validate filter expression 'prior(1,a.LongBoxed)': Prior function cannot be used in this context [");
    
            TryInvalid(epService, "select * from pattern [every a=" + typeof(SupportBean).FullName + "(prev(1, a.LongBoxed))]",
                    "Failed to validate filter expression 'prev(1,a.LongBoxed)': Previous function cannot be used in this context [");
    
            TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + "(5 - 10)",
                    "Filter expression not returning a boolean value: '5-10' [");
    
            TryInvalid(epService, "select * from " + typeof(SupportBeanWithEnum).FullName + "(TheString=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_1)",
                    "Failed to validate filter expression 'TheString=ENUM_VALUE_1': Implicit conversion from datatype '" + Name.Clean<SupportEnum>() + "' to '" + Name.Clean<string>() + "' is not allowed [");
    
            TryInvalid(epService, "select * from " + typeof(SupportBeanWithEnum).FullName + "(supportEnum=A.b)",
                    "Failed to validate filter expression 'supportEnum=A.b': Failed to resolve property 'A.b' to a stream or nested property in a stream [");
    
            TryInvalid(epService, "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" +
                            typeof(SupportBean).FullName + "(DoubleBoxed not in (DoubleBoxed, x.IntBoxed, 9))]",
                    "Failed to validate filter expression 'DoubleBoxed not in (DoubleBoxed,x.I...(45 chars)': Failed to find a stream named 'x' (did you mean 'b'?) [");
    
            TryInvalid(epService, "select * from pattern [a=" + typeof(SupportBean).FullName
                            + " -> b=" + typeof(SupportBean).FullName + "(cluedo.IntPrimitive=a.IntPrimitive)"
                            + " -> c=" + typeof(SupportBean).FullName
                            + "]",
                    "Failed to validate filter expression 'cluedo.IntPrimitive=a.IntPrimitive': Failed to resolve property 'cluedo.IntPrimitive' to a stream or nested property in a stream [");
        }
    
        private void RunAssertionPatternWithExpr(EPServiceProvider epService) {
            string text = "select * from pattern [every a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportMarketDataBean).FullName + "(a.LongBoxed=volume*2)]";
            TryPatternWithExpr(epService, text);
    
            text = "select * from pattern [every a=" + typeof(SupportBean).FullName + " -> " +
                    "b=" + typeof(SupportMarketDataBean).FullName + "(volume*2=a.LongBoxed)]";
            TryPatternWithExpr(epService, text);
        }
    
        private void TryPatternWithExpr(EPServiceProvider epService, string text) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBeanLong(epService, 10L);
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 0L, ""));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 5L, ""));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanLong(epService, 0L);
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 0L, ""));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 1L, ""));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanLong(epService, 20L);
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 0, 10L, ""));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.RemoveAllEventHandlers();
            stmt.Dispose();
        }
    
        private void RunAssertionMathExpression(EPServiceProvider epService) {
            string text;
    
            text = "select * from " + typeof(SupportBean).FullName + "(IntBoxed*DoubleBoxed > 20)";
            TryArithmetic(epService, text);
    
            text = "select * from " + typeof(SupportBean).FullName + "(20 < IntBoxed*DoubleBoxed)";
            TryArithmetic(epService, text);
    
            text = "select * from " + typeof(SupportBean).FullName + "(20/IntBoxed < DoubleBoxed)";
            TryArithmetic(epService, text);
    
            text = "select * from " + typeof(SupportBean).FullName + "(20/IntBoxed/DoubleBoxed < 1)";
            TryArithmetic(epService, text);
        }
    
        private void RunAssertionShortAndByteArithmetic(EPServiceProvider epService) {
            string epl = "select " +
                    "ShortPrimitive + ShortBoxed as c0," +
                    "bytePrimitive + byteBoxed as c1, " +
                    "ShortPrimitive - ShortBoxed as c2," +
                    "bytePrimitive - byteBoxed as c3, " +
                    "ShortPrimitive * ShortBoxed as c4," +
                    "bytePrimitive * byteBoxed as c5, " +
                    "ShortPrimitive / ShortBoxed as c6," +
                    "bytePrimitive / byteBoxed as c7," +
                    "ShortPrimitive + LongPrimitive as c8," +
                    "bytePrimitive + LongPrimitive as c9 " +
                    "from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".Split(',');
    
            foreach (string field in fields) {
                Type expected = typeof(int?);
                if (field.Equals("c6") || field.Equals("c7")) {
                    expected = typeof(double?);
                }
                if (field.Equals("c8") || field.Equals("c9")) {
                    expected = typeof(long?);
                }
                Assert.AreEqual(expected, stmt.EventType.GetPropertyType(field), "for field " + field);
            }
    
            var bean = new SupportBean();
            bean.ShortPrimitive = (short) 5;
            bean.ShortBoxed = (short) 6;
            bean.BytePrimitive = (byte) 4;
            bean.ByteBoxed = (byte) 2;
            bean.LongPrimitive = 10;
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{11, 6, -1, 2, 30, 8, 5d / 6d, 2d, 15L, 14L});
    
            stmt.Dispose();
        }
    
        private void TryArithmetic(EPServiceProvider epService, string text) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBeanIntDouble(epService, 5, 5d);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanIntDouble(epService, 5, 4d);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanIntDouble(epService, 5, 4.001d);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void AssertSendReceive(EPServiceProvider epService, SupportUpdateListener listener, int[] ints, bool[] booleans) {
            for (int i = 0; i < ints.Length; i++) {
                epService.EPRuntime.SendEvent(new TestEvent(ints[i]));
                Assert.AreEqual(booleans[i], listener.GetAndClearIsInvoked());
            }
        }
    
        private void RunAssertionExpressionReversed(EPServiceProvider epService) {
            string expr = "select * from " + typeof(SupportBean).FullName + "(5 = IntBoxed)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBean(epService, "IntBoxed", 5);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void SendBeanIntDouble(EPServiceProvider epService, int? intBoxed, double? doubleBoxed) {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanIntDoubleString(EPServiceProvider epService, int? intBoxed, double? doubleBoxed, string theString) {
            var theEvent = new SupportBean();
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            theEvent.TheString = theString;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanIntIntDouble(EPServiceProvider epService, int intPrimitive, int? intBoxed, double? doubleBoxed) {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.IntBoxed = intBoxed;
            theEvent.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanLong(EPServiceProvider epService, long longBoxed) {
            var theEvent = new SupportBean();
            theEvent.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanString(EPServiceProvider epService, string theString) {
            var num = new SupportBean(theString, -1);
            epService.EPRuntime.SendEvent(num);
        }
    
        private void SendBean(EPServiceProvider epService, string fieldName, Object value) {
            var theEvent = new SupportBean();
            if (fieldName.Equals("TheString")) {
                theEvent.TheString = (string) value;
            } else if (fieldName.Equals("BoolPrimitive")) {
                theEvent.BoolPrimitive = (bool) value;
            } else if (fieldName.Equals("IntBoxed")) {
                theEvent.IntBoxed = (int?) value;
            } else if (fieldName.Equals("LongBoxed")) {
                theEvent.LongBoxed = (long) value;
            } else {
                throw new ArgumentException("field name not known");
            }
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void AssertListeners(SupportUpdateListener[] listeners, bool[] invoked) {
            for (int i = 0; i < invoked.Length; i++) {
                Assert.AreEqual(invoked[i], listeners[i].GetAndClearIsInvoked(), "Failed for listener " + i);
            }
        }

        public class MyEvent
        {
            public string Property1 => throw new EPRuntimeException("I should not have been called!");
            public string Property2 => "2";
        }

        public class TestEvent
        {
            private readonly int _x;

            public TestEvent(int x)
            {
                _x = x;
            }

            public int X => _x;

            public bool MyInstanceMethodAlwaysTrue()
            {
                return true;
            }

            public bool MyInstanceMethodEventBean(EventBean @event, string propertyName, int expected)
            {
                var value = @event.Get(propertyName);
                return value.Equals(expected);
            }
        }
    }
} // end of namespace
