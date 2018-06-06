///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.subscriber;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using Map = IDictionary<string, object>;

    public class ExecClientSubscriberBind : RegressionExecution {
        private static readonly string[] FIELDS = "TheString,IntPrimitive".Split(',');
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventTypeAutoName(typeof(SupportBean).Namespace);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionBindings(epService);
            RunAssertionSubscriberAndListener(epService);
        }
    
        private void RunAssertionBindings(EPServiceProvider epService) {
    
            // just wildcard
            EPStatement stmtJustWildcard = epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString='E2')");
            TryAssertionJustWildcard(epService, stmtJustWildcard, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionJustWildcard(epService, stmtJustWildcard, new SupportSubscriberRowByRowSpecificWStmt());
            stmtJustWildcard.Dispose();
    
            // wildcard with props
            EPStatement stmtWildcardWProps = epService.EPAdministrator.CreateEPL("select *, IntPrimitive + 2, 'x'||TheString||'x' from " + typeof(SupportBean).FullName);
            TryAssertionWildcardWProps(epService, stmtWildcardWProps, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionWildcardWProps(epService, stmtWildcardWProps, new SupportSubscriberRowByRowSpecificWStmt());
            stmtWildcardWProps.Dispose();
    
            // nested
            EPStatement stmtNested = epService.EPAdministrator.CreateEPL("select nested, nested.nestedNested from SupportBeanComplexProps");
            TryAssertionNested(epService, stmtNested, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionNested(epService, stmtNested, new SupportSubscriberRowByRowSpecificWStmt());
            stmtNested.Dispose();
    
            // enum
            EPStatement stmtEnum = epService.EPAdministrator.CreateEPL("select TheString, supportEnum from SupportBeanWithEnum");
            TryAssertionEnum(epService, stmtEnum, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionEnum(epService, stmtEnum, new SupportSubscriberRowByRowSpecificWStmt());
            stmtEnum.Dispose();
    
            // null-typed select value
            EPStatement stmtNullSelected = epService.EPAdministrator.CreateEPL("select null, LongBoxed from SupportBean");
            TryAssertionNullSelected(epService, stmtNullSelected, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionNullSelected(epService, stmtNullSelected, new SupportSubscriberRowByRowSpecificWStmt());
            stmtNullSelected.Dispose();
    
            // widening
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionWidening(epService, rep, new SupportSubscriberRowByRowSpecificNStmt());
            }
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionWidening(epService, rep, new SupportSubscriberRowByRowSpecificWStmt());
            }
    
            // r-stream select
            TryAssertionRStreamSelect(epService, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionRStreamSelect(epService, new SupportSubscriberRowByRowSpecificWStmt());
    
            // stream-selected join
            TryAssertionStreamSelectWJoin(epService, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionStreamSelectWJoin(epService, new SupportSubscriberRowByRowSpecificWStmt());
    
            // stream-wildcard join
            TryAssertionStreamWildcardJoin(epService, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionStreamWildcardJoin(epService, new SupportSubscriberRowByRowSpecificWStmt());
    
            // bind wildcard join
            TryAssertionBindWildcardJoin(epService, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionBindWildcardJoin(epService, new SupportSubscriberRowByRowSpecificWStmt());
    
            // output limit
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionOutputLimitNoJoin(epService, rep, new SupportSubscriberRowByRowSpecificNStmt());
            }
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionOutputLimitNoJoin(epService, rep, new SupportSubscriberRowByRowSpecificWStmt());
            }
    
            // output limit join
            TryAssertionOutputLimitJoin(epService, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionOutputLimitJoin(epService, new SupportSubscriberRowByRowSpecificWStmt());
    
            // binding-to-map
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindMap(epService, rep, new SupportSubscriberMultirowMapNStmt());
            }
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindMap(epService, rep, new SupportSubscriberMultirowMapWStmt());
            }
    
            // binding-to-objectarray
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindObjectArr(epService, rep, new SupportSubscriberMultirowObjectArrayNStmt());
            }
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindObjectArr(epService, rep, new SupportSubscriberMultirowObjectArrayWStmt());
            }
    
            // binding-to-underlying-array
            TryAssertionBindWildcardIRStream(epService, new SupportSubscriberMultirowUnderlyingNStmt());
            TryAssertionBindWildcardIRStream(epService, new SupportSubscriberMultirowUnderlyingWStmt());
    
            // object[] and "Object..." binding
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionObjectArrayDelivery(epService, rep, new SupportSubscriberRowByRowObjectArrayPlainNStmt());
                TryAssertionObjectArrayDelivery(epService, rep, new SupportSubscriberRowByRowObjectArrayPlainWStmt());
                TryAssertionObjectArrayDelivery(epService, rep, new SupportSubscriberRowByRowObjectArrayVarargNStmt());
                TryAssertionObjectArrayDelivery(epService, rep, new SupportSubscriberRowByRowObjectArrayVarargWStmt());
            }
    
            // Map binding
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionRowMapDelivery(epService, rep, new SupportSubscriberRowByRowMapNStmt());
                TryAssertionRowMapDelivery(epService, rep, new SupportSubscriberRowByRowMapWStmt());
            }
    
            // static methods
            TryAssertionStaticMethod(epService);
    
            // IR stream individual calls
            TryAssertionBindUpdateIRStream(epService, new SupportSubscriberRowByRowFullNStmt());
            TryAssertionBindUpdateIRStream(epService, new SupportSubscriberRowByRowFullWStmt());
    
            // no-params subscriber
            EPStatement stmtNoParamsSubscriber = epService.EPAdministrator.CreateEPL("select null from SupportBean");
            TryAssertionNoParams(epService, stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseNStmt());
            TryAssertionNoParams(epService, stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseWStmt());
            stmtNoParamsSubscriber.Dispose();
    
            // named-method subscriber
            EPStatement stmtNamedMethod = epService.EPAdministrator.CreateEPL("select TheString from SupportBean");
            TryAsserionNamedMethod(epService, stmtNamedMethod, new SupportSubscriberMultirowUnderlyingNamedMethodNStmt());
            TryAsserionNamedMethod(epService, stmtNamedMethod, new SupportSubscriberMultirowUnderlyingNamedMethodWStmt());
            stmtNamedMethod.Dispose();
    
            // prefer the EPStatement-footprint over the non-EPStatement footprint
            TryAssertionPreferEPStatement(epService);
        }
    
        private void RunAssertionSubscriberAndListener(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("insert into A1 select s.*, 1 as a from SupportBean as s");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select a1.* from A1 as a1");
    
            var listener = new SupportUpdateListener();
            var subscriber = new SupportSubscriberRowByRowObjectArrayPlainNStmt();
    
            stmt.Events += listener.Update;
            stmt.Subscriber = subscriber;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", theEvent.Get("TheString"));
            Assert.AreEqual(1, theEvent.Get("IntPrimitive"));
            Assert.That(theEvent.Underlying, Is.InstanceOf<Pair<object,Map>>());
    
            foreach (string property in stmt.EventType.PropertyNames) {
                EventPropertyGetter getter = stmt.EventType.GetGetter(property);
                getter.Get(theEvent);
            }
        }
    
        private void TryAssertionBindUpdateIRStream(EPServiceProvider epService, SupportSubscriberRowByRowFullBase subscriber) {
            string stmtText = "select irstream TheString, IntPrimitive from " + typeof(SupportBean).FullName + "#length_batch(2)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(stmt, 2, 0, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(stmt, 2, 2, new object[][]{new object[] {"E3", 3}, new object[] {"E4", 4}}, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}});
        }
    
        private void TryAssertionBindObjectArr(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, SupportSubscriberMultirowObjectArrayBase subscriber) {
            string stmtText = eventRepresentationEnum.GetAnnotationText() + " select irstream TheString, IntPrimitive from " + typeof(SupportBean).FullName + "#length_batch(2)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Subscriber = subscriber;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(stmt, FIELDS, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(stmt, FIELDS, new object[][]{new object[] {"E3", 3}, new object[] {"E4", 4}}, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            stmt.Dispose();
        }
    
        private void TryAssertionBindMap(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, SupportSubscriberMultirowMapBase subscriber) {
            string stmtText = eventRepresentationEnum.GetAnnotationText() + " select irstream TheString, IntPrimitive from " + typeof(SupportBean).FullName + "#length_batch(2)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Subscriber = subscriber;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(stmt, FIELDS, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(stmt, FIELDS, new object[][]{new object[] {"E3", 3}, new object[] {"E4", 4}}, new object[][] {new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            stmt.Dispose();
        }
    
        private void TryAssertionWidening(
            EPServiceProvider epService, 
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " select bytePrimitive, IntPrimitive, LongPrimitive, FloatPrimitive from SupportBean(TheString='E1')");
            stmt.Subscriber = subscriber;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));
    
            var bean = new SupportBean();
            bean.TheString = "E1";
            bean.BytePrimitive = (byte) 1;
            bean.IntPrimitive = 2;
            bean.LongPrimitive = 3;
            bean.FloatPrimitive = 4;
            epService.EPRuntime.SendEvent(bean);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{1, 2L, 3d, 4d});
    
            stmt.Dispose();
        }
    
        private void TryAssertionObjectArrayDelivery(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, SupportSubscriberRowByRowObjectArrayBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select TheString, IntPrimitive from SupportBean#unique(TheString)");
            stmt.Subscriber = subscriber;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertOneAndReset(stmt, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            subscriber.AssertOneAndReset(stmt, new object[]{"E2", 10});
    
            stmt.Dispose();
        }
    
        private void TryAssertionRowMapDelivery(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, SupportSubscriberRowByRowMapBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select irstream TheString, IntPrimitive from SupportBean#unique(TheString)");
            stmt.Subscriber = subscriber;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertIRStreamAndReset(stmt, FIELDS, new object[]{"E1", 1}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            subscriber.AssertIRStreamAndReset(stmt, FIELDS, new object[]{"E2", 10}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            subscriber.AssertIRStreamAndReset(stmt, FIELDS, new object[]{"E1", 2}, new object[]{"E1", 1});
    
            stmt.Dispose();
        }
    
        private void TryAssertionNested(EPServiceProvider epService, EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
            stmt.Subscriber = subscriber;
    
            SupportBeanComplexProps theEvent = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(theEvent);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{theEvent.Nested, theEvent.Nested.NestedNested});
        }
    
        private void TryAssertionEnum(EPServiceProvider epService, EPStatement stmtEnum, SupportSubscriberRowByRowSpecificBase subscriber) {
            stmtEnum.Subscriber = subscriber;
    
            var theEvent = new SupportBeanWithEnum("abc", SupportEnum.ENUM_VALUE_1);
            epService.EPRuntime.SendEvent(theEvent);
            subscriber.AssertOneReceivedAndReset(stmtEnum, new object[]{theEvent.TheString, theEvent.SupportEnum});
        }
    
        private void TryAssertionNullSelected(EPServiceProvider epService, EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
            stmt.Subscriber = subscriber;
            epService.EPRuntime.SendEvent(new SupportBean());
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{null, null});
        }
    
        private void TryAssertionStreamSelectWJoin(EPServiceProvider epService, SupportSubscriberRowByRowSpecificBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select null, s1, s0 from SupportBean#keepall as s0, SupportMarketDataBean#keepall as s1 where s0.TheString = s1.symbol");
            stmt.Subscriber = subscriber;
    
            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            epService.EPRuntime.SendEvent(s0);
            epService.EPRuntime.SendEvent(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{null, s1, s0});
    
            stmt.Dispose();
        }
    
        private void TryAssertionBindWildcardJoin(EPServiceProvider epService, SupportSubscriberRowByRowSpecificBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#keepall as s0, SupportMarketDataBean#keepall as s1 where s0.TheString = s1.symbol");
            stmt.Subscriber = subscriber;
    
            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            epService.EPRuntime.SendEvent(s0);
            epService.EPRuntime.SendEvent(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0, s1});
    
            stmt.Dispose();
        }
    
        private void TryAssertionJustWildcard(EPServiceProvider epService, EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
            stmt.Subscriber = subscriber;
            var theEvent = new SupportBean("E2", 1);
            epService.EPRuntime.SendEvent(theEvent);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{theEvent});
        }
    
        private void TryAssertionStreamWildcardJoin(EPServiceProvider epService, SupportSubscriberRowByRowSpecificBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select TheString || '<', s1.* as s1, s0.* as s0 from SupportBean#keepall as s0, SupportMarketDataBean#keepall as s1 where s0.TheString = s1.symbol");
            stmt.Subscriber = subscriber;
    
            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            epService.EPRuntime.SendEvent(s0);
            epService.EPRuntime.SendEvent(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{"E1<", s1, s0});
    
            stmt.Dispose();
        }
    
        private void TryAssertionWildcardWProps(EPServiceProvider epService, EPStatement stmt, SupportSubscriberRowByRowSpecificBase subscriber) {
            stmt.Subscriber = subscriber;
    
            var s0 = new SupportBean("E1", 100);
            epService.EPRuntime.SendEvent(s0);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0, 102, "xE1x"});
        }
    
        private void TryAssertionOutputLimitNoJoin(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, SupportSubscriberRowByRowSpecificBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select TheString, IntPrimitive from SupportBean output every 2 events");
            stmt.Subscriber = subscriber;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            subscriber.AssertMultipleReceivedAndReset(stmt, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            stmt.Dispose();
        }
    
        private void TryAssertionOutputLimitJoin(EPServiceProvider epService, SupportSubscriberRowByRowSpecificBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean#keepall, SupportMarketDataBean#keepall where symbol = TheString output every 2 events");
            stmt.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("E1", 0, 1L, ""));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            subscriber.AssertMultipleReceivedAndReset(stmt, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}});
            stmt.Dispose();
        }
    
        private void TryAssertionRStreamSelect(EPServiceProvider epService, SupportSubscriberRowByRowSpecificBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select rstream s0 from SupportBean#unique(TheString) as s0");
            stmt.Subscriber = subscriber;
    
            // send event
            var s0 = new SupportBean("E1", 100);
            epService.EPRuntime.SendEvent(s0);
            subscriber.AssertNoneReceived();
    
            var s1 = new SupportBean("E2", 200);
            epService.EPRuntime.SendEvent(s1);
            subscriber.AssertNoneReceived();
    
            var s2 = new SupportBean("E1", 300);
            epService.EPRuntime.SendEvent(s2);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0});
    
            stmt.Dispose();
        }
    
        private void TryAssertionBindWildcardIRStream(EPServiceProvider epService, SupportSubscriberMultirowUnderlyingBase subscriber) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#length_batch(2)");
            stmt.Subscriber = subscriber;
    
            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportBean("E2", 200);
            epService.EPRuntime.SendEvent(s0);
            epService.EPRuntime.SendEvent(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{s0, s1}, null);
    
            var s2 = new SupportBean("E3", 300);
            var s3 = new SupportBean("E4", 400);
            epService.EPRuntime.SendEvent(s2);
            epService.EPRuntime.SendEvent(s3);
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{s2, s3}, new object[]{s0, s1});
    
            stmt.Dispose();
        }
    
        private void TryAssertionStaticMethod(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from " + typeof(SupportBean).FullName);
    
            var subscriber = new SupportSubscriberRowByRowStatic();
            stmt.Subscriber = subscriber;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(new object[][]{new object[] {"E1", 100}}, SupportSubscriberRowByRowStatic.GetAndResetIndicate());
    
            var subscriberWStmt = new SupportSubscriberRowByRowStaticWStatement();
            stmt.Subscriber = subscriberWStmt;
            epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            EPAssertionUtil.AssertEqualsExactOrder(new object[][]{new object[] {"E2", 200}}, SupportSubscriberRowByRowStaticWStatement.GetIndicate());
            Assert.AreEqual(stmt, SupportSubscriberRowByRowStaticWStatement.GetStatements()[0]);
            subscriberWStmt.Reset();
    
            stmt.Dispose();
        }
    
        private void TryAssertionNoParams(EPServiceProvider epService, EPStatement stmt, SupportSubscriberNoParamsBase subscriber) {
            stmt.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            subscriber.AssertCalledAndReset(stmt);
        }
    
        private void TryAsserionNamedMethod(EPServiceProvider epService, EPStatement stmt, SupportSubscriberMultirowUnderlyingBase subscriber) {
            stmt.Subscriber = new EPSubscriber(subscriber, "SomeNewDataMayHaveArrived");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{"E1"}, null);
        }
    
        private void TryAssertionPreferEPStatement(EPServiceProvider epService) {
            var subscriber = new SupportSubscriberUpdateBothFootprints();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean");
            stmt.Subscriber = subscriber;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            subscriber.AssertOneReceivedAndReset(stmt, new object[]{"E1", 10});
    
            stmt.Dispose();
        }
    }
} // end of namespace
