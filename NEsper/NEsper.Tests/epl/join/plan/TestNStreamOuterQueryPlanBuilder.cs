///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;
using com.espertech.esper.support.epl;
using com.espertech.esper.type;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestNStreamOuterQueryPlanBuilder 
    {
        [Test]
        public void TestGraphOuterJoins()
        {
            var descList = new OuterJoinDesc[2];
            descList[0] = SupportOuterJoinDescFactory.MakeDesc("IntPrimitive", "s0", "IntBoxed", "s1", OuterJoinType.RIGHT);
            descList[1] = SupportOuterJoinDescFactory.MakeDesc("SimpleProperty", "s2", "TheString", "s1", OuterJoinType.FULL);
    
            var graph = NStreamOuterQueryPlanBuilder.GraphOuterJoins(3, descList);
    
            // assert the inner and outer streams for each stream
            AssertInners(new int[][] { null, new int[] { 0, 2 }, new int[] { 1 } }, graph);
            AssertOuters(new int[][] { new int[] { 1 }, new int[] { 2 }, new int[] { 1 } }, graph);
    
            descList[0] = SupportOuterJoinDescFactory.MakeDesc("IntPrimitive", "s1", "IntBoxed", "s0", OuterJoinType.LEFT);
            descList[1] = SupportOuterJoinDescFactory.MakeDesc("SimpleProperty", "s2", "TheString", "s1", OuterJoinType.RIGHT);
    
            graph = NStreamOuterQueryPlanBuilder.GraphOuterJoins(3, descList);
    
            // assert the inner and outer streams for each stream
            AssertInners(new int[][] { new int[] { 1 }, null, new int[] { 1 } }, graph);
            AssertOuters(new int[][] { null, new int[] { 0, 2 }, null }, graph);
    
            try
            {
                NStreamOuterQueryPlanBuilder.GraphOuterJoins(3, new OuterJoinDesc[0]);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestRecursiveBuild()
        {
            var streamNum = 2;
            var queryGraph = new QueryGraph(6, null, false);
            var outerInnerGraph = new OuterInnerDirectionalGraph(6);
            var completedStreams = new HashSet<int>();
            var substreamsPerStream = new LinkedHashMap<int, int[]>();
            var requiredPerStream = new bool[6];
    
            outerInnerGraph.Add(3, 2).Add(2, 1).Add(4, 3).Add(1, 0).Add(3, 5);
            queryGraph.AddStrictEquals(2, "", null, 3, "", null);
            queryGraph.AddStrictEquals(3, "", null, 4, "", null);
            queryGraph.AddStrictEquals(3, "", null, 5, "", null);
            queryGraph.AddStrictEquals(2, "", null, 1, "", null);
            queryGraph.AddStrictEquals(1, "", null, 0, "", null);
    
            ICollection<InterchangeablePair<int, int>> innerJoins = new HashSet<InterchangeablePair<int, int>>();
            var innerJoinGraph = new InnerJoinGraph(6, innerJoins);
            var streamStack = new Stack<int>();
    
            NStreamOuterQueryPlanBuilder.RecursiveBuild(streamNum, streamStack, queryGraph, outerInnerGraph, innerJoinGraph, completedStreams,
                    substreamsPerStream, requiredPerStream, new DependencyGraph(6, false));
    
            Assert.AreEqual(6, substreamsPerStream.Count);
            EPAssertionUtil.AssertEqualsExactOrder(substreamsPerStream[2], new int[]{3, 1});
            EPAssertionUtil.AssertEqualsExactOrder(substreamsPerStream.Get(3), new int[]{4, 5});
            EPAssertionUtil.AssertEqualsExactOrder(substreamsPerStream[1], new int[]{0});
            EPAssertionUtil.AssertEqualsExactOrder(substreamsPerStream.Get(4), new int[]{});
            EPAssertionUtil.AssertEqualsExactOrder(substreamsPerStream.Get(5), new int[]{});
            EPAssertionUtil.AssertEqualsExactOrder(substreamsPerStream[0], new int[]{});
    
            NStreamOuterQueryPlanBuilder.VerifyJoinedPerStream(2, substreamsPerStream);
            EPAssertionUtil.AssertEqualsExactOrder(requiredPerStream, new bool[]{false, false, false, true, true, false}
            );
    
        }
    
        [Test]
        public void TestVerifyJoinedPerStream()
        {
            // stream relationships not filled
            TryVerifyJoinedPerStream(Convert(new int[][] { new int[] { 1, 2 } }));
    
            // stream relationships duplicates
            TryVerifyJoinedPerStream(Convert(new int[][] { new int[] { 1, 2 }, new int[] { 1 }, new int[] { } }));
            TryVerifyJoinedPerStream(Convert(new int[][] { new int[] { 1, 2 }, new int[] { }, new int[] { 2 } }));
    
            // stream relationships out of range
            TryVerifyJoinedPerStream(Convert(new int[][] { new int[] { 1, 3 }, new int[] { }, new int[] { } }));
    
            // stream relationships missing stream
            TryVerifyJoinedPerStream(Convert(new int[][] { new int[] { 1 }, new int[] { }, new int[] { } }));
        }
    
        private void TryVerifyJoinedPerStream(IDictionary<int, int[]> map)
        {
            try
            {
                NStreamOuterQueryPlanBuilder.VerifyJoinedPerStream(0, map);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    
        private void AssertInners(int[][] innersPerStream, OuterInnerDirectionalGraph graph)
        {
            for (var i = 0; i < innersPerStream.Length; i++)
            {
                EPAssertionUtil.AssertEqualsAnyOrder(innersPerStream[i], graph.GetInner(i));
            }
        }
        private void AssertOuters(int[][] outersPerStream, OuterInnerDirectionalGraph graph)
        {
            for (var i = 0; i < outersPerStream.Length; i++)
            {
                EPAssertionUtil.AssertEqualsAnyOrder(outersPerStream[i], graph.GetOuter(i));
            }
        }
    
        private IDictionary<int, int[]> Convert(int[][] array)
        {
            IDictionary<int, int[]> result = new Dictionary<int, int[]>();
            for (var i = 0; i < array.Length; i++)
            {
                result.Put(i, array[i]);
            }
            return result;
        }
    
    }
}
