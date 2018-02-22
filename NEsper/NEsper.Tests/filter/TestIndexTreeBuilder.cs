///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestIndexTreeBuilder 
    {
        private IList<FilterHandle> _matches;
        private EventBean _eventBean;
        private EventType _eventType;
        private FilterHandle[] _testFilterCallback;
        private FilterServiceGranularLockFactory _lockFactory; 
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _lockFactory = new FilterServiceGranularLockFactoryReentrant(
                _container.Resolve<IReaderWriterLockManager>());

            var testBean = new SupportBean();
            testBean.IntPrimitive = 50;
            testBean.DoublePrimitive = 0.5;
            testBean.TheString = "jack";
            testBean.LongPrimitive = 10;
            testBean.ShortPrimitive = (short) 20;
    
            _eventBean = SupportEventBeanFactory.CreateObject(testBean);
            _eventType = _eventBean.EventType;
    
            _matches = new List<FilterHandle>();
    
            // Allocate a couple of callbacks
            _testFilterCallback = new SupportFilterHandle[20];
            for (var i = 0; i < _testFilterCallback.Length; i++)
            {
                _testFilterCallback[i] = new SupportFilterHandle();
            }
        }
    
        [Test]
        public void TestBuildWithMatch()
        {
            var topNode = new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock());
    
            // Add some parameter-less expression
            var filterSpec = MakeFilterValues();
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[0], topNode, _lockFactory);
            Assert.IsTrue(topNode.Contains(_testFilterCallback[0]));
    
            // Attempt a match
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 1);
            _matches.Clear();
    
            // Add a filter that won't match, with a single parameter matching against an int
            filterSpec = MakeFilterValues("IntPrimitive", FilterOperator.EQUAL, 100);
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[1], topNode, _lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 1);
            Assert.IsTrue(topNode.Indizes[0].Count == 1);
    
            // Match again
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 1);
            _matches.Clear();
    
            // Add a filter that will match
            filterSpec = MakeFilterValues("IntPrimitive", FilterOperator.EQUAL, 50);
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[2], topNode, _lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 1);
            Assert.IsTrue(topNode.Indizes[0].Count == 2);
    
            // match
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 2);
            _matches.Clear();
    
            // Add some filter against a double
            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1);
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[3], topNode, _lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 2);
            Assert.IsTrue(topNode.Indizes[0].Count == 2);
            Assert.IsTrue(topNode.Indizes[1].Count == 1);
    
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 3);
            _matches.Clear();
    
            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS_OR_EQUAL, 0.5);
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[4], topNode, _lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 3);
            Assert.IsTrue(topNode.Indizes[0].Count == 2);
            Assert.IsTrue(topNode.Indizes[1].Count == 1);
            Assert.IsTrue(topNode.Indizes[2].Count == 1);
    
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 4);
            _matches.Clear();
    
            // Add an filterSpec against double and string
            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1,
                                          "TheString", FilterOperator.EQUAL, "jack");
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[5], topNode, _lockFactory);
            Assert.IsTrue(topNode.Indizes.Count == 3);
            Assert.IsTrue(topNode.Indizes[0].Count == 2);
            Assert.IsTrue(topNode.Indizes[1].Count == 1);
            Assert.IsTrue(topNode.Indizes[2].Count == 1);
            var nextLevelSetNode = (FilterHandleSetNode) topNode.Indizes[1][1.1d];
            Assert.IsTrue(nextLevelSetNode != null);
            Assert.IsTrue(nextLevelSetNode.Indizes.Count == 1);
    
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 5);
            _matches.Clear();
    
            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1,
                                          "TheString", FilterOperator.EQUAL, "beta");
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[6], topNode, _lockFactory);
    
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 5);
            _matches.Clear();
    
            filterSpec = MakeFilterValues("DoublePrimitive", FilterOperator.LESS, 1.1,
                                          "TheString", FilterOperator.EQUAL, "jack");
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[7], topNode, _lockFactory);
            Assert.IsTrue(nextLevelSetNode.Indizes.Count == 1);
            var nodeTwo = (FilterHandleSetNode) nextLevelSetNode.Indizes[0]["jack"];
            Assert.IsTrue(nodeTwo.FilterCallbackCount == 2);
    
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 6);
            _matches.Clear();
    
            // Try depth first
            filterSpec = MakeFilterValues("TheString", FilterOperator.EQUAL, "jack",
                                          "LongPrimitive", FilterOperator.EQUAL, 10L,
                                          "ShortPrimitive", FilterOperator.EQUAL, (short) 20);
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[8], topNode, _lockFactory);
    
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 7);
            _matches.Clear();
    
            // Add an filterSpec in the middle
            filterSpec = MakeFilterValues("LongPrimitive", FilterOperator.EQUAL, 10L,
                                          "TheString", FilterOperator.EQUAL, "jack");
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[9], topNode, _lockFactory);
    
            filterSpec = MakeFilterValues("LongPrimitive", FilterOperator.EQUAL, 10L,
                                          "TheString", FilterOperator.EQUAL, "jim");
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[10], topNode, _lockFactory);
    
            filterSpec = MakeFilterValues("LongPrimitive", FilterOperator.EQUAL, 10L,
                                          "TheString", FilterOperator.EQUAL, "joe");
            IndexTreeBuilder.Add(filterSpec, _testFilterCallback[11], topNode, _lockFactory);
    
            topNode.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 8);
            _matches.Clear();
        }
    
        [Test]
        public void TestBuildMatchRemove()
        {
            var top = new FilterHandleSetNode(_container.RWLockManager().CreateDefaultLock());
    
            // Add a parameter-less filter
            var filterSpecNoParams = MakeFilterValues();
            var pathAddedTo = IndexTreeBuilder.Add(filterSpecNoParams, _testFilterCallback[0], top, _lockFactory);
    
            // Try a match
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 1);
            _matches.Clear();
    
            // Remove filter
            IndexTreeBuilder.Remove(_eventType, _testFilterCallback[0], ToArrayPath(pathAddedTo[0]), top);
    
            // Match should not be found
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 0);
            _matches.Clear();
    
            // Add a depth-first filterSpec
            var filterSpecOne = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack",
                    "LongPrimitive", FilterOperator.EQUAL, 10L,
                    "ShortPrimitive", FilterOperator.EQUAL, (short) 20);
            var pathAddedToOne = IndexTreeBuilder.Add(filterSpecOne, _testFilterCallback[1], top, _lockFactory);
    
            var filterSpecTwo = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack",
                    "LongPrimitive", FilterOperator.EQUAL, 10L,
                    "ShortPrimitive", FilterOperator.EQUAL, (short) 20);
            var pathAddedToTwo = IndexTreeBuilder.Add(filterSpecTwo, _testFilterCallback[2], top, _lockFactory);
    
            var filterSpecThree = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack",
                    "LongPrimitive", FilterOperator.EQUAL, 10L);
            var pathAddedToThree = IndexTreeBuilder.Add(filterSpecThree, _testFilterCallback[3], top, _lockFactory);
    
            var filterSpecFour = MakeFilterValues(
                    "TheString", FilterOperator.EQUAL, "jack");
            var pathAddedToFour = IndexTreeBuilder.Add(filterSpecFour, _testFilterCallback[4], top, _lockFactory);
    
            var filterSpecFive = MakeFilterValues(
                    "LongPrimitive", FilterOperator.EQUAL, 10L);
            var pathAddedToFive = IndexTreeBuilder.Add(filterSpecFive, _testFilterCallback[5], top, _lockFactory);
    
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 5);
            _matches.Clear();
    
            // Remove some of the nodes
            IndexTreeBuilder.Remove(_eventType, _testFilterCallback[2], ToArrayPath(pathAddedToTwo[0]), top);
    
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 4);
            _matches.Clear();
    
            // Remove some of the nodes
            IndexTreeBuilder.Remove(_eventType, _testFilterCallback[4], ToArrayPath(pathAddedToFour[0]), top);
    
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 3);
            _matches.Clear();
    
            // Remove some of the nodes
            IndexTreeBuilder.Remove(_eventType, _testFilterCallback[5], ToArrayPath(pathAddedToFive[0]), top);
    
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 2);
            _matches.Clear();
    
            // Remove some of the nodes
            IndexTreeBuilder.Remove(_eventType, _testFilterCallback[1], ToArrayPath(pathAddedToOne[0]), top);
    
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 1);
            _matches.Clear();
    
            // Remove some of the nodes
            IndexTreeBuilder.Remove(_eventType, _testFilterCallback[3], ToArrayPath(pathAddedToThree[0]), top);
    
            top.MatchEvent(_eventBean, _matches);
            Assert.IsTrue(_matches.Count == 0);
            _matches.Clear();
        }
    
        private EventTypeIndexBuilderIndexLookupablePair[] ToArrayPath(IEnumerable<EventTypeIndexBuilderIndexLookupablePair> path)
        {
            return path.ToArray();
        }
    
        private FilterValueSet MakeFilterValues(params object[] filterSpecArgs)
        {
            var spec = SupportFilterSpecBuilder.Build(_eventType, filterSpecArgs);
            var filterValues = spec.GetValueSet(null, null, null);
            return filterValues;
        }
    }
}
