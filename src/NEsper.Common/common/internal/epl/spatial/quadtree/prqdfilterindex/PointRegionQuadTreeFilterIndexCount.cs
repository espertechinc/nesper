///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexCount
    {
        public static int Count(PointRegionQuadTree<object> quadTree)
        {
            return Count(quadTree.Root);
        }

        private static int Count(PointRegionQuadTreeNode node)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object> treeNodeLeaf) {
                return CountLeaf(treeNodeLeaf);
            }

            var branch = (PointRegionQuadTreeNodeBranch)node;
            return Count(branch.Nw) + Count(branch.Ne) + Count(branch.Sw) + Count(branch.Se);
        }

        private static int CountLeaf(PointRegionQuadTreeNodeLeaf<object> leaf)
        {
            if (leaf.Points == null) {
                return 0;
            }

            if (leaf.Points is XYPointWOpaqueValue) {
                return CountCallbacks(leaf.Points);
            }

            var coll = leaf.Points.Unwrap<XYPointWOpaqueValue>();
            var count = 0;
            foreach (var p in coll) {
                count += CountCallbacks(p.OpaqueValue);
            }

            return count;
        }

        private static int CountCallbacks(object points)
        {
            if (points is FilterHandleSize filterHandleSize) {
                return filterHandleSize.FilterCallbackCount;
            }

            return 1;
        }
    }
} // end of namespace