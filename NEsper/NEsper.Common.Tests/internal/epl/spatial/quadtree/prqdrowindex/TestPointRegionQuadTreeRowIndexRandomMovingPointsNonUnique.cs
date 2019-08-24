///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class TestPointRegionQuadTreeRowIndexRandomMovingPointsNonUnique : AbstractCommonTest
    {
        [Test]
        public void TestNonUnique()
        {
            var tools = new SupportQuadTreeToolNonUnique<PointRegionQuadTree<object>>(
                POINTREGION_FACTORY,
                SupportGeneratorPointNonUniqueInteger.INSTANCE,
                POINTREGION_RI_ADDERNONUNIQUE,
                POINTREGION_RI_REMOVER,
                POINTREGION_RI_QUERIER,
                true);
            SupportExecNonUniqueRandomMovingRectangles.RunAssertion(tools);
        }
    }
} // end of namespace
