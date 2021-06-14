///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif.SupportMXCIFQuadTreeUtil;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex.SupportMXCIFQuadTreeFilterIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    [TestFixture]
    public class TestMxcifQuadTreeFilterIndexRandomMovingPoints : AbstractCommonTest
    {
        [Test]
        public void TestIt()
        {
            var tools = new SupportQuadTreeToolUnique<MXCIFQuadTree>(
                MXCIF_FACTORY,
                null,
                MXCIF_FI_ADDER,
                MXCIF_FI_REMOVER,
                MXCIF_FI_QUERIER,
                false);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 1, 1);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 10, 10);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 0.1, 0.1);
        }
    }
} // end of namespace
