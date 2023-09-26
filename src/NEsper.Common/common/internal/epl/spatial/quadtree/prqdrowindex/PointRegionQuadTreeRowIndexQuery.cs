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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    public class PointRegionQuadTreeRowIndexQuery
    {
        public static ICollection<object> QueryRange(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            double width,
            double height)
        {
            return QueryNode(quadTree.Root, x, y, width, height, null);
        }

        private static ICollection<object> QueryNode(
            PointRegionQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            ICollection<object> result)
        {
            if (!node.Bb.IntersectsBoxIncludingEnd(x, y, width, height)) {
                return result;
            }

            if (node is PointRegionQuadTreeNodeLeaf<object> leaf) {
                return Visit(leaf, x, y, width, height, result);
            }

            var branch = (PointRegionQuadTreeNodeBranch)node;
            result = QueryNode(branch.Nw, x, y, width, height, result);
            result = QueryNode(branch.Ne, x, y, width, height, result);
            result = QueryNode(branch.Sw, x, y, width, height, result);
            result = QueryNode(branch.Se, x, y, width, height, result);
            return result;
        }

        private static ICollection<object> Visit(
            PointRegionQuadTreeNodeLeaf<object> node,
            double x,
            double y,
            double width,
            double height,
            ICollection<object> result)
        {
            var points = node.Points;
            if (points == null) {
                return result;
            }

            if (points is XYPointMultiType type) {
                return Visit(type, x, y, width, height, result);
            }

            var collection = (ICollection<XYPointMultiType>)points;
            foreach (var point in collection) {
                result = Visit(point, x, y, width, height, result);
            }

            return result;
        }

        private static ICollection<object> Visit(
            XYPointMultiType point,
            double x,
            double y,
            double width,
            double height,
            ICollection<object> result)
        {
            if (!BoundingBox.ContainsPoint(x, y, width, height, point.X, point.Y)) {
                return result;
            }

            if (result == null) {
                result = new ArrayDeque<object>(4);
            }

            point.CollectInto(result);
            return result;
        }
    }
} // end of namespace