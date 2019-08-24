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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexCollect<TL, TT>
    {
        public static void CollectRange(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TL, TT> collector)
        {
            CollectRange(quadTree.Root, x, y, width, height, eventBean, target, collector);
        }

        private static void CollectRange(
            PointRegionQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TL, TT> collector)
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

        private static void CollectLeaf(
            PointRegionQuadTreeNodeLeaf<object> node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TL, TT> collector)
        {
            var points = node.Points;
            if (points == null) {
                return;
            }

            if (points is XYPointWValue<TL> pointWValueWithType) {
                if (BoundingBox.ContainsPoint(x, y, width, height, pointWValueWithType.X, pointWValueWithType.Y)) {
                    collector.CollectInto(eventBean, pointWValueWithType.Value, target);
                }

                return;
            }
            else if (points is XYPointWValue<object> pointWValueWithoutType) {
                if (BoundingBox.ContainsPoint(
                    x,
                    y,
                    width,
                    height,
                    pointWValueWithoutType.X,
                    pointWValueWithoutType.Y)) {
                    collector.CollectInto(eventBean, (TL) pointWValueWithoutType.Value, target);
                }

                return;
            }
            else if (points is IEnumerable<XYPointWValue<TL>> enumerableWithType) {
                foreach (var point in enumerableWithType) {
                    if (BoundingBox.ContainsPoint(x, y, width, height, point.X, point.Y)) {
                        collector.CollectInto(eventBean, point.Value, target);
                    }
                }
            }
            else if (points is IEnumerable<XYPointWValue<object>> enumerableWithoutType) {
                foreach (var point in enumerableWithoutType) {
                    if (BoundingBox.ContainsPoint(x, y, width, height, point.X, point.Y)) {
                        collector.CollectInto(eventBean, (TL) point.Value, target);
                    }
                }
            }
            else {
                throw new IllegalStateException("unknown type for points");
            }
        }
    }
} // end of namespace