///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.join.@base;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestQueryPlanBuilder 
    {
        private EventType[] _typesPerStream;
        private bool[] _isHistorical;
        private DependencyGraph _dependencyGraph;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _typesPerStream = new EventType[] {
                _container.Resolve<EventAdapterService>().AddBeanType(typeof(SupportBean_S0).FullName, typeof(SupportBean_S0), true, true, true),
                _container.Resolve<EventAdapterService>().AddBeanType(typeof(SupportBean_S1).FullName, typeof(SupportBean_S1), true, true, true)
            };
            _dependencyGraph = new DependencyGraph(2, false);
            _isHistorical = new bool[2];
        }
    
        [Test]
        public void TestGetPlan()
        {
            var descList = new OuterJoinDesc[] {
                    SupportOuterJoinDescFactory.MakeDesc("IntPrimitive", "s0", "IntBoxed", "s1", OuterJoinType.LEFT)
            };
    
            var queryGraph = new QueryGraph(2, null, false);
            var plan = QueryPlanBuilder.GetPlan(_typesPerStream, new OuterJoinDesc[0], queryGraph, null, new HistoricalViewableDesc(5), _dependencyGraph, null, new StreamJoinAnalysisResult(2), true, null, null);
            AssertPlan(plan);
    
            plan = QueryPlanBuilder.GetPlan(_typesPerStream, descList, queryGraph, null, new HistoricalViewableDesc(5), _dependencyGraph, null, new StreamJoinAnalysisResult(2), true, null, null);
            AssertPlan(plan);
    
            FilterExprAnalyzer.Analyze(SupportExprNodeFactory.MakeEqualsNode(), queryGraph, false);
            plan = QueryPlanBuilder.GetPlan(_typesPerStream, descList, queryGraph, null, new HistoricalViewableDesc(5), _dependencyGraph, null, new StreamJoinAnalysisResult(2), true, null, null);
            AssertPlan(plan);
    
            plan = QueryPlanBuilder.GetPlan(_typesPerStream, new OuterJoinDesc[0], queryGraph, null, new HistoricalViewableDesc(5), _dependencyGraph, null, new StreamJoinAnalysisResult(2), true, null, null);
            AssertPlan(plan);
        }
    
        private void AssertPlan(QueryPlan plan)
        {
            Assert.AreEqual(2, plan.ExecNodeSpecs.Length);
            Assert.AreEqual(2, plan.ExecNodeSpecs.Length);
        }
    }
}
