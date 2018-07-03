///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestIndexTreeBuilderMultithreaded 
    {
        private List<FilterSpecCompiled> _testFilterSpecs;
        private List<EventBean> _matchedEvents;
        private List<EventBean> _unmatchedEvents;
    
        private EventType _eventType;
    
        private FilterHandleSetNode _topNode;
        private List<FilterHandle> _filterCallbacks;
        private List<ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>> _pathsAddedTo;
        private FilterServiceGranularLockFactory _lockFactory;

        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _lockFactory = new FilterServiceGranularLockFactoryReentrant(_container.RWLockManager());
            _eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            _topNode = new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock());
            _filterCallbacks = new List<FilterHandle>();
            _pathsAddedTo = new List<ArrayDeque<EventTypeIndexBuilderIndexLookupablePair>>();
    
            _testFilterSpecs = new List<FilterSpecCompiled>();
            _matchedEvents = new List<EventBean>();
            _unmatchedEvents = new List<EventBean>();
    
            // Any int and double value specified here must match only the current filter spec not any other filter spec
            _testFilterSpecs.Add(MakeSpec(new Object[] { "IntPrimitive", FilterOperator.GREATER_OR_EQUAL, 100000 }));
            _matchedEvents.Add(MakeEvent(9999999, -1));
            _unmatchedEvents.Add(MakeEvent(0, -1));
    
            _testFilterSpecs.Add(MakeSpec(new Object[] { "IntPrimitive", FilterOperator.GREATER_OR_EQUAL, 10,
                                              "DoublePrimitive", FilterOperator.EQUAL, 0.5}));
            _matchedEvents.Add(MakeEvent(10, 0.5));
            _unmatchedEvents.Add(MakeEvent(0, 0.5));
    
            _testFilterSpecs.Add(MakeSpec(new Object[] { "DoublePrimitive", FilterOperator.EQUAL, 0.8}));
            _matchedEvents.Add(MakeEvent(-1, 0.8));
            _unmatchedEvents.Add(MakeEvent(-1, 0.1));
    
            _testFilterSpecs.Add(MakeSpec(new Object[] { "DoublePrimitive", FilterOperator.EQUAL, 99.99,
                                                         "IntPrimitive", FilterOperator.LESS, 1}));
            _matchedEvents.Add(MakeEvent(0, 99.99));
            _unmatchedEvents.Add(MakeEvent(2, 0.5));

            _testFilterSpecs.Add(MakeSpec(new Object[] { "DoublePrimitive", FilterOperator.GREATER, .99,
                                                         "IntPrimitive", FilterOperator.EQUAL, 5001}));
            _matchedEvents.Add(MakeEvent(5001, 1.1));
            _unmatchedEvents.Add(MakeEvent(5002, 0.98));
    
            _testFilterSpecs.Add(MakeSpec(new Object[] { "IntPrimitive", FilterOperator.LESS, -99000}));
            _matchedEvents.Add(MakeEvent(-99001, -1));
            _unmatchedEvents.Add(MakeEvent(-98999, -1));
    
            _testFilterSpecs.Add(MakeSpec(new Object[] { "IntPrimitive", FilterOperator.GREATER_OR_EQUAL, 11,
                                              "DoublePrimitive", FilterOperator.GREATER, 888.0}));
            _matchedEvents.Add(MakeEvent(11, 888.001));
            _unmatchedEvents.Add(MakeEvent(10, 888));
    
            _testFilterSpecs.Add(MakeSpec(new Object[] { "IntPrimitive", FilterOperator.EQUAL, 973,
                                              "DoublePrimitive", FilterOperator.EQUAL, 709.0}));
            _matchedEvents.Add(MakeEvent(973, 709));
            _unmatchedEvents.Add(MakeEvent(0, 0.5));
    
            _testFilterSpecs.Add(MakeSpec(new Object[] { "IntPrimitive", FilterOperator.EQUAL, 973,
                                              "DoublePrimitive", FilterOperator.EQUAL, 655.0}));
            _matchedEvents.Add(MakeEvent(973, 655));
            _unmatchedEvents.Add(MakeEvent(33838, 655.5));
        }
    
        [Test]
        public void TestVerifyFilterSpecSet()
        {
            // Add all the above filter definitions
            foreach (var filterSpec in _testFilterSpecs)
            {
                var filterValues = filterSpec.GetValueSet(null, null, null);
                FilterHandle callback = new SupportFilterHandle();
                _filterCallbacks.Add(callback);
                _pathsAddedTo.Add(IndexTreeBuilder.Add(filterValues, callback, _topNode, _lockFactory)[0]);
            }
    
            // None of the not-matching events should cause any match
            foreach (var theEvent in _unmatchedEvents)
            {
                IList<FilterHandle> matches = new List<FilterHandle>();
                _topNode.MatchEvent(theEvent, matches);
                Assert.IsTrue(matches.Count == 0);
            }
    
            // All of the matching events should cause exactly one match
            foreach (var theEvent in _matchedEvents)
            {
                IList<FilterHandle> matches = new List<FilterHandle>();
                _topNode.MatchEvent(theEvent, matches);
                Assert.IsTrue(matches.Count == 1);
            }
    
            // Remove all expressions previously added
            var count = 0;
            foreach (var treePath in _pathsAddedTo)
            {
                var callback = _filterCallbacks[count++];
                IndexTreeBuilder.Remove(_eventType, callback, treePath.ToArray(), _topNode);
            }
    
            // After the remove no matches are expected
            foreach (var theEvent in _matchedEvents)
            {
                IList<FilterHandle> matches = new List<FilterHandle>();
                _topNode.MatchEvent(theEvent, matches);
                Assert.IsTrue(matches.Count == 0);
            }
        }
    
        [Test]
        public void TestMultithreaded()
        {
            var topNode = new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock());
    
            PerformMultithreadedTest(topNode, 2, 1000, 1);
            PerformMultithreadedTest(topNode, 3, 1000, 1);
            PerformMultithreadedTest(topNode, 4, 1000, 1);

            PerformMultithreadedTest(new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock()), 2, 1000, 1);
            PerformMultithreadedTest(new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock()), 3, 1000, 1);
            PerformMultithreadedTest(new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock()), 4, 1000, 1);
        }
    
        private void PerformMultithreadedTest(
            FilterHandleSetNode topNode,
            int numberOfThreads,
            int numberOfRunnables,
            int numberOfSecondsSleep)
        {
            Log.Info(".performMultithreadedTest Loading thread pool work queue,numberOfRunnables=" + numberOfRunnables);

            var pool = new DedicatedExecutorService("test", numberOfThreads, new ImperfectBlockingQueue<Runnable>());
    
            for (var i = 0; i < numberOfRunnables; i++)
            {
                var runnable = new IndexTreeBuilderRunnable(
                    _eventType, topNode, 
                    _testFilterSpecs, 
                    _matchedEvents, 
                    _unmatchedEvents);
                pool.Submit(runnable.Run);
            }
    
            Log.Info(".performMultithreadedTest Starting thread pool, threads=" + numberOfThreads);
    
            // Sleep X seconds
            Sleep(numberOfSecondsSleep);
    
            Log.Info(".performMultithreadedTest Completed, numberOfRunnables=" + numberOfRunnables +
                     "  numberOfThreads=" + numberOfThreads +
                     "  completed=" + pool.NumExecuted);
    
            pool.Shutdown();
            pool.AwaitTermination(TimeSpan.FromSeconds(5));
            //pool.AwaitTermination(TimeSpan.FromSeconds(1));

            Assert.AreEqual(pool.NumExecuted, numberOfRunnables);
        }
    
        private void Sleep(int sec)
        {
            Thread.Sleep(sec * 1000);
        }
    
        private FilterSpecCompiled MakeSpec(Object[] args)
        {
            return SupportFilterSpecBuilder.Build(_eventType, args);
        }
    
        private EventBean MakeEvent(int aInt, double aDouble)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = aInt;
            bean.DoublePrimitive = aDouble;
            return SupportEventBeanFactory.CreateObject(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
