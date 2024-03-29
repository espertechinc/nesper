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
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex.SupportMXCIFQuadTreeFilterIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    [TestFixture]
    public class TestMxcifQuadTreeFilterIndexRandomAddThenRemove : AbstractCommonTest
    {
        [Test]
        public void TestRun()
        {
            var toolsOne = new SupportQuadTreeToolUnique<MXCIFQuadTree>(
                MXCIF_FACTORY,
                SupportGeneratorPointUniqueByXYDouble.INSTANCE,
                MXCIF_FI_ADDER,
                MXCIF_FI_REMOVER,
                MXCIF_FI_QUERIER,
                false);
            SupportExecRandomAddThenRemove.RunAssertion(toolsOne);

            var toolsTwo = new SupportQuadTreeToolUnique<MXCIFQuadTree>(
                MXCIF_FACTORY,
                SupportGeneratorRectangleUniqueByXYWH.INSTANCE,
                MXCIF_FI_ADDER,
                MXCIF_FI_REMOVER,
                MXCIF_FI_QUERIER,
                false);
            SupportExecRandomAddThenRemove.RunAssertion(toolsTwo);
        }
    }
} // end of namespace
