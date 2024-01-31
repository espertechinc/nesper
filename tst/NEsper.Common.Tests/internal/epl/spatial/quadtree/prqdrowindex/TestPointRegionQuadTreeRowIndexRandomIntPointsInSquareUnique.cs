///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion.SupportPointRegionQuadTreeUtil;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex.SupportPointRegionQuadTreeRowIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    [TestFixture]
    public class TestPointRegionQuadTreeRowIndexRandomIntPointsInSquareUnique : AbstractCommonTest
    {
        [Test]
        public void TestRandomIntPoints()
        {
            SupportQuadTreeToolUnique<PointRegionQuadTree<object>> tools = new SupportQuadTreeToolUnique<PointRegionQuadTree<object>>(
                POINTREGION_FACTORY,
                SupportGeneratorPointUniqueByXYInteger.INSTANCE,
                POINTREGION_RI_ADDERUNIQUE,
                POINTREGION_RI_REMOVER,
                POINTREGION_RI_QUERIER,
                false);
            SupportExecUniqueRandomIntPointsInSquare.RunAssertion(tools);
        }
    }
} // end of namespace
