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
using com.espertech.esper.common.@internal.type;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    [TestFixture]
    public class TestTwoStreamQueryPlanBuilder : AbstractCommonTest
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
            graph.AddStrictEquals(0, "P01", Make(0, "P01"), 1, "P11", Make(1, "P11"));
            graph.AddStrictEquals(0, "P02", Make(0, "P02"), 1, "P12", Make(1, "P12"));
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
            var specDesc = TwoStreamQueryPlanBuilder.Build(typesPerStream, graph, null, new StreamJoinAnalysisResultCompileTime(2), null);
            var spec = specDesc.Forge;

            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P01", "P02" }, spec.IndexSpecs[0].IndexProps[0]);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P11", "P12" }, spec.IndexSpecs[1].IndexProps[0]);
            ClassicAssert.AreEqual(2, spec.ExecNodeSpecs.Length);
        }

        [Test]
        public void TestBuildOuter()
        {
            var graph = MakeQueryGraph();
            var specDesc = TwoStreamQueryPlanBuilder.Build(typesPerStream, graph, OuterJoinType.LEFT, new StreamJoinAnalysisResultCompileTime(2), null);
            var spec = specDesc.Forge;

            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P01", "P02" }, spec.IndexSpecs[0].IndexProps[0]);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "P11", "P12" }, spec.IndexSpecs[1].IndexProps[0]);
            ClassicAssert.AreEqual(2, spec.ExecNodeSpecs.Length);
            ClassicAssert.AreEqual(typeof(TableOuterLookupNodeForge), spec.ExecNodeSpecs[0].GetType());
            ClassicAssert.AreEqual(typeof(TableLookupNodeForge), spec.ExecNodeSpecs[1].GetType());
        }
    }
} // end of namespace
