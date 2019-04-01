///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexCollect
    {
        public static void CollectRange<TL, TT>(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target, QuadTreeCollector<TL, TT> collector)
        {
            CollectRange(quadTree.Root, x, y, width, height, eventBean, target, collector);
        }

        private static void CollectRange<L, T>(
            PointRegionQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            T target,
            QuadTreeCollector<L, T> collector)
        {
            if (!node.Bb.IntersectsBoxIncludingEnd(x, y, width, height)) {
                return;
            }

            if (node is PointRegionQuadTreeNodeLeaf<object>) {
                var leaf = (PointRegionQuadTreeNodeLeaf<object>) node;
                CollectLeaf(leaf, x, y, width, height, eventBean, target, collector);
                return;
            }

            var branch = (PointRegionQuadTreeNodeBranch) node;
            CollectRange(branch.Nw, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Ne, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Sw, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Se, x, y, width, height, eventBean, target, collector);
        }

        private static void CollectLeaf<L, T>(
            PointRegionQuadTreeNodeLeaf<object> node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            T target,
            QuadTreeCollector<L, T> collector)
        {
            var points = node.Points;
            if (points == null) {
                return;
            }

            if (points is XYPointWValue<L>) {
                var point = (XYPointWValue<L>) points;
                if (BoundingBox.ContainsPoint(x, y, width, height, point.X, point.Y)) {
                    collector.CollectInto(eventBean, point.Value, target);
                }

                return;
            }

            var collection = (ICollection<XYPointWValue<L>>) points;
            foreach (var point in collection) {
                if (BoundingBox.ContainsPoint(x, y, width, height, point.X, point.Y)) {
                    collector.CollectInto(eventBean, point.Value, target);
                }
            }
        }
    }
} // end of namespace