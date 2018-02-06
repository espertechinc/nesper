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
    public class PointRegionQuadTreeFactory<TL>
        where TL : class
    {
        public const int DEFAULT_LEAF_CAPACITY = 4;
        public const int DEFAULT_MAX_TREE_HEIGHT = 20;

        public static PointRegionQuadTree<TL> Make(
            double x, 
            double y, 
            double width, 
            double height,
            int leafCapacity, 
            int maxTreeHeight)
        {
            var bb = new BoundingBox(x, y, x + width, y + height);
            var leaf = new PointRegionQuadTreeNodeLeaf<TL>(bb, 1, null, 0);
            return new PointRegionQuadTree<TL>(leafCapacity, maxTreeHeight, leaf);
        }

        public static PointRegionQuadTree<TL> Make(
            double x, 
            double y, 
            double width, 
            double height)
        {
            return Make(x, y, width, height, DEFAULT_LEAF_CAPACITY, DEFAULT_MAX_TREE_HEIGHT);
        }
    }
} // end of namespace