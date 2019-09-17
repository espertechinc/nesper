///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.subscriber;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeSubscriber : RegressionExecution
    {
        private static readonly string[] FIELDS = new [] { "TheString","IntPrimitive" };

        public void Run(RegressionEnvironment env)
        {
            RunAssertionBindings(env);
            RunAssertionSubscriberAndListener(env);
            RunAssertionBindWildcardJoin(env);
            RunAssertionInvocationTargetEx(env);
            RunAssertionNamedWindow(env);
            RunAssertionStartStopStatement(env);
            RunAssertionVariables(env);
            RunAssertionSimpleSelectUpdateOnly(env);
            RunAssertionPerformanceSyntheticUndelivered(env);
            RunAssertionPerformanceSynthetic(env);
        }

        private void RunAssertionBindings(RegressionEnvironment env)
        {
            // just wildcard
            var stmtJustWildcard = env.CompileDeploy("@Name('s0') select * from SupportBean(TheString='E2')")
                .Statement("s0");
            TryAssertionJustWildcard(env, stmtJustWildcard, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionJustWildcard(env, stmtJustWildcard, new SupportSubscriberRowByRowSpecificWStmt());
            env.UndeployAll();

            // wildcard with props
            var stmtWildcardWProps =
                env.CompileDeploy("@Name('s0') select *, IntPrimitive + 2, 'x'||TheString||'x' from SupportBean")
                    .Statement("s0");
            TryAssertionWildcardWProps(env, stmtWildcardWProps, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionWildcardWProps(env, stmtWildcardWProps, new SupportSubscriberRowByRowSpecificWStmt());
            env.UndeployAll();

            // nested
            var stmtNested =
                env.CompileDeploy("@Name('s0') select Nested, Nested.NestedNested from SupportBeanComplexProps")
                    .Statement("s0");
            TryAssertionNested(env, stmtNested, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionNested(env, stmtNested, new SupportSubscriberRowByRowSpecificWStmt());
            env.UndeployAll();

            // enum
            var stmtEnum = env.CompileDeploy("@Name('s0') select TheString, SupportEnum from SupportBeanWithEnum")
                .Statement("s0");
            TryAssertionEnum(env, stmtEnum, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionEnum(env, stmtEnum, new SupportSubscriberRowByRowSpecificWStmt());
            env.UndeployAll();

            // null-typed select value
            var stmtNullSelected = env.CompileDeploy("@Name('s0') select null, LongBoxed from SupportBean")
                .Statement("s0");
            TryAssertionNullSelected(env, stmtNullSelected, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionNullSelected(env, stmtNullSelected, new SupportSubscriberRowByRowSpecificWStmt());
            env.UndeployAll();

            // widening
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionWidening(env, rep, new SupportSubscriberRowByRowSpecificNStmt());
            }

            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionWidening(env, rep, new SupportSubscriberRowByRowSpecificWStmt());
            }

            // r-stream select
            TryAssertionRStreamSelect(env, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionRStreamSelect(env, new SupportSubscriberRowByRowSpecificWStmt());

            // stream-selected join
            TryAssertionStreamSelectWJoin(env, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionStreamSelectWJoin(env, new SupportSubscriberRowByRowSpecificWStmt());

            // stream-wildcard join
            TryAssertionStreamWildcardJoin(env, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionStreamWildcardJoin(env, new SupportSubscriberRowByRowSpecificWStmt());

            // bind wildcard join
            TryAssertionBindWildcardJoin(env, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionBindWildcardJoin(env, new SupportSubscriberRowByRowSpecificWStmt());

            // output limit
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionOutputLimitNoJoin(env, rep, new SupportSubscriberRowByRowSpecificNStmt());
            }

            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionOutputLimitNoJoin(env, rep, new SupportSubscriberRowByRowSpecificWStmt());
            }

            // output limit join
            TryAssertionOutputLimitJoin(env, new SupportSubscriberRowByRowSpecificNStmt());
            TryAssertionOutputLimitJoin(env, new SupportSubscriberRowByRowSpecificWStmt());

            // binding-to-map
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindMap(env, rep, new SupportSubscriberMultirowMapNStmt());
            }

            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindMap(env, rep, new SupportSubscriberMultirowMapWStmt());
            }

            // binding-to-objectarray
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindObjectArr(env, rep, new SupportSubscriberMultirowObjectArrayNStmt());
            }

            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionBindObjectArr(env, rep, new SupportSubscriberMultirowObjectArrayWStmt());
            }

            // binding-to-underlying-array
            TryAssertionBindWildcardIRStream(env, new SupportSubscriberMultirowUnderlyingNStmt());
            TryAssertionBindWildcardIRStream(env, new SupportSubscriberMultirowUnderlyingWStmt());

            // Object[] and "Object..." binding
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayPlainNStmt());
                TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayPlainWStmt());
                TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayVarargNStmt());
                TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayVarargWStmt());
            }

            // Map binding
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionRowMapDelivery(env, rep, new SupportSubscriberRowByRowMapNStmt());
                TryAssertionRowMapDelivery(env, rep, new SupportSubscriberRowByRowMapWStmt());
            }

            // static methods
            TryAssertionStaticMethod(env);

            // IR stream individual calls
            TryAssertionBindUpdateIRStream(env, new SupportSubscriberRowByRowFullNStmt());
            TryAssertionBindUpdateIRStream(env, new SupportSubscriberRowByRowFullWStmt());

            // no-params subscriber
            var stmtNoParamsSubscriber = env.CompileDeploy("@Name('s0') select null from SupportBean").Statement("s0");
            TryAssertionNoParams(env, stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseNStmt());
            TryAssertionNoParams(env, stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseWStmt());
            env.UndeployAll();

            // named-method subscriber
            var stmtNamedMethod = env.CompileDeploy("@Name('s0') select TheString from SupportBean").Statement("s0");
            TryAsserionNamedMethod(env, stmtNamedMethod, new SupportSubscriberMultirowUnderlyingNamedMethodNStmt());
            TryAsserionNamedMethod(env, stmtNamedMethod, new SupportSubscriberMultirowUnderlyingNamedMethodWStmt());
            env.UndeployAll();

            // prefer the EPStatement-footprint over the non-EPStatement footprint
            TryAssertionPreferEPStatement(env);

            env.UndeployAll();
        }

        private void TryAssertionJustWildcard(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmt.SetSubscriber(subscriber);
            var theEvent = new SupportBean("E2", 1);
            env.SendEventBean(theEvent);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {theEvent});
        }

        private void RunAssertionSubscriberAndListener(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("insert into A1 select s.*, 1 as a from SupportBean as s", path);
            var stmt = env.CompileDeploy("@Name('s0') select a1.* from A1 as a1", path).Statement("s0");

            var listener = new SupportUpdateListener();
            var subscriber = new SupportSubscriberRowByRowObjectArrayPlainNStmt();

            stmt.AddListener(listener);
            stmt.SetSubscriber(subscriber);
            env.SendEventBean(new SupportBean("E1", 1));

            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", theEvent.Get("TheString"));
            Assert.AreEqual(1, theEvent.Get("IntPrimitive"));
            Assert.IsTrue(theEvent.Underlying is Pair<object, object>);

            foreach (var property in stmt.EventType.PropertyNames) {
                var getter = stmt.EventType.GetGetter(property);
                getter.Get(theEvent);
            }

            env.UndeployAll();
        }

        private void TryAssertionBindUpdateIRStream(
            RegressionEnvironment env,
            SupportSubscriberRowByRowFullBase subscriber)
        {
            var stmtText = "@Name('s0') select irstream TheString, IntPrimitive from SupportBean" + "#length_batch(2)";
            var stmt = env.CompileDeploy(stmtText).Statement("s0");
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                2,
                0,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}},
                null);

            env.SendEventBean(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                2,
                2,
                new[] {new object[] {"E3", 3}, new object[] {"E4", 4}},
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

            env.UndeployAll();
        }

        private void TryAssertionBindObjectArr(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberMultirowObjectArrayBase subscriber)
        {
            var stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " @Name('s0') select irstream TheString, IntPrimitive from SupportBean" +
                           "#length_batch(2)";
            var stmt = env.CompileDeploy(stmtText).Statement("s0");
            stmt.SetSubscriber(subscriber);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}},
                null);

            env.SendEventBean(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new[] {new object[] {"E3", 3}, new object[] {"E4", 4}},
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

            env.UndeployAll();
        }

        private void TryAssertionBindMap(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberMultirowMapBase subscriber)
        {
            var stmtText = eventRepresentationEnum.GetAnnotationText() +
                           " @Name('s0') select irstream TheString, IntPrimitive from SupportBean" +
                           "#length_batch(2)";
            var stmt = env.CompileDeploy(stmtText).Statement("s0");
            stmt.SetSubscriber(subscriber);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}},
                null);

            env.SendEventBean(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new[] {new object[] {"E3", 3}, new object[] {"E4", 4}},
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

            env.UndeployAll();
        }

        private void TryAssertionWidening(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationText() +
                    " @Name('s0') select BytePrimitive, IntPrimitive, LongPrimitive, FloatPrimitive from SupportBean(TheString='E1')")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            var bean = new SupportBean();
            bean.TheString = "E1";
            bean.BytePrimitive = 1;
            bean.IntPrimitive = 2;
            bean.LongPrimitive = 3;
            bean.FloatPrimitive = 4;
            env.SendEventBean(bean);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {1, 2L, 3d, 4d});

            env.UndeployAll();
        }

        private void TryAssertionObjectArrayDelivery(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowObjectArrayBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationText() +
                    " @Name('s0') select TheString, IntPrimitive from SupportBean#unique(TheString)")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertOneAndReset(
                stmt,
                new object[] {"E1", 1});

            env.SendEventBean(new SupportBean("E2", 10));
            subscriber.AssertOneAndReset(
                stmt,
                new object[] {"E2", 10});

            env.UndeployAll();
        }

        private void TryAssertionRowMapDelivery(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowMapBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationText() +
                    " @Name('s0') select irstream TheString, IntPrimitive from SupportBean#unique(TheString)")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertIRStreamAndReset(
                stmt,
                FIELDS,
                new object[] {"E1", 1},
                null);

            env.SendEventBean(new SupportBean("E2", 10));
            subscriber.AssertIRStreamAndReset(
                stmt,
                FIELDS,
                new object[] {"E2", 10},
                null);

            env.SendEventBean(new SupportBean("E1", 2));
            subscriber.AssertIRStreamAndReset(
                stmt,
                FIELDS,
                new object[] {"E1", 2},
                new object[] {"E1", 1});

            env.UndeployAll();
        }

        private void TryAssertionNested(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmt.SetSubscriber(subscriber);

            var theEvent = SupportBeanComplexProps.MakeDefaultBean();
            env.SendEventBean(theEvent);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {theEvent.Nested, theEvent.Nested.NestedNested});
        }

        private void TryAssertionEnum(
            RegressionEnvironment env,
            EPStatement stmtEnum,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmtEnum.SetSubscriber(subscriber);

            var theEvent = new SupportBeanWithEnum("abc", SupportEnum.ENUM_VALUE_1);
            env.SendEventBean(theEvent);
            subscriber.AssertOneReceivedAndReset(
                stmtEnum,
                new object[] {theEvent.TheString, theEvent.SupportEnum});
        }

        private void TryAssertionNullSelected(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmt.SetSubscriber(subscriber);
            env.SendEventBean(new SupportBean());
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {null, null});
        }

        private void TryAssertionStreamSelectWJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@Name('s0') select null, S1, S0 from SupportBean#keepall as S0, SupportMarketDataBean#keepall as S1 where S0.TheString = S1.Symbol")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {null, s1, s0});

            env.UndeployAll();
        }

        private void TryAssertionBindWildcardJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@Name('s0') select * from SupportBean#keepall as S0, SupportMarketDataBean#keepall as S1 where S0.TheString = S1.Symbol")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {s0, s1});

            env.UndeployAll();
        }

        private void TryAssertionStreamWildcardJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@Name('s0') select TheString || '<', S1.* as S1, S0.* as S0 from SupportBean#keepall as S0, SupportMarketDataBean#keepall as S1 where S0.TheString = S1.Symbol")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {"E1<", s1, s0});

            env.UndeployAll();
        }

        private static void TryAssertionWildcardWProps(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            env.SendEventBean(s0);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {s0, 102, "xE1x"});
        }

        private void TryAssertionOutputLimitNoJoin(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    eventRepresentationEnum.GetAnnotationText() +
                    " @Name('s0') select TheString, IntPrimitive from SupportBean output every 2 events")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertMultipleReceivedAndReset(
                stmt,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

            env.UndeployAll();
        }

        private void TryAssertionOutputLimitJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@Name('s0') select TheString, IntPrimitive from SupportBean#keepall, SupportMarketDataBean#keepall where Symbol = TheString output every 2 events")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportMarketDataBean("E1", 0, 1L, ""));
            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E1", 2));
            subscriber.AssertMultipleReceivedAndReset(
                stmt,
                new[] {new object[] {"E1", 1}, new object[] {"E1", 2}});
            env.UndeployAll();
        }

        private void TryAssertionRStreamSelect(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy("@Name('s0') select rstream S0 from SupportBean#unique(TheString) as S0")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            // send event
            var s0 = new SupportBean("E1", 100);
            env.SendEventBean(s0);
            subscriber.AssertNoneReceived();

            var s1 = new SupportBean("E2", 200);
            env.SendEventBean(s1);
            subscriber.AssertNoneReceived();

            var s2 = new SupportBean("E1", 300);
            env.SendEventBean(s2);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {s0});

            env.UndeployAll();
        }

        private void TryAssertionBindWildcardIRStream(
            RegressionEnvironment env,
            SupportSubscriberMultirowUnderlyingBase subscriber)
        {
            var stmt = env.CompileDeploy("@Name('s0') select irstream * from SupportBean#length_batch(2)")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportBean("E2", 200);
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {s0, s1},
                null);

            var s2 = new SupportBean("E3", 300);
            var s3 = new SupportBean("E4", 400);
            env.SendEventBean(s2);
            env.SendEventBean(s3);
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {s2, s3},
                new object[] {s0, s1});

            env.UndeployAll();
        }

        private void TryAssertionStaticMethod(RegressionEnvironment env)
        {
            var stmt = env.CompileDeploy("@Name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");

            var subscriber = new SupportSubscriberRowByRowStatic();
            stmt.SetSubscriber(subscriber);
            env.SendEventBean(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {new object[] {"E1", 100}},
                SupportSubscriberRowByRowStatic.GetAndResetIndicate());

            var subscriberWStmt = new SupportSubscriberRowByRowStaticWStatement();
            stmt.SetSubscriber(subscriberWStmt);
            env.SendEventBean(new SupportBean("E2", 200));
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {new object[] {"E2", 200}},
                SupportSubscriberRowByRowStaticWStatement.Indicate);
            Assert.AreEqual(stmt, SupportSubscriberRowByRowStaticWStatement.Statements[0]);
            subscriberWStmt.Reset();

            env.UndeployAll();
        }

        private void TryAssertionNoParams(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberNoParamsBase subscriber)
        {
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportBean());
            subscriber.AssertCalledAndReset(stmt);
        }

        private void TryAsserionNamedMethod(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberMultirowUnderlyingBase subscriber)
        {
            stmt.SetSubscriber(subscriber, "someNewDataMayHaveArrived");

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {"E1"},
                null);
        }

        private void TryAssertionPreferEPStatement(RegressionEnvironment env)
        {
            var subscriber = new SupportSubscriberUpdateBothFootprints();
            var stmt = env.CompileDeploy("@Name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportBean("E1", 10));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {"E1", 10});

            env.UndeployAll();
        }

        private void RunAssertionBindWildcardJoin(RegressionEnvironment env)
        {
            var stmtOne = env.CompileDeploy("@Name('s0') select * from SupportBean").Statement("s0");
            TryInvalid(this, stmtOne, "Subscriber object does not provide a public method by name 'Update'");
            TryInvalid(
                new DummySubscriberEmptyUpd(),
                stmtOne,
                "No suitable subscriber method named 'Update' found, expecting a method that takes 1 parameter of type SupportBean");
            TryInvalid(
                new DummySubscriberMultipleUpdate(),
                stmtOne,
                "No suitable subscriber method named 'Update' found, expecting a method that takes 1 parameter of type SupportBean");
            TryInvalid(
                new DummySubscriberUpdate(),
                stmtOne,
                "Subscriber method named 'Update' for parameter number 1 is not assignable, expecting type 'SupportBean' but found type 'SupportMarketDataBean'");
            TryInvalid(
                new DummySubscriberPrivateUpd(),
                stmtOne,
                "Subscriber object does not provide a public method by name 'Update'");
            env.UndeployModuleContaining("s0");

            var stmtTwo = env.CompileDeploy("@Name('s0') select IntPrimitive from SupportBean").Statement("s0");
            var message = "Subscriber 'updateRStream' method footprint must match 'Update' method footprint";
            TryInvalid(new DummySubscriberMismatchUpdateRStreamOne(), stmtTwo, message);
            TryInvalid(new DummySubscriberMismatchUpdateRStreamTwo(), stmtTwo, message);

            env.UndeployAll();
        }

        private void RunAssertionInvocationTargetEx(RegressionEnvironment env)
        {
            // smoke test, need to consider log file; test for ESPER-331
            var stmt = env.CompileDeploy("@Name('s0') select * from SupportMarketDataBean").Statement("s0");
            stmt.SetSubscriber(new DummySubscriberException());
            stmt.Events += (
                sender,
                updateEventArgs) => throw new EPException("test exception 2");
            stmt.AddListenerWithReplay(
                new DelegateUpdateListener(
                    (
                            sender,
                            eventArgs) =>
                        throw new EPException("test exception 3")));

            // no exception expected
            env.SendEventBean(new SupportMarketDataBean("IBM", 0, 0L, ""));

            env.UndeployAll();
        }

        private void RunAssertionStartStopStatement(RegressionEnvironment env)
        {
            var subscriber = new SubscriberInterface();

            var compiled = env.Compile("@Name('s0') select * from SupportMarkerInterface");
            var stmt = env.Deploy(compiled).Statement("s0");
            stmt.SetSubscriber(subscriber);

            var a1 = new SupportBean_A("A1");
            env.SendEventBean(a1);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {a1}, subscriber.GetAndResetIndicate().ToArray());

            var b1 = new SupportBean_B("B1");
            env.SendEventBean(b1);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {b1}, subscriber.GetAndResetIndicate().ToArray());

            env.UndeployAll();

            var c1 = new SupportBean_C("C1");
            env.SendEventBean(c1);
            Assert.AreEqual(0, subscriber.GetAndResetIndicate().Count);

            env.Deploy(compiled).Statement("s0").SetSubscriber(subscriber);

            var d1 = new SupportBean_D("D1");
            env.SendEventBean(d1);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {d1}, subscriber.GetAndResetIndicate().ToArray());

            env.UndeployAll();
        }

        private void RunAssertionVariables(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var fields = new [] { "myvar" };
            var subscriberCreateVariable = new SubscriberMap();
            var stmtTextCreate = "@Name('s0') create variable string myvar = 'abc'";
            var stmt = env.CompileDeploy(stmtTextCreate, path).Statement("s0");
            stmt.SetSubscriber(subscriberCreateVariable);

            var subscriberSetVariable = new SubscriberMap();
            var stmtTextSet = "@Name('s1') on SupportBean set myvar = TheString";
            stmt = env.CompileDeploy(stmtTextSet, path).Statement("s1");
            stmt.SetSubscriber(subscriberSetVariable);

            env.SendEventBean(new SupportBean("def", 1));
            EPAssertionUtil.AssertPropsMap(
                subscriberCreateVariable.GetAndResetIndicate()[0],
                fields,
                "def");
            EPAssertionUtil.AssertPropsMap(
                subscriberSetVariable.GetAndResetIndicate()[0],
                fields,
                "def");

            env.UndeployAll();
        }

        private void RunAssertionNamedWindow(RegressionEnvironment env)
        {
            TryAssertionNamedWindow(env, EventRepresentationChoice.MAP);
        }

        private void TryAssertionNamedWindow(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var fields = new [] { "key","value" };
            var path = new RegressionPath();
            var subscriberNamedWindow = new SubscriberMap();
            var stmtTextCreate = eventRepresentationEnum.GetAnnotationText() +
                                 " @Name('create') create window MyWindow#keepall as select TheString as key, IntPrimitive as value from SupportBean";
            var stmt = env.CompileDeploy(stmtTextCreate, path).Statement("create");
            stmt.SetSubscriber(subscriberNamedWindow);

            var subscriberInsertInto = new SubscriberFields();
            var stmtTextInsertInto =
                "@Name('insert') insert into MyWindow select TheString as key, IntPrimitive as value from SupportBean";
            stmt = env.CompileDeploy(stmtTextInsertInto, path).Statement("insert");
            stmt.SetSubscriber(subscriberInsertInto);

            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(
                subscriberNamedWindow.GetAndResetIndicate()[0],
                fields,
                "E1",
                1);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {new object[] {"E1", 1}},
                subscriberInsertInto.GetAndResetIndicate());

            // test on-delete
            var subscriberDelete = new SubscriberMap();
            var stmtTextDelete =
                "@Name('ondelete') on SupportMarketDataBean S0 delete from MyWindow s1 where S0.Symbol = S1.Key";
            stmt = env.CompileDeploy(stmtTextDelete, path).Statement("ondelete");
            stmt.SetSubscriber(subscriberDelete);

            env.SendEventBean(new SupportMarketDataBean("E1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(
                subscriberDelete.GetAndResetIndicate()[0],
                fields,
                "E1",
                1);

            // test on-select
            var subscriberSelect = new SubscriberMap();
            var stmtTextSelect = "@Name('onselect') on SupportMarketDataBean S0 select key, value from MyWindow s1";
            stmt = env.CompileDeploy(stmtTextSelect, path).Statement("onselect");
            stmt.SetSubscriber(subscriberSelect);

            env.SendEventBean(new SupportBean("E2", 2));
            env.SendEventBean(new SupportMarketDataBean("M1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(
                subscriberSelect.GetAndResetIndicate()[0],
                fields,
                "E2",
                2);

            env.UndeployAll();
        }

        private void RunAssertionSimpleSelectUpdateOnly(RegressionEnvironment env)
        {
            var subscriber = new SupportSubscriberRowByRowSpecificNStmt();
            var stmt = env.CompileDeploy("@Name('s0') select TheString, IntPrimitive from SupportBean#lastevent")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            // get statement, attach listener
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);

            // send event
            env.SendEventBean(new SupportBean("E1", 100));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {"E1", 100});
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(),
                FIELDS,
                new[] {new object[] {"E1", 100}});
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {"E1", 100});

            // remove listener
            stmt.RemoveAllEventHandlers();

            // send event
            env.SendEventBean(new SupportBean("E2", 200));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {"E2", 200});
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(),
                FIELDS,
                new[] {new object[] {"E2", 200}});
            Assert.IsFalse(listener.IsInvoked);

            // add listener
            var stmtAwareListener = new SupportUpdateListener();
            stmt.AddListener(stmtAwareListener);

            // send event
            env.SendEventBean(new SupportBean("E3", 300));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                new object[] {"E3", 300});
            EPAssertionUtil.AssertPropsPerRow(
                stmt.GetEnumerator(),
                FIELDS,
                new[] {new object[] {"E3", 300}});
            EPAssertionUtil.AssertProps(
                stmtAwareListener.AssertOneGetNewAndReset(),
                FIELDS,
                new object[] {"E3", 300});

            // subscriber with EPStatement in the footprint
            stmt.RemoveAllEventHandlers();
            var subsWithStatement = new SupportSubscriberRowByRowSpecificWStmt();
            stmt.Subscriber = subsWithStatement;
            env.SendEventBean(new SupportBean("E10", 999));
            subsWithStatement.AssertOneReceivedAndReset(
                stmt,
                new object[] {"E10", 999});

            env.UndeployAll();
        }

        private void RunAssertionPerformanceSyntheticUndelivered(RegressionEnvironment env)
        {
            var numLoop = 100000;
            env.CompileDeploy("select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)");

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < numLoop; i++) {
                env.SendEventBean(new SupportBean("E1", 1000 + i));
            }

            var end = PerformanceObserver.MilliTime;

            Assert.IsTrue(end - start < 1000, "delta=" + (end - start));
            env.UndeployAll();
        }

        private void RunAssertionPerformanceSynthetic(RegressionEnvironment env)
        {
            var numLoop = 100000;
            var stmt = env
                .CompileDeploy("@Name('s0') select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)")
                .Statement("s0");
            IList<object[]> results = new List<object[]>();

            stmt.Events += (
                sender,
                updateEventArgs) => {
                var newEvents = updateEventArgs.NewEvents;
                var theString = (string) newEvents[0].Get("TheString");
                var val = newEvents[0].Get("IntPrimitive").AsInt();
                results.Add(new object[] {theString, val});
            };

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < numLoop; i++) {
                env.SendEventBean(new SupportBean("E1", 1000 + i));
            }

            var end = PerformanceObserver.MilliTime;

            Assert.AreEqual(numLoop, results.Count);
            for (var i = 0; i < numLoop; i++) {
                EPAssertionUtil.AssertEqualsAnyOrder(
                    results[i],
                    new object[] {"E1", 1000 + i});
            }

            Assert.IsTrue(end - start < 1000, "delta=" + (end - start));

            env.UndeployAll();
        }

        private void TryInvalid(
            object subscriber,
            EPStatement stmt,
            string message)
        {
            try {
                stmt.SetSubscriber(subscriber);
                Assert.Fail();
            }
            catch (EPSubscriberException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }

        public class DummySubscriberException
        {
            public void Update(SupportMarketDataBean bean)
            {
                throw new EPException("DummySubscriberException-generated");
            }
        }

        public class DummySubscriberEmptyUpd
        {
            public void Update()
            {
            }
        }

        public class DummySubscriberPrivateUpd
        {
            private void Update(SupportBean bean)
            {
            }
        }

        public class DummySubscriberUpdate
        {
            public void Update(SupportMarketDataBean dummy)
            {
            }
        }

        public class DummySubscriberMultipleUpdate
        {
            public void Update(long x)
            {
            }

            public void Update(int x)
            {
            }
        }

        public class DummySubscriberMismatchUpdateRStreamOne
        {
            public void Update(int value)
            {
            }

            public void UpdateRStream(
                EPStatement stmt,
                int value)
            {
            }
        }

        public class DummySubscriberMismatchUpdateRStreamTwo
        {
            public void Update(
                EPStatement stmt,
                int value)
            {
            }

            public void UpdateRStream(int value)
            {
            }
        }

        public class SubscriberFields
        {
            private List<object[]> indicate = new List<object[]>();

            public void Update(
                string key,
                int value)
            {
                indicate.Add(new object[] {key, value});
            }

            internal IList<object[]> GetAndResetIndicate()
            {
                IList<object[]> result = indicate;
                indicate = new List<object[]>();
                return result;
            }
        }

        public class SubscriberInterface
        {
            private List<SupportMarkerInterface> indicate = new List<SupportMarkerInterface>();

            public void Update(SupportMarkerInterface impl)
            {
                indicate.Add(impl);
            }

            internal IList<SupportMarkerInterface> GetAndResetIndicate()
            {
                IList<SupportMarkerInterface> result = indicate;
                indicate = new List<SupportMarkerInterface>();
                return result;
            }
        }

        public class SubscriberMap
        {
            private List<IDictionary<string, object>> indicate = new List<IDictionary<string, object>>();

            public void Update(IDictionary<string, object> row)
            {
                indicate.Add(row);
            }

            internal IList<IDictionary<string, object>> GetAndResetIndicate()
            {
                IList<IDictionary<string, object>> result = indicate;
                indicate = new List<IDictionary<string, object>>();
                return result;
            }
        }
    }
} // end of namespace