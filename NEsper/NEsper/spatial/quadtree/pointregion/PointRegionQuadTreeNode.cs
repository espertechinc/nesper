///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.spatial.quadtree.core;

namespace com.espertech.esper.spatial.quadtree.pointregion
{
    public abstract class PointRegionQuadTreeNode
    {
        protected PointRegionQuadTreeNode(BoundingBox bb, int level)
        {
            Bb = bb;
            Level = level;
        }

        public BoundingBox Bb { get; }

        public int Level { get; }
    }
} // end of namespace