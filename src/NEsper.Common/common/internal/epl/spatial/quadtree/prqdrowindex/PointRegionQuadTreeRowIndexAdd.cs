///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    public class PointRegionQuadTreeRowIndexAdd
    {
        /// <summary>
        ///     Add value.
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="value">value to add</param>
        /// <param name="tree">quadtree</param>
        /// <param name="unique">true for unique</param>
        /// <param name="indexName">index name</param>
        /// <returns>true for added, false for not-responsible for this point</returns>
        public static bool Add(
            double x,
            double y,
            object value,
            PointRegionQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            var root = tree.Root;
            if (!root.Bb.ContainsPoint(x, y)) {
                return false;
            }

            tree.Root = AddToNode(x, y, value, root, tree, unique, indexName);
            return true;
        }

        private static PointRegionQuadTreeNode AddToNode(
            double x,
            double y,
            object value,
            PointRegionQuadTreeNode node,
            PointRegionQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object> leaf) {
                if (leaf.Count < tree.LeafCapacity || leaf.Level >= tree.MaxTreeHeight) {
                    // can be multiple as value can be a collection
                    var numAdded = AddToLeaf(leaf, x, y, value, unique, indexName);
                    leaf.IncCount(numAdded);

                    if (leaf.Count <= tree.LeafCapacity || leaf.Level >= tree.MaxTreeHeight) {
                        return leaf;
                    }
                }

                node = Subdivide(leaf, tree, unique, indexName);
            }

            var branch = (PointRegionQuadTreeNodeBranch)node;
            AddToBranch(branch, x, y, value, tree, unique, indexName);
            return node;
        }

        private static void AddToBranch(
            PointRegionQuadTreeNodeBranch branch,
            double x,
            double y,
            object value,
            PointRegionQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            var quadrant = branch.Bb.GetQuadrant(x, y);
            if (quadrant == QuadrantEnum.NW) {
                branch.Nw = AddToNode(x, y, value, branch.Nw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantEnum.NE) {
                branch.Ne = AddToNode(x, y, value, branch.Ne, tree, unique, indexName);
            }
            else if (quadrant == QuadrantEnum.SW) {
                branch.Sw = AddToNode(x, y, value, branch.Sw, tree, unique, indexName);
            }
            else {
                branch.Se = AddToNode(x, y, value, branch.Se, tree, unique, indexName);
            }
        }

        private static PointRegionQuadTreeNode Subdivide(
            PointRegionQuadTreeNodeLeaf<object> leaf,
            PointRegionQuadTree<object> tree,
            bool unique,
            string indexName)
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
            if (points is XYPointMultiType type) {
                SubdividePoint(type, branch, tree, unique, indexName);
            }
            else {
                foreach (var point in (ICollection<XYPointMultiType>)points) {
                    SubdividePoint(point, branch, tree, unique, indexName);
                }
            }

            return branch;
        }

        private static void SubdividePoint(
            XYPointMultiType point,
            PointRegionQuadTreeNodeBranch branch,
            PointRegionQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            var x = point.X;
            var y = point.Y;
            var quadrant = branch.Bb.GetQuadrant(x, y);
            switch (quadrant) {
                case QuadrantEnum.NW:
                    branch.Nw = AddToNode(x, y, point, branch.Nw, tree, unique, indexName);
                    break;

                case QuadrantEnum.NE:
                    branch.Ne = AddToNode(x, y, point, branch.Ne, tree, unique, indexName);
                    break;

                case QuadrantEnum.SW:
                    branch.Sw = AddToNode(x, y, point, branch.Sw, tree, unique, indexName);
                    break;

                default:
                    branch.Se = AddToNode(x, y, point, branch.Se, tree, unique, indexName);
                    break;
            }
        }

        public static int AddToLeaf(
            PointRegionQuadTreeNodeLeaf<object> leaf,
            double x,
            double y,
            object value,
            bool unique,
            string indexName)
        {
            var currentValue = leaf.Points;

            // value can be multitype itself since we may subdivide-add and don't want to allocate a new object
            if (value is XYPointMultiType type) {
                if (type.X != x && type.Y != y) {
                    throw new IllegalStateException();
                }

                if (currentValue == null) {
                    leaf.Points = type;
                    return type.Count();
                }

                if (currentValue is XYPointMultiType multiType) {
                    if (multiType.X == x && multiType.Y == y) {
                        if (unique) {
                            throw HandleUniqueViolation(indexName, multiType.X, multiType.Y);
                        }

                        multiType.AddMultiType(type);
                        return type.Count();
                    }

                    var collectionX = new LinkedList<XYPointMultiType>();
                    collectionX.AddLast(multiType);
                    collectionX.AddLast(type);
                    leaf.Points = collectionX;
                    return type.Count();
                }

                var xyPointMultiTypes = (ICollection<XYPointMultiType>)currentValue;
                foreach (var other in xyPointMultiTypes) {
                    if (other.X == x && other.Y == y) {
                        if (unique) {
                            throw HandleUniqueViolation(indexName, other.X, other.Y);
                        }

                        other.AddMultiType(type);
                        return type.Count();
                    }
                }

                xyPointMultiTypes.Add(type);
                return type.Count();
            }

            if (currentValue == null) {
                var point = new XYPointMultiType(x, y, value);
                leaf.Points = point;
                return 1;
            }

            if (currentValue is XYPointMultiType pointMultiType) {
                if (pointMultiType.X == x && pointMultiType.Y == y) {
                    if (unique) {
                        throw HandleUniqueViolation(indexName, pointMultiType.X, pointMultiType.Y);
                    }

                    pointMultiType.AddSingleValue(value);
                    return 1;
                }

                var xyPointMultiTypes = new LinkedList<XYPointMultiType>();
                xyPointMultiTypes.AddLast(pointMultiType);
                xyPointMultiTypes.AddLast(new XYPointMultiType(x, y, value));
                leaf.Points = xyPointMultiTypes;
                return 1;
            }

            var collection = (ICollection<XYPointMultiType>)currentValue;
            foreach (var other in collection) {
                if (other.X == x && other.Y == y) {
                    if (unique) {
                        throw HandleUniqueViolation(indexName, other.X, other.Y);
                    }

                    other.AddSingleValue(value);
                    return 1;
                }
            }

            collection.Add(new XYPointMultiType(x, y, value));
            return 1;
        }

        private static EPException HandleUniqueViolation(
            string indexName,
            double x,
            double y)
        {
            return PropertyHashedEventTableUnique.HandleUniqueIndexViolation(
                indexName,
                "(" + x.RenderAny() + "," + y.RenderAny() + ")");
        }
    }
} // end of namespace