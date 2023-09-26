///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexGet<TL>
    {
        public static TL Get(
            double x,
            double y,
            PointRegionQuadTree<object> tree)
        {
            PointRegionQuadTreeFilterIndexCheckBB.CheckBB(tree.Root.Bb, x, y);
            return Get(x, y, tree.Root);
        }

        private static TL Get(
            double x,
            double y,
            PointRegionQuadTreeNode node)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object> leaf) {
                if (leaf.Points == null) {
                    return default;
                }

                if (leaf.Points is XYPointWValue<TL> value) {
                    if (value.X == x && value.Y == y) {
                        return value.Value;
                    }

                    return default;
                }

                var collection = (ICollection<XYPointWValue<TL>>)leaf.Points;
                foreach (var point in collection) {
                    if (point.X == x && point.Y == y) {
                        return point.Value;
                    }
                }

                return default;
            }

            var branch = (PointRegionQuadTreeNodeBranch)node;
            var q = node.Bb.GetQuadrant(x, y);
            switch (q) {
                case QuadrantEnum.NW:
                    return Get(x, y, branch.Nw);

                case QuadrantEnum.NE:
                    return Get(x, y, branch.Ne);

                case QuadrantEnum.SW:
                    return Get(x, y, branch.Sw);

                default:
                    return Get(x, y, branch.Se);
            }
        }
    }
} // end of namespace