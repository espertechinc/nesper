///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [TestFixture]
    public class TestQueryGraphForge : AbstractTestBase
    {
        private QueryGraphForge queryGraph;
        private EventType[] types;

        [SetUp]
        public void SetUp()
        {
            queryGraph = new QueryGraphForge(3, null, false);
            types = new EventType[] {
                supportEventTypeFactory.CreateMapType(CreateType("p0,p00,p01,p02")),
                supportEventTypeFactory.CreateMapType(CreateType("p1,p10,p11,p12")),
                supportEventTypeFactory.CreateMapType(CreateType("p2,p20,p21")),
                supportEventTypeFactory.CreateMapType(CreateType("p3,p30,p31")),
                supportEventTypeFactory.CreateMapType(CreateType("p4,p40,p41,p42")),
            };
        }

        [Test]
        public void TestFillEquivalency()
        {
            // test with just 3 streams
            queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 1, "p10", Make(1, "p10"));
            queryGraph.AddStrictEquals(1, "p10", Make(1, "p10"), 2, "p20", Make(2, "p20"));

            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.AreEqual(0, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 0, 2).Length);
            Assert.AreEqual(0, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 0, 2).Length);

            QueryGraphForge.FillEquivalentNav(types, queryGraph);

            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 2));
            string[] expectedOne = new string[] { "p00" };
            string[] expectedTwo = new string[] { "p20" };
            Assert.IsTrue(Arrays.Equals(expectedOne, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 0, 2)));
            Assert.IsTrue(Arrays.Equals(expectedTwo, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 0, 2)));

            // test with 5 streams, connect all streams to all streams
            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(0, "p0", Make(0, "p0"), 1, "p1", Make(1, "p1"));
            queryGraph.AddStrictEquals(3, "p3", Make(3, "p3"), 4, "p4", Make(4, "p4"));
            queryGraph.AddStrictEquals(2, "p2", Make(2, "p2"), 3, "p3", Make(3, "p3"));
            queryGraph.AddStrictEquals(1, "p1", Make(1, "p1"), 2, "p2", Make(2, "p2"));

            QueryGraphForge.FillEquivalentNav(types, queryGraph);

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    Assert.IsTrue(queryGraph.IsNavigableAtAll(i, j), "Not navigable: i=" + i + " j=" + j);
                }
            }
        }

        [Test]
        public void TestAdd()
        {
            // Try invalid add
            try
            {
                queryGraph.AddStrictEquals(1, null, null, 2, null, null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // expected
            }

            // Try invalid add
            try
            {
                queryGraph.AddStrictEquals(1, "a", null, 1, "b", null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // expected
            }

            // Try :        s1.p11 = s2.p21  and  s2.p22 = s3.p31
            Assert.IsTrue(queryGraph.AddStrictEquals(1, "p11", Make(1, "p11"), 2, "p21", Make(2, "p21")));

            try
            {
                queryGraph.AddStrictEquals(2, "p22", null, 3, "p31", null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // success
            }

            try
            {
                queryGraph.AddStrictEquals(2, "p22", null, 3, "p31", null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // success
            }

            log.Debug(queryGraph.ToString());
        }

        [Test]
        public void TestIsNavigable()
        {
            ExprIdentNode fake = supportExprNodeFactory.MakeIdentNode("theString", "s0");

            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(1, 2));

            queryGraph.AddStrictEquals(0, "p1", fake, 1, "p2", fake);
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(1, 2));

            queryGraph.AddStrictEquals(2, "p1", fake, 1, "p2", fake);
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsTrue(queryGraph.IsNavigableAtAll(1, 2));

            queryGraph.AddStrictEquals(2, "p1", fake, 0, "p2", fake);
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsTrue(queryGraph.IsNavigableAtAll(1, 2));
        }

        [Test]
        public void TestGetNavigableStreams()
        {
            ExprIdentNode fake = supportExprNodeFactory.MakeIdentNode("theString", "s0");

            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(3, "p3", fake, 4, "p4", fake);
            queryGraph.AddStrictEquals(2, "p2", fake, 3, "p3", fake);
            queryGraph.AddStrictEquals(1, "p1", fake, 2, "p2", fake);

            Assert.AreEqual(0, queryGraph.GetNavigableStreams(0).Count);
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 2 }, queryGraph.GetNavigableStreams(1));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 1, 3 }, queryGraph.GetNavigableStreams(2));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 2, 4 }, queryGraph.GetNavigableStreams(3));
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 3 }, queryGraph.GetNavigableStreams(4));
        }

        [Test]
        public void TestGetProperties()
        {
            // s1.p11 = s0.p01 and s0.p02 = s1.p12
            queryGraph.AddStrictEquals(1, "p11", Make(1, "p11"), 0, "p01", Make(0, "p01"));
            queryGraph.AddStrictEquals(0, "p02", Make(0, "p02"), 1, "p12", Make(1, "p12"));
            log.Debug(queryGraph.ToString());

            string[] expectedOne = new string[] { "p11", "p12" };
            string[] expectedTwo = new string[] { "p01", "p02" };
            Assert.IsTrue(Arrays.Equals(expectedTwo, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 1, 0)));
            Assert.IsTrue(Arrays.Equals(expectedOne, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 0, 1)));
            Assert.IsTrue(Arrays.Equals(expectedOne, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 1, 0)));
            Assert.IsTrue(Arrays.Equals(expectedTwo, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 0, 1)));
        }

        private IDictionary<string, object> CreateType(string propCSV)
        {
            string[] props = propCSV.SplitCsv();
            IDictionary<string, object> type = new Dictionary<string, object>();
            for (int i = 0; i < props.Length; i++)
            {
                type.Put(props[i], typeof(string));
            }
            return type;
        }

        private ExprIdentNode Make(int stream, string p)
        {
            return new ExprIdentNodeImpl(types[stream], p, stream);
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace