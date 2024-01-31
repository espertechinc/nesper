///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class TestMxcifQuadTreeRowIndexRandomMovingPointsNonUnique : AbstractCommonTest
    {
        [Test]
        public void TestNonUnique()
        {
            SupportQuadTreeToolNonUnique<MXCIFQuadTree> tools = new SupportQuadTreeToolNonUnique<MXCIFQuadTree>(
                MXCIF_FACTORY,
                SupportGeneratorRectangleNonUniqueIntersecting.INSTANCE,
                MXCIF_RI_ADDERNONUNIQUE,
                MXCIF_RI_REMOVER,
                MXCIF_RI_QUERIER,
                false);
            SupportExecNonUniqueRandomMovingRectangles.RunAssertion(tools);
        }
    }
} // end of namespace
