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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestQueryGraph 
    {
        private QueryGraph _queryGraph;
        private EventType[] _types;
    
        [SetUp]
        public void SetUp()
        {
            _queryGraph = new QueryGraph(3, null, false);
            _types = new[] {
                    SupportEventTypeFactory.CreateMapType(CreateType("p0,p00,p01,p02")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p1,p10,p11,p12")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p2,p20,p21")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p3,p30,p31")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p4,p40,p41,p42")),
                };
        }
    
        [Test]
        public void TestFillEquivalency()
        {
            // test with just 3 streams
            _queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 1, "p10", Make(1, "p10"));
            _queryGraph.AddStrictEquals(1, "p10", Make(1, "p10"), 2, "p20", Make(2, "p20"));

            Assert.IsFalse(_queryGraph.IsNavigableAtAll(0, 2));
            Assert.AreEqual(0, QueryGraphTestUtil.GetStrictKeyProperties(_queryGraph, 0, 2).Count);
            Assert.AreEqual(0, QueryGraphTestUtil.GetIndexProperties(_queryGraph, 0, 2).Count);
    
            QueryGraph.FillEquivalentNav(_types, _queryGraph);
    
            Assert.IsTrue(_queryGraph.IsNavigableAtAll(0, 2));
            String[] expectedOne = new String[] {"p00"};
            String[] expectedTwo = new String[] {"p20"};
            Assert.IsTrue(Collections.AreEqual(expectedOne, QueryGraphTestUtil.GetStrictKeyProperties(_queryGraph, 0, 2)));
            Assert.IsTrue(Collections.AreEqual(expectedTwo, QueryGraphTestUtil.GetIndexProperties(_queryGraph, 0, 2)));
    
            // test with 5 streams, connect all streams to all streams
            _queryGraph = new QueryGraph(5, null, false);
            _queryGraph.AddStrictEquals(0, "p0", Make(0, "p0"), 1, "p1", Make(1, "p1"));
            _queryGraph.AddStrictEquals(3, "p3", Make(3, "p3"), 4, "p4", Make(4, "p4"));
            _queryGraph.AddStrictEquals(2, "p2", Make(2, "p2"), 3, "p3", Make(3, "p3"));
            _queryGraph.AddStrictEquals(1, "p1", Make(1, "p1"), 2, "p2", Make(2, "p2"));
    
            QueryGraph.FillEquivalentNav(_types, _queryGraph);
    
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    Assert.IsTrue(_queryGraph.IsNavigableAtAll(i, j), "Not navigable: i=" + i + " j=" + j);
                }
            }
        }
    
        [Test]
        public void TestAdd()
        {
            // Try invalid add
            try
            {
                _queryGraph.AddStrictEquals(1, null, null, 2, null, null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
    
            // Try invalid add
            try
            {
                _queryGraph.AddStrictEquals(1, "a", null, 1, "b", null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
    
            // Try :        s1.p11 = s2.p21  and  s2.p22 = s3.p31
            Assert.IsTrue(_queryGraph.AddStrictEquals(1, "p11", Make(1, "p11"), 2, "p21", Make(2, "p21")));
    
            try {
                _queryGraph.AddStrictEquals(2, "p22", null, 3, "p31", null);
                Assert.Fail();
            }
            catch (ArgumentException) {
                // success
            }
    
            try {
                _queryGraph.AddStrictEquals(2, "p22", null, 3, "p31", null);
                Assert.Fail();
            }
            catch (ArgumentException) {
                // success
            }
    
            Log.Debug(_queryGraph.ToString());
        }
    
        [Test]
        public void TestIsNavigable()
        {
            Assert.IsFalse(_queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(_queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsFalse(_queryGraph.IsNavigableAtAll(1, 2));
    
            _queryGraph.AddStrictEquals(0, "p1", null, 1, "p2", null);
            Assert.IsTrue(_queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(_queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsFalse(_queryGraph.IsNavigableAtAll(1, 2));
    
            _queryGraph.AddStrictEquals(2, "p1", null, 1, "p2", null);
            Assert.IsTrue(_queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(_queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsTrue(_queryGraph.IsNavigableAtAll(1, 2));
    
            _queryGraph.AddStrictEquals(2, "p1", null, 0, "p2", null);
            Assert.IsTrue(_queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsTrue(_queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsTrue(_queryGraph.IsNavigableAtAll(1, 2));
        }
    
        [Test]
        public void TestGetNavigableStreams()
        {
            _queryGraph = new QueryGraph(5, null, false);
            _queryGraph.AddStrictEquals(3, "p3", null, 4, "p4", null);
            _queryGraph.AddStrictEquals(2, "p2", null, 3, "p3", null);
            _queryGraph.AddStrictEquals(1, "p1", null, 2, "p2", null);
    
            Assert.AreEqual(0, _queryGraph.GetNavigableStreams(0).Count);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {2}, _queryGraph.GetNavigableStreams(1));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {1,3}, _queryGraph.GetNavigableStreams(2));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {2,4}, _queryGraph.GetNavigableStreams(3));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] {3}, _queryGraph.GetNavigableStreams(4));
        }
    
        [Test]
        public void TestGetProperties()
        {
            // s1.p11 = s0.p01 and s0.p02 = s1.p12
            _queryGraph.AddStrictEquals(1, "p11", Make(1, "p11"), 0, "p01", Make(0, "p01"));
            _queryGraph.AddStrictEquals(0, "p02", Make(0, "p02"), 1, "p12", Make(1, "p12"));
            Log.Debug(_queryGraph.ToString());
    
            String[] expectedOne = new String[] {"p11", "p12"};
            String[] expectedTwo = new String[] {"p01", "p02"};
            Assert.IsTrue(Collections.AreEqual(expectedTwo, QueryGraphTestUtil.GetIndexProperties(_queryGraph, 1, 0)));
            Assert.IsTrue(Collections.AreEqual(expectedOne, QueryGraphTestUtil.GetIndexProperties(_queryGraph, 0, 1)));
            Assert.IsTrue(Collections.AreEqual(expectedOne, QueryGraphTestUtil.GetStrictKeyProperties(_queryGraph, 1, 0)));
            Assert.IsTrue(Collections.AreEqual(expectedTwo, QueryGraphTestUtil.GetStrictKeyProperties(_queryGraph, 0, 1)));
        }
    
        private static IDictionary<String, Object> CreateType(String propCSV) {
            String[] props = propCSV.Split(',');
            IDictionary<String, Object> type = new Dictionary<String, Object>();
            for (int i = 0; i < props.Length; i++) {
                type.Put(props[i], typeof(string));
            }
            return type;
        }

        private ExprIdentNode Make(int stream, String p)
        {
            return new ExprIdentNodeImpl(_types[stream], p, stream);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
