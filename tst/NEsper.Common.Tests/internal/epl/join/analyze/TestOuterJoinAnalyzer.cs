///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.type;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.join.analyze
{
    [TestFixture]
    public class TestOuterJoinAnalyzer : AbstractCommonTest
    {
        [Test]
        public void TestAnalyze()
        {
            var descList = new OuterJoinDesc[2];
            descList[0] = SupportOuterJoinDescFactory.MakeDesc(
                container, "IntPrimitive", "s0", "IntBoxed", "s1", OuterJoinType.LEFT);
            descList[1] = SupportOuterJoinDescFactory.MakeDesc(
                container, "SimpleProperty", "s2", "TheString", "s1", OuterJoinType.LEFT);
            // simpleProperty in s2

            var graph = new QueryGraphForge(3, null, false);
            OuterJoinAnalyzer.Analyze(descList, graph);
            ClassicAssert.AreEqual(3, graph.NumStreams);

            ClassicAssert.IsTrue(graph.IsNavigableAtAll(0, 1));
            ClassicAssert.AreEqual(1, SupportQueryGraphTestUtil.GetStrictKeyProperties(graph, 0, 1).Length);
            ClassicAssert.AreEqual("IntPrimitive", SupportQueryGraphTestUtil.GetStrictKeyProperties(graph, 0, 1)[0]);
            ClassicAssert.AreEqual(1, SupportQueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 0).Length);
            ClassicAssert.AreEqual("IntBoxed", SupportQueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 0)[0]);

            ClassicAssert.IsTrue(graph.IsNavigableAtAll(1, 2));
            ClassicAssert.AreEqual("TheString", SupportQueryGraphTestUtil.GetStrictKeyProperties(graph, 1, 2)[0]);
            ClassicAssert.AreEqual("SimpleProperty", SupportQueryGraphTestUtil.GetStrictKeyProperties(graph, 2, 1)[0]);
        }
    }
} // end of namespace
