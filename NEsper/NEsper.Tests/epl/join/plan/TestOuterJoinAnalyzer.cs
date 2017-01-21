///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.spec;
using com.espertech.esper.support.epl;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestOuterJoinAnalyzer 
    {
        [Test]
        public void TestAnalyze()
        {
            var descList = new OuterJoinDesc[2];
            descList[0] = SupportOuterJoinDescFactory.MakeDesc("IntPrimitive", "s0", "IntBoxed", "s1", OuterJoinType.LEFT);
            descList[1] = SupportOuterJoinDescFactory.MakeDesc("SimpleProperty", "s2", "TheString", "s1", OuterJoinType.LEFT);
            // simpleProperty in s2
    
            var graph = new QueryGraph(3, null, false);
            OuterJoinAnalyzer.Analyze(descList, graph);
            Assert.AreEqual(3, graph.NumStreams);
    
            Assert.IsTrue(graph.IsNavigableAtAll(0, 1));
            Assert.AreEqual(1, QueryGraphTestUtil.GetStrictKeyProperties(graph, 0, 1).Count);
            Assert.AreEqual("IntPrimitive", QueryGraphTestUtil.GetStrictKeyProperties(graph, 0, 1)[0]);
            Assert.AreEqual(1, QueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 0).Count);
            Assert.AreEqual("IntBoxed", QueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 0)[0]);
    
            Assert.IsTrue(graph.IsNavigableAtAll(1, 2));
            Assert.AreEqual("TheString", QueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 2)[0]);
            Assert.AreEqual("SimpleProperty", QueryGraphTestUtil.GetStrictKeyProperties(graph, 2, 1)[0]);
        }
    }
}
