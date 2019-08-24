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
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexCollect<TL, TT>
    {
        public static void CollectRange(
            MXCIFQuadTree<object> quadTree,
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
            MXCIFQuadTreeNode<object> node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TL, TT> collector)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object> leaf)
            {
                CollectNode(leaf, x, y, width, height, eventBean, target, collector);
                return;
            }

            MXCIFQuadTreeNodeBranch<object> branch = (MXCIFQuadTreeNodeBranch<object>) node;
            CollectNode(branch, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Nw, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Ne, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Sw, x, y, width, height, eventBean, target, collector);
            CollectRange(branch.Se, x, y, width, height, eventBean, target, collector);
        }

        private static void CollectNode(
            MXCIFQuadTreeNode<object> node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TL, TT> collector)
        {
            object rectangles = node.Data;
            if (rectangles == null) {
                return;
            }

            if (rectangles is XYWHRectangleWValue<TL> rectangleWValue) {
                if (BoundingBox.IntersectsBoxIncludingEnd(
                    x,
                    y,
                    x + width,
                    y + height,
                    rectangleWValue.X,
                    rectangleWValue.Y,
                    rectangleWValue.W,
                    rectangleWValue.H)) {
                    collector.CollectInto(eventBean, rectangleWValue.Value, target);
                }
            }
            else if (rectangles is XYWHRectangleWValue<object> rectangleWValueGeneric) {
                if (BoundingBox.IntersectsBoxIncludingEnd(
                    x,
                    y,
                    x + width,
                    y + height,
                    rectangleWValueGeneric.X,
                    rectangleWValueGeneric.Y,
                    rectangleWValueGeneric.W,
                    rectangleWValueGeneric.H)) {
                    collector.CollectInto(eventBean, (TL) rectangleWValueGeneric.Value, target);
                }
            }
            else if (rectangles is IEnumerable<XYWHRectangleWValue<TL>> enumerableWithType) {
                foreach (var rectangle in enumerableWithType) {
                    if (BoundingBox.IntersectsBoxIncludingEnd(
                        x,
                        y,
                        x + width,
                        y + height,
                        rectangle.X,
                        rectangle.Y,
                        rectangle.W,
                        rectangle.H)) {
                        collector.CollectInto(eventBean, rectangle.Value, target);
                    }
                }
            }
            else if (rectangles is IEnumerable<XYWHRectangleWValue<object>> enumerableWithoutType) {
                foreach (var rectangle in enumerableWithoutType) {
                    if (BoundingBox.IntersectsBoxIncludingEnd(
                        x,
                        y,
                        x + width,
                        y + height,
                        rectangle.X,
                        rectangle.Y,
                        rectangle.W,
                        rectangle.H)) {
                        collector.CollectInto(eventBean, (TL) rectangle.Value, target);
                    }
                }
            }
            else {
                throw new IllegalStateException("unknown type for rectangles");
            }
        }
    }
} // end of namespace