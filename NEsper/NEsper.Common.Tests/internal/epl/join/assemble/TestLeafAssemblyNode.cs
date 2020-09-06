///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    [TestFixture]
    public class TestLeafAssemblyNode : AbstractCommonTest
    {
        private SupportJoinProcNode parentNode;
        private LeafAssemblyNode leafNode;

        [SetUp]
        public void SetUp()
        {
            leafNode = new LeafAssemblyNode(1, 4);
            parentNode = new SupportJoinProcNode(-1, 4);
            parentNode.AddChild(leafNode);
        }

        [Test, RunInApplicationDomain]
        public void TestProcess()
        {
            IList<Node>[] result = supportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 2, 2);

            leafNode.Process(result, new List<EventBean[]>(), null);

            Assert.AreEqual(4, parentNode.RowsList.Count);
            Assert.AreEqual(result[1][0].Events.First(), parentNode.RowsList[0][1]);   // compare event
        }

        [Test, RunInApplicationDomain]
        public void TestChildResult()
        {
            try
            {
                leafNode.Result(null, 0, null, null, null, null);
                Assert.Fail();
            }
            catch (UnsupportedOperationException)
            {
                // expected
            }
        }
    }
} // end of namespace
