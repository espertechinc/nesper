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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    [TestFixture]
    public class TestQueryPlanIndexBuilder : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            types = new[] {
                supportEventTypeFactory.CreateMapType(CreateType("P00,P01")),
                supportEventTypeFactory.CreateMapType(CreateType("P10")),
                supportEventTypeFactory.CreateMapType(CreateType("P20,P21")),
                supportEventTypeFactory.CreateMapType(CreateType("P30,P31")),
                supportEventTypeFactory.CreateMapType(CreateType("P40,P41,P42"))
            };

            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(0, "P00", Make(0, "P00"), 1, "P10", Make(1, "P10"));
            queryGraph.AddStrictEquals(0, "P01", Make(0, "P01"), 2, "P20", Make(2, "P20"));
            queryGraph.AddStrictEquals(4, "P40", Make(4, "P40"), 3, "P30", Make(3, "P30"));
            queryGraph.AddStrictEquals(4, "P41", Make(4, "P41"), 3, "P31", Make(3, "P31"));
            queryGraph.AddStrictEquals(4, "P42", Make(4, "P42"), 2, "P21", Make(2, "P21"));
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

        [Test, RunInApplicationDomain]
        public void TestBuildIndexSpec()
        {
            var indexes = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, types, new string[queryGraph.NumStreams][][]);

            string[][] expected = { new string[] { "P00" }, new string[] { "P01" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[0].IndexProps);

            expected = new string[][] { new string[] { "P10" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[1].IndexProps);

            expected = new string[][] { new string[] { "P20" }, new string[] { "P21" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[2].IndexProps);

            expected = new string[][] { new string[] { "P30", "P31" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[3].IndexProps);

            expected = new string[][] { new string[] { "P42" }, new string[] { "P40", "P41" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[4].IndexProps);

            // Test no index, should have a single entry with a zero-length property name array
            queryGraph = new QueryGraphForge(3, null, false);
            indexes = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, types, new string[queryGraph.NumStreams][][]);
            Assert.AreEqual(1, indexes[1].IndexProps.Length);
        }

        [Test, RunInApplicationDomain]
        public void TestIndexAlreadyExists()
        {
            queryGraph = new QueryGraphForge(5, null, false);
            queryGraph.AddStrictEquals(0, "P00", Make(0, "P00"), 1, "P10", Make(1, "P10"));
            queryGraph.AddStrictEquals(0, "P00", Make(0, "P00"), 2, "P20", Make(2, "P20"));

            var indexes = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, types, new string[queryGraph.NumStreams][][]);

            string[][] expected = { new string[] { "P00" } };
            EPAssertionUtil.AssertEqualsExactOrder(expected, indexes[0].IndexProps);
        }
    }
} // end of namespace
