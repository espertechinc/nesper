///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexCollect<TL, TT>
    {
        public static void CollectRange(
            MXCIFQuadTree<object> quadTree,
            double x, double y, 
            double width, double height,
            EventBean eventBean, TT target, 
            QuadTreeCollector<TL, TT> collector)
        {
            CollectRange(quadTree.Root, x, y, width, height, eventBean, target, collector);
        }

        private static void CollectRange(
            MXCIFQuadTreeNode<object> node, 
            double x, double y,
            double width, double height,
            EventBean eventBean, TT target, 
            QuadTreeCollector<TL, TT> collector)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object> leaf)
            {
                CollectNode(leaf, x, y, width, height, eventBean, target, collector);
                return;
            }

            var branch = (MXCIFQuadTreeNodeBranch<object>) node;
            CollectNode(branch, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Nw, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Ne, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Sw, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Se, x, y, width, height, eventBean, target, collector);
        }

        private static void CollectNode(
            MXCIFQuadTreeNode<object> node, 
            double x, double y,
            double width, double height,
            EventBean eventBean, TT target,
            QuadTreeCollector<TL, TT> collector)
        {
            var rectangles = node.Data;
            if (rectangles == null)
            {
                return;
            }

            if (rectangles is XYWHRectangleWValue<TL> rectangleX)
            {
                if (BoundingBox.IntersectsBoxIncludingEnd(
                    x, 
                    y,
                    x + width, 
                    y + height, 
                    rectangleX.X, 
                    rectangleX.Y,
                    rectangleX.W,
                    rectangleX.H))
                {
                    collector.CollectInto(eventBean, rectangleX.Value, target);
                }

                return;
            }

            var collection = (ICollection<XYWHRectangleWValue<TL>>) rectangles;
            foreach (XYWHRectangleWValue<TL> rectangle in collection) {
                if (BoundingBox.IntersectsBoxIncludingEnd(
                    x, y, x + width, y + height,
                    rectangle.X,
                    rectangle.Y,
                    rectangle.W,
                    rectangle.H)) {
                    collector.CollectInto(eventBean, rectangle.Value, target);
                }
            }
        }
    }
} // end of namespace
