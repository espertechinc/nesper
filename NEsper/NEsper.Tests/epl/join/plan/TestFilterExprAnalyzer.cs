///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.scopetest;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestFilterExprAnalyzer 
    {
        [Test]
        public void TestAnalyzeEquals()
        {
            // s0.IntPrimitive = s1.IntBoxed
            ExprEqualsNode equalsNode = SupportExprNodeFactory.MakeEqualsNode();
    
            QueryGraph graph = new QueryGraph(2, null, false);
            FilterExprAnalyzer.AnalyzeEqualsNode(equalsNode, graph, false);
    
            Assert.IsTrue(graph.IsNavigableAtAll(0, 1));
            EPAssertionUtil.AssertEqualsExactOrder(new String[] { "IntPrimitive" }, QueryGraphTestUtil.GetStrictKeyProperties(graph, 0, 1));
            EPAssertionUtil.AssertEqualsExactOrder(new String[] { "IntPrimitive" }, QueryGraphTestUtil.GetIndexProperties(graph, 1, 0));
            EPAssertionUtil.AssertEqualsExactOrder(new String[] { "IntBoxed" }, QueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 0));
            EPAssertionUtil.AssertEqualsExactOrder(new String[] { "IntBoxed" }, QueryGraphTestUtil.GetIndexProperties(graph, 0, 1));
        }
    
        [Test]
        public void TestAnalyzeAnd()
        {
            ExprAndNode andNode = SupportExprNodeFactory.Make2SubNodeAnd();
    
            QueryGraph graph = new QueryGraph(2, null, false);
            FilterExprAnalyzer.AnalyzeAndNode(andNode, graph, false);
    
            Assert.IsTrue(graph.IsNavigableAtAll(0, 1));
            EPAssertionUtil.AssertEqualsExactOrder(QueryGraphTestUtil.GetStrictKeyProperties(graph, 0, 1), new String[] { "IntPrimitive", "TheString" });
            EPAssertionUtil.AssertEqualsExactOrder(QueryGraphTestUtil.GetIndexProperties(graph, 1, 0), new String[] { "IntPrimitive", "TheString" });
            EPAssertionUtil.AssertEqualsExactOrder(QueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 0), new String[] { "IntBoxed", "TheString" });
            EPAssertionUtil.AssertEqualsExactOrder(QueryGraphTestUtil.GetIndexProperties(graph, 0, 1), new String[] { "IntBoxed", "TheString" });
        }
    }
}
