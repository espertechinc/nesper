///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.filter;
using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexCount
    {
        public static int Count(PointRegionQuadTree<object> quadTree)
        {
            return Count(quadTree.Root);
        }

        private static int Count(PointRegionQuadTreeNode node)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object>)
            {
                var leaf = (PointRegionQuadTreeNodeLeaf<object>) node;
                return CountLeaf(leaf);
            }

            var branch = (PointRegionQuadTreeNodeBranch) node;
            return Count(branch.Nw) + Count(branch.Ne) + Count(branch.Sw) + Count(branch.Se);
        }

        private static int CountLeaf(PointRegionQuadTreeNodeLeaf<object> leaf)
        {
            if (leaf.Points == null)
                return 0;
            if (leaf.Points is XYPointWValue<object>)
                return CountCallbacks(leaf.Points);

            var coll = (ICollection<XYPointWValue<object>>) leaf.Points;
            var count = 0;
            foreach (var p in coll)
                count += CountCallbacks(p.Value);
            return count;
        }

        private static int CountCallbacks(object points)
        {
            if (points is FilterHandleSetNode)
                return ((FilterHandleSetNode) points).FilterCallbackCount;
            if (points is FilterParamIndexBase)
                return ((FilterParamIndexBase) points).Count;

            return 1;
        }
    }
} // end of namespace