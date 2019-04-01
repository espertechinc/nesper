///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLStreamExpr : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionChainedParameterized(epService);
            RunAssertionStreamFunction(epService);
            RunAssertionInstanceMethodOuterJoin(epService);
            RunAssertionInstanceMethodStatic(epService);
            RunAssertionStreamInstanceMethodAliased(epService);
            RunAssertionStreamInstanceMethodNoAlias(epService);
            RunAssertionJoinStreamSelectNoWildcard(epService);
            RunAssertionPatternStreamSelectNoWildcard(epService);
            RunAssertionInvalidSelect(epService);
        }
    
        private void RunAssertionChainedParameterized(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportChainTop", typeof(SupportChainTop));
    
            string subexpr = "top.GetChildOne(\"abc\",10).GetChildTwo(\"append\")";
            string epl = "select " +
                    subexpr +
                    " from SupportChainTop as top";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionChainedParam(epService, listener, stmt, subexpr);
    
            listener.Reset();
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
    
            TryAssertionChainedParam(epService, listener, stmt, subexpr);
    
            // test property hosts a method
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanStaticOuter", typeof(SupportBeanStaticOuter));
            stmt = epService.EPAdministrator.CreateEPL("select Inside.MyString as val," +
                    "inside.insideTwo.MyOtherString as val2 " +
                    "from SupportBeanStaticOuter");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanStaticOuter());
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("hello", result.Get("val"));
            Assert.AreEqual("hello2", result.Get("val2"));
    
            stmt.Dispose();
        }
    
        private void TryAssertionChainedParam(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt, string subexpr) {
    
            var rows = new object[][]{
                    new object[] {subexpr, typeof(SupportChainChildTwo)}
            };
            for (int i = 0; i < rows.Length; i++) {
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }
    
            epService.EPRuntime.SendEvent(new SupportChainTop());
            Object result = listener.AssertOneGetNewAndReset().Get(subexpr);
            Assert.AreEqual("abcappend", ((SupportChainChildTwo) result).GetText());
        }
    
        private void RunAssertionStreamFunction(EPServiceProvider epService) {
            string prefix = "select * from " + typeof(SupportMarketDataBean).FullName + " as s0 where " +
                    typeof(SupportStaticMethodLib).FullName;
            TryAssertionStreamFunction(epService, prefix + ".VolumeGreaterZero(s0)");
            TryAssertionStreamFunction(epService, prefix + ".VolumeGreaterZero(*)");
            TryAssertionStreamFunction(epService, prefix + ".VolumeGreaterZeroEventBean(s0)");
            TryAssertionStreamFunction(epService, prefix + ".VolumeGreaterZeroEventBean(*)");
        }
    
        private void TryAssertionStreamFunction(EPServiceProvider epService, string epl) {
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(epl);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ACME", 0, 0L, null));
            Assert.IsFalse(listenerOne.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ACME", 0, 100L, null));
            Assert.IsTrue(listenerOne.IsInvoked);
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionInstanceMethodOuterJoin(EPServiceProvider epService) {
            string textOne = "select symbol, s1.TheString as TheString from " +
                    typeof(SupportMarketDataBean).FullName + "#keepall as s0 " +
                    "left outer join " +
                    typeof(SupportBean).FullName + "#keepall as s1 on s0.symbol=s1.TheString";
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            epService.EPRuntime.SendEvent(eventA);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new string[]{"symbol", "TheString"}, new object[]{"ACME", null});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionInstanceMethodStatic(EPServiceProvider epService) {
            string textOne = "select symbol, s1.SimpleProperty as simpleprop, s1.MakeDefaultBean() as def from " +
                    typeof(SupportMarketDataBean).FullName + "#keepall as s0 " +
                    "left outer join " +
                    typeof(SupportBeanComplexProps).FullName + "#keepall as s1 on s0.symbol=s1.simpleProperty";
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            epService.EPRuntime.SendEvent(eventA);
            EventBean theEvent = listenerOne.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, new string[]{"symbol", "simpleprop"}, new object[]{"ACME", null});
            Assert.IsNull(theEvent.Get("def"));
    
            SupportBeanComplexProps eventComplexProps = SupportBeanComplexProps.MakeDefaultBean();
            eventComplexProps.SimpleProperty = "ACME";
            epService.EPRuntime.SendEvent(eventComplexProps);
            theEvent = listenerOne.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, new string[]{"symbol", "simpleprop"}, new object[]{"ACME", "ACME"});
            Assert.IsNotNull(theEvent.Get("def"));
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionStreamInstanceMethodAliased(EPServiceProvider epService) {
            string textOne = "select s0.Volume as volume, s0.Symbol as symbol, s0.GetPriceTimesVolume(2) as pvf from " +
                    typeof(SupportMarketDataBean).FullName + " as s0 ";
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            EventType type = stmtOne.EventType;
            Assert.AreEqual(3, type.PropertyNames.Length);
            Assert.AreEqual(typeof(long?), type.GetPropertyType("volume"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double), type.GetPropertyType("pvf"));
    
            var eventA = new SupportMarketDataBean("ACME", 4, 99L, null);
            epService.EPRuntime.SendEvent(eventA);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new string[]{"volume", "symbol", "pvf"}, new object[]{99L, "ACME", 4d * 99L * 2});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionStreamInstanceMethodNoAlias(EPServiceProvider epService) {
            string textOne = "select s0.Volume, s0.GetPriceTimesVolume(3) from " +
                    typeof(SupportMarketDataBean).FullName + " as s0 ";
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            EventType type = stmtOne.EventType;
            Assert.AreEqual(2, type.PropertyNames.Length);
            Assert.AreEqual(typeof(long?), type.GetPropertyType("s0.Volume"));
            Assert.AreEqual(typeof(double), type.GetPropertyType("s0.GetPriceTimesVolume(3)"));
    
            var eventA = new SupportMarketDataBean("ACME", 4, 2L, null);
            epService.EPRuntime.SendEvent(eventA);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), new string[]{"s0.Volume", "s0.GetPriceTimesVolume(3)"}, new object[]{2L, 4d * 2L * 3d});
    
            // try instance method that accepts EventBean
            epService.EPAdministrator.Configuration.AddEventType("MyTestEvent", typeof(MyTestEvent));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select " +
                    "s0.GetValueAsInt(s0, 'id') as c0," +
                    "s0.GetValueAsInt(*, 'id') as c1" +
                    " from MyTestEvent as s0");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new MyTestEvent(10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[]{10, 10});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionJoinStreamSelectNoWildcard(EPServiceProvider epService) {
            // try with alias
            string textOne = "select s0 as s0stream, s1 as s1stream from " +
                    typeof(SupportMarketDataBean).FullName + "#keepall as s0, " +
                    typeof(SupportBean).FullName + "#keepall as s1";
    
            // Attach listener to feed
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtOne.Text);
            Assert.AreEqual(textOne, model.ToEPL());
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            EventType type = stmtOne.EventType;
            Assert.AreEqual(2, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0stream"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1stream"));
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            epService.EPRuntime.SendEvent(eventA);
    
            var eventB = new SupportBean();
            epService.EPRuntime.SendEvent(eventB);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new string[]{"s0stream", "s1stream"}, new object[]{eventA, eventB});
    
            stmtOne.Dispose();
    
            // try no alias
            textOne = "select s0, s1 from " +
                    typeof(SupportMarketDataBean).FullName + "#keepall as s0, " +
                    typeof(SupportBean).FullName + "#keepall as s1";
    
            // Attach listener to feed
            stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            stmtOne.Events += listenerOne.Update;
    
            type = stmtOne.EventType;
            Assert.AreEqual(2, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));
    
            epService.EPRuntime.SendEvent(eventA);
            epService.EPRuntime.SendEvent(eventB);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new string[]{"s0", "s1"}, new object[]{eventA, eventB});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionPatternStreamSelectNoWildcard(EPServiceProvider epService) {
            // try with alias
            string textOne = "select * from pattern [every e1=" + typeof(SupportMarketDataBean).FullName + " -> e2=" +
                    typeof(SupportBean).FullName + "(" + typeof(SupportStaticMethodLib).FullName + ".CompareEvents(e1, e2))]";
    
            // Attach listener to feed
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            epService.EPRuntime.SendEvent(eventA);
    
            var eventB = new SupportBean("ACME", 1);
            epService.EPRuntime.SendEvent(eventB);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new string[]{"e1", "e2"}, new object[]{eventA, eventB});
    
            stmtOne.Dispose();
        }
    
        private void RunAssertionInvalidSelect(EPServiceProvider epService) {
            TryInvalid(epService, "select s0.GetString(1,2,3) from " + typeof(SupportBean).FullName + " as s0", null);
    
            TryInvalid(epService, "select s0.abc() from " + typeof(SupportBean).FullName + " as s0",
                    "Error starting statement: Failed to validate select-clause expression 's0.abc()': Failed to solve 'abc' to either a date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'abc': Could not find enumeration method, date-time method or instance method named 'abc' in class '" + typeof(SupportBean).FullName + "' taking no parameters [");
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            TryInvalid(epService, "select s.TheString from pattern [every [2] s=SupportBean] ee",
                    "Error starting statement: Failed to validate select-clause expression 's.TheString': Failed to resolve property 's.TheString' (property 's' is an indexed property and requires an index or enumeration method to access values) [select s.TheString from pattern [every [2] s=SupportBean] ee]");
        }
    
        private void TryInvalid(EPServiceProvider epService, string clause, string message) {
            try {
                epService.EPAdministrator.CreateEPL(clause);
                Assert.Fail();
            } catch (EPStatementException ex) {
                if (message != null) {
                    SupportMessageAssertUtil.AssertMessage(ex, message);
                }
            }
        }
    
        public class MyTestEvent {
    
            private int id;

            public int Id => id;

            public MyTestEvent(int id) {
                this.id = id;
            }
    
            public int GetValueAsInt(EventBean @event, string propertyName) {
                return @event.Get(propertyName).AsInt();
            }
        }
    }
} // end of namespace
