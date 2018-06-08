///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexEmpty
    {
        public static bool IsEmpty(PointRegionQuadTree<object> quadTree)
        {
            return IsEmpty(quadTree.Root);
        }
    
        public static bool IsEmpty(PointRegionQuadTreeNode node)
        {
            return node is PointRegionQuadTreeNodeLeaf<object> leaf && leaf.Points == null;
        }
    }
} // end of namespace
