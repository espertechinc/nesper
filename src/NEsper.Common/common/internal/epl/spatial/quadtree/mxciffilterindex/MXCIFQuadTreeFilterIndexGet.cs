///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexGet
    {
        public static object Get(
            double x,
            double y,
            double width,
            double height,
            MXCIFQuadTree tree)
        {
            MXCIFQuadTreeFilterIndexCheckBB.CheckBB(tree.Root.Bb, x, y, width, height);
            return Get(x, y, width, height, tree.Root);
        }

        private static object Get(
            double x,
            double y,
            double width,
            double height,
            MXCIFQuadTreeNode node)
        {
            if (node is MXCIFQuadTreeNodeLeaf) {
                var leaf = (MXCIFQuadTreeNodeLeaf) node;
                return GetFromData(x, y, width, height, leaf.Data);
            }

            var branch = (MXCIFQuadTreeNodeBranch) node;
            var q = node.Bb.GetQuadrantApplies(x, y, width, height);
            switch (q) {
                case QuadrantAppliesEnum.NW:
                    return Get(x, y, width, height, branch.Nw);

                case QuadrantAppliesEnum.NE:
                    return Get(x, y, width, height, branch.Ne);

                case QuadrantAppliesEnum.SW:
                    return Get(x, y, width, height, branch.Sw);

                case QuadrantAppliesEnum.SE:
                    return Get(x, y, width, height, branch.Se);

                case QuadrantAppliesEnum.SOME:
                    return GetFromData(x, y, width, height, branch.Data);
            }

            throw new IllegalStateException("Not applicable to any quadrant");
        }

        private static object GetFromData(
            double x,
            double y,
            double width,
            double height,
            object data)
        {
            if (data == null) {
                return null;
            }

            if (data is XYWHRectangleWValue) {
                var value = (XYWHRectangleWValue) data;
                if (value.CoordinateEquals(x, y, width, height)) {
                    return value.Value;
                }

                return null;
            }

            foreach (var rectangle in (ICollection<XYWHRectangleWValue>) data) {
                if (rectangle.CoordinateEquals(x, y, width, height)) {
                    return rectangle.Value;
                }
            }

            return null;
        }
    }
} // end of namespace