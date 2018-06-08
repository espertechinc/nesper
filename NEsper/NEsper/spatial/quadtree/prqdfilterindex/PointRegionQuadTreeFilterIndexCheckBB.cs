///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.spatial.quadtree.core;

namespace com.espertech.esper.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexCheckBB
    {
        public static void CheckBB(BoundingBox bb, double x, double y)
        {
            if (!bb.ContainsPoint(x, y))
            {
                throw new EPException("Point (" + x + "," + y + ") not in " + bb);
            }
        }
    }
} // end of namespace
