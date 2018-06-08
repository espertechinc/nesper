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
    public class PointRegionQuadTreeNodeBranch : PointRegionQuadTreeNode
    {
        public PointRegionQuadTreeNodeBranch(
            BoundingBox bb, int level,
            PointRegionQuadTreeNode nw,
            PointRegionQuadTreeNode ne,
            PointRegionQuadTreeNode sw,
            PointRegionQuadTreeNode se)
            : base(bb, level)
        {
            Nw = nw;
            Ne = ne;
            Sw = sw;
            Se = se;
        }

        public PointRegionQuadTreeNode Nw { get; set; }

        public PointRegionQuadTreeNode Ne { get; set; }

        public PointRegionQuadTreeNode Sw { get; set; }

        public PointRegionQuadTreeNode Se { get; set; }
    }
} // end of namespace
