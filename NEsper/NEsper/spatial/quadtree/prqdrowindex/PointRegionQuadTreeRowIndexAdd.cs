///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.spatial.quadtree.prqdrowindex
{
    public class PointRegionQuadTreeRowIndexAdd
    {
        /// <summary>
        /// Add value.
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="value">value to add</param>
        /// <param name="tree">quadtree</param>
        /// <param name="unique">true for unique</param>
        /// <param name="indexName">index name</param>
        /// <returns>true for added, false for not-responsible for this point</returns>
        public static bool Add(
            double x, double y,
            object value, 
            PointRegionQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            var root = tree.Root;
            if (!root.Bb.ContainsPoint(x, y))
            {
                return false;
            }

            var replacement = AddToNode(x, y, value, root, tree, unique, indexName);
            tree.Root = replacement;
            return true;
        }

        private static PointRegionQuadTreeNode AddToNode(
            double x, double y, object value,
            PointRegionQuadTreeNode node,
            PointRegionQuadTree<object> tree, 
            bool unique, string indexName)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object>)
            {
                var leaf = (PointRegionQuadTreeNodeLeaf<object>) node;
                if (leaf.Count < tree.LeafCapacity || node.Level >= tree.MaxTreeHeight)
                {
                    // can be multiple as value can be a collection
                    var numAdded = AddToLeaf(leaf, x, y, value, unique, indexName);
                    leaf.IncCount(numAdded);

                    if (leaf.Count <= tree.LeafCapacity || node.Level >= tree.MaxTreeHeight)
                    {
                        return leaf;
                    }
                }

                node = Subdivide(leaf, tree, unique, indexName);
            }

            var branch = (PointRegionQuadTreeNodeBranch) node;
            AddToBranch(branch, x, y, value, tree, unique, indexName);
            return node;
        }

        private static void AddToBranch(
            PointRegionQuadTreeNodeBranch branch,
            double x, double y, object value,
            PointRegionQuadTree<object> tree,
            bool unique, string indexName)
        {
            var quadrant = branch.Bb.GetQuadrant(x, y);
            if (quadrant == QuadrantEnum.NW)
            {
                branch.Nw = AddToNode(x, y, value, branch.Nw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantEnum.NE)
            {
                branch.Ne = AddToNode(x, y, value, branch.Ne, tree, unique, indexName);
            }
            else if (quadrant == QuadrantEnum.SW)
            {
                branch.Sw = AddToNode(x, y, value, branch.Sw, tree, unique, indexName);
            }
            else
            {
                branch.Se = AddToNode(x, y, value, branch.Se, tree, unique, indexName);
            }
        }

        private static PointRegionQuadTreeNode Subdivide(
            PointRegionQuadTreeNodeLeaf<object> leaf,
            PointRegionQuadTree<object> tree, 
            bool unique, string indexName)
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
            if (points is XYPointMultiType)
            {
                var point = (XYPointMultiType) points;
                SubdividePoint(point, branch, tree, unique, indexName);
            }
            else
            {
                var collection = (ICollection<XYPointMultiType>) points;
                foreach (var point in collection)
                {
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
            switch (quadrant)
            {
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
            double x, double y, object value,
            bool unique, string indexName)
        {
            var currentValue = leaf.Points;

            // value can be multitype itself since we may subdivide-add and don't want to allocate a new object
            if (value is XYPointMultiType)
            {
                var point = (XYPointMultiType) value;
                if (point.X != x && point.Y != y)
                {
                    throw new IllegalStateException();
                }

                if (currentValue == null)
                {
                    leaf.Points = point;
                    return point.Count();
                }

                if (currentValue is XYPointMultiType)
                {
                    var other = (XYPointMultiType) currentValue;
                    if (other.X == x && other.Y == y)
                    {
                        if (unique)
                        {
                            throw HandleUniqueViolation(indexName, other.X, other.Y);
                        }

                        other.AddMultiType(point);
                        return point.Count();
                    }

                    var collectionX = new LinkedList<XYPointMultiType>();
                    collectionX.AddLast(other);
                    collectionX.AddLast(point);
                    leaf.Points = collectionX;
                    return point.Count();
                }

                var xyPointMultiTypes = (ICollection<XYPointMultiType>) currentValue;
                foreach (var other in xyPointMultiTypes)
                {
                    if (other.X == x && other.Y == y)
                    {
                        if (unique)
                        {
                            throw HandleUniqueViolation(indexName, other.X, other.Y);
                        }

                        other.AddMultiType(point);
                        return point.Count();
                    }
                }

                xyPointMultiTypes.Add(point);
                return point.Count();
            }

            if (currentValue == null)
            {
                var point = new XYPointMultiType(x, y, value);
                leaf.Points = point;
                return 1;
            }

            if (currentValue is XYPointMultiType)
            {
                var other = (XYPointMultiType) currentValue;
                if (other.X == x && other.Y == y)
                {
                    if (unique)
                    {
                        throw HandleUniqueViolation(indexName, other.X, other.Y);
                    }

                    other.AddSingleValue(value);
                    return 1;
                }

                var xyPointMultiTypes = new LinkedList<XYPointMultiType>();
                xyPointMultiTypes.AddLast(other);
                xyPointMultiTypes.AddLast(new XYPointMultiType(x, y, value));
                leaf.Points = xyPointMultiTypes;
                return 1;
            }

            var collection = (ICollection<XYPointMultiType>) currentValue;
            foreach (var other in collection)
            {
                if (other.X == x && other.Y == y)
                {
                    if (unique)
                    {
                        throw HandleUniqueViolation(indexName, other.X, other.Y);
                    }

                    other.AddSingleValue(value);
                    return 1;
                }
            }

            collection.Add(new XYPointMultiType(x, y, value));
            return 1;
        }

        private static EPException HandleUniqueViolation(string indexName, double x, double y)
        {
            return PropertyIndexedEventTableUnique.HandleUniqueIndexViolation(indexName, "(" + x + "," + y + ")");
        }
    }
} // end of namespace
