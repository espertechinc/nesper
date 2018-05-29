///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexSet<TL>
    {
        internal static void Set(
            double x, double y,
            TL value,
            PointRegionQuadTree<object> tree)
        {
            var root = tree.Root;
            PointRegionQuadTreeFilterIndexCheckBB.CheckBB(root.Bb, x, y);
            tree.Root = SetOnNode(x, y, value, root, tree);
        }

        private static PointRegionQuadTreeNode SetOnNode(
            double x, double y,
            XYPointWValue<TL> value,
            PointRegionQuadTreeNode node,
            PointRegionQuadTree<object> tree)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object> leaf) {
                var count = SetOnLeaf(leaf, x, y, value);
                leaf.IncCount(count);

                if (leaf.Count <= tree.LeafCapacity || node.Level >= tree.MaxTreeHeight) {
                    return leaf;
                }

                node = Subdivide(leaf, tree);
            }

            var branch = (PointRegionQuadTreeNodeBranch) node;
            AddToBranch(branch, x, y, value, tree);
            return node;
        }

        private static PointRegionQuadTreeNode SetOnNode(
            double x, double y,
            TL value,
            PointRegionQuadTreeNode node,
            PointRegionQuadTree<object> tree)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object> leaf) {
                var count = SetOnLeaf(leaf, x, y, value);
                leaf.IncCount(count);

                if (leaf.Count <= tree.LeafCapacity || node.Level >= tree.MaxTreeHeight) {
                    return leaf;
                }

                node = Subdivide(leaf, tree);
            }

            var branch = (PointRegionQuadTreeNodeBranch) node;
            AddToBranch(branch, x, y, value, tree);
            return node;
        }

        private static void AddToBranch(
            PointRegionQuadTreeNodeBranch branch,
            double x, double y,
            XYPointWValue<TL> value,
            PointRegionQuadTree<object> tree)
        {
            switch (branch.Bb.GetQuadrant(x, y))
            {
                case QuadrantEnum.NW:
                    branch.Nw = SetOnNode(x, y, value, branch.Nw, tree);
                    break;
                case QuadrantEnum.NE:
                    branch.Ne = SetOnNode(x, y, value, branch.Ne, tree);
                    break;
                case QuadrantEnum.SW:
                    branch.Sw = SetOnNode(x, y, value, branch.Sw, tree);
                    break;
                default:
                    branch.Se = SetOnNode(x, y, value, branch.Se, tree);
                    break;
            }
        }

        private static void AddToBranch(
            PointRegionQuadTreeNodeBranch branch,
            double x, double y,
            TL value,
            PointRegionQuadTree<object> tree)
        {
            switch (branch.Bb.GetQuadrant(x, y)) {
                case QuadrantEnum.NW:
                    branch.Nw = SetOnNode(x, y, value, branch.Nw, tree);
                    break;
                case QuadrantEnum.NE:
                    branch.Ne = SetOnNode(x, y, value, branch.Ne, tree);
                    break;
                case QuadrantEnum.SW:
                    branch.Sw = SetOnNode(x, y, value, branch.Sw, tree);
                    break;
                default:
                    branch.Se = SetOnNode(x, y, value, branch.Se, tree);
                    break;
            }
        }

        private static PointRegionQuadTreeNode Subdivide(
            PointRegionQuadTreeNodeLeaf<object> leaf,
            PointRegionQuadTree<object> tree)
        {
            var w = (leaf.Bb.MaxX - leaf.Bb.MinX) / 2d;
            var h = (leaf.Bb.MaxY - leaf.Bb.MinY) / 2d;
            var minx = leaf.Bb.MinX;
            var miny = leaf.Bb.MinY;

            var bbNW = new BoundingBox(minx, miny, minx + w, miny + h);
            var bbNE = new BoundingBox(minx + w, miny, leaf.Bb.MaxX, miny + h);
            var bbSW = new BoundingBox(minx, miny + h, minx + w, leaf.Bb.MaxY);
            var bbSE = new BoundingBox(minx + w, miny + h, leaf.Bb.MaxX, leaf.Bb.MaxY);
            var nw = new PointRegionQuadTreeNodeLeaf<object>(bbNW, leaf.Level + 1, null, 0);
            var ne = new PointRegionQuadTreeNodeLeaf<object>(bbNE, leaf.Level + 1, null, 0);
            var sw = new PointRegionQuadTreeNodeLeaf<object>(bbSW, leaf.Level + 1, null, 0);
            var se = new PointRegionQuadTreeNodeLeaf<object>(bbSE, leaf.Level + 1, null, 0);
            var branch = new PointRegionQuadTreeNodeBranch(leaf.Bb, leaf.Level, nw, ne, sw, se);

            var points = leaf.Points;
            if (points is XYPointWValue<TL>) {
                var point = (XYPointWValue<TL>) points;
                SubdividePoint(point, branch, tree);
            }
            else {
                foreach (var point in (ICollection<XYPointWValue<TL>>) points) {
                    SubdividePoint(point, branch, tree);
                }
            }

            return branch;
        }

        private static void SubdividePoint(
            XYPointWValue<TL> point,
            PointRegionQuadTreeNodeBranch branch,
            PointRegionQuadTree<object> tree)
        {
            var x = point.X;
            var y = point.Y;

            switch (branch.Bb.GetQuadrant(x, y)) {
                case QuadrantEnum.NW:
                    branch.Nw = SetOnNode(x, y, point, branch.Nw, tree);
                    break;
                case QuadrantEnum.NE:
                    branch.Ne = SetOnNode(x, y, point, branch.Ne, tree);
                    break;
                case QuadrantEnum.SW:
                    branch.Sw = SetOnNode(x, y, point, branch.Sw, tree);
                    break;
                default:
                    branch.Se = SetOnNode(x, y, point, branch.Se, tree);
                    break;
            }
        }

        private static int SetOnLeaf(
            PointRegionQuadTreeNodeLeaf<object> leaf,
            double x, double y,
            XYPointWValue<TL> pointXY)
        {
            if (pointXY.X != x && pointXY.Y != y) {
                throw new IllegalStateException();
            }

            return SetOnLeaf(leaf, x, y, pointXY.Value);
        }

        private static int SetOnLeaf(
            PointRegionQuadTreeNodeLeaf<object> leaf,
            double x, double y,
            TL value)
        {
            var currentValue = leaf.Points;
            if (currentValue == null) {
                leaf.Points = new XYPointWValue<TL>(x, y, value);
                return 1;
            }

            if (currentValue is XYPointWValue<TL> otherXY) {
                if (otherXY.X == x && otherXY.Y == y) {
                    otherXY.Value = value;
                    return 0;
                }

                var collectionX = new List<XYPointWValue<TL>>();
                collectionX.Add(otherXY);
                collectionX.Add(new XYPointWValue<TL>(x, y, value));
                leaf.Points = collectionX;
                return 1;
            }

            var collection = currentValue.Unwrap<XYPointWValue<TL>>();
            foreach (var other in collection) {
                if (other.X == x && other.Y == y) {
                    other.Value = value;
                    return 0;
                }
            }

            collection.Add(new XYPointWValue<TL>(x, y, value));
            return 1;
        }
    }
} // end of namespace
