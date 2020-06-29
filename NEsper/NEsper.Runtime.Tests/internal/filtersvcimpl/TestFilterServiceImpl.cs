///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.collection;
using com.espertech.esper.common;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestFilterServiceImpl : AbstractRuntimeTest
    {
        private SupportEventBeanFactory supportEventBeanFactory;

        [SetUp]
        public void SetUp()
        {
            supportEventBeanFactory = SupportEventBeanFactory.GetInstance(container);

            filterService = new FilterServiceLockCoarse(container.RWLockManager(), false);

            eventTypeOne = supportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            eventTypeTwo = supportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));

            filterSpecs = new List<Pair<EventType, FilterValueSetParam[][]>>();
            filterSpecs.Add(
                new Pair<EventType, FilterValueSetParam[][]>(
                    eventTypeOne,
                    SupportFilterSpecBuilder.Build(eventTypeOne, new object[0]).GetValueSet(null, null, null, null)));
            filterSpecs.Add(
                new Pair<EventType, FilterValueSetParam[][]>(
                    eventTypeOne,
                    SupportFilterSpecBuilder.Build(
                            eventTypeOne,
                            new object[] {
                                "IntPrimitive", FilterOperator.RANGE_CLOSED, 10, 20,
                                "TheString", FilterOperator.EQUAL, "HELLO",
                                "BoolPrimitive", FilterOperator.EQUAL, false,
                                "DoubleBoxed", FilterOperator.GREATER, 100d
                            })
                        .GetValueSet(null, null, null, null)));
            filterSpecs.Add(
                new Pair<EventType, FilterValueSetParam[][]>(
                    eventTypeTwo,
                    SupportFilterSpecBuilder.Build(eventTypeTwo, new object[0]).GetValueSet(null, null, null, null)));
            filterSpecs.Add(
                new Pair<EventType, FilterValueSetParam[][]>(
                    eventTypeTwo,
                    SupportFilterSpecBuilder.Build(
                            eventTypeTwo,
                            new object[] {
                                "MyInt", FilterOperator.RANGE_HALF_CLOSED, 1, 10,
                                "MyString", FilterOperator.EQUAL, "Hello"
                            })
                        .GetValueSet(null, null, null, null)));

            // Create callbacks and add
            filterCallbacks = new List<SupportFilterHandle>();
            for (var i = 0; i < filterSpecs.Count; i++)
            {
                filterCallbacks.Add(new SupportFilterHandle());
                filterService.Add(filterSpecs[i].First, filterSpecs[i].Second, filterCallbacks[i]);
            }

            // Create events
            matchesExpected = new List<int[]>();
            events = new List<EventBean>();

            events.Add(MakeTypeOneEvent(15, "HELLO", false, 101));
            matchesExpected.Add(new[] { 1, 1, 0, 0 });

            events.Add(MakeTypeTwoEvent("Hello", 100));
            matchesExpected.Add(new[] { 0, 0, 1, 0 });

            events.Add(MakeTypeTwoEvent("Hello", 1)); // eventNumber = 2
            matchesExpected.Add(new[] { 0, 0, 1, 0 });

            events.Add(MakeTypeTwoEvent("Hello", 2));
            matchesExpected.Add(new[] { 0, 0, 1, 1 });

            events.Add(MakeTypeOneEvent(15, "HELLO", true, 100));
            matchesExpected.Add(new[] { 1, 0, 0, 0 });

            events.Add(MakeTypeOneEvent(15, "HELLO", false, 99));
            matchesExpected.Add(new[] { 1, 0, 0, 0 });

            events.Add(MakeTypeOneEvent(9, "HELLO", false, 100));
            matchesExpected.Add(new[] { 1, 0, 0, 0 });

            events.Add(MakeTypeOneEvent(10, "no", false, 100));
            matchesExpected.Add(new[] { 1, 0, 0, 0 });

            events.Add(MakeTypeOneEvent(15, "HELLO", false, 999999)); // number 8
            matchesExpected.Add(new[] { 1, 1, 0, 0 });

            events.Add(MakeTypeTwoEvent("Hello", 10));
            matchesExpected.Add(new[] { 0, 0, 1, 1 });

            events.Add(MakeTypeTwoEvent("Hello", 11));
            matchesExpected.Add(new[] { 0, 0, 1, 0 });
        }

        private EventType eventTypeOne;
        private EventType eventTypeTwo;
        private FilterServiceLockCoarse filterService;
        private IList<Pair<EventType, FilterValueSetParam[][]>> filterSpecs;
        private IList<SupportFilterHandle> filterCallbacks;
        private IList<EventBean> events;
        private IList<int[]> matchesExpected;

        private EventBean MakeTypeOneEvent(
            int intPrimitive,
            string theString,
            bool boolPrimitive,
            double doubleBoxed)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.TheString = theString;
            bean.BoolPrimitive = boolPrimitive;
            bean.DoubleBoxed = doubleBoxed;
            return supportEventBeanFactory.CreateObject(bean);
        }

        private EventBean MakeTypeTwoEvent(
            string myString,
            int myInt)
        {
            var bean = new SupportBeanSimple(myString, myInt);
            return supportEventBeanFactory.CreateObject(bean);
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Test for removing a callback that is waiting to occur,
        ///     ie. a callback is removed which was a result of an evaluation and it
        ///     thus needs to be removed from the tree AND the current dispatch list.
        /// </summary>
        [Test, RunInApplicationDomain]
        public void TestActiveCallbackRemove()
        {
            var spec = SupportFilterSpecBuilder.Build(eventTypeOne, new object[0]).GetValueSet(null, null, null, null);
            var callbackTwo = new SupportFilterHandle();

            // callback that removes another matching filter spec callback
            var callbackOne = new MySupportFilterHandle(
                filterService,
                callbackTwo,
                eventTypeOne,
                spec);

            filterService.Add(eventTypeOne, spec, callbackOne);
            filterService.Add(eventTypeOne, spec, callbackTwo);

            // send event
            var theEvent = MakeTypeOneEvent(1, "HELLO", false, 1);
            var matches = new List<FilterHandle>();
            filterService.Evaluate(theEvent, (ICollection<FilterHandle>) matches, (ExprEvaluatorContext) TODO);
            foreach (var match in matches)
            {
                ((FilterHandleCallback) match).MatchFound(theEvent, null);
            }

            // Callback two MUST be invoked, was removed by callback one, but since the
            // callback invocation order should not matter, the second one MUST also execute
            Assert.That(callbackTwo.GetAndResetCountInvoked(), Is.EqualTo(1));
        }

        [Test, RunInApplicationDomain]
        public void TestEvalEvents()
        {
            for (var i = 0; i < events.Count; i++)
            {
                IList<FilterHandle> matchList = new List<FilterHandle>();
                filterService.Evaluate(events[i], (ICollection<FilterHandle>) matchList, (ExprEvaluatorContext) TODO);
                foreach (var match in matchList)
                {
                    var handle = (SupportFilterHandle) match;
                    handle.MatchFound(events[i], null);
                }

                var matches = matchesExpected[i];

                for (var j = 0; j < matches.Length; j++)
                {
                    SupportFilterHandle callback = filterCallbacks[j];

                    if (matches[j] != callback.GetAndResetCountInvoked())
                    {
                        log.Debug(".testEvalEvents Match failed, event=" + events[i].Underlying);
                        log.Debug(".testEvalEvents Match failed, eventNumber=" + i + " index=" + j);
                        Assert.IsTrue(false);
                    }
                }
            }
        }

        private class MySupportFilterHandle : SupportFilterHandle
        {
            private readonly FilterServiceLockCoarse filterService;
            private readonly FilterHandle callback;
            private readonly EventType eventType;
            private readonly FilterValueSetParam[][] spec;

            public MySupportFilterHandle(FilterServiceLockCoarse filterService,
                FilterHandle callback,
                EventType eventType,
                FilterValueSetParam[][] spec)
            {
                this.filterService = filterService;
                this.callback = callback;
                this.eventType = eventType;
                this.spec = spec;
            }

            public override void MatchFound(
                EventBean theEvent,
                ICollection<FilterHandleCallback> allStmtMatches)
            {
                log.Debug(".matchFound Removing callbackTwo");
                filterService.Remove(callback, eventType, spec);
            }
        }
    }
} // end of namespace
