///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    [TestFixture]
    public class TestIndexTreeBuilder : AbstractRuntimeTest
    {
        private IList<FilterHandle> matches;
        private EventBean eventBean;
        private EventType eventType;
        private FilterHandle[] testFilterCallback;
        private FilterServiceGranularLockFactory lockFactory;

        [SetUp]
        public void SetUp()
        {
            lockFactory = new FilterServiceGranularLockFactoryReentrant(
                container.RWLockManager());

            var testBean = new SupportBean();
            testBean.IntPrimitive = 50;
            testBean.DoublePrimitive = 0.5;
            testBean.TheString = "jack";
            testBean.LongPrimitive = 10;
            testBean.ShortPrimitive = (short) 20;

            eventBean = SupportEventBeanFactory
                .GetInstance(container)
                .CreateObject(testBean);
            eventType = eventBean.EventType;

            matches = new List<FilterHandle>();

            // Allocate a couple of callbacks
            testFilterCallback = new SupportFilterHandle[20];
            for (var i = 0; i < testFilterCallback.Length; i++)
            {
                testFilterCallback[i] = new SupportFilterHandle();
            }
        }

        [Test, RunInApplicationDomain]
        public void TestBuildWithMatch()
        {
            var topNode = new FilterHandleSetNode(new SlimReaderWriterLock());

            // Add some parameter-less expression
            var filterSpec = MakeFilterValues();
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[0], topNode, lockFactory);
            Assert.IsTrue(topNode.Contains(testFilterCallback[0]));

            // Attempt a match
            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 1);
            matches.Clear();

            // Add a filter that won't match, with a single parameter matching against an int
            filterSpec = MakeFilterValues("IntPrimitive", FilterOperator.EQUAL, 100);
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[1], topNode, lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 1);
            Assert.IsTrue(topNode.Indizes[0].CountExpensive == 1);

            // Match again
            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 1);
            matches.Clear();

            // Add a filter that will match
            filterSpec = MakeFilterValues("IntPrimitive", FilterOperator.EQUAL, 50);
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[2], topNode, lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 1);
            Assert.IsTrue(topNode.Indizes[0].CountExpensive == 2);

            // match
            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 2);
            matches.Clear();

            // Add some filter against a double
            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1);
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[3], topNode, lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 2);
            Assert.IsTrue(topNode.Indizes[0].CountExpensive == 2);
            Assert.IsTrue(topNode.Indizes[1].CountExpensive == 1);

            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 3);
            matches.Clear();

            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS_OR_EQUAL, 0.5);
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[4], topNode, lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 3);
            Assert.IsTrue(topNode.Indizes[0].CountExpensive == 2);
            Assert.IsTrue(topNode.Indizes[1].CountExpensive == 1);
            Assert.IsTrue(topNode.Indizes[2].CountExpensive == 1);

            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 4);
            matches.Clear();

            // Add an filterSpec against double and string
            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1,
                    "TheString", FilterOperator.EQUAL, "jack");
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[5], topNode, lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 3);
            Assert.IsTrue(topNode.Indizes[0].CountExpensive == 2);
            Assert.IsTrue(topNode.Indizes[1].CountExpensive == 1);
            Assert.IsTrue(topNode.Indizes[2].CountExpensive == 1);
            var nextLevelSetNode = (FilterHandleSetNode) topNode.Indizes[1].Get(1.1d);
            Assert.IsTrue(nextLevelSetNode != null);
            Assert.IsTrue(nextLevelSetNode.Indizes.Count == 1);

            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 5);
            matches.Clear();

            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1,
                    "TheString", FilterOperator.EQUAL, "beta");
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[6], topNode, lockFactory);

            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 5);
            matches.Clear();

            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1,
                    "TheString", FilterOperator.EQUAL, "jack");
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[7], topNode, lockFactory);
            Assert.IsTrue(nextLevelSetNode.Indizes.Count == 1);
            var nodeTwo = (FilterHandleSetNode) nextLevelSetNode.Indizes[0].Get("jack");
            Assert.IsTrue(nodeTwo.FilterCallbackCount == 2);

            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 6);
            matches.Clear();

            // Try depth first
            filterSpec = MakeFilterValues("TheString", FilterOperator.EQUAL, "jack",
                    "LongPrimitive", FilterOperator.EQUAL, 10L,
                    "ShortPrimitive", FilterOperator.EQUAL, (short) 20);
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[8], topNode, lockFactory);

            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 7);
            matches.Clear();

            // Add an filterSpec in the middle
            filterSpec = MakeFilterValues("LongPrimitive", FilterOperator.EQUAL, 10L,
                    "TheString", FilterOperator.EQUAL, "jack");
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[9], topNode, lockFactory);

            filterSpec = MakeFilterValues("LongPrimitive", FilterOperator.EQUAL, 10L,
                    "TheString", FilterOperator.EQUAL, "jim");
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[10], topNode, lockFactory);

            filterSpec = MakeFilterValues("LongPrimitive", FilterOperator.EQUAL, 10L,
                    "TheString", FilterOperator.EQUAL, "joe");
            IndexTreeBuilderAdd.Add(filterSpec, testFilterCallback[11], topNode, lockFactory);

            topNode.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 8);
            matches.Clear();
        }

        [Test, RunInApplicationDomain]
        public void TestBuildMatchRemove()
        {
            var top = new FilterHandleSetNode(new SlimReaderWriterLock());

            // Add a parameter-less filter
            var filterSpecNoParams = MakeFilterValues();
            IndexTreeBuilderAdd.Add(filterSpecNoParams, testFilterCallback[0], top, lockFactory);

            // Try a match
            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 1);
            matches.Clear();

            // Remove filter
            IndexTreeBuilderRemove.Remove(eventType, testFilterCallback[0], filterSpecNoParams[0], top);

            // Match should not be found
            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 0);
            matches.Clear();

            // Add a depth-first filterSpec
            var filterSpecOne = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack",
                    "LongPrimitive", FilterOperator.EQUAL, 10L,
                    "ShortPrimitive", FilterOperator.EQUAL, (short) 20);
            IndexTreeBuilderAdd.Add(filterSpecOne, testFilterCallback[1], top, lockFactory);

            var filterSpecTwo = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack",
                    "LongPrimitive", FilterOperator.EQUAL, 10L,
                    "ShortPrimitive", FilterOperator.EQUAL, (short) 20);
            IndexTreeBuilderAdd.Add(filterSpecTwo, testFilterCallback[2], top, lockFactory);

            var filterSpecThree = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack",
                    "LongPrimitive", FilterOperator.EQUAL, 10L);
            IndexTreeBuilderAdd.Add(filterSpecThree, testFilterCallback[3], top, lockFactory);

            var filterSpecFour = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack");
            IndexTreeBuilderAdd.Add(filterSpecFour, testFilterCallback[4], top, lockFactory);

            var filterSpecFive = MakeFilterValues(
                    "LongPrimitive", FilterOperator.EQUAL, 10L);
            IndexTreeBuilderAdd.Add(filterSpecFive, testFilterCallback[5], top, lockFactory);

            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 5);
            matches.Clear();

            // Remove some of the nodes
            IndexTreeBuilderRemove.Remove(eventType, testFilterCallback[2], filterSpecTwo[0], top);

            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 4);
            matches.Clear();

            // Remove some of the nodes
            IndexTreeBuilderRemove.Remove(eventType, testFilterCallback[4], filterSpecFour[0], top);

            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 3);
            matches.Clear();

            // Remove some of the nodes
            IndexTreeBuilderRemove.Remove(eventType, testFilterCallback[5], filterSpecFive[0], top);

            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 2);
            matches.Clear();

            // Remove some of the nodes
            IndexTreeBuilderRemove.Remove(eventType, testFilterCallback[1], filterSpecOne[0], top);

            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 1);
            matches.Clear();

            // Remove some of the nodes
            IndexTreeBuilderRemove.Remove(eventType, testFilterCallback[3], filterSpecThree[0], top);

            top.MatchEvent(eventBean, matches, null);
            Assert.IsTrue(matches.Count == 0);
            matches.Clear();
        }

        private FilterValueSetParam[][] MakeFilterValues(params object[] filterSpecArgs)
        {
            var spec = SupportFilterSpecBuilder.Build(eventType, filterSpecArgs);
            return spec.GetValueSet(null, null, null, null);
        }
    }
} // end of namespace
