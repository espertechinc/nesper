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
    public class TestMxcifQuadTreeFilterIndexRandomIntPointsInSquare : AbstractCommonTest
    {
        [Test]
        public void TestRandomIntPoints()
        {
            var tools = new SupportQuadTreeToolUnique<MXCIFQuadTree>(
                MXCIF_FACTORY,
                SupportGeneratorPointUniqueByXYInteger.INSTANCE,
                MXCIF_FI_ADDER,
                MXCIF_FI_REMOVER,
                MXCIF_FI_QUERIER,
                true);
            SupportExecUniqueRandomIntPointsInSquare.RunAssertion(tools);
        }
    }
} // end of namespace
