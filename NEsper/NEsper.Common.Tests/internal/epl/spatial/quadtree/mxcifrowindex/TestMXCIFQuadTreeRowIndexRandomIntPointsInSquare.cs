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
    public class TestMxcifQuadTreeRowIndexRandomIntPointsInSquare : AbstractCommonTest
    {
        [Test]
        public void TestRandomIntPoints()
        {
            SupportQuadTreeToolUnique<MXCIFQuadTree> tools = new SupportQuadTreeToolUnique<MXCIFQuadTree>(
                MXCIF_FACTORY,
                SupportGeneratorPointUniqueByXYInteger.INSTANCE,
                MXCIF_RI_ADDERUNIQUE,
                MXCIF_RI_REMOVER,
                MXCIF_RI_QUERIER,
                false);
            SupportExecUniqueRandomIntPointsInSquare.RunAssertion(tools);
        }
    }
} // end of namespace
