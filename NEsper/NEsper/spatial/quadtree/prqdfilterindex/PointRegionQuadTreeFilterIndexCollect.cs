///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.spatial.quadtree.prqdfilterindex
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
            if (!node.Bb.IntersectsBoxIncludingEnd(x, y, width, height)) return;
            if (node is PointRegionQuadTreeNodeLeaf<object> leaf)
            {
                CollectLeaf(leaf, x, y, width, height, eventBean, target, collector);
                return;
            }

            PointRegionQuadTreeNodeBranch branch = (PointRegionQuadTreeNodeBranch) node;
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
            if (points == null) return;
            if (points is XYPointWValue<TL> point)
            {
                if (BoundingBox.ContainsPoint(x, y, width, height, point.X, point.Y))
                    collector.CollectInto(eventBean, point.Value, target);
                return;
            }

            var collection = (ICollection<XYPointWValue<TL>>) points;
            foreach(var pointX in collection)
            {
                if (BoundingBox.ContainsPoint(x, y, width, height, pointX.X, pointX.Y))
                    collector.CollectInto(eventBean, pointX.Value, target);
            }
        }
    }
} // end of namespace