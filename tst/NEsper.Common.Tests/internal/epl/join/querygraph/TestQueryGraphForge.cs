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
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    [TestFixture]
    public class TestQueryGraphForge : AbstractCommonTest
    {
        private QueryGraphForge queryGraph;
        private EventType[] types;

        [SetUp]
        public void SetUp()
        {
            queryGraph = new QueryGraphForge(3, null, false);
            types = new EventType[] {
                supportEventTypeFactory.CreateMapType(CreateType("P0,P00,P01,P02")),
                supportEventTypeFactory.CreateMapType(CreateType("P1,P10,P11,P12")),
                supportEventTypeFactory.CreateMapType(CreateType("P2,P20,P21")),
                supportEventTypeFactory.CreateMapType(CreateType("P3,P30,P31")),
                supportEventTypeFactory.CreateMapType(CreateType("P4,P40,P41,P42")),
            };
        }

        [Test]
        public void TestFillEquivalency()
        {
            // test with just 3 streams
            queryGraph.AddStrictEquals(0, "P00", Make(0, "P00"), 1, "P10", Make(1, "P10"));
            queryGraph.AddStrictEquals(1, "P10", Make(1, "P10"), 2, "P20", Make(2, "P20"));

            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.AreEqual(0, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 0, 2).Length);
            Assert.AreEqual(0, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 0, 2).Length);

            QueryGraphForge.FillEquivalentNav(types, queryGraph);

            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 2));
            string[] expectedOne = new string[] { "P00" };
            string[] expectedTwo = new string[] { "P20" };
            Assert.IsTrue(Arrays.AreEqual(expectedOne, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 0, 2)));
            Assert.IsTrue(Arrays.AreEqual(expectedTwo, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 0, 2)));

            // test with 5 streams, connect all streams to all streams
            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(0, "P0", Make(0, "P0"), 1, "P1", Make(1, "P1"));
            queryGraph.AddStrictEquals(3, "P3", Make(3, "P3"), 4, "P4", Make(4, "P4"));
            queryGraph.AddStrictEquals(2, "P2", Make(2, "P2"), 3, "P3", Make(3, "P3"));
            queryGraph.AddStrictEquals(1, "P1", Make(1, "P1"), 2, "P2", Make(2, "P2"));

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
            catch (ArgumentException)
            {
                // expected
            }

            // Try invalid add
            try
            {
                queryGraph.AddStrictEquals(1, "a", null, 1, "b", null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }

            // Try :        s1.p11 = s2.p21  and  s2.p22 = s3.p31
            Assert.IsTrue(queryGraph.AddStrictEquals(1, "P11", Make(1, "P11"), 2, "P21", Make(2, "P21")));

            try
            {
                queryGraph.AddStrictEquals(2, "P22", null, 3, "P31", null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // success
            }

            try
            {
                queryGraph.AddStrictEquals(2, "P22", null, 3, "P31", null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // success
            }

            log.Debug(queryGraph.ToString());
        }

        [Test]
        public void TestIsNavigable()
        {
            ExprIdentNode fake = supportExprNodeFactory.MakeIdentNode("TheString", "s0");

            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(1, 2));

            queryGraph.AddStrictEquals(0, "P1", fake, 1, "P2", fake);
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(1, 2));

            queryGraph.AddStrictEquals(2, "P1", fake, 1, "P2", fake);
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsFalse(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsTrue(queryGraph.IsNavigableAtAll(1, 2));

            queryGraph.AddStrictEquals(2, "P1", fake, 0, "P2", fake);
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 1));
            Assert.IsTrue(queryGraph.IsNavigableAtAll(0, 2));
            Assert.IsTrue(queryGraph.IsNavigableAtAll(1, 2));
        }

        [Test]
        public void TestGetNavigableStreams()
        {
            ExprIdentNode fake = supportExprNodeFactory.MakeIdentNode("TheString", "s0");

            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(3, "p3", fake, 4, "p4", fake);
            queryGraph.AddStrictEquals(2, "P2", fake, 3, "p3", fake);
            queryGraph.AddStrictEquals(1, "P1", fake, 2, "P2", fake);

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
            queryGraph.AddStrictEquals(1, "P11", Make(1, "P11"), 0, "P01", Make(0, "P01"));
            queryGraph.AddStrictEquals(0, "P02", Make(0, "P02"), 1, "P12", Make(1, "P12"));
            log.Debug(queryGraph.ToString());

            string[] expectedOne = new string[] { "P11", "P12" };
            string[] expectedTwo = new string[] { "P01", "P02" };
            Assert.IsTrue(Arrays.AreEqual(expectedTwo, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 1, 0)));
            Assert.IsTrue(Arrays.AreEqual(expectedOne, SupportQueryGraphTestUtil.GetIndexProperties(queryGraph, 0, 1)));
            Assert.IsTrue(Arrays.AreEqual(expectedOne, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 1, 0)));
            Assert.IsTrue(Arrays.AreEqual(expectedTwo, SupportQueryGraphTestUtil.GetStrictKeyProperties(queryGraph, 0, 1)));
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
