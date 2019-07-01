///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.context.aifactory.@select;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    [TestFixture]
    public class TestTwoStreamQueryPlanBuilder : AbstractTestBase
    {
        private EventType[] typesPerStream;

        [SetUp]
        public void SetUp()
        {
            typesPerStream = new EventType[] {
                supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S0)),
                supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S1))
            };
        }

        private QueryGraphForge MakeQueryGraph()
        {
            var graph = new QueryGraphForge(2, null, false);
            graph.AddStrictEquals(0, "p01", Make(0, "p01"), 1, "p11", Make(1, "p11"));
            graph.AddStrictEquals(0, "p02", Make(0, "p02"), 1, "p12", Make(1, "p12"));
            return graph;
        }

        private ExprIdentNode Make(
            int stream,
            string p)
        {
            return new ExprIdentNodeImpl(typesPerStream[stream], p, stream);
        }

        [Test]
        public void TestBuildNoOuter()
        {
            var graph = MakeQueryGraph();
            var spec = TwoStreamQueryPlanBuilder.Build(typesPerStream, graph, null, new StreamJoinAnalysisResultCompileTime(2));

            EPAssertionUtil.AssertEqualsExactOrder(new[] { "p01", "p02" }, spec.IndexSpecs[0].IndexProps[0]);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "p11", "p12" }, spec.IndexSpecs[1].IndexProps[0]);
            Assert.AreEqual(2, spec.ExecNodeSpecs.Length);
        }

        [Test]
        public void TestBuildOuter()
        {
            var graph = MakeQueryGraph();
            var spec = TwoStreamQueryPlanBuilder.Build(typesPerStream, graph, OuterJoinType.LEFT, new StreamJoinAnalysisResultCompileTime(2));

            EPAssertionUtil.AssertEqualsExactOrder(new[] { "p01", "p02" }, spec.IndexSpecs[0].IndexProps[0]);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "p11", "p12" }, spec.IndexSpecs[1].IndexProps[0]);
            Assert.AreEqual(2, spec.ExecNodeSpecs.Length);
            Assert.AreEqual(typeof(TableOuterLookupNodeForge), spec.ExecNodeSpecs[0].GetType());
            Assert.AreEqual(typeof(TableLookupNodeForge), spec.ExecNodeSpecs[1].GetType());
        }
    }
} // end of namespace
