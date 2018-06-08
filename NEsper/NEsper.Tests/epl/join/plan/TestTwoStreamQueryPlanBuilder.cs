///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestTwoStreamQueryPlanBuilder 
    {
        private EventType[] _typesPerStream;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _typesPerStream = new EventType[]
            {
                _container.Resolve<EventAdapterService>().AddBeanType(typeof(SupportBean_S0).FullName, typeof(SupportBean_S0), true, true, true),
                _container.Resolve<EventAdapterService>().AddBeanType(typeof(SupportBean_S1).FullName, typeof(SupportBean_S1), true, true, true)
            };
        }
    
        [Test]
        public void TestBuildNoOuter()
        {
            QueryGraph graph = MakeQueryGraph();
            QueryPlan spec = TwoStreamQueryPlanBuilder.Build(_typesPerStream, graph, null, new String[2][][], new TableMetadata[2]);

            EPAssertionUtil.AssertEqualsExactOrder(spec.IndexSpecs[0].IndexProps[0], new String[] { "P01", "P02" });
            EPAssertionUtil.AssertEqualsExactOrder(spec.IndexSpecs[1].IndexProps[0], new String[] { "P11", "P12" });
            Assert.AreEqual(2, spec.ExecNodeSpecs.Length);
        }
    
        [Test]
        public void TestBuildOuter()
        {
            QueryGraph graph = MakeQueryGraph();
            QueryPlan spec = TwoStreamQueryPlanBuilder.Build(_typesPerStream, graph, OuterJoinType.LEFT, new String[2][][], new TableMetadata[2]);

            EPAssertionUtil.AssertEqualsExactOrder(spec.IndexSpecs[0].IndexProps[0], new String[] { "P01", "P02" });
            EPAssertionUtil.AssertEqualsExactOrder(spec.IndexSpecs[1].IndexProps[0], new String[] { "P11", "P12" });
            Assert.AreEqual(2, spec.ExecNodeSpecs.Length);
            Assert.AreEqual(typeof(TableOuterLookupNode), spec.ExecNodeSpecs[0].GetType());
            Assert.AreEqual(typeof(TableLookupNode), spec.ExecNodeSpecs[1].GetType());
        }
    
        private QueryGraph MakeQueryGraph()
        {
            QueryGraph graph = new QueryGraph(2, null, false);
            graph.AddStrictEquals(0, "P01", Make(0, "P01"), 1, "P11", Make(1, "P11"));
            graph.AddStrictEquals(0, "P02", Make(0, "P02"), 1, "P12", Make(1, "P12"));
            return graph;
        }

        private ExprIdentNode Make(int stream, String p)
        {
            return new ExprIdentNodeImpl(_typesPerStream[stream], p, stream);
        }
    }
}
