///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    public class MXCIFQuadTreeRowIndexQuery
    {
        public static ICollection<object> QueryRange(
            MXCIFQuadTree quadTree,
            double x,
            double y,
            double width,
            double height)
        {
            return QueryNode(quadTree.Root, x, y, width, height, null);
        }

        private static ICollection<object> QueryNode(
            MXCIFQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            ICollection<object> result)
        {
            if (node is MXCIFQuadTreeNodeLeaf leaf) {
                return Visit(leaf, x, y, width, height, result);
            }

            var branch = (MXCIFQuadTreeNodeBranch) node;
            result = Visit(branch, x, y, width, height, result);
            result = QueryNode(branch.Nw, x, y, width, height, result);
            result = QueryNode(branch.Ne, x, y, width, height, result);
            result = QueryNode(branch.Sw, x, y, width, height, result);
            result = QueryNode(branch.Se, x, y, width, height, result);
            return result;
        }

        private static ICollection<object> Visit(
            MXCIFQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            ICollection<object> result)
        {
            var data = node.Data;
            if (data == null) {
                return result;
            }

            if (data is XYWHRectangleMultiType point) {
                return Visit(point, x, y, width, height, result);
            } else if (data is IList<XYWHRectangleMultiType> listData) {
                var listDataCount = listData.Count;
                for (var ii = 0; ii < listDataCount; ii++) {
                    result = Visit(listData[ii], x, y, width, height, result);
                }
            } else if (data is IEnumerable<XYWHRectangleMultiType> enumData) {
                foreach (var rectangle in enumData) {
                    result = Visit(rectangle, x, y, width, height, result);
                }
            }
            else {
                throw new IllegalStateException("type-erasure failure");
            }

            return result;
        }

        private static ICollection<object> Visit(
            XYWHRectangleMultiType rectangle,
            double x,
            double y,
            double width,
            double height,
            ICollection<object> result)
        {
            if (!BoundingBox.IntersectsBoxIncludingEnd(
                x,
                y,
                x + width,
                y + height,
                rectangle.X,
                rectangle.Y,
                rectangle.W,
                rectangle.H)) {
                return result;
            }

            if (result == null) {
                result = new ArrayDeque<object>(4);
            }

            rectangle.CollectInto(result);
            return result;
        }
    }
} // end of namespace