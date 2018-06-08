///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.mxcif;
using com.espertech.esper.spatial.quadtree.mxcifrowindex;

namespace com.espertech.esper.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexGet<TL>
    {
        public static TL Get(double x, double y, double width, double height, MXCIFQuadTree<object> tree)
        {
            MXCIFQuadTreeFilterIndexCheckBB.CheckBB(tree.Root.Bb, x, y, width, height);
            return Get(x, y, width, height, tree.Root);
        }

        private static TL Get(double x, double y, double width, double height, MXCIFQuadTreeNode<object> node)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object>) {
                var leaf = (MXCIFQuadTreeNodeLeaf<object>) node;
                return GetFromData(x, y, width, height, leaf.Data);
            }

            var branch = (MXCIFQuadTreeNodeBranch<object>) node;
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

        private static TL GetFromData(double x, double y, double width, double height, object data)
        {
            if (data == null) {
                return default(TL);
            }

            if (data is XYWHRectangleWValue<TL>) {
                var value = (XYWHRectangleWValue<TL>) data;
                if (value.CoordinateEquals(x, y, width, height)) {
                    return value.Value;
                }

                return default(TL);
            }

            var collection = (ICollection<XYWHRectangleWValue<TL>>) data;
            foreach (var rectangle in collection) {
                if (rectangle.CoordinateEquals(x, y, width, height)) {
                    return (TL) rectangle.Value;
                }
            }

            return default(TL);
        }
    }
} // end of namespace