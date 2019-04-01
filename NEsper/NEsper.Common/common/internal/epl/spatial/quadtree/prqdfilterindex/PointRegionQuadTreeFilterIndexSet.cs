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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexSet<TL>
    {
        public static void Set(double x, double y, TL value, PointRegionQuadTree<object> tree)
        {
            var root = tree.Root;
            CheckBB(root.Bb, x, y);
            var replacement = SetOnNode(x, y, value, root, tree);
            tree.Root = replacement;
        }

        private static PointRegionQuadTreeNode SetOnNode(
            double x, double y,
            TL value, 
            PointRegionQuadTreeNode node, 
            PointRegionQuadTree<object> tree)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object>) {
                var leaf = (PointRegionQuadTreeNodeLeaf<object>) node;
                var count = SetOnLeaf(leaf, x, y, value);
                leaf.IncCount(count);

                if (leaf.Count <= tree.LeafCapacity || node.Level >= tree.MaxTreeHeight) {
                    return leaf;
                }

                node = Subdivide(leaf, tree);
            }

            PointRegionQuadTreeNodeBranch branch = (PointRegionQuadTreeNodeBranch) node;
            AddToBranch(branch, x, y, value, tree);
            return node;
        }

        private static void AddToBranch(
            PointRegionQuadTreeNodeBranch branch, 
            double x, 
            double y, 
            object value,
            PointRegionQuadTree<object> tree)
        {
            QuadrantEnum quadrant = branch.Bb.GetQuadrant(x, y);
            if (quadrant == QuadrantEnum.NW) {
                branch.Nw = SetOnNode(x, y, value, branch.Nw, tree);
            }
            else if (quadrant == QuadrantEnum.NE) {
                branch.Ne = SetOnNode(x, y, value, branch.Ne, tree);
            }
            else if (quadrant == QuadrantEnum.SW) {
                branch.Sw = SetOnNode(x, y, value, branch.Sw, tree);
            }
            else {
                branch.Se = SetOnNode(x, y, value, branch.Se, tree);
            }
        }

        private static PointRegionQuadTreeNode Subdivide(
            PointRegionQuadTreeNodeLeaf<object> leaf, PointRegionQuadTree<object> tree)
        {
            var w = (leaf.Bb.MaxX - leaf.Bb.MinX) / 2d;
            var h = (leaf.Bb.MaxY - leaf.Bb.MinY) / 2d;
            double minx = leaf.Bb.MinX;
            double miny = leaf.Bb.MinY;

            var bbNW = new BoundingBox(minx, miny, minx + w, miny + h);
            var bbNE = new BoundingBox(minx + w, miny, leaf.Bb.MaxX, miny + h);
            var bbSW = new BoundingBox(minx, miny + h, minx + w, leaf.Bb.MaxY);
            var bbSE = new BoundingBox(minx + w, miny + h, leaf.Bb.MaxX, leaf.Bb.MaxY);
            PointRegionQuadTreeNode nw = new PointRegionQuadTreeNodeLeaf<object>(bbNW, leaf.Level + 1, null, 0);
            PointRegionQuadTreeNode ne = new PointRegionQuadTreeNodeLeaf<object>(bbNE, leaf.Level + 1, null, 0);
            PointRegionQuadTreeNode sw = new PointRegionQuadTreeNodeLeaf<object>(bbSW, leaf.Level + 1, null, 0);
            PointRegionQuadTreeNode se = new PointRegionQuadTreeNodeLeaf<object>(bbSE, leaf.Level + 1, null, 0);
            PointRegionQuadTreeNodeBranch branch = new PointRegionQuadTreeNodeBranch(
                leaf.Bb, leaf.Level, nw, ne, sw, se);

            var points = leaf.Points;
            if (points is XYPointWValue<TL>) {
                XYPointWValue<TL> point = (XYPointWValue<TL>) points;
                SubdividePoint(point, branch, tree);
            }
            else {
                var collection = (ICollection<XYPointWValue<TL>>) points;
                foreach (var point in collection) {
                    SubdividePoint(point, branch, tree);
                }
            }

            return branch;
        }

        private static void SubdividePoint(
            XYPointWValue<TL> point, PointRegionQuadTreeNodeBranch branch, PointRegionQuadTree<object> tree)
        {
            var x = point.X;
            var y = point.Y;
            QuadrantEnum quadrant = branch.Bb.GetQuadrant(x, y);
            if (quadrant == QuadrantEnum.NW) {
                branch.Nw = SetOnNode(x, y, point, branch.Nw, tree);
            }
            else if (quadrant == QuadrantEnum.NE) {
                branch.Ne = SetOnNode(x, y, point, branch.Ne, tree);
            }
            else if (quadrant == QuadrantEnum.SW) {
                branch.Sw = SetOnNode(x, y, point, branch.Sw, tree);
            }
            else {
                branch.Se = SetOnNode(x, y, point, branch.Se, tree);
            }
        }

        private static int SetOnLeaf(PointRegionQuadTreeNodeLeaf<object> leaf, double x, double y, TL value)
        {
            var currentValue = leaf.Points;

            if (value is XYPointWValue<TL>) {
                var point = (XYPointWValue<TL>) value;
                if (point.X != x && point.Y != y) {
                    throw new IllegalStateException();
                }

                if (currentValue == null) {
                    leaf.Points = point;
                    return 1;
                }

                if (currentValue is XYPointWValue<TL>) {
                    var other = (XYPointWValue<TL>) currentValue;
                    if (other.X == x && other.Y == y) {
                        other.Value = value;
                        return 0; // replaced
                    }

                    ICollection<XYPointWValue<TL>> collection = new LinkedList<XYPointWValue<TL>>();
                    collection.Add(other);
                    collection.Add(point);
                    leaf.Points = collection;
                    return 1;
                }

                var collection = (ICollection<XYPointWValue<TL>>) currentValue;
                foreach (var other in collection) {
                    if (other.X == x && other.Y == y) {
                        other.Value = value;
                        return 0;
                    }
                }

                collection.Add(point);
                return 1;
            }

            if (currentValue == null) {
                XYPointWValue<TL> point = new XYPointWValue<TL>(x, y, value);
                leaf.Points = point;
                return 1;
            }

            if (currentValue is XYPointWValue<TL>) {
                var other = (XYPointWValue<TL>) currentValue;
                if (other.X == x && other.Y == y) {
                    other.Value = value;
                    return 0;
                }

                ICollection<XYPointWValue<TL>> collection = new LinkedList<XYPointWValue<TL>>();
                collection.Add(other);
                collection.Add(new XYPointWValue<TL>(x, y, value));
                leaf.Points = collection;
                return 1;
            }

            var collection = (ICollection<XYPointWValue<TL>>) currentValue;
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