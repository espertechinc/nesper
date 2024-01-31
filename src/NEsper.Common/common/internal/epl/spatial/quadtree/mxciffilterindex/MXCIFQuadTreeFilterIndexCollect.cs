///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexCollect<TT>
    {
        public static void CollectRange(
            MXCIFQuadTree quadTree,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TT> collector,
            ExprEvaluatorContext ctx)
        {
            CollectRange(quadTree.Root, x, y, width, height, eventBean, target, collector, ctx);
        }

        private static void CollectRange(
            MXCIFQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TT> collector,
            ExprEvaluatorContext ctx)
        {
            if (node is MXCIFQuadTreeNodeLeaf leaf) {
                CollectNode(leaf, x, y, width, height, eventBean, target, collector, ctx);
                return;
            }

            var branch = (MXCIFQuadTreeNodeBranch)node;
            CollectNode(branch, x, y, width, height, eventBean, target, collector, ctx);
            CollectRange(branch.Nw, x, y, width, height, eventBean, target, collector, ctx);
            CollectRange(branch.Ne, x, y, width, height, eventBean, target, collector, ctx);
            CollectRange(branch.Sw, x, y, width, height, eventBean, target, collector, ctx);
            CollectRange(branch.Se, x, y, width, height, eventBean, target, collector, ctx);
        }

        private static void CollectNode(
            MXCIFQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            EventBean eventBean,
            TT target,
            QuadTreeCollector<TT> collector,
            ExprEvaluatorContext ctx)
        {
            var rectangles = node.Data;
            if (rectangles == null) {
                return;
            }

            if (rectangles is XYWHRectangleWValue rectangleWValue) {
                if (BoundingBox.IntersectsBoxIncludingEnd(
                        x,
                        y,
                        x + width,
                        y + height,
                        rectangleWValue.X,
                        rectangleWValue.Y,
                        rectangleWValue.W,
                        rectangleWValue.H)) {
                    collector.CollectInto(eventBean, rectangleWValue.Value, target, ctx);
                }
            }
            else if (rectangles is List<XYWHRectangleWValue> listWithType) {
                // Using the enumerable structure has some overhead when called a lot.  So, when
                // we know the value is a list (which it should be), then we attempt to use a
                // traditional loop to avoid the overhead.  Need to measure this against the time
                // cost for List.ForEach()

                var listWithTypeCount = listWithType.Count;
                for (var ii = 0; ii < listWithTypeCount; ii++) {
                    var rectangle = listWithType[ii];
                    if (BoundingBox.IntersectsBoxIncludingEnd(
                            x,
                            y,
                            x + width,
                            y + height,
                            rectangle.X,
                            rectangle.Y,
                            rectangle.W,
                            rectangle.H)) {
                        collector.CollectInto(eventBean, rectangle.Value, target, ctx);
                    }
                }
            }
            else if (rectangles is IEnumerable<XYWHRectangleWValue> enumerableWithType) {
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
                        collector.CollectInto(eventBean, rectangle.Value, target, ctx);
                    }
                }
            }
            else {
                throw new IllegalStateException("unknown type for rectangles");
            }
        }
    }
} // end of namespace