///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    public class MXCIFQuadTreeFilterIndexCheckBB
    {
        public static void CheckBB(
            BoundingBox bb,
            double x,
            double y,
            double width,
            double height)
        {
            if (!bb.IntersectsBoxIncludingEnd(x, y, width, height)) {
                throw new EPException(string.Format("Rectangle ({0},{1},{2},{3}) not in {4}", 
                    x.RenderAny(),
                    y.RenderAny(),
                    width.RenderAny(),
                    height.RenderAny(),
                    bb));
            }
        }
    }
} // end of namespace