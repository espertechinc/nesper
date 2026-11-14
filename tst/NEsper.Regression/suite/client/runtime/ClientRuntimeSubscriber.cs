///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.subscriber;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeSubscriber
    {
        private static readonly string[] FIELDS = "TheString,IntPrimitive".SplitCsv();

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithBindings(execs);
            WithSubscriberAndListener(execs);
            WithBindWildcardJoin(execs);
            WithInvocationTargetEx(execs);
            WithNamedWindow(execs);
            WithStartStopStatement(execs);
            WithVariables(execs);
            WithSimpleSelectUpdateOnly(execs);
            WithPerformanceSyntheticUndelivered(execs);
            WithPerformanceSynthetic(execs);
            WithAddRemove(execs);
            WithDelegateBindings(execs);
            WithDelegateInvalid(execs);
            WithDelegateSeparateThread(execs);
            WithDelegateRemoveStream(execs);
            WithDelegateUnderlying(execs);
            WithDelegateMulticastException(execs);
            WithDelegateRemoveAndSwap(execs);
            WithDelegateWrongMethodName(execs);
            WithDelegateFunc(execs);
            WithBatchDeliveryException(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithBatchDeliveryException(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberBatchDeliveryException());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateRemoveStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateRemoveStream());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateUnderlying(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateUnderlying());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateMulticastException(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateMulticastException());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateRemoveAndSwap(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateRemoveAndSwap());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateWrongMethodName(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateWrongMethodName());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateFunc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateBindings(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateBindings());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithDelegateSeparateThread(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberDelegateSeparateThread());
            return execs;
        }

        public static IList<RegressionExecution> WithAddRemove(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberAddRemove());
            return execs;
        }

        public static IList<RegressionExecution> WithPerformanceSynthetic(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberPerformanceSynthetic());
            return execs;
        }

        public static IList<RegressionExecution> WithPerformanceSyntheticUndelivered(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberPerformanceSyntheticUndelivered());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleSelectUpdateOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberSimpleSelectUpdateOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithVariables(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberVariables());
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopStatement(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberStartStopStatement());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithInvocationTargetEx(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberInvocationTargetEx());
            return execs;
        }

        public static IList<RegressionExecution> WithBindWildcardJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberBindWildcardJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSubscriberAndListener(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberSubscriberAndListener());
            return execs;
        }

        public static IList<RegressionExecution> WithBindings(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSubscriberBindings());
            return execs;
        }

        private class ClientRuntimeSubscriberAddRemove : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var supportSubscriber = new SupportSubscriber();
                var epl = "@name('s0') select * from SupportBean";
                env.CompileDeploy(epl);
                env.Statement("s0").SetSubscriber(supportSubscriber);

                env.SendEventBean(new SupportBean());
                supportSubscriber.AssertOneGetNewAndReset();

                env.Statement("s0").Subscriber = null;

                env.SendEventBean(new SupportBean());
                ClassicAssert.IsFalse(supportSubscriber.IsInvoked);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberBindings : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // just wildcard
                var stmtJustWildcard = env.CompileDeploy("@name('s0') select * from SupportBean(TheString='E2')")
                    .Statement("s0");
                TryAssertionJustWildcard(env, stmtJustWildcard, new SupportSubscriberRowByRowSpecificNStmt());
                TryAssertionJustWildcard(env, stmtJustWildcard, new SupportSubscriberRowByRowSpecificWStmt());
                env.UndeployAll();

                // wildcard with props
                var stmtWildcardWProps =
                    env.CompileDeploy("@name('s0') select *, IntPrimitive + 2, 'x'||TheString||'x' from SupportBean")
                        .Statement("s0");
                TryAssertionWildcardWProps(env, stmtWildcardWProps, new SupportSubscriberRowByRowSpecificNStmt());
                TryAssertionWildcardWProps(env, stmtWildcardWProps, new SupportSubscriberRowByRowSpecificWStmt());
                env.UndeployAll();

                // nested
                var stmtNested =
                    env.CompileDeploy("@name('s0') select Nested, Nested.NestedNested from SupportBeanComplexProps")
                        .Statement("s0");
                TryAssertionNested(env, stmtNested, new SupportSubscriberRowByRowSpecificNStmt());
                TryAssertionNested(env, stmtNested, new SupportSubscriberRowByRowSpecificWStmt());
                env.UndeployAll();

                // enum
                var stmtEnum = env.CompileDeploy("@name('s0') select TheString, SupportEnum from SupportBeanWithEnum")
                    .Statement("s0");
                TryAssertionEnum(env, stmtEnum, new SupportSubscriberRowByRowSpecificNStmt());
                TryAssertionEnum(env, stmtEnum, new SupportSubscriberRowByRowSpecificWStmt());
                env.UndeployAll();

                // null-typed select value
                var stmtNullSelected = env.CompileDeploy("@name('s0') select null, LongBoxed from SupportBean")
                    .Statement("s0");
                TryAssertionNullSelected(env, stmtNullSelected, new SupportSubscriberRowByRowSpecificNStmt());
                TryAssertionNullSelected(env, stmtNullSelected, new SupportSubscriberRowByRowSpecificWStmt());
                env.UndeployAll();

                // widening
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionWidening(env, rep, new SupportSubscriberRowByRowSpecificNStmt());
                }

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
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
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionOutputLimitNoJoin(env, rep, new SupportSubscriberRowByRowSpecificNStmt());
                }

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionOutputLimitNoJoin(env, rep, new SupportSubscriberRowByRowSpecificWStmt());
                }

                // output limit join
                TryAssertionOutputLimitJoin(env, new SupportSubscriberRowByRowSpecificNStmt());
                TryAssertionOutputLimitJoin(env, new SupportSubscriberRowByRowSpecificWStmt());

                // binding-to-map
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionBindMap(env, rep, new SupportSubscriberMultirowMapNStmt());
                }

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionBindMap(env, rep, new SupportSubscriberMultirowMapWStmt());
                }

                // binding-to-objectarray
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionBindObjectArr(env, rep, new SupportSubscriberMultirowObjectArrayNStmt());
                }

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionBindObjectArr(env, rep, new SupportSubscriberMultirowObjectArrayWStmt());
                }

                // binding-to-underlying-array
                TryAssertionBindWildcardIRStream(env, new SupportSubscriberMultirowUnderlyingNStmt());
                TryAssertionBindWildcardIRStream(env, new SupportSubscriberMultirowUnderlyingWStmt());

                // Object[] and "Object..." binding
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayPlainNStmt());
                    TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayPlainWStmt());
                    TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayVarargNStmt());
                    TryAssertionObjectArrayDelivery(env, rep, new SupportSubscriberRowByRowObjectArrayVarargWStmt());
                }

                // Map binding
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionRowMapDelivery(env, rep, new SupportSubscriberRowByRowMapNStmt());
                    TryAssertionRowMapDelivery(env, rep, new SupportSubscriberRowByRowMapWStmt());
                }

                // static methods
                TryAssertionStaticMethod(env);

                // IR stream individual calls
                TryAssertionBindUpdateIRStream(env, new SupportSubscriberRowByRowFullNStmt());
                TryAssertionBindUpdateIRStream(env, new SupportSubscriberRowByRowFullWStmt());

                // no-params subscriber
                var stmtNoParamsSubscriber =
                    env.CompileDeploy("@name('s0') select null from SupportBean").Statement("s0");
                TryAssertionNoParams(env, stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseNStmt());
                TryAssertionNoParams(env, stmtNoParamsSubscriber, new SupportSubscriberNoParamsBaseWStmt());
                env.UndeployAll();

                // named-method subscriber
                var stmtNamedMethod =
                    env.CompileDeploy("@name('s0') select TheString from SupportBean").Statement("s0");
                TryAssertionNamedMethod(
                    env,
                    stmtNamedMethod,
                    new SupportSubscriberMultirowUnderlyingNamedMethodNStmt());
                TryAssertionNamedMethod(
                    env,
                    stmtNamedMethod,
                    new SupportSubscriberMultirowUnderlyingNamedMethodWStmt());
                env.UndeployAll();

                // prefer the EPStatement-footprint over the non-EPStatement footprint
                TryAssertionPreferEPStatement(env);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberSubscriberAndListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public insert into A1 select s.*, 1 as a from SupportBean as s", path);
                var stmt = env.CompileDeploy("@name('s0') select a1.* from A1 as a1", path).Statement("s0");

                var listener = new SupportUpdateListener();
                var subscriber = new SupportSubscriberRowByRowObjectArrayPlainNStmt();

                stmt.AddListener(listener);
                stmt.SetSubscriber(subscriber);
                env.SendEventBean(new SupportBean("E1", 1));

                var theEvent = listener.AssertOneGetNewAndReset();
                ClassicAssert.AreEqual("E1", theEvent.Get("TheString"));
                ClassicAssert.AreEqual(1, theEvent.Get("IntPrimitive"));
                Assert.That(theEvent.Underlying, Is.InstanceOf<Pair<object, IDictionary<string, object>>>());

                foreach (var property in stmt.EventType.PropertyNames) {
                    var getter = stmt.EventType.GetGetter(property);
                    getter.Get(theEvent);
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private static void TryAssertionJustWildcard(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmt.SetSubscriber(subscriber);
            var theEvent = new SupportBean("E2", 1);
            env.SendEventBean(theEvent);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { theEvent });
        }

        private static void TryAssertionBindUpdateIRStream(
            RegressionEnvironment env,
            SupportSubscriberRowByRowFullBase subscriber)
        {
            var stmtText = "@name('s0') select irstream TheString, IntPrimitive from SupportBean" + "#length_batch(2)";
            var stmt = env.CompileDeploy(stmtText).Statement("s0");
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                2,
                0,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } },
                null);

            env.SendEventBean(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                2,
                2,
                new object[][] { new object[] { "E3", 3 }, new object[] { "E4", 4 } },
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

            env.UndeployAll();
        }

        private static void TryAssertionBindObjectArr(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberMultirowObjectArrayBase subscriber)
        {
            var stmtText =
                $"{eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedStringInt))} @name('s0') select irstream TheString, IntPrimitive from SupportBean#length_batch(2)";
            var stmt = env.CompileDeploy(stmtText).Statement("s0");
            stmt.SetSubscriber(subscriber);
            ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } },
                null);

            env.SendEventBean(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new object[][] { new object[] { "E3", 3 }, new object[] { "E4", 4 } },
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

            env.UndeployAll();
        }

        private static void TryAssertionBindMap(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberMultirowMapBase subscriber)
        {
            var stmtText =
                $"{eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedStringInt))} @name('s0') select irstream TheString, IntPrimitive from SupportBean#length_batch(2)";
            var stmt = env.CompileDeploy(stmtText).Statement("s0");
            stmt.SetSubscriber(subscriber);
            ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } },
                null);

            env.SendEventBean(new SupportBean("E3", 3));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E4", 4));
            subscriber.AssertOneReceivedAndReset(
                stmt,
                FIELDS,
                new object[][] { new object[] { "E3", 3 }, new object[] { "E4", 4 } },
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

            env.UndeployAll();
        }

        private static void TryAssertionWidening(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    $"{eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedWidenedEvent))} @name('s0') select BytePrimitive, IntPrimitive, LongPrimitive, FloatPrimitive from SupportBean(TheString='E1')")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            var bean = new SupportBean();
            bean.TheString = "E1";
            bean.BytePrimitive = 1;
            bean.IntPrimitive = 2;
            bean.LongPrimitive = 3;
            bean.FloatPrimitive = 4;
            env.SendEventBean(bean);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { 1, 2L, 3d, 4d });

            env.UndeployAll();
        }

        private static void TryAssertionObjectArrayDelivery(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowObjectArrayBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    $"{eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedStringInt))} @name('s0') select TheString, IntPrimitive from SupportBean#unique(TheString)")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertOneAndReset(stmt, new object[] { "E1", 1 });

            env.SendEventBean(new SupportBean("E2", 10));
            subscriber.AssertOneAndReset(stmt, new object[] { "E2", 10 });

            env.UndeployAll();
        }

        private static void TryAssertionRowMapDelivery(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowMapBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    $"{eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedStringInt))} @name('s0') select irstream TheString, IntPrimitive from SupportBean#unique(TheString)")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertIRStreamAndReset(stmt, FIELDS, new object[] { "E1", 1 }, null);

            env.SendEventBean(new SupportBean("E2", 10));
            subscriber.AssertIRStreamAndReset(stmt, FIELDS, new object[] { "E2", 10 }, null);

            env.SendEventBean(new SupportBean("E1", 2));
            subscriber.AssertIRStreamAndReset(stmt, FIELDS, new object[] { "E1", 2 }, new object[] { "E1", 1 });

            env.UndeployAll();
        }

        private static void TryAssertionNested(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmt.SetSubscriber(subscriber);

            var theEvent = SupportBeanComplexProps.MakeDefaultBean();
            env.SendEventBean(theEvent);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { theEvent.Nested, theEvent.Nested.NestedNested });
        }

        private static void TryAssertionEnum(
            RegressionEnvironment env,
            EPStatement stmtEnum,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmtEnum.SetSubscriber(subscriber);

            var theEvent = new SupportBeanWithEnum("abc", SupportEnum.ENUM_VALUE_1);
            env.SendEventBean(theEvent);
            subscriber.AssertOneReceivedAndReset(stmtEnum, new object[] { theEvent.TheString, theEvent.SupportEnum });
        }

        private static void TryAssertionNullSelected(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            stmt.SetSubscriber(subscriber);
            env.SendEventBean(new SupportBean());
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { null, null });
        }

        private static void TryAssertionStreamSelectWJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@name('s0') select null, s1, s0 from SupportBean#keepall as s0, SupportMarketDataBean#keepall as s1 where s0.TheString = s1.Symbol")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { null, s1, s0 });

            env.UndeployAll();
        }

        private static void TryAssertionBindWildcardJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@name('s0') select * from SupportBean#keepall as s0, SupportMarketDataBean#keepall as s1 where s0.TheString = s1.Symbol")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { s0, s1 });

            env.UndeployAll();
        }

        private static void TryAssertionStreamWildcardJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@name('s0') select TheString || '<', s1.* as s1, s0.* as s0 from SupportBean#keepall as s0, SupportMarketDataBean#keepall as s1 where s0.TheString = s1.Symbol")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportMarketDataBean("E1", 0, 0L, "");
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { "E1<", s1, s0 });

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
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { s0, 102, "xE1x" });
        }

        private static void TryAssertionOutputLimitNoJoin(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    $"{eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedStringInt))} @name('s0') select TheString, IntPrimitive from SupportBean output every 2 events")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);
            ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E2", 2));
            subscriber.AssertMultipleReceivedAndReset(
                stmt,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

            env.UndeployAll();
        }

        private static void TryAssertionOutputLimitJoin(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy(
                    "@name('s0') select TheString, IntPrimitive from SupportBean#keepall, SupportMarketDataBean#keepall where Symbol = TheString output every 2 events")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportMarketDataBean("E1", 0, 1L, ""));
            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertNoneReceived();

            env.SendEventBean(new SupportBean("E1", 2));
            subscriber.AssertMultipleReceivedAndReset(
                stmt,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E1", 2 } });
            env.UndeployAll();
        }

        private static void TryAssertionRStreamSelect(
            RegressionEnvironment env,
            SupportSubscriberRowByRowSpecificBase subscriber)
        {
            var stmt = env.CompileDeploy("@name('s0') select rstream s0 from SupportBean#unique(TheString) as s0")
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
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { s0 });

            env.UndeployAll();
        }

        private static void TryAssertionBindWildcardIRStream(
            RegressionEnvironment env,
            SupportSubscriberMultirowUnderlyingBase subscriber)
        {
            var stmt = env.CompileDeploy("@name('s0') select irstream * from SupportBean#length_batch(2)")
                .Statement("s0");
            stmt.SetSubscriber(subscriber);

            var s0 = new SupportBean("E1", 100);
            var s1 = new SupportBean("E2", 200);
            env.SendEventBean(s0);
            env.SendEventBean(s1);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { s0, s1 }, null);

            var s2 = new SupportBean("E3", 300);
            var s3 = new SupportBean("E4", 400);
            env.SendEventBean(s2);
            env.SendEventBean(s3);
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { s2, s3 }, new object[] { s0, s1 });

            env.UndeployAll();
        }

        private static void TryAssertionStaticMethod(RegressionEnvironment env)
        {
            var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");

            var subscriber = new SupportSubscriberRowByRowStatic();
            stmt.SetSubscriber(subscriber);
            env.SendEventBean(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[][] { new object[] { "E1", 100 } },
                SupportSubscriberRowByRowStatic.GetAndResetIndicate());

            var subscriberWStmt = new SupportSubscriberRowByRowStaticWStatement();
            stmt.SetSubscriber(subscriberWStmt);
            env.SendEventBean(new SupportBean("E2", 200));
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[][] { new object[] { "E2", 200 } },
                SupportSubscriberRowByRowStaticWStatement.Indicate);
            ClassicAssert.AreEqual(stmt, SupportSubscriberRowByRowStaticWStatement.Statements[0]);
            subscriberWStmt.Reset();

            env.UndeployAll();
        }

        private static void TryAssertionNoParams(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberNoParamsBase subscriber)
        {
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportBean());
            subscriber.AssertCalledAndReset(stmt);
        }

        private static void TryAssertionNamedMethod(
            RegressionEnvironment env,
            EPStatement stmt,
            SupportSubscriberMultirowUnderlyingBase subscriber)
        {
            stmt.SetSubscriber(subscriber, "SomeNewDataMayHaveArrived");

            env.SendEventBean(new SupportBean("E1", 1));
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { "E1" }, null);
        }

        private static void TryAssertionPreferEPStatement(RegressionEnvironment env)
        {
            var subscriber = new SupportSubscriberUpdateBothFootprints();
            var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");
            stmt.SetSubscriber(subscriber);

            env.SendEventBean(new SupportBean("E1", 10));
            subscriber.AssertOneReceivedAndReset(stmt, new object[] { "E1", 10 });

            env.UndeployAll();
        }

        private class ClientRuntimeSubscriberBindWildcardJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtOne = env.CompileDeploy("@name('s0') select * from SupportBean").Statement("s0");
                TryInvalid(this, stmtOne, "Subscriber object does not provide a public method by name 'Update'");
                TryInvalid(
                    new DummySubscriberEmptyUpd(),
                    stmtOne,
                    "No suitable subscriber method named 'Update' found, expecting a method that takes 1 parameter of type com.espertech.esper.common.internal.support.SupportBean");
                TryInvalid(
                    new DummySubscriberMultipleUpdate(),
                    stmtOne,
                    "No suitable subscriber method named 'Update' found, expecting a method that takes 1 parameter of type com.espertech.esper.common.internal.support.SupportBean");
                TryInvalid(
                    new DummySubscriberUpdate(),
                    stmtOne,
                    "Subscriber method named 'Update' for parameter number 1 is not assignable, expecting type 'com.espertech.esper.common.internal.support.SupportBean' but found type 'com.espertech.esper.regressionlib.support.bean.SupportMarketDataBean'");
                TryInvalid(
                    new DummySubscriberPrivateUpd(),
                    stmtOne,
                    "Subscriber object does not provide a public method by name 'Update'");
                env.UndeployModuleContaining("s0");

                var stmtTwo = env.CompileDeploy("@name('s0') select IntPrimitive from SupportBean").Statement("s0");
                var message = "Subscriber 'UpdateRStream' method footprint must match 'Update' method footprint";
                TryInvalid(new DummySubscriberMismatchUpdateRStreamOne(), stmtTwo, message);
                TryInvalid(new DummySubscriberMismatchUpdateRStreamTwo(), stmtTwo, message);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberInvocationTargetEx : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // smoke test, need to consider log file; test for ESPER-331
                var stmt = env.CompileDeploy("@name('s0') select * from SupportMarketDataBean").Statement("s0");
                stmt.Subscriber = new DummySubscriberException();
                stmt.AddListener(
                    new DelegateUpdateListener(
                        (
                                sender,
                                args) =>
                            throw new EPRuntimeException("test exception 2")));
                stmt.AddListenerWithReplay(
                    new DelegateUpdateListener(
                        (
                                sender,
                                args) =>
                            throw new EPRuntimeException("test exception 3")));

                // no exception expected
                env.SendEventBean(new SupportMarketDataBean("IBM", 0, 0L, ""));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberStartStopStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subscriber = new SubscriberInterface();

                var compiled = env.Compile("@name('s0') select * from SupportMarkerInterface");
                var stmt = env.Deploy(compiled).Statement("s0");
                stmt.SetSubscriber(subscriber);

                var a1 = new SupportBean_A("A1");
                env.SendEventBean(a1);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { a1 }, subscriber.GetAndResetIndicate().ToArray());

                var b1 = new SupportBean_B("B1");
                env.SendEventBean(b1);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { b1 }, subscriber.GetAndResetIndicate().ToArray());

                env.UndeployAll();

                var c1 = new SupportBean_C("C1");
                env.SendEventBean(c1);
                ClassicAssert.AreEqual(0, subscriber.GetAndResetIndicate().Count);

                env.Deploy(compiled).Statement("s0").SetSubscriber(subscriber);

                var d1 = new SupportBean_D("D1");
                env.SendEventBean(d1);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { d1 }, subscriber.GetAndResetIndicate().ToArray());

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberVariables : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "myvar".SplitCsv();
                var subscriberCreateVariable = new SubscriberMap();
                var stmtTextCreate = "@name('s0') @public create variable string myvar = 'abc'";
                var stmt = env.CompileDeploy(stmtTextCreate, path).Statement("s0");
                stmt.SetSubscriber(subscriberCreateVariable);

                var subscriberSetVariable = new SubscriberMap();
                var stmtTextSet = "@name('s1') on SupportBean set myvar = TheString";
                stmt = env.CompileDeploy(stmtTextSet, path).Statement("s1");
                stmt.SetSubscriber(subscriberSetVariable);

                env.SendEventBean(new SupportBean("def", 1));
                EPAssertionUtil.AssertPropsMap(
                    subscriberCreateVariable.GetAndResetIndicate()[0],
                    fields,
                    new object[] { "def" });
                EPAssertionUtil.AssertPropsMap(
                    subscriberSetVariable.GetAndResetIndicate()[0],
                    fields,
                    new object[] { "def" });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionNamedWindow(env, EventRepresentationChoice.MAP);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private static void TryAssertionNamedWindow(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var fields = "key,value".SplitCsv();
            var path = new RegressionPath();
            var subscriberNamedWindow = new SubscriberMap();
            var stmtTextCreate =
                $"{eventRepresentationEnum.GetAnnotationText()} @name('create') @public create window MyWindow#keepall as select TheString as key, IntPrimitive as value from SupportBean";
            var stmt = env.CompileDeploy(stmtTextCreate, path).Statement("create");
            stmt.SetSubscriber(subscriberNamedWindow);

            var subscriberInsertInto = new SubscriberFields();
            var stmtTextInsertInto =
                "@name('insert') insert into MyWindow select TheString as key, IntPrimitive as value from SupportBean";
            stmt = env.CompileDeploy(stmtTextInsertInto, path).Statement("insert");
            stmt.SetSubscriber(subscriberInsertInto);

            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(
                subscriberNamedWindow.GetAndResetIndicate()[0],
                fields,
                new object[] { "E1", 1 });
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[][] { new object[] { "E1", 1 } },
                subscriberInsertInto.GetAndResetIndicate());

            // test on-delete
            var subscriberDelete = new SubscriberMap();
            var stmtTextDelete =
                "@name('ondelete') on SupportMarketDataBean s0 delete from MyWindow s1 where s0.Symbol = s1.key";
            stmt = env.CompileDeploy(stmtTextDelete, path).Statement("ondelete");
            stmt.SetSubscriber(subscriberDelete);

            env.SendEventBean(new SupportMarketDataBean("E1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(subscriberDelete.GetAndResetIndicate()[0], fields, new object[] { "E1", 1 });

            // test on-select
            var subscriberSelect = new SubscriberMap();
            var stmtTextSelect = "@name('onselect') on SupportMarketDataBean s0 select key, value from MyWindow s1";
            stmt = env.CompileDeploy(stmtTextSelect, path).Statement("onselect");
            stmt.SetSubscriber(subscriberSelect);

            env.SendEventBean(new SupportBean("E2", 2));
            env.SendEventBean(new SupportMarketDataBean("M1", 0, 1L, ""));
            EPAssertionUtil.AssertPropsMap(subscriberSelect.GetAndResetIndicate()[0], fields, new object[] { "E2", 2 });

            env.UndeployAll();
        }

        private class ClientRuntimeSubscriberSimpleSelectUpdateOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subscriber = new SupportSubscriberRowByRowSpecificNStmt();
                var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean#lastevent")
                    .Statement("s0");
                stmt.SetSubscriber(subscriber);

                // get statement, attach listener
                var listener = new SupportUpdateListener();
                stmt.AddListener(listener);

                // send event
                env.SendEventBean(new SupportBean("E1", 100));
                subscriber.AssertOneReceivedAndReset(stmt, new object[] { "E1", 100 });
                EPAssertionUtil.AssertPropsPerRow(
                    stmt.GetEnumerator(),
                    FIELDS,
                    new object[][] { new object[] { "E1", 100 } });
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[] { "E1", 100 });

                // remove listener
                stmt.RemoveAllListeners();

                // send event
                env.SendEventBean(new SupportBean("E2", 200));
                subscriber.AssertOneReceivedAndReset(stmt, new object[] { "E2", 200 });
                EPAssertionUtil.AssertPropsPerRow(
                    stmt.GetEnumerator(),
                    FIELDS,
                    new object[][] { new object[] { "E2", 200 } });
                ClassicAssert.IsFalse(listener.IsInvoked);

                // add listener
                var stmtAwareListener = new SupportUpdateListener();
                stmt.AddListener(stmtAwareListener);

                // send event
                env.SendEventBean(new SupportBean("E3", 300));
                subscriber.AssertOneReceivedAndReset(stmt, new object[] { "E3", 300 });
                EPAssertionUtil.AssertPropsPerRow(
                    stmt.GetEnumerator(),
                    FIELDS,
                    new object[][] { new object[] { "E3", 300 } });
                EPAssertionUtil.AssertProps(
                    stmtAwareListener.AssertOneGetNewAndReset(),
                    FIELDS,
                    new object[] { "E3", 300 });

                // subscriber with EPStatement in the footprint
                stmt.RemoveAllListeners();
                var subsWithStatement = new SupportSubscriberRowByRowSpecificWStmt();
                stmt.Subscriber = subsWithStatement;
                env.SendEventBean(new SupportBean("E10", 999));
                subsWithStatement.AssertOneReceivedAndReset(stmt, new object[] { "E10", 999 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberPerformanceSyntheticUndelivered : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var numLoop = 100000;
                env.CompileDeploy("select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)");

                var delta = PerformanceObserver.TimeMillis(() => {
                    for (var i = 0; i < numLoop; i++) {
                        env.SendEventBean(new SupportBean("E1", 1000 + i));
                    }
                });

                Assert.That(delta, Is.LessThan(1000));
                env.UndeployAll();
            }
        }

        private class ClientRuntimeSubscriberPerformanceSynthetic : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var numLoop = 100000;
                var stmt = env
                    .CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)")
                    .Statement("s0");
                IList<object[]> results = new List<object[]>();

                UpdateListener listener = new DelegateUpdateListener(
                    (
                        sender,
                        args) => {
                        var newEvents = args.NewEvents;
                        var theString = (string)newEvents[0].Get("TheString");
                        var val = (int)newEvents[0].Get("IntPrimitive");
                        results.Add(new object[] { theString, val });
                    });
                stmt.AddListener(listener);

                var start = PerformanceObserver.MilliTime;
                for (var i = 0; i < numLoop; i++) {
                    env.SendEventBean(new SupportBean("E1", 1000 + i));
                }

                var end = PerformanceObserver.MilliTime;

                ClassicAssert.AreEqual(numLoop, results.Count);
                for (var i = 0; i < numLoop; i++) {
                    EPAssertionUtil.AssertEqualsAnyOrder(results[i], new object[] { "E1", 1000 + i });
                }

                Assert.That(end - start, Is.LessThan(1000));

                env.UndeployAll();
            }
        }

        private class ClientRuntimeSubscriberDelegateBindings : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");

                // typed columns
                var typed = new List<object[]>();
                stmt.Subscriber = new Action<string, int>((theString, intPrimitive) => typed.Add(new object[] { theString, intPrimitive }));
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E1", 1 } }, typed.ToArray());

                // typed columns with widening
                var widened = new List<object[]>();
                stmt.SetSubscriber(new Action<string, long>((theString, intPrimitive) => widened.Add(new object[] { theString, intPrimitive })));
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E2", 2L } }, widened.ToArray());

                // map row
                var maps = new List<IDictionary<string, object>>();
                stmt.SetSubscriber(new Action<IDictionary<string, object>>(row => maps.Add(row)));
                env.SendEventBean(new SupportBean("E3", 3));
                ClassicAssert.AreEqual(1, maps.Count);
                EPAssertionUtil.AssertPropsMap(maps[0], FIELDS, "E3", 3);

                // object-array row
                var rows = new List<object[]>();
                stmt.SetSubscriber(new Action<object[]>(row => rows.Add(row)));
                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E4", 4 } }, rows.ToArray());

                // statement as the first parameter
                var statements = new List<EPStatement>();
                var withStmt = new List<object[]>();
                stmt.SetSubscriber(
                    new Action<EPStatement, string, int>(
                        (statement, theString, intPrimitive) => {
                            statements.Add(statement);
                            withStmt.Add(new object[] { theString, intPrimitive });
                        }));
                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E5", 5 } }, withStmt.ToArray());
                ClassicAssert.AreEqual(1, statements.Count);
                ClassicAssert.AreSame(stmt, statements[0]);

                // statement and map row
                var mapsWStmt = new List<IDictionary<string, object>>();
                stmt.SetSubscriber(
                    new Action<EPStatement, IDictionary<string, object>>(
                        (statement, row) => {
                            ClassicAssert.AreSame(stmt, statement);
                            mapsWStmt.Add(row);
                        }));
                env.SendEventBean(new SupportBean("E6", 6));
                ClassicAssert.AreEqual(1, mapsWStmt.Count);
                EPAssertionUtil.AssertPropsMap(mapsWStmt[0], FIELDS, "E6", 6);

                // row arrays (insert and remove stream)
                var rowArrays = new List<object[][]>();
                stmt.SetSubscriber(new Action<object[][], object[][]>((insertStream, removeStream) => rowArrays.Add(insertStream)));
                env.SendEventBean(new SupportBean("E7", 7));
                ClassicAssert.AreEqual(1, rowArrays.Count);
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E7", 7 } }, rowArrays[0]);

                // explicit method name
                var named = new List<string>();
                stmt.SetSubscriber(new Action<string, int>((theString, intPrimitive) => named.Add(theString)), "Invoke");
                env.SendEventBean(new SupportBean("E8", 8));
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E8" }, named.ToArray());

                // multicast delegate: every target receives the row
                var first = new List<string>();
                var second = new List<string>();
                var multicast = new Action<string, int>((theString, intPrimitive) => first.Add(theString));
                multicast += (theString, intPrimitive) => second.Add(theString);
                stmt.SetSubscriber(multicast);
                env.SendEventBean(new SupportBean("E9", 9));
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E9" }, first.ToArray());
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E9" }, second.ToArray());

                // a delegate subscriber is returned as set
                ClassicAssert.AreSame(multicast, stmt.Subscriber);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select * from SupportBean").Statement("s0");
                TryInvalid(
                    new Action<SupportMarketDataBean>(bean => { }),
                    stmt,
                    "Subscriber method named 'Invoke' for parameter number 1 is not assignable, expecting type 'com.espertech.esper.common.internal.support.SupportBean' but found type 'com.espertech.esper.regressionlib.support.bean.SupportMarketDataBean'");
                TryInvalid(
                    new Action<SupportBean, int>((bean, value) => { }),
                    stmt,
                    "No suitable subscriber method named 'Invoke' found, expecting a method that takes 1 parameter of type com.espertech.esper.common.internal.support.SupportBean");
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateSeparateThread : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select TheString as c0 from SupportBean").Statement("s0");

                var latch = new CountDownLatch(1);
                IList<string> values = new List<string>();
                stmt.Subscriber = new Action<string>(
                    value => {
                        values.Add(value);
                        latch.CountDown();
                    });

                var executorService = Executors.NewSingleThreadExecutor();
                executorService.Submit(() => env.SendEventBean(new SupportBean("hello", 1)));
                ClassicAssert.IsTrue(latch.Await(TimeSpan.FromSeconds(10)));
                executorService.Shutdown();

                ClassicAssert.AreEqual(1, values.Count);
                ClassicAssert.AreEqual("hello", values[0]);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateRemoveStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env
                    .CompileDeploy("@name('s0') select irstream TheString, IntPrimitive from SupportBean#length(1)")
                    .Statement("s0");

                // a typed delegate has no 'UpdateRStream' counterpart: it receives the insert stream only
                var typed = new List<object[]>();
                stmt.SetSubscriber(new Action<string, int>((theString, intPrimitive) => typed.Add(new object[] { theString, intPrimitive })));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } },
                    typed.ToArray());

                // a row-array delegate receives both streams
                var insertStreams = new List<object[][]>();
                var removeStreams = new List<object[][]>();
                stmt.SetSubscriber(
                    new Action<object[][], object[][]>(
                        (insertStream, removeStream) => {
                            insertStreams.Add(insertStream);
                            removeStreams.Add(removeStream);
                        }));
                env.SendEventBean(new SupportBean("E3", 3));
                ClassicAssert.AreEqual(1, insertStreams.Count);
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E3", 3 } }, insertStreams[0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E2", 2 } }, removeStreams[0]);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateUnderlying : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select * from SupportBean").Statement("s0");

                var beans = new List<SupportBean>();
                stmt.SetSubscriber(new Action<SupportBean>(bean => beans.Add(bean)));
                var sent = new SupportBean("E1", 1);
                env.SendEventBean(sent);
                ClassicAssert.AreEqual(1, beans.Count);
                ClassicAssert.AreSame(sent, beans[0]);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateMulticastException : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");

                var first = new List<string>();
                var last = new List<string>();
                var multicast = new Action<string, int>((theString, intPrimitive) => first.Add(theString));
                multicast += (theString, intPrimitive) => throw new EPRuntimeException("delegate-generated");
                multicast += (theString, intPrimitive) => last.Add(theString);
                stmt.SetSubscriber(multicast);

                // the exception is logged by the subscriber exception handling and does not reach the sender;
                // targets after the throwing one are not invoked
                Assert.DoesNotThrow(() => env.SendEventBean(new SupportBean("E1", 1)));
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1" }, first.ToArray());
                ClassicAssert.AreEqual(0, last.Count);

                // delivery continues for subsequent events
                Assert.DoesNotThrow(() => env.SendEventBean(new SupportBean("E2", 2)));
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1", "E2" }, first.ToArray());
                ClassicAssert.AreEqual(0, last.Count);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateRemoveAndSwap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");

                // set, then remove
                var received = new List<string>();
                var action = new Action<string, int>((theString, intPrimitive) => received.Add(theString));
                stmt.Subscriber = action;
                env.SendEventBean(new SupportBean("E1", 1));
                stmt.Subscriber = null;
                ClassicAssert.IsNull(stmt.Subscriber);
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1" }, received.ToArray());

                // swap delegate to object subscriber
                stmt.Subscriber = action;
                env.SendEventBean(new SupportBean("E3", 3));
                var objectSubscriber = new SubscriberFields();
                stmt.Subscriber = objectSubscriber;
                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1", "E3" }, received.ToArray());
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[][] { new object[] { "E4", 4 } },
                    objectSubscriber.GetAndResetIndicate().ToArray());

                // and back
                stmt.Subscriber = action;
                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1", "E3", "E5" }, received.ToArray());
                ClassicAssert.AreEqual(0, objectSubscriber.GetAndResetIndicate().Count);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateWrongMethodName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");
                try {
                    stmt.SetSubscriber(new Action<string, int>((theString, intPrimitive) => { }), "Update");
                    Assert.Fail();
                }
                catch (EPSubscriberException ex) {
                    ClassicAssert.AreEqual("Subscriber object does not provide a public method by name 'Update'", ex.Message);
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberDelegateFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");

                // a delegate with a return value is accepted; the return value is ignored
                var received = new List<object[]>();
                stmt.SetSubscriber(
                    new Func<string, int, object>(
                        (theString, intPrimitive) => {
                            received.Add(new object[] { theString, intPrimitive });
                            return "ignored";
                        }));
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertEqualsExactOrder(new object[][] { new object[] { "E1", 1 } }, received.ToArray());

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class ClientRuntimeSubscriberBatchDeliveryException : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtCols = env.CompileDeploy("@name('s0') select TheString, IntPrimitive from SupportBean").Statement("s0");

                // object subscriber with a row-array 'Update'
                var objectSubscriber = new DummySubscriberObjectArrThrowsFirst();
                TryAssertionBatchThrowsFirst(env, stmtCols, objectSubscriber, () => objectSubscriber.Calls);

                // row-array delegates
                var calls = 0;
                TryAssertionBatchThrowsFirst(
                    env, stmtCols,
                    new Action<object[][], object[][]>((insertStream, removeStream) => ThrowOnFirst(ref calls)),
                    () => calls);
                calls = 0;
                TryAssertionBatchThrowsFirst(
                    env, stmtCols,
                    new Action<EPStatement, object[][], object[][]>((statement, insertStream, removeStream) => ThrowOnFirst(ref calls)),
                    () => calls);

                // map-array delegates
                calls = 0;
                TryAssertionBatchThrowsFirst(
                    env, stmtCols,
                    new Action<IDictionary<string, object>[], IDictionary<string, object>[]>((insertStream, removeStream) => ThrowOnFirst(ref calls)),
                    () => calls);
                calls = 0;
                TryAssertionBatchThrowsFirst(
                    env, stmtCols,
                    new Action<EPStatement, IDictionary<string, object>[], IDictionary<string, object>[]>(
                        (statement, insertStream, removeStream) => ThrowOnFirst(ref calls)),
                    () => calls);
                env.UndeployAll();

                // typed-array delegates
                var stmtUnd = env.CompileDeploy("@name('s0') select * from SupportBean").Statement("s0");
                calls = 0;
                TryAssertionBatchThrowsFirst(
                    env, stmtUnd,
                    new Action<SupportBean[], SupportBean[]>((insertStream, removeStream) => ThrowOnFirst(ref calls)),
                    () => calls);
                calls = 0;
                TryAssertionBatchThrowsFirst(
                    env, stmtUnd,
                    new Action<EPStatement, SupportBean[], SupportBean[]>((statement, insertStream, removeStream) => ThrowOnFirst(ref calls)),
                    () => calls);
                env.UndeployAll();
            }

            private static void ThrowOnFirst(ref int calls)
            {
                calls++;
                if (calls == 1) {
                    throw new EPRuntimeException("subscriber-generated");
                }
            }

            private static void TryAssertionBatchThrowsFirst(
                RegressionEnvironment env,
                EPStatement stmt,
                object subscriber,
                Func<int> calls)
            {
                stmt.SetSubscriber(subscriber);

                // the exception is handled by the subscriber exception handling and not thrown to the sender
                Assert.DoesNotThrow(() => env.SendEventBean(new SupportBean("E1", 1)));
                ClassicAssert.AreEqual(1, calls.Invoke());

                // the next event is still delivered
                Assert.DoesNotThrow(() => env.SendEventBean(new SupportBean("E2", 2)));
                ClassicAssert.AreEqual(2, calls.Invoke());
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        public class DummySubscriberObjectArrThrowsFirst
        {
            public int Calls { get; private set; }

            public void Update(
                object[][] insertStream,
                object[][] removeStream)
            {
                Calls++;
                if (Calls == 1) {
                    throw new EPRuntimeException("DummySubscriberObjectArrThrowsFirst-generated");
                }
            }
        }

        private static void TryInvalid(
            object subscriber,
            EPStatement stmt,
            string message)
        {
            try {
                stmt.SetSubscriber(subscriber);
                Assert.Fail();
            }
            catch (EPSubscriberException ex) {
                ClassicAssert.AreEqual(message, ex.Message);
            }
        }

        public class DummySubscriberException
        {
            public void Update(SupportMarketDataBean bean)
            {
                throw new EPRuntimeException("DummySubscriberException-generated");
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
            private IList<object[]> indicate = new List<object[]>();

            public void Update(
                string key,
                int value)
            {
                indicate.Add(new object[] { key, value });
            }

            public IList<object[]> GetAndResetIndicate()
            {
                var result = indicate;
                indicate = new List<object[]>();
                return result;
            }
        }

        public class SubscriberInterface
        {
            private IList<SupportMarkerInterface> indicate = new List<SupportMarkerInterface>();

            public void Update(SupportMarkerInterface impl)
            {
                indicate.Add(impl);
            }

            public IList<SupportMarkerInterface> GetAndResetIndicate()
            {
                var result = indicate;
                indicate = new List<SupportMarkerInterface>();
                return result;
            }
        }

        public class SubscriberMap
        {
            private IList<IDictionary<string, object>> indicate = new List<IDictionary<string, object>>();

            public void Update(IDictionary<string, object> row)
            {
                indicate.Add(row);
            }

            public IList<IDictionary<string, object>> GetAndResetIndicate()
            {
                var result = indicate;
                indicate = new List<IDictionary<string, object>>();
                return result;
            }
        }

        public class MyLocalJsonProvidedWidenedEvent
        {
            public byte BytePrimitive;
            public int IntPrimitive;
            public long LongPrimitive;
            public float FloatPrimitive;
        }

        public class MyLocalJsonProvidedStringInt
        {
            public string TheString;
            public int? IntPrimitive;
        }
    }
} // end of namespace