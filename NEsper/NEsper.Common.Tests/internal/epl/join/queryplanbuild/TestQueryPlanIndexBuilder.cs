///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    [TestFixture]
    public class TestQueryPlanIndexBuilder : CommonTest
    {
        [SetUp]
        public void SetUp()
        {
            types = new[] {
                supportEventTypeFactory.CreateMapType(CreateType("p00,p01")),
                supportEventTypeFactory.CreateMapType(CreateType("p10")),
                supportEventTypeFactory.CreateMapType(CreateType("p20,p21")),
                supportEventTypeFactory.CreateMapType(CreateType("p30,p31")),
                supportEventTypeFactory.CreateMapType(CreateType("p40,p41,p42"))
            };

            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 1, "p10", Make(1, "p10"));
            queryGraph.AddStrictEquals(0, "p01", Make(0, "p01"), 2, "p20", Make(2, "p20"));
            queryGraph.AddStrictEquals(4, "p40", Make(4, "p40"), 3, "p30", Make(3, "p30"));
            queryGraph.AddStrictEquals(4, "p41", Make(4, "p41"), 3, "p31", Make(3, "p31"));
            queryGraph.AddStrictEquals(4, "p42", Make(4, "p42"), 2, "p21", Make(2, "p21"));
        }

        private QueryGraphForge queryGraph;
        private EventType[] types;

        private IDictionary<string, object> CreateType(string propCSV)
        {
            var props = propCSV.SplitCsv();
            IDictionary<string, object> type = new Dictionary<string, object>();
            for (var i = 0; i < props.Length; i++)
            {
                type.Put(props[i], typeof(string));
            }

            return type;
        }

        private ExprIdentNode Make(
            int stream,
            string p)
        {
            return new ExprIdentNodeImpl(types[stream], p, stream);
        }

        [Test]
        public void TestBuildIndexSpec()
        {
            var indexes = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, types, new string[queryGraph.NumStreams][][]);

            string[][] expected = { new string[] { "p00" }, new string[] { "p01" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[0].IndexProps);

            expected = new string[][] { new string[] { "p10" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[1].IndexProps);

            expected = new string[][] { new string[] { "p20" }, new string[] { "p21" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[2].IndexProps);

            expected = new string[][] { new string[] { "p30", "p31" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[3].IndexProps);

            expected = new string[][] { new string[] { "p42" }, new string[] { "p40", "p41" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[4].IndexProps);

            // Test no index, should have a single entry with a zero-length property name array
            queryGraph = new QueryGraphForge(3, null, false);
            indexes = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, types, new string[queryGraph.NumStreams][][]);
            Assert.AreEqual(1, indexes[1].IndexProps.Length);
        }

        [Test]
        public void TestIndexAlreadyExists()
        {
            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 1, "p10", Make(1, "p10"));
            queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 2, "p20", Make(2, "p20"));

            var indexes = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, types, new string[queryGraph.NumStreams][][]);

            string[][] expected = { new string[] { "p00" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[0].IndexProps);
        }
    }
} // end of namespace