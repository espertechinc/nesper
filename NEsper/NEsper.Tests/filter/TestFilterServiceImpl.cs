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
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;

using com.espertech.esper.compat.logging;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterServiceImpl 
    {
        private EventType _eventTypeOne;
        private EventType _eventTypeTwo;
        private FilterServiceLockCoarse _filterService;
        private List<FilterValueSet> _filterSpecs;
        private List<SupportFilterHandle> _filterCallbacks;
        private List<EventBean> _events;
        private List<int[]> _matchesExpected;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _filterService = new FilterServiceLockCoarse(
                _container.LockManager(), _container.RWLockManager(), false);
    
            _eventTypeOne = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            _eventTypeTwo = SupportEventTypeFactory.CreateBeanType(typeof(SupportBeanSimple));
    
            _filterSpecs = new List<FilterValueSet>();
            _filterSpecs.Add(SupportFilterSpecBuilder.Build(_eventTypeOne, new Object[0]).GetValueSet(null, null, null));
            _filterSpecs.Add(SupportFilterSpecBuilder.Build(_eventTypeOne, new Object[] {
                    "IntPrimitive", FilterOperator.RANGE_CLOSED, 10, 20,
                    "TheString", FilterOperator.EQUAL, "HELLO",
                    "BoolPrimitive", FilterOperator.EQUAL, false,
                    "DoubleBoxed", FilterOperator.GREATER, 100d} ).GetValueSet(null, null, null));
            _filterSpecs.Add(SupportFilterSpecBuilder.Build(_eventTypeTwo, new Object[0]).GetValueSet(null, null, null));
            _filterSpecs.Add(SupportFilterSpecBuilder.Build(_eventTypeTwo, new Object[] {
                    "MyInt", FilterOperator.RANGE_HALF_CLOSED, 1, 10,
                    "MyString", FilterOperator.EQUAL, "Hello" }).GetValueSet(null, null, null));
    
            // Create callbacks and add
            _filterCallbacks = new List<SupportFilterHandle>();
            for (int i = 0; i < _filterSpecs.Count; i++)
            {
                _filterCallbacks.Add(new SupportFilterHandle());
                _filterService.Add(_filterSpecs[i], _filterCallbacks[i]);
            }
    
            // Create events
            _matchesExpected = new List<int[]>();
            _events = new List<EventBean>();
    
            _events.Add(MakeTypeOneEvent(15, "HELLO", false, 101));
            _matchesExpected.Add(new[] {1, 1, 0, 0});
    
            _events.Add(MakeTypeTwoEvent("Hello", 100));
            _matchesExpected.Add(new[] {0, 0, 1, 0});

            _events.Add(MakeTypeTwoEvent("Hello", 1));       // eventNumber = 2
            _matchesExpected.Add(new[] {0, 0, 1, 0});

            _events.Add(MakeTypeTwoEvent("Hello", 2));
            _matchesExpected.Add(new[] {0, 0, 1, 1});

            _events.Add(MakeTypeOneEvent(15, "HELLO", true, 100));
            _matchesExpected.Add(new[] {1, 0, 0, 0});

            _events.Add(MakeTypeOneEvent(15, "HELLO", false, 99));
            _matchesExpected.Add(new[] {1, 0, 0, 0});

            _events.Add(MakeTypeOneEvent(9, "HELLO", false, 100));
            _matchesExpected.Add(new[] {1, 0, 0, 0});

            _events.Add(MakeTypeOneEvent(10, "no", false, 100));
            _matchesExpected.Add(new[] {1, 0, 0, 0});

            _events.Add(MakeTypeOneEvent(15, "HELLO", false, 999999));      // number 8
            _matchesExpected.Add(new[] {1, 1, 0, 0});

            _events.Add(MakeTypeTwoEvent("Hello", 10));
            _matchesExpected.Add(new[] {0, 0, 1, 1});

            _events.Add(MakeTypeTwoEvent("Hello", 11));
            _matchesExpected.Add(new[] {0, 0, 1, 0});
        }
    
        [Test]
        public void TestEvalEvents()
        {
            for (int i = 0; i < _events.Count; i++)
            {
                var matchList = new List<FilterHandle>();
                _filterService.Evaluate(_events[i], matchList);
                foreach (FilterHandle match in matchList)
                {
                    var handle = (SupportFilterHandle) match;
                    handle.MatchFound(_events[i], null);
                }
    
                int[] matches = _matchesExpected[i];
    
                for (int j = 0; j < matches.Length; j++)
                {
                    SupportFilterHandle callback = _filterCallbacks[j];
    
                    if (matches[j] != callback.GetAndResetCountInvoked())
                    {
                        Log.Debug(".testEvalEvents Match failed, theEvent=" + _events[i].Underlying);
                        Log.Debug(".testEvalEvents Match failed, eventNumber=" + i + " index=" + j);
                        Assert.Fail();
                    }
                }
            }
        }

        /// <summary>
        /// Test for removing a callback that is waiting to occur,
        /// ie. a callback is removed which was a result of an evaluation and it 
        /// thus needs to be removed from the tree AND the current dispatch list.
        /// </summary>
        [Test]
        public void TestActiveCallbackRemove()
        {
            var spec = SupportFilterSpecBuilder.Build(_eventTypeOne, new Object[0]).GetValueSet(null, null, null);
            var callbackTwo = new SupportFilterHandle();
    
            // callback that removes another matching filter spec callback
            Atomic<FilterServiceEntry> filterServiceEntryOne = new Atomic<FilterServiceEntry>();
            FilterHandleCallback callbackOne = new ProxyFilterHandleCallback
            {
                ProcStatementId = () => 1,
                ProcIsSubselect = () => false,
                ProcMatchFound = (e, allStmtMatches) =>
                {
                    Log.Debug(".matchFound Removing callbackTwo");
                    _filterService.Remove(callbackTwo, filterServiceEntryOne.Value);
                }
            };

            FilterServiceEntry filterServiceEntry = _filterService.Add(spec, callbackOne);
            filterServiceEntryOne.Set(filterServiceEntry);
            _filterService.Add(spec, callbackTwo);
    
            // send event
            var theEvent = MakeTypeOneEvent(1, "HELLO", false, 1);
            var matches = new List<FilterHandle>();
            _filterService.Evaluate(theEvent, matches);
            foreach (FilterHandle match in matches)
            {
                var handle = (FilterHandleCallback) match;
                handle.MatchFound(theEvent, null);
            }
    
            // Callback two MUST be invoked, was removed by callback one, but since the
            // callback invocation order should not matter, the second one MUST also execute
            Assert.AreEqual(1, callbackTwo.GetAndResetCountInvoked());
        }
    
        private static EventBean MakeTypeOneEvent(int intPrimitive, string stringValue, bool boolPrimitive, double doubleBoxed)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.TheString = stringValue;
            bean.BoolPrimitive = boolPrimitive;
            bean.DoubleBoxed = doubleBoxed;
            return SupportEventBeanFactory.CreateObject(bean);
        }
    
        private static EventBean MakeTypeTwoEvent(String myString, int myInt)
        {
            var bean = new SupportBeanSimple(myString, myInt);
            return SupportEventBeanFactory.CreateObject(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
