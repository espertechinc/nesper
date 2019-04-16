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
    public class MXCIFQuadTreeFilterIndexSet<TL>
    {
        public static void Set(
            double x,
            double y,
            double width,
            double height,
            TL value,
            MXCIFQuadTree<object> tree)
        {
            var root = tree.Root;
            MXCIFQuadTreeFilterIndexCheckBB.CheckBB(root.Bb, x, y, width, height);
            var replacement = SetOnNode(x, y, width, height, value, root, tree);
            tree.Root = replacement;
        }

        private static MXCIFQuadTreeNode<object> SetOnNode(
            double x,
            double y,
            double width,
            double height,
            TL value,
            MXCIFQuadTreeNode<object> node,
            MXCIFQuadTree<object> tree)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object>) {
                var leaf = (MXCIFQuadTreeNodeLeaf<object>) node;
                var count = SetOnNode(leaf, x, y, width, height, value);
                leaf.IncCount(count);

                if (leaf.Count <= tree.LeafCapacity || node.Level >= tree.MaxTreeHeight) {
                    return leaf;
                }

                node = Subdivide(leaf, tree);
            }

            var branch = (MXCIFQuadTreeNodeBranch<object>) node;
            AddToBranch(branch, x, y, width, height, value, tree);
            return node;
        }

        private static void AddToBranch(
            MXCIFQuadTreeNodeBranch<object> branch,
            double x,
            double y,
            double width,
            double height,
            object value,
            MXCIFQuadTree<object> tree)
        {
            QuadrantAppliesEnum quadrant = branch.Bb.GetQuadrantApplies(x, y, width, height);
            if (quadrant == QuadrantAppliesEnum.NW) {
                branch.Nw = SetOnNode(x, y, width, height, value, branch.Nw, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.NE) {
                branch.Ne = SetOnNode(x, y, width, height, value, branch.Ne, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SW) {
                branch.Sw = SetOnNode(x, y, width, height, value, branch.Sw, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SE) {
                branch.Se = SetOnNode(x, y, width, height, value, branch.Se, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SOME) {
                var count = SetOnNode(branch, x, y, width, height, value);
                branch.IncCount(count);
            }
            else {
                throw new IllegalStateException("Quandrant not applies to any");
            }
        }

        private static MXCIFQuadTreeNode<object> Subdivide(
            MXCIFQuadTreeNodeLeaf<object> leaf,
            MXCIFQuadTree<object> tree)
        {
            var w = (leaf.Bb.MaxX - leaf.Bb.MinX) / 2d;
            var h = (leaf.Bb.MaxY - leaf.Bb.MinY) / 2d;
            double minx = leaf.Bb.MinX;
            double miny = leaf.Bb.MinY;

            var bbNW = new BoundingBox(minx, miny, minx + w, miny + h);
            var bbNE = new BoundingBox(minx + w, miny, leaf.Bb.MaxX, miny + h);
            var bbSW = new BoundingBox(minx, miny + h, minx + w, leaf.Bb.MaxY);
            var bbSE = new BoundingBox(minx + w, miny + h, leaf.Bb.MaxX, leaf.Bb.MaxY);
            MXCIFQuadTreeNode<object> nw = new MXCIFQuadTreeNodeLeaf<object>(bbNW, leaf.Level + 1, null, 0);
            MXCIFQuadTreeNode<object> ne = new MXCIFQuadTreeNodeLeaf<object>(bbNE, leaf.Level + 1, null, 0);
            MXCIFQuadTreeNode<object> sw = new MXCIFQuadTreeNodeLeaf<object>(bbSW, leaf.Level + 1, null, 0);
            MXCIFQuadTreeNode<object> se = new MXCIFQuadTreeNodeLeaf<object>(bbSE, leaf.Level + 1, null, 0);
            MXCIFQuadTreeNodeBranch<object> branch = new MXCIFQuadTreeNodeBranch<object>(
                leaf.Bb, leaf.Level, null, 0, nw, ne, sw, se);

            object rectangles = leaf.Data;
            if (rectangles is XYWHRectangleWValue<TL>) {
                XYWHRectangleWValue<TL> rectangle = (XYWHRectangleWValue<TL>) rectangles;
                Subdivide(rectangle, branch, tree);
            }
            else {
                var collection = (ICollection<XYWHRectangleWValue<TL>>) rectangles;
                foreach (var rectangle in collection) {
                    Subdivide(rectangle, branch, tree);
                }
            }

            return branch;
        }

        private static void Subdivide(
            XYWHRectangleWValue<TL> rectangle,
            MXCIFQuadTreeNodeBranch<object> branch,
            MXCIFQuadTree<object> tree)
        {
            var x = rectangle.X;
            var y = rectangle.Y;
            var w = rectangle.W;
            var h = rectangle.H;
            QuadrantAppliesEnum quadrant = branch.Bb.GetQuadrantApplies(x, y, w, h);
            if (quadrant == QuadrantAppliesEnum.NW) {
                branch.Nw = SetOnNode(x, y, w, h, rectangle, branch.Nw, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.NE) {
                branch.Ne = SetOnNode(x, y, w, h, rectangle, branch.Ne, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SW) {
                branch.Sw = SetOnNode(x, y, w, h, rectangle, branch.Sw, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SE) {
                branch.Se = SetOnNode(x, y, w, h, rectangle, branch.Se, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SOME) {
                var numAdded = SetOnNode(branch, x, y, w, h, rectangle);
                branch.IncCount(numAdded);
            }
            else {
                throw new IllegalStateException("No intersection");
            }
        }

        private static int SetOnNode(
            MXCIFQuadTreeNode<object> node,
            double x,
            double y,
            double width,
            double height,
            TL value)
        {
            object currentValue = node.Data;

            if (value is XYWHRectangleWValue<TL>) {
                var rectangle = (XYWHRectangleWValue<TL>) value;
                if (!rectangle.CoordinateEquals(x, y, width, height)) {
                    throw new IllegalStateException();
                }

                if (currentValue == null) {
                    node.Data = rectangle;
                    return 1;
                }

                if (currentValue is XYWHRectangleWValue<TL>) {
                    var other = (XYWHRectangleWValue<TL>) currentValue;
                    if (other.CoordinateEquals(x, y, width, height)) {
                        other.Value = value;
                        return 0; // replaced
                    }

                    ICollection<XYWHRectangleWValue<TL>> collectionX = new LinkedList<XYWHRectangleWValue<TL>>();
                    collectionX.Add(other);
                    collectionX.Add(rectangle);
                    node.Data = collectionX;
                    return 1;
                }

                var collectionY = (ICollection<XYWHRectangleWValue<TL>>) currentValue;
                foreach (var other in collectionY) {
                    if (other.CoordinateEquals(x, y, width, height)) {
                        other.Value = value;
                        return 0;
                    }
                }

                collectionY.Add(rectangle);
                return 1;
            }

            if (currentValue == null) {
                XYWHRectangleWValue<TL> point = new XYWHRectangleWValue<TL>(x, y, width, height, value);
                node.Data = point;
                return 1;
            }

            if (currentValue is XYWHRectangleWValue<TL>) {
                var other = (XYWHRectangleWValue<TL>) currentValue;
                if (other.CoordinateEquals(x, y, width, height)) {
                    other.Value = value;
                    return 0;
                }

                ICollection<XYWHRectangleWValue<TL>> collectionX = new LinkedList<XYWHRectangleWValue<TL>>();
                collectionX.Add(other);
                collectionX.Add(new XYWHRectangleWValue<TL>(x, y, width, height, value));
                node.Data = collectionX;
                return 1;
            }

            var collectionZ = (ICollection<XYWHRectangleWValue<TL>>) currentValue;
            foreach (var other in collectionZ) {
                if (other.CoordinateEquals(x, y, width, height)) {
                    other.Value = value;
                    return 0;
                }
            }

            collectionZ.Add(new XYWHRectangleWValue<TL>(x, y, width, height, value));
            return 1;
        }
    }
} // end of namespace