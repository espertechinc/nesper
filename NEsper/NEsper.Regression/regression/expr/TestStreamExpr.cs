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
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestStreamExpr 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestChainedParameterized()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportChainTop", typeof(SupportChainTop));
    
            var subexpr ="top.GetChildOne(\"abc\",10).GetChildTwo(\"append\")";
            var epl = "select " +
                    subexpr +
                    " from SupportChainTop as top";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            RunAssertionChainedParam(stmt, subexpr);
    
            _listener.Reset();
            stmt.Dispose();
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            RunAssertionChainedParam(stmt, subexpr);
    
            // test property hosts a method
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanStaticOuter", typeof(SupportBeanStaticOuter));
            stmt = _epService.EPAdministrator.CreateEPL("select inside.MyString as val," +
                    "inside.insideTwo.MyOtherString as val2 " +
                    "from SupportBeanStaticOuter");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanStaticOuter());
            var result = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("hello", result.Get("val"));
            Assert.AreEqual("hello2", result.Get("val2"));
        }
    
        private void RunAssertionChainedParam(EPStatement stmt, String subexpr) {
    
            Object[][] rows =
            {
                new Object[] {subexpr, typeof(SupportChainChildTwo)}
            };
            for (var i = 0; i < rows.Length; i++) {
                var prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }
    
            _epService.EPRuntime.SendEvent(new SupportChainTop());
            var result = _listener.AssertOneGetNewAndReset().Get(subexpr);
            Assert.AreEqual("abcappend", ((SupportChainChildTwo)result).GetText());
        }
    
        [Test]
        public void TestStreamFunction()
        {
            var prefix = "select * from " + typeof(SupportMarketDataBean).FullName + " as s0 where " +
                    typeof(SupportStaticMethodLib).FullName;
            RunAssertionStreamFunction(prefix + ".VolumeGreaterZero(s0)");
            RunAssertionStreamFunction(prefix + ".VolumeGreaterZero(*)");
            RunAssertionStreamFunction(prefix + ".VolumeGreaterZeroEventBean(s0)");
            RunAssertionStreamFunction(prefix + ".VolumeGreaterZeroEventBean(*)");
        }
    
        private void RunAssertionStreamFunction(String epl) {
    
            var stmtOne = _epService.EPAdministrator.CreateEPL(epl);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ACME", 0, 0L, null));
            Assert.IsFalse(listenerOne.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ACME", 0, 100L, null));
            Assert.IsTrue(listenerOne.IsInvoked);
    
            stmtOne.Dispose();
        }
    
        [Test]
        public void TestInstanceMethodOuterJoin()
        {
            var textOne = "select symbol, s1.TheString as TheString from " +
                                typeof(SupportMarketDataBean).FullName + "#keepall as s0 " +
                                "left outer join " +
                                typeof(SupportBean).FullName + "#keepall as s1 on s0.symbol=s1.TheString";
    
            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            _epService.EPRuntime.SendEvent(eventA);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new String[]{"symbol", "TheString"}, new Object[]{"ACME", null});
        }
    
        [Test]
        public void TestInstanceMethodStatic()
        {
            var textOne = "select symbol, s1.SimpleProperty as simpleprop, s1.MakeDefaultBean() as def from " +
                                typeof(SupportMarketDataBean).FullName + "#keepall as s0 " +
                                "left outer join " +
                                typeof(SupportBeanComplexProps).FullName + "#keepall as s1 on s0.symbol=s1.simpleProperty";
    
            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            _epService.EPRuntime.SendEvent(eventA);
            var theEvent = listenerOne.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, new String[]{"symbol", "simpleprop"}, new Object[]{"ACME", null});
            Assert.IsNull(theEvent.Get("def"));
    
            var eventComplexProps = SupportBeanComplexProps.MakeDefaultBean();
            eventComplexProps.SimpleProperty = "ACME";
            _epService.EPRuntime.SendEvent(eventComplexProps);
            theEvent = listenerOne.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, new String[]{"symbol", "simpleprop"}, new Object[]{"ACME", "ACME"});
            Assert.NotNull(theEvent.Get("def"));
        }
    
        [Test]
        public void TestStreamInstanceMethodAliased()
        {
            var textOne = "select s0.Volume as volume, s0.Symbol as symbol, s0.GetPriceTimesVolume(2) as pvf from " +
                                typeof(SupportMarketDataBean).FullName + " as s0 ";
    
            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var type = stmtOne.EventType;
            Assert.AreEqual(3, type.PropertyNames.Length);
            Assert.AreEqual(typeof(long?), type.GetPropertyType("volume"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double), type.GetPropertyType("pvf"));
    
            var eventA = new SupportMarketDataBean("ACME", 4, 99L, null);
            _epService.EPRuntime.SendEvent(eventA);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new String[]{"volume", "symbol", "pvf"}, new Object[]{99L, "ACME", 4d * 99L * 2});
        }
    
        [Test]
        public void TestStreamInstanceMethodNoAlias()
        {
            var textOne = "select s0.Volume, s0.GetPriceTimesVolume(3) from " +
                                typeof(SupportMarketDataBean).FullName + " as s0 ";
    
            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            var type = stmtOne.EventType;
            Assert.AreEqual(2, type.PropertyNames.Length);
            Assert.AreEqual(typeof(long?), type.GetPropertyType("s0.Volume"));
            Assert.AreEqual(typeof(double), type.GetPropertyType("s0.GetPriceTimesVolume(3)"));
    
            var eventA = new SupportMarketDataBean("ACME", 4, 2L, null);
            _epService.EPRuntime.SendEvent(eventA);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), new String[]{"s0.Volume", "s0.GetPriceTimesVolume(3)"}, new Object[]{2L, 4d * 2L * 3d});
    
            // try instance method that accepts EventBean
            _epService.EPAdministrator.Configuration.AddEventType("MyTestEvent", typeof(MyTestEvent));
            var stmt = _epService.EPAdministrator.CreateEPL("select " +
                    "s0.GetValueAsInt(s0, 'id') as c0," +
                    "s0.GetValueAsInt(*, 'id') as c1" +
                    " from MyTestEvent as s0");
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new MyTestEvent(10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[] {10, 10});
        }
    
        [Test]
        public void TestJoinStreamSelectNoWildcard()
        {
            // try with alias
            var textOne = "select s0 as s0stream, s1 as s1stream from " +
                                typeof(SupportMarketDataBean).FullName + "#keepall as s0, " +
                                typeof(SupportBean).FullName + "#keepall as s1";
    
            // Attach listener to feed
            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            var model = _epService.EPAdministrator.CompileEPL(stmtOne.Text);
            Assert.AreEqual(textOne, model.ToEPL());
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var type = stmtOne.EventType;
            Assert.AreEqual(2, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0stream"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1stream"));
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            _epService.EPRuntime.SendEvent(eventA);
    
            var eventB = new SupportBean();
            _epService.EPRuntime.SendEvent(eventB);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new String[]{"s0stream", "s1stream"}, new Object[]{eventA, eventB});
    
            stmtOne.Dispose();
    
            // try no alias
            textOne = "select s0, s1 from " +
                                typeof(SupportMarketDataBean).FullName + "#keepall as s0, " +
                                typeof(SupportBean).FullName + "#keepall as s1";
    
            // Attach listener to feed
            stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            stmtOne.Events += listenerOne.Update;
    
            type = stmtOne.EventType;
            Assert.AreEqual(2, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));
    
            _epService.EPRuntime.SendEvent(eventA);
            _epService.EPRuntime.SendEvent(eventB);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new String[]{"s0", "s1"}, new Object[]{eventA, eventB});
        }
    
        [Test]
        public void TestPatternStreamSelectNoWildcard()
        {
            // try with alias
            var textOne = "select * from pattern [every e1=" + typeof(SupportMarketDataBean).FullName + " -> e2=" +
                                typeof(SupportBean).FullName + "(" + typeof(SupportStaticMethodLib).FullName + ".CompareEvents(e1, e2))]";
    
            // Attach listener to feed
            var stmtOne = _epService.EPAdministrator.CreateEPL(textOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            var eventA = new SupportMarketDataBean("ACME", 0, 0L, null);
            _epService.EPRuntime.SendEvent(eventA);
    
            var eventB = new SupportBean("ACME", 1);
            _epService.EPRuntime.SendEvent(eventB);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), new String[]{"e1", "e2"}, new Object[]{eventA, eventB});
    
            stmtOne.Dispose();
        }
    
        [Test]
        public void TestInvalidSelect()
        {
            TryInvalid("select s0.GetString(1,2,3) from " + typeof(SupportBean).FullName + " as s0", null);
    
            TryInvalid("select s0.abc() from " + typeof(SupportBean).FullName + " as s0",
                       "Error starting statement: Failed to validate select-clause expression 's0.abc()': Failed to solve 'abc' to either a date-time or enumeration method, an event property or a method on the event underlying object: Failed to resolve method 'abc': Could not find enumeration method, date-time method or instance method named 'abc' in class '" + Name.Of<SupportBean>() + "' taking no parameters [select s0.abc() from " + typeof(SupportBean).FullName+ " as s0]");

            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            TryInvalid("select s.TheString from pattern [every [2] s=SupportBean] ee",
                "Error starting statement: Failed to validate select-clause expression 's.TheString': Failed to resolve property 's.TheString' (property 's' is an indexed property and requires an index or enumeration method to access values) [select s.TheString from pattern [every [2] s=SupportBean] ee]");
 
        }

        private void TryInvalid(String clause, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(clause);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                if (message != null)
                {
                    SupportMessageAssertUtil.AssertMessage(ex, message);
                }
            }
        }

        internal class MyTestEvent
        {
            internal MyTestEvent(int id)
            {
                Id = id;
            }

            public int Id { get; private set; }

            public int GetValueAsInt(EventBean @event, String propertyName) {
                return @event.Get(propertyName).AsInt();
            }
        }
    }
} // end of namespace
