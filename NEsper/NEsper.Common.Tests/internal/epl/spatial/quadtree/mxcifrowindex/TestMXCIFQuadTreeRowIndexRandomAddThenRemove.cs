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

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    [TestFixture]
    public class TestMxcifQuadTreeRowIndexRandomAddThenRemove : AbstractCommonTest
    {
        [Test, RunInApplicationDomain]
        public void TestRun()
        {
            SupportQuadTreeToolUnique<PointRegionQuadTree<object>> tools = new SupportQuadTreeToolUnique<PointRegionQuadTree<object>>(
                POINTREGION_FACTORY,
                SupportGeneratorRectangleNonUniqueIntersecting.INSTANCE,
                POINTREGION_RI_ADDERUNIQUE,
                POINTREGION_RI_REMOVER,
                POINTREGION_RI_QUERIER,
                true);
            SupportExecRandomAddThenRemove.RunAssertion(tools);
        }
    }
} // end of namespace
