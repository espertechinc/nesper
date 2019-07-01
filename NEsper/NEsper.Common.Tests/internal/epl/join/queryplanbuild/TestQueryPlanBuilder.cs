///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.@select;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    [TestFixture]
    public class TestQueryPlanBuilder : AbstractTestBase
    {
        private EventType[] typesPerStream;
        private DependencyGraph dependencyGraph;

        [SetUp]
        public void SetUp()
        {
            typesPerStream = new EventType[] {
                supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S0)),
                supportEventTypeFactory.CreateBeanType(typeof(SupportBean_S1))
            };
            dependencyGraph = new DependencyGraph(2, false);
        }

        private void AssertPlan(QueryPlanForge plan)
        {
            Assert.AreEqual(2, plan.ExecNodeSpecs.Length);
            Assert.AreEqual(2, plan.ExecNodeSpecs.Length);
        }

        [Test]
        public void TestGetPlan()
        {
            OuterJoinDesc[] descList = {
                SupportOuterJoinDescFactory.MakeDesc(
                    container, "IntPrimitive", "s0", "IntBoxed", "s1", OuterJoinType.LEFT)
            };

            var queryGraph = new QueryGraphForge(2, null, false);
            var plan = QueryPlanBuilder.GetPlan(
                typesPerStream,
                new OuterJoinDesc[0],
                queryGraph,
                null,
                new HistoricalViewableDesc(5),
                dependencyGraph,
                null,
                new StreamJoinAnalysisResultCompileTime(2),
                true,
                null,
                null);
            AssertPlan(plan);

            plan = QueryPlanBuilder.GetPlan(
                typesPerStream,
                descList,
                queryGraph,
                null,
                new HistoricalViewableDesc(5),
                dependencyGraph,
                null,
                new StreamJoinAnalysisResultCompileTime(2),
                true,
                null,
                null);
            AssertPlan(plan);

            FilterExprAnalyzer.Analyze(SupportExprNodeFactory.GetInstance(container).MakeEqualsNode(), queryGraph, false);
            plan = QueryPlanBuilder.GetPlan(
                typesPerStream,
                descList,
                queryGraph,
                null,
                new HistoricalViewableDesc(5),
                dependencyGraph,
                null,
                new StreamJoinAnalysisResultCompileTime(2),
                true,
                null,
                null);
            AssertPlan(plan);

            plan = QueryPlanBuilder.GetPlan(
                typesPerStream,
                new OuterJoinDesc[0],
                queryGraph,
                null,
                new HistoricalViewableDesc(5),
                dependencyGraph,
                null,
                new StreamJoinAnalysisResultCompileTime(2),
                true,
                null,
                null);
            AssertPlan(plan);
        }
    }
} // end of namespace
