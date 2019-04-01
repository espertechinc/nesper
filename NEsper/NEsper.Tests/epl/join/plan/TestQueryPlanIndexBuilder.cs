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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestQueryPlanIndexBuilder 
    {
        private QueryGraph _queryGraph;
        private EventType[] _types;
    
        [SetUp]
        public void SetUp()
        {
            _types = new EventType[] {
                    SupportEventTypeFactory.CreateMapType(CreateType("p00,p01")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p10")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p20,p21")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p30,p31")),
                    SupportEventTypeFactory.CreateMapType(CreateType("p40,p41,p42")),
                };

            _queryGraph = new QueryGraph(5, null, false);
            _queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 1, "p10", Make(1, "p10"));
            _queryGraph.AddStrictEquals(0, "p01", Make(0, "p01"), 2, "p20", Make(2, "p20"));
            _queryGraph.AddStrictEquals(4, "p40", Make(4, "p40"), 3, "p30", Make(3, "p30"));
            _queryGraph.AddStrictEquals(4, "p41", Make(4, "p41"), 3, "p31", Make(3, "p31"));
            _queryGraph.AddStrictEquals(4, "p42", Make(4, "p42"), 2, "p21", Make(2, "p21"));
        }
    
        [Test]
        public void TestBuildIndexSpec()
        {
            QueryPlanIndex[] indexes = QueryPlanIndexBuilder.BuildIndexSpec(_queryGraph, _types, new String[_queryGraph.NumStreams][][]);

            String[][] expected = new String[][] { new [] { "p00" }, new [] { "p01" } };
            EPAssertionUtil.AssertEqualsExactOrder(indexes[0].IndexProps, expected);
    
            expected = new String[][] { new [] {"p10"} };
            EPAssertionUtil.AssertEqualsExactOrder(indexes[1].IndexProps, expected);
    
            expected = new String[][] { new [] {"p20"}, new [] {"p21"} };
            EPAssertionUtil.AssertEqualsExactOrder(indexes[2].IndexProps, expected);
    
            expected = new String[][] { new [] {"p30", "p31"} };
            EPAssertionUtil.AssertEqualsExactOrder(indexes[3].IndexProps, expected);
    
            expected = new String[][] { new [] {"p42"}, new [] {"p40", "p41"} };
            EPAssertionUtil.AssertEqualsExactOrder(indexes[4].IndexProps, expected);
    
            // Test no index, should have a single entry with a zero-length property name array
            _queryGraph = new QueryGraph(3, null, false);
            indexes = QueryPlanIndexBuilder.BuildIndexSpec(_queryGraph, _types, new String[_queryGraph.NumStreams][][]);
            Assert.AreEqual(1, indexes[1].IndexProps.Length);
        }
    
        [Test]
        public void TestIndexAlreadyExists()
        {
            _queryGraph = new QueryGraph(5, null, false);
            _queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 1, "p10", Make(1, "p10"));
            _queryGraph.AddStrictEquals(0, "p00", Make(0, "p00"), 2, "p20", Make(2, "p20"));

            QueryPlanIndex[] indexes = QueryPlanIndexBuilder.BuildIndexSpec(_queryGraph, _types, new String[_queryGraph.NumStreams][][]);
    
            String[][] expected = new String[][] { new [] {"p00"} };
            EPAssertionUtil.AssertEqualsExactOrder(indexes[0].IndexProps, expected);
        }
    
        private IDictionary<String, Object> CreateType(String propCSV) {
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
    }
}
