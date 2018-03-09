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
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    using Map = IDictionary<string, object>;

    public class ExecEPLSelectExprStreamSelector : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionInvalidSelectWildcardProperty(epService);
            RunAssertionInsertTransposeNestedProperty(epService);
            RunAssertionInsertFromPattern(epService);
            RunAssertionObjectModelJoinAlias(epService);
            RunAssertionNoJoinWildcardNoAlias(epService);
            RunAssertionJoinWildcardNoAlias(epService);
            RunAssertionNoJoinWildcardWithAlias(epService);
            RunAssertionJoinWildcardWithAlias(epService);
            RunAssertionNoJoinWithAliasWithProperties(epService);
            RunAssertionJoinWithAliasWithProperties(epService);
            RunAssertionNoJoinNoAliasWithProperties(epService);
            RunAssertionJoinNoAliasWithProperties(epService);
            RunAssertionAloneNoJoinNoAlias(epService);
            RunAssertionAloneNoJoinAlias(epService);
            RunAssertionAloneJoinAlias(epService);
            RunAssertionAloneJoinNoAlias(epService);
            RunAssertionInvalidSelect(epService);
        }

        private void RunAssertionInvalidSelectWildcardProperty(EPServiceProvider epService)
        {
            try
            {
                string stmtOneText = "select simpleProperty.* as a from " + typeof(SupportBeanComplexProps).FullName +
                                     " as s0";
                epService.EPAdministrator.CreateEPL(stmtOneText);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                SupportMessageAssertUtil.AssertMessage(
                    ex, "Error starting statement: The property wildcard syntax must be used without column name");
            }
        }

        private void RunAssertionInsertTransposeNestedProperty(EPServiceProvider epService)
        {
            string stmtOneText = "insert into StreamA select Nested.* from " +
                                 typeof(SupportBeanComplexProps).FullName + " as s0";
            var listenerOne = new SupportUpdateListener();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtOneText);
            stmtOne.Events += listenerOne.Update;
            Assert.AreEqual(
                typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested), stmtOne.EventType.UnderlyingType);

            string stmtTwoText = "select NestedValue from StreamA";
            var listenerTwo = new SupportUpdateListener();
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTwoText);
            stmtTwo.Events += listenerTwo.Update;
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("NestedValue"));

            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());

            Assert.AreEqual("NestedValue", listenerOne.AssertOneGetNewAndReset().Get("NestedValue"));
            Assert.AreEqual("NestedValue", listenerTwo.AssertOneGetNewAndReset().Get("NestedValue"));

            stmtOne.Dispose();
            stmtTwo.Dispose();
        }

        private void RunAssertionInsertFromPattern(EPServiceProvider epService)
        {
            string stmtOneText = "insert into streamA select a.* from pattern [every a=" +
                                 typeof(SupportBean).FullName + "]";
            var listenerOne = new SupportUpdateListener();
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtOneText);
            stmtOne.Events += listenerOne.Update;

            string stmtTwoText = "insert into streamA select a.* from pattern [every a=" +
                                 typeof(SupportBean).FullName + " where timer:within(30 sec)]";
            var listenerTwo = new SupportUpdateListener();
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTwoText);
            stmtTwo.Events += listenerTwo.Update;

            EventType eventType = stmtOne.EventType;
            Assert.AreEqual(typeof(SupportBean), eventType.UnderlyingType);

            object theEvent = SendBeanEvent(epService, "E1", 10);
            Assert.AreSame(theEvent, listenerTwo.AssertOneGetNewAndReset().Underlying);

            theEvent = SendBeanEvent(epService, "E2", 10);
            Assert.AreSame(theEvent, listenerTwo.AssertOneGetNewAndReset().Underlying);

            string stmtThreeText = "insert into streamB select a.*, 'abc' as abc from pattern [every a=" +
                                   typeof(SupportBean).FullName + " where timer:within(30 sec)]";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtThreeText);
            Assert.AreEqual(typeof(Pair<object, Map>), stmtThree.EventType.UnderlyingType);
            Assert.AreEqual(typeof(string), stmtThree.EventType.GetPropertyType("abc"));
            Assert.AreEqual(typeof(string), stmtThree.EventType.GetPropertyType("TheString"));

            stmtOne.Dispose();
            stmtTwo.Dispose();
        }

        private void RunAssertionObjectModelJoinAlias(EPServiceProvider epService)
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .AddStreamWildcard("s0")
                .AddStreamWildcard("s1", "s1stream")
                .AddWithAsProvidedName("TheString", "sym");
            model.FromClause = FromClause.Create()
                .Add(FilterStream.Create(typeof(SupportBean).FullName, "s0").AddView("keepall"))
                .Add(FilterStream.Create(typeof(SupportMarketDataBean).FullName, "s1").AddView("keepall"));

            EPStatement stmt = epService.EPAdministrator.Create(model);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            string epl = "select s0.*, s1.* as s1stream, TheString as sym from " + typeof(SupportBean).FullName +
                         "#keepall as s0, " +
                         typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            Assert.AreEqual(epl, model.ToEPL());
            EPStatementObjectModel modelReverse = epService.EPAdministrator.CompileEPL(model.ToEPL());
            Assert.AreEqual(epl, modelReverse.ToEPL());

            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
            Assert.AreEqual(typeof(Pair<object, Map>), type.UnderlyingType);

            SendBeanEvent(epService, "E1");
            Assert.IsFalse(testListener.IsInvoked);

            object theEvent = SendMarketEvent(epService, "E1");
            EventBean outevent = testListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, outevent.Get("s1stream"));

            stmt.Dispose();
        }

        private void RunAssertionNoJoinWildcardNoAlias(EPServiceProvider epService)
        {
            string epl = "select *, win.* from " + typeof(SupportBean).FullName + "#length(3) as win";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.IsTrue(type.PropertyNames.Length > 15);
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

            object theEvent = SendBeanEvent(epService, "E1", 16);
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Underlying);

            stmt.Dispose();
        }

        private void RunAssertionJoinWildcardNoAlias(EPServiceProvider epService)
        {
            string epl = "select *, s1.* from " + typeof(SupportBean).FullName + "#length(3) as s0, " +
                         typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(7, type.PropertyNames.Length);
            Assert.AreEqual(typeof(long?), type.GetPropertyType("volume"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
            Assert.AreEqual(typeof(Pair<object, Map>), type.UnderlyingType);

            object eventOne = SendBeanEvent(epService, "E1", 13);
            Assert.IsFalse(testListener.IsInvoked);

            object eventTwo = SendMarketEvent(epService, "E2");
            var fields = new string[] {"s0", "s1", "symbol", "volume"};
            EventBean received = testListener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[] {eventOne, eventTwo, "E2", 0L});

            stmt.Dispose();
        }

        private void RunAssertionNoJoinWildcardWithAlias(EPServiceProvider epService)
        {
            string epl = "select *, win.* as s0 from " + typeof(SupportBean).FullName + "#length(3) as win";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.IsTrue(type.PropertyNames.Length > 15);
            Assert.AreEqual(typeof(Pair<object, Map>), type.UnderlyingType);
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));

            object theEvent = SendBeanEvent(epService, "E1", 15);
            var fields = new string[] {"TheString", "IntPrimitive", "s0"};
            EPAssertionUtil.AssertProps(
                testListener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 15, theEvent});

            stmt.Dispose();
        }

        private void RunAssertionJoinWildcardWithAlias(EPServiceProvider epService)
        {
            string epl = "select *, s1.* as s1stream, s0.* as s0stream from " + typeof(SupportBean).FullName +
                         "#length(3) as s0, " +
                         typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(4, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);

            object eventOne = SendBeanEvent(epService, "E1", 13);
            Assert.IsFalse(testListener.IsInvoked);

            object eventTwo = SendMarketEvent(epService, "E2");
            var fields = new string[] {"s0", "s1", "s0stream", "s1stream"};
            EventBean received = testListener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[] {eventOne, eventTwo, eventOne, eventTwo});

            stmt.Dispose();
        }

        private void RunAssertionNoJoinWithAliasWithProperties(EPServiceProvider epService)
        {
            string epl = "select TheString.* as s0, IntPrimitive as a, TheString.* as s1, IntPrimitive as b from " +
                         typeof(SupportBean).FullName + "#length(3) as TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(4, type.PropertyNames.Length);
            Assert.AreEqual(typeof(Map), type.UnderlyingType);
            Assert.AreEqual(typeof(int), type.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), type.GetPropertyType("b"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s1"));

            object theEvent = SendBeanEvent(epService, "E1", 12);
            var fields = new string[] {"s0", "s1", "a", "b"};
            EPAssertionUtil.AssertProps(
                testListener.AssertOneGetNewAndReset(), fields, new object[] {theEvent, theEvent, 12, 12});

            stmt.Dispose();
        }

        private void RunAssertionJoinWithAliasWithProperties(EPServiceProvider epService)
        {
            string epl = "select IntPrimitive, s1.* as s1stream, TheString, symbol as sym, s0.* as s0stream from " +
                         typeof(SupportBean).FullName + "#length(3) as s0, " +
                         typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(5, type.PropertyNames.Length);
            Assert.AreEqual(typeof(int), type.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1stream"));
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0stream"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("sym"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);

            object eventOne = SendBeanEvent(epService, "E1", 13);
            Assert.IsFalse(testListener.IsInvoked);

            object eventTwo = SendMarketEvent(epService, "E2");
            var fields = new string[] {"IntPrimitive", "sym", "TheString", "s0stream", "s1stream"};
            EventBean received = testListener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[] {13, "E2", "E1", eventOne, eventTwo});
            EventBean theEvent = (EventBean) ((Map) received.Underlying).Get("s0stream");
            Assert.AreSame(eventOne, theEvent.Underlying);

            stmt.Dispose();
        }

        private void RunAssertionNoJoinNoAliasWithProperties(EPServiceProvider epService)
        {
            string epl = "select IntPrimitive as a, string.*, IntPrimitive as b from " + typeof(SupportBean).FullName +
                         "#length(3) as string";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(22, type.PropertyNames.Length);
            Assert.AreEqual(typeof(Pair<object, Map>), type.UnderlyingType);
            Assert.AreEqual(typeof(int), type.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), type.GetPropertyType("b"));
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));

            SendBeanEvent(epService, "E1", 10);
            var fields = new string[] {"a", "TheString", "IntPrimitive", "b"};
            EPAssertionUtil.AssertProps(
                testListener.AssertOneGetNewAndReset(), fields, new object[] {10, "E1", 10, 10});

            stmt.Dispose();
        }

        private void RunAssertionJoinNoAliasWithProperties(EPServiceProvider epService)
        {
            string epl = "select IntPrimitive, s1.*, symbol as sym from " + typeof(SupportBean).FullName +
                         "#length(3) as s0, " +
                         typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(7, type.PropertyNames.Length);
            Assert.AreEqual(typeof(int), type.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(Pair<object, Map>), type.UnderlyingType);

            SendBeanEvent(epService, "E1", 11);
            Assert.IsFalse(testListener.IsInvoked);

            object theEvent = SendMarketEvent(epService, "E1");
            var fields = new string[] {"IntPrimitive", "sym", "symbol"};
            EventBean received = testListener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[] {11, "E1", "E1"});
            Assert.AreSame(theEvent, ((Pair<object, Map>) received.Underlying).First);

            stmt.Dispose();
        }

        private void RunAssertionAloneNoJoinNoAlias(EPServiceProvider epService)
        {
            string epl = "select TheString.* from " + typeof(SupportBean).FullName + "#length(3) as TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.IsTrue(type.PropertyNames.Length > 10);
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

            object theEvent = SendBeanEvent(epService, "E1");
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Underlying);

            stmt.Dispose();
        }

        private void RunAssertionAloneNoJoinAlias(EPServiceProvider epService)
        {
            string epl = "select TheString.* as s0 from " + typeof(SupportBean).FullName + "#length(3) as TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(1, type.PropertyNames.Length);
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);

            object theEvent = SendBeanEvent(epService, "E1");
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Get("s0"));

            stmt.Dispose();
        }

        private void RunAssertionAloneJoinAlias(EPServiceProvider epService)
        {
            string epl = "select s1.* as s1 from " + typeof(SupportBean).FullName + "#length(3) as s0, " +
                         typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(SupportMarketDataBean), type.GetPropertyType("s1"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);

            SendBeanEvent(epService, "E1");
            Assert.IsFalse(testListener.IsInvoked);

            object theEvent = SendMarketEvent(epService, "E1");
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Get("s1"));

            stmt.Dispose();

            // reverse streams
            epl = "select s0.* as szero from " + typeof(SupportBean).FullName + "#length(3) as s0, " +
                  typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += testListener.Update;

            type = stmt.EventType;
            Assert.AreEqual(typeof(SupportBean), type.GetPropertyType("szero"));
            Assert.AreEqual(typeof(Map), type.UnderlyingType);

            SendMarketEvent(epService, "E1");
            Assert.IsFalse(testListener.IsInvoked);

            theEvent = SendBeanEvent(epService, "E1");
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Get("szero"));

            stmt.Dispose();
        }

        private void RunAssertionAloneJoinNoAlias(EPServiceProvider epService)
        {
            string epl = "select s1.* from " + typeof(SupportBean).FullName + "#length(3) as s0, " +
                         typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;

            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(long?), type.GetPropertyType("volume"));
            Assert.AreEqual(typeof(SupportMarketDataBean), type.UnderlyingType);

            SendBeanEvent(epService, "E1");
            Assert.IsFalse(testListener.IsInvoked);

            object theEvent = SendMarketEvent(epService, "E1");
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Underlying);

            stmt.Dispose();

            // reverse streams
            epl = "select s0.* from " + typeof(SupportBean).FullName + "#length(3) as s0, " +
                  typeof(SupportMarketDataBean).FullName + "#keepall as s1";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += testListener.Update;

            type = stmt.EventType;
            Assert.AreEqual(typeof(string), type.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(SupportBean), type.UnderlyingType);

            SendMarketEvent(epService, "E1");
            Assert.IsFalse(testListener.IsInvoked);

            theEvent = SendBeanEvent(epService, "E1");
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Underlying);

            stmt.Dispose();
        }

        private void RunAssertionInvalidSelect(EPServiceProvider epService)
        {
            SupportMessageAssertUtil.TryInvalid(
                epService,
                "select TheString.* as TheString, TheString from " + typeof(SupportBean).FullName +
                "#length(3) as TheString",
                "Error starting statement: Column name 'TheString' appears more then once in select clause");

            SupportMessageAssertUtil.TryInvalid(
                epService, "select s1.* as abc from " + typeof(SupportBean).FullName + "#length(3) as s0",
                "Error starting statement: Stream selector 's1.*' does not match any stream name in the from clause [");

            SupportMessageAssertUtil.TryInvalid(
                epService, "select s0.* as abc, s0.* as abc from " + typeof(SupportBean).FullName + "#length(3) as s0",
                "Error starting statement: Column name 'abc' appears more then once in select clause");

            SupportMessageAssertUtil.TryInvalid(
                epService,
                "select s0.*, s1.* from " + typeof(SupportBean).FullName + "#keepall as s0, " +
                typeof(SupportBean).FullName + "#keepall as s1",
                "Error starting statement: A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation");
        }

        private SupportBean SendBeanEvent(EPServiceProvider epService, string s)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private SupportBean SendBeanEvent(EPServiceProvider epService, string s, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }

        private SupportMarketDataBean SendMarketEvent(EPServiceProvider epService, string s)
        {
            var bean = new SupportMarketDataBean(s, 0d, 0L, "");
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
} // end of namespace
