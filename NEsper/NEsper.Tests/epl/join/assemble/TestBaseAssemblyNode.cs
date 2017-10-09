///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportunit.epl.join;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.assemble
{
    [TestFixture]
    public class TestBaseAssemblyNode 
    {
        [Test]
        public void TestGetSubstreams()
        {
            SupportJoinProcNode top = new SupportJoinProcNode(2, 3);
    
            SupportJoinProcNode child_1 = new SupportJoinProcNode(5, 3);
            SupportJoinProcNode child_2 = new SupportJoinProcNode(1, 3);
            top.AddChild(child_1);
            top.AddChild(child_2);
    
            SupportJoinProcNode child_1_1 = new SupportJoinProcNode(6, 3);
            SupportJoinProcNode child_1_2 = new SupportJoinProcNode(7, 3);
            child_1.AddChild(child_1_1);
            child_1.AddChild(child_1_2);
    
            SupportJoinProcNode child_1_1_1 = new SupportJoinProcNode(0, 3);
            child_1_1.AddChild(child_1_1_1);
    
            int[] result = top.Substreams;
            EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 2, 5, 1, 6, 7, 0 }, result);
        }
    }
}
