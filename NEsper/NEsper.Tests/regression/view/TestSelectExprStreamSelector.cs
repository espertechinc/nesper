///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestSelectExprStreamSelector  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp() {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(
                    SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestInvalidSelectWildcardProperty() {
            try {
                var stmtOneText = "select simpleProperty.* as a from "
                        + typeof(SupportBeanComplexProps).FullName + " as s0";
    
                _epService.EPAdministrator.CreateEPL(stmtOneText);
                Assert.Fail();
            } catch (Exception ex) {
                Assert.AreEqual(
                        "Error starting statement: The property wildcard syntax must be used without column name [select simpleProperty.* as a from com.espertech.esper.support.bean.SupportBeanComplexProps as s0]",
                        ex.Message);
            }
        }
    
        [Test]
        public void TestInsertTransposeNestedProperty() {
            var stmtOneText = "insert into StreamA select nested.* from "
                    + typeof(SupportBeanComplexProps).FullName + " as s0";
            var listenerOne = new SupportUpdateListener();
            var stmtOne = _epService.EPAdministrator.CreateEPL(
                    stmtOneText);
    
            stmtOne.Events += listenerOne.Update;
            Assert.AreEqual(
                    typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested),
                    stmtOne.EventType.UnderlyingType);

            var stmtTwoText = "select NestedValue from StreamA";
            var listenerTwo = new SupportUpdateListener();
            var stmtTwo = _epService.EPAdministrator.CreateEPL(
                    stmtTwoText);
    
            stmtTwo.Events += listenerTwo.Update;
            Assert.AreEqual(typeof(string),
                    stmtTwo.EventType.GetPropertyType("NestedValue"));
    
            _epService.EPRuntime.SendEvent(
                    SupportBeanComplexProps.MakeDefaultBean());

            Assert.AreEqual("NestedValue",
                    listenerOne.AssertOneGetNewAndReset().Get("NestedValue"));
            Assert.AreEqual("NestedValue",
                    listenerTwo.AssertOneGetNewAndReset().Get("NestedValue"));
        }
    
        [Test]
        public void TestInsertFromPattern() {
            var stmtOneText = "insert into streamA select a.* from pattern [every a="
                    + typeof(SupportBean).FullName + "]";
            var listenerOne = new SupportUpdateListener();
            var stmtOne = _epService.EPAdministrator.CreateEPL(
                    stmtOneText);
    
            stmtOne.Events += listenerOne.Update;
    
            var stmtTwoText = "insert into streamA select a.* from pattern [every a="
                    + typeof(SupportBean).FullName + " where timer:within(30 sec)]";
            var listenerTwo = new SupportUpdateListener();
            var stmtTwo = _epService.EPAdministrator.CreateEPL(
                    stmtTwoText);
    
            stmtTwo.Events += listenerTwo.Update;
    
            var eventType = stmtOne.EventType;
    
            Assert.AreEqual(typeof(SupportBean), eventType.UnderlyingType);
    
            Object theEvent = SendBeanEvent("E1", 10);
    
            Assert.AreSame(theEvent,
                    listenerTwo.AssertOneGetNewAndReset().Underlying);
    
            theEvent = SendBeanEvent("E2", 10);
            Assert.AreSame(theEvent,
                    listenerTwo.AssertOneGetNewAndReset().Underlying);
    
            var stmtThreeText = "insert into streamB select a.*, 'abc' as abc from pattern [every a="
                    + typeof(SupportBean).FullName + " where timer:within(30 sec)]";
            var stmtThree = _epService.EPAdministrator.CreateEPL(
                    stmtThreeText);
    
            Assert.AreEqual(typeof(Pair<string, object>), stmtThree.EventType.UnderlyingType);
            Assert.AreEqual(typeof(string),
                    stmtThree.EventType.GetPropertyType("abc"));
            Assert.AreEqual(typeof(string),
                    stmtThree.EventType.GetPropertyType("TheString"));
        }
    
        [Test]
        public void TestObjectModelJoinAlias()
        {
            var model = new EPStatementObjectModel();
    
            model.SelectClause = SelectClause.Create().AddStreamWildcard("s0").AddStreamWildcard("s1", "s1stream").AddWithAsProvidedName(
                "TheString", "sym");
            model.FromClause = FromClause.Create().Add(FilterStream.Create(typeof(SupportBean).FullName, "s0").AddView("win", "keepall")).Add(
                FilterStream.Create(typeof(SupportMarketDataBean).FullName, "s1").AddView(
                    "win", "keepall"));
    
            var selectTestView = _epService.EPAdministrator.Create(model);
    
            selectTestView.Events += _testListener.Update;
    
            var viewExpr = "select s0.*, s1.* as s1stream, TheString as sym from "
                    + typeof(SupportBean).FullName + ".win:keepall() as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
    
            Assert.AreEqual(viewExpr, model.ToEPL());
            var modelReverse = _epService.EPAdministrator.CompileEPL(
                    model.ToEPL());
    
            Assert.AreEqual(viewExpr, modelReverse.ToEPL());
    
            var type = selectTestView.EventType;
    
            Assert.AreEqual(typeof(SupportMarketDataBean),
                    type.GetPropertyType("s1stream"));
            Assert.AreEqual(typeof(Pair<string, object>), type.UnderlyingType);
    
            SendBeanEvent("E1");
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object theEvent = SendMarketEvent("E1");
            var outevent = _testListener.AssertOneGetNewAndReset();
    
            Assert.AreSame(theEvent, outevent.Get("s1stream"));
        }
    
        [Test]
        public void TestNoJoinWildcardNoAlias() {
            var viewExpr = "select *, win.* from " + typeof(SupportBean).FullName
                    + ".win:length(3) as win";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.IsTrue(type.PropertyNames.Length > 15);
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);
    
            Object theEvent = SendBeanEvent("E1", 16);
    
            Assert.AreSame(theEvent,
                    _testListener.AssertOneGetNewAndReset().Underlying);
        }
    
        [Test]
        public void TestJoinWildcardNoAlias() {
            var viewExpr = "select *, s1.* from " + typeof(SupportBean).FullName
                    + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.AreEqual(7, type.PropertyNames.Length);
            Assert.AreEqual(typeof(long?), type.GetPropertyType("volume"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
            Assert.AreEqual(typeof(Pair<string, object>), type.UnderlyingType);
    
            Object eventOne = SendBeanEvent("E1", 13);
    
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object eventTwo = SendMarketEvent("E2");
            var fields = new String[]
            {
                "s0", "s1", "symbol", "volume"
            };
            var received = _testListener.AssertOneGetNewAndReset();
    
            EPAssertionUtil.AssertProps(received, fields, new Object[]
                    {
                eventOne, eventTwo, "E2", 0L
            }
                    );
        }
    
        [Test]
        public void TestNoJoinWildcardWithAlias() {
            var viewExpr = "select *, win.* as s0 from "
                    + typeof(SupportBean).FullName + ".win:length(3) as win";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;
    
            Assert.IsTrue(type.PropertyNames.Length > 15);
            Assert.AreEqual(typeof(Pair<string, object>), type.UnderlyingType);
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
    
            Object theEvent = SendBeanEvent("E1", 15);
            var fields = new String[]
                    {
                "TheString", "IntPrimitive", "s0"
            }
                    ;
    
            EPAssertionUtil.AssertProps(_testListener.AssertOneGetNewAndReset(),
                    fields, new Object[]
                    {
                "E1", 15, theEvent
            }
                    );
        }
    
        [Test]
        public void TestJoinWildcardWithAlias() {
            var viewExpr = "select *, s1.* as s1stream, s0.* as s0stream from "
                    + typeof(SupportBean).FullName + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.AreEqual(4, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportMarketDataBean),
                    type.GetPropertyType("s1stream"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);
    
            Object eventOne = SendBeanEvent("E1", 13);
    
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object eventTwo = SendMarketEvent("E2");
            var fields = new String[]
                    {
                "s0", "s1", "s0stream", "s1stream"
            }
                    ;
            var received = _testListener.AssertOneGetNewAndReset();
    
            EPAssertionUtil.AssertProps(received, fields, new Object[]
                    {
                eventOne, eventTwo, eventOne, eventTwo
            }
                    );
        }
    
        [Test]
        public void TestNoJoinWithAliasWithProperties() {
            var viewExpr = "select TheString.* as s0, IntPrimitive as a, TheString.* as s1, IntPrimitive as b from "
                    + typeof(SupportBean).FullName + ".win:length(3) as TheString";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.AreEqual(4, type.PropertyNames.Length);
            Assert.AreEqual(typeof(Map), type.UnderlyingType);
            Assert.AreEqual(typeof(int?), type.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));
    
            Object theEvent = SendBeanEvent("E1", 12);
            var fields = new String[]
                    {
                "s0", "s1", "a", "b"
            }
                    ;
    
            EPAssertionUtil.AssertProps(_testListener.AssertOneGetNewAndReset(),
                    fields, new Object[]
                    {
                theEvent, theEvent, 12, 12
            }
                    );
        }
    
        [Test]
        public void TestJoinWithAliasWithProperties() {
            var viewExpr = "select IntPrimitive, s1.* as s1stream, TheString, symbol as sym, s0.* as s0stream from "
                    + typeof(SupportBean).FullName + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.AreEqual(5, type.PropertyNames.Length);
            Assert.AreEqual(typeof(int?), type.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(SupportMarketDataBean),
                    type.GetPropertyType("s1stream"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("sym"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);
    
            Object eventOne = SendBeanEvent("E1", 13);
    
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object eventTwo = SendMarketEvent("E2");
            var fields = new String[]
                    {
                "IntPrimitive", "sym", "TheString", "s0stream", "s1stream"
            }
                    ;
            var received = _testListener.AssertOneGetNewAndReset();
    
            EPAssertionUtil.AssertProps(received, fields, new Object[]
                    {
                13, "E2", "E1", eventOne, eventTwo
            }
                    );
            var theEvent = (EventBean) ((Map) received.Underlying).Get(
                    "s0stream");
    
            Assert.AreSame(eventOne, theEvent.Underlying);
        }
    
        [Test]
        public void TestNoJoinNoAliasWithProperties() {
            var viewExpr = "select IntPrimitive as a, string.*, IntPrimitive as b from "
                    + typeof(SupportBean).FullName + ".win:length(3) as string";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.AreEqual(22, type.PropertyNames.Length);
            Assert.AreEqual(typeof(Pair<string, object>), type.UnderlyingType);
            Assert.AreEqual(typeof(int?), type.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), type.GetPropertyType("b"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
    
            SendBeanEvent("E1", 10);
            var fields = new String[]
                    {
                "a", "TheString", "IntPrimitive", "b"
            }
                    ;
    
            EPAssertionUtil.AssertProps(_testListener.AssertOneGetNewAndReset(),
                    fields, new Object[]
                    {
                10, "E1", 10, 10
            }
                    );
        }
    
        [Test]
        public void TestJoinNoAliasWithProperties() {
            var viewExpr = "select IntPrimitive, s1.*, symbol as sym from "
                    + typeof(SupportBean).FullName + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.AreEqual(7, type.PropertyNames.Length);
            Assert.AreEqual(typeof(int?), type.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(Pair<string, object>), type.UnderlyingType);
    
            SendBeanEvent("E1", 11);
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object theEvent = SendMarketEvent("E1");
            var fields = new String[]
                    {
                "IntPrimitive", "sym", "symbol"
            }
                    ;
            var received = _testListener.AssertOneGetNewAndReset();
    
            EPAssertionUtil.AssertProps(received, fields, new Object[]
                    {
                11, "E1", "E1"
            }
                    );
            Assert.AreSame(theEvent, ((Pair<object,IDictionary<string,object>>) received.Underlying).First);
        }
    
        [Test]
        public void TestAloneNoJoinNoAlias() {
            var viewExpr = "select TheString.* from "
                    + typeof(SupportBean).FullName + ".win:length(3) as TheString";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.IsTrue(type.PropertyNames.Length > 10);
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);
    
            Object theEvent = SendBeanEvent("E1");
    
            Assert.AreSame(theEvent,
                    _testListener.AssertOneGetNewAndReset().Underlying);
        }
    
        [Test]
        public void TestAloneNoJoinAlias() {
            var viewExpr = "select TheString.* as s0 from "
                    + typeof(SupportBean).FullName + ".win:length(3) as TheString";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;

            Assert.AreEqual(1, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);
    
            Object theEvent = SendBeanEvent("E1");
    
            Assert.AreSame(theEvent, _testListener.AssertOneGetNewAndReset().Get("s0"));
        }
    
        [Test]
        public void TestAloneJoinAlias() {
            var viewExpr = "select s1.* as s1 from "
                    + typeof(SupportBean).FullName + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;
    
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);
    
            SendBeanEvent("E1");
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object theEvent = SendMarketEvent("E1");
    
            Assert.AreSame(theEvent, _testListener.AssertOneGetNewAndReset().Get("s1"));
    
            selectTestView.Dispose();
    
            // reverse streams
            viewExpr = "select s0.* as szero from " + typeof(SupportBean).FullName
                    + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            type = selectTestView.EventType;
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("szero"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);
    
            SendMarketEvent("E1");
            Assert.IsFalse(_testListener.IsInvoked);
    
            theEvent = SendBeanEvent("E1");
            Assert.AreSame(theEvent, _testListener.AssertOneGetNewAndReset().Get("szero"));
        }
    
        [Test]
        public void TestAloneJoinNoAlias() {
            var viewExpr = "select s1.* from " + typeof(SupportBean).FullName
                    + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            var selectTestView = _epService.EPAdministrator.CreateEPL(
                    viewExpr);
    
            selectTestView.Events += _testListener.Update;
    
            var type = selectTestView.EventType;
    
            Assert.AreEqual(typeof(long?), type.GetPropertyType("volume"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.UnderlyingType);
    
            SendBeanEvent("E1");
            Assert.IsFalse(_testListener.IsInvoked);
    
            Object theEvent = SendMarketEvent("E1");
    
            Assert.AreSame(theEvent,
                    _testListener.AssertOneGetNewAndReset().Underlying);
    
            selectTestView.Dispose();
    
            // reverse streams
            viewExpr = "select s0.* from " + typeof(SupportBean).FullName
                    + ".win:length(3) as s0, "
                    + typeof(SupportMarketDataBean).FullName + ".win:keepall() as s1";
            selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _testListener.Update;
    
            type = selectTestView.EventType;
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);
    
            SendMarketEvent("E1");
            Assert.IsFalse(_testListener.IsInvoked);
    
            theEvent = SendBeanEvent("E1");
            Assert.AreSame(theEvent,
                    _testListener.AssertOneGetNewAndReset().Underlying);
        }
    
        [Test]
        public void TestInvalidSelect() {
            TryInvalid(
                    "select TheString.* as TheString, TheString from "
                            + typeof(SupportBean).FullName
                            + ".win:length(3) as TheString",
                            "Error starting statement: Column name 'TheString' appears more then once in select clause [select TheString.* as TheString, TheString from com.espertech.esper.support.bean.SupportBean.win:length(3) as TheString]");
    
            TryInvalid(
                    "select s1.* as abc from " + typeof(SupportBean).FullName
                    + ".win:length(3) as s0",
                    "Error starting statement: Stream selector 's1.*' does not match any stream name in the from clause [select s1.* as abc from com.espertech.esper.support.bean.SupportBean.win:length(3) as s0]");
    
            TryInvalid(
                    "select s0.* as abc, s0.* as abc from "
                            + typeof(SupportBean).FullName + ".win:length(3) as s0",
                            "Error starting statement: Column name 'abc' appears more then once in select clause [select s0.* as abc, s0.* as abc from com.espertech.esper.support.bean.SupportBean.win:length(3) as s0]");
    
            TryInvalid(
                    "select s0.*, s1.* from " + typeof(SupportBean).FullName
                    + ".win:keepall() as s0, " + typeof(SupportBean).FullName
                    + ".win:keepall() as s1",
                    "Error starting statement: A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation [select s0.*, s1.* from com.espertech.esper.support.bean.SupportBean.win:keepall() as s0, com.espertech.esper.support.bean.SupportBean.win:keepall() as s1]");
        }
    
        private void TryInvalid(String clause, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(clause);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private SupportBean SendBeanEvent(String s) {
            var bean = new SupportBean();
    
            bean.TheString = s;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendBeanEvent(String s, int intPrimitive) {
            var bean = new SupportBean();
    
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportMarketDataBean SendMarketEvent(String s) {
            var bean = new SupportMarketDataBean(s, 0d, 0L, "");
    
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}
