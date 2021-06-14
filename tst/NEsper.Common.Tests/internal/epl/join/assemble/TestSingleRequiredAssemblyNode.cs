///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    [TestFixture]
    public class TestSingleRequiredAssemblyNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            reqNode = new BranchRequiredAssemblyNode(1, 3);
            parentNode = new SupportJoinProcNode(-1, 3);
            parentNode.AddChild(reqNode);
        }

        private SupportJoinProcNode parentNode;
        private BranchRequiredAssemblyNode reqNode;

        [Test]
        public void TestChildResult()
        {
            TestSingleOptionalAssemblyNode.TestChildResult(
                reqNode, parentNode, supportJoinResultNodeFactory);
        }

        [Test]
        public void TestProcess()
        {
            // the node does nothing when asked to process as it doesn't originate events
        }
    }
} // end of namespace
