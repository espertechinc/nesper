///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestIndexTreeBuilderMultithreaded : AbstractRuntimeTest
    {
        [SetUp]
        public void SetUp()
        {
            lockFactory = new FilterServiceGranularLockFactoryReentrant(
                Container.RWLockManager());

            eventType = SupportEventTypeFactory
                .GetInstance(Container)
                .CreateBeanType(typeof(SupportBean));
            topNode = new FilterHandleSetNode(new SlimReaderWriterLock());
            filterCallbacks = new List<FilterHandle>();

            testFilterSpecs = new List<FilterSpecActivatable>();
            matchedEvents = new List<EventBean>();
            unmatchedEvents = new List<EventBean>();

            // Any int and double value specified here must match only the current filter spec not any other filter spec
            testFilterSpecs.Add(MakeSpec(new object[] { "IntPrimitive", FilterOperator.GREATER_OR_EQUAL, 100000 }));
            matchedEvents.Add(MakeEvent(9999999, -1));
            unmatchedEvents.Add(MakeEvent(0, -1));

            testFilterSpecs.Add(
                MakeSpec(
                    new object[] {
                        "IntPrimitive", FilterOperator.GREATER_OR_EQUAL, 10,
                        "DoublePrimitive", FilterOperator.EQUAL, 0.5
                    }));
            matchedEvents.Add(MakeEvent(10, 0.5));
            unmatchedEvents.Add(MakeEvent(0, 0.5));

            testFilterSpecs.Add(MakeSpec(new object[] { "DoublePrimitive", FilterOperator.EQUAL, 0.8 }));
            matchedEvents.Add(MakeEvent(-1, 0.8));
            unmatchedEvents.Add(MakeEvent(-1, 0.1));

            testFilterSpecs.Add(
                MakeSpec(
                    new object[] {
                        "DoublePrimitive", FilterOperator.EQUAL, 99.99,
                        "IntPrimitive", FilterOperator.LESS, 1
                    }));
            matchedEvents.Add(MakeEvent(0, 99.99));
            unmatchedEvents.Add(MakeEvent(2, 0.5));

            testFilterSpecs.Add(
                MakeSpec(
                    new object[] {
                        "DoublePrimitive", FilterOperator.GREATER, .99,
                        "IntPrimitive", FilterOperator.EQUAL, 5001
                    }));
            matchedEvents.Add(MakeEvent(5001, 1.1));
            unmatchedEvents.Add(MakeEvent(5002, 0.98));

            testFilterSpecs.Add(MakeSpec(new object[] { "IntPrimitive", FilterOperator.LESS, -99000 }));
            matchedEvents.Add(MakeEvent(-99001, -1));
            unmatchedEvents.Add(MakeEvent(-98999, -1));

            testFilterSpecs.Add(
                MakeSpec(
                    new object[] {
                        "IntPrimitive", FilterOperator.GREATER_OR_EQUAL, 11,
                        "DoublePrimitive", FilterOperator.GREATER, 888.0
                    }));
            matchedEvents.Add(MakeEvent(11, 888.001));
            unmatchedEvents.Add(MakeEvent(10, 888));

            testFilterSpecs.Add(
                MakeSpec(
                    new object[] {
                        "IntPrimitive", FilterOperator.EQUAL, 973,
                        "DoublePrimitive", FilterOperator.EQUAL, 709.0
                    }));
            matchedEvents.Add(MakeEvent(973, 709));
            unmatchedEvents.Add(MakeEvent(0, 0.5));

            testFilterSpecs.Add(
                MakeSpec(
                    new object[] {
                        "IntPrimitive", FilterOperator.EQUAL, 973,
                        "DoublePrimitive", FilterOperator.EQUAL, 655.0
                    }));
            matchedEvents.Add(MakeEvent(973, 655));
            unmatchedEvents.Add(MakeEvent(33838, 655.5));
        }

        private IList<FilterSpecActivatable> testFilterSpecs;
        private IList<EventBean> matchedEvents;
        private IList<EventBean> unmatchedEvents;

        private EventType eventType;

        private FilterHandleSetNode topNode;
        private IList<FilterHandle> filterCallbacks;
        private FilterServiceGranularLockFactory lockFactory;

        private void PerformMultithreadedTest(
            FilterHandleSetNode topNode,
            int numberOfThreads,
            int numberOfRunnables,
            int numberOfSecondsSleep)
        {
            Log.Info(".PerformMultithreadedTest Loading thread pool work queue,numberOfRunnables={0}", numberOfRunnables);

            var pool = Executors.NewMultiThreadedExecutor(numberOfThreads);
            //var pool = new ThreadPoolExecutor(0, numberOfThreads, 99999,
            //    TimeUnit.SECONDS, new LinkedBlockingQueue<Runnable>());

            for (var i = 0; i < numberOfRunnables; i++)
            {
                var runnable = new SupportIndexTreeBuilderRunnable(
                    eventType,
                    topNode,
                    testFilterSpecs,
                    matchedEvents,
                    unmatchedEvents,
                    Container.RWLockManager());

                pool.Submit(() => runnable.Run());
            }

            Log.Info(".PerformMultithreadedTest Starting thread pool, threads={0}", numberOfThreads);
            //pool.CorePoolSize = numberOfThreads;

            // Sleep X seconds
            Sleep(numberOfSecondsSleep);

            Log.Info(".PerformMultithreadedTest Completed, numberOfRunnables={0}  numberOfThreads={1}  completed={2}",
                    numberOfRunnables,
                    numberOfThreads,
                    pool.NumExecuted);

            pool.Shutdown();
            pool.AwaitTermination(1, TimeUnit.SECONDS);

            ClassicAssert.IsTrue(pool.NumExecuted == numberOfRunnables);
        }

        private void Sleep(int sec)
        {
            try
            {
                Thread.Sleep(sec * 1000);
            }
            catch (ThreadInterruptedException e)
            {
                Log.Warn("Interrupted: {}", e.Message, e);
            }
        }

        private FilterSpecActivatable MakeSpec(object[] args)
        {
            return SupportFilterSpecBuilder.Build(eventType, args);
        }

        private EventBean MakeEvent(
            int aInt,
            double aDouble)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = aInt;
            bean.DoublePrimitive = aDouble;
            return SupportEventBeanFactory
                .GetInstance(Container)
                .CreateObject(bean);
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test, RunInApplicationDomain]
        public void TestMultithreaded()
        {
            var topNode = new FilterHandleSetNode(new SlimReaderWriterLock());

            PerformMultithreadedTest(topNode, 2, 1000, 1);
            PerformMultithreadedTest(topNode, 3, 1000, 1);
            PerformMultithreadedTest(topNode, 4, 1000, 1);

            PerformMultithreadedTest(new FilterHandleSetNode(new SlimReaderWriterLock()), 2, 1000, 1);
            PerformMultithreadedTest(new FilterHandleSetNode(new SlimReaderWriterLock()), 3, 1000, 1);
            PerformMultithreadedTest(new FilterHandleSetNode(new SlimReaderWriterLock()), 4, 1000, 1);
        }

        [Test, RunInApplicationDomain]
        public void TestVerifyFilterSpecSet()
        {
            // Add all the above filter definitions
            foreach (var filterSpec in testFilterSpecs)
            {
                var filterValues = filterSpec.GetValueSet(null, null, null, null);
                FilterHandle callback = new SupportFilterHandle();
                filterCallbacks.Add(callback);
                IndexTreeBuilderAdd.Add(filterValues, callback, topNode, lockFactory);
            }

            // None of the not-matching events should cause any match
            foreach (var theEvent in unmatchedEvents)
            {
                IList<FilterHandle> matches = new List<FilterHandle>();
                topNode.MatchEvent(theEvent, matches, null);
                ClassicAssert.IsTrue(matches.Count == 0);
            }

            // All of the matching events should cause exactly one match
            foreach (var theEvent in matchedEvents)
            {
                IList<FilterHandle> matches = new List<FilterHandle>();
                topNode.MatchEvent(theEvent, matches, null);
                ClassicAssert.IsTrue(matches.Count == 1);
            }

            // Remove all expressions previously added
            var count = 0;
            foreach (var filterSpec in testFilterSpecs)
            {
                var callback = filterCallbacks[count++];
                var filterValues = filterSpec.GetValueSet(null, null, null, null);
                IndexTreeBuilderRemove.Remove(eventType, callback, filterValues[0], topNode);
            }

            // After the remove no matches are expected
            foreach (var theEvent in matchedEvents)
            {
                IList<FilterHandle> matches = new List<FilterHandle>();
                topNode.MatchEvent(theEvent, matches, null);
                ClassicAssert.IsTrue(matches.Count == 0);
            }
        }
    }
} // end of namespace
