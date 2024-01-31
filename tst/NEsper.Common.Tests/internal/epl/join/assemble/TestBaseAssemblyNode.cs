///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    [TestFixture]
    public class TestAssemblyNode : AbstractCommonTest
    {
        [Test]
        public void TestGetSubstreams()
        {
            var top = new SupportJoinProcNode(2, 3);

            var child_1 = new SupportJoinProcNode(5, 3);
            var child_2 = new SupportJoinProcNode(1, 3);
            top.AddChild(child_1);
            top.AddChild(child_2);

            var child_1_1 = new SupportJoinProcNode(6, 3);
            var child_1_2 = new SupportJoinProcNode(7, 3);
            child_1.AddChild(child_1_1);
            child_1.AddChild(child_1_2);

            var child_1_1_1 = new SupportJoinProcNode(0, 3);
            child_1_1.AddChild(child_1_1_1);

            var result = top.Substreams;
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { 2, 5, 1, 6, 7, 0 }, result);
        }
    }
} // end of namespace
