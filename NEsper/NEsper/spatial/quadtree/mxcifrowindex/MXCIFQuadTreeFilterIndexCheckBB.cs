///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.spatial.quadtree.core;

namespace com.espertech.esper.spatial.quadtree.mxcifrowindex
{
    public class MXCIFQuadTreeFilterIndexCheckBB
    {
        public static void CheckBB(BoundingBox bb, double x, double y, double width, double height)
        {
            if (!bb.IntersectsBoxIncludingEnd(x, y, width, height))
            {
                throw new EPException("Rectangle (" + x + "," + y + "," + width + "," + height + ") not in " + bb);
            }
        }
    }
} // end of namespace
