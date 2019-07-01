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
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex.SupportMXCIFQuadTreeRowIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    [TestFixture]
    public class TestMxcifQuadTreeRowIndexRandomMovingPointsUnique : AbstractTestBase
    {
        [Test]
        public void TestUnique()
        {
            var tools = new SupportQuadTreeToolUnique<MXCIFQuadTree<object>>(
                MXCIF_FACTORY,
                null,
                MXCIF_RI_ADDERUNIQUE,
                MXCIF_RI_REMOVER,
                MXCIF_RI_QUERIER,
                false);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 5, 5);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 1, 1);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 0.5, 0.5);
            SupportExecUniqueRandomMovingRectangles.RunAssertion(tools, 0.99, 0.99);
        }
    }
} // end of namespace
