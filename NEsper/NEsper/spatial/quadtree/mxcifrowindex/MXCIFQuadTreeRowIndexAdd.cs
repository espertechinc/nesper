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
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.spatial.quadtree.mxcifrowindex
{
    public class MXCIFQuadTreeRowIndexAdd
    {
        /// <summary>
        /// Add value.
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="value">value to add</param>
        /// <param name="tree">quadtree</param>
        /// <param name="unique">true for unique</param>
        /// <param name="indexName">index name</param>
        /// <returns>true for added, false for not-responsible for this point</returns>
        public static bool Add(double x, double y, double width, double height, object value,
            MXCIFQuadTree<object> tree, bool unique, string indexName)
        {
            var root = tree.Root;
            if (!root.Bb.IntersectsBoxIncludingEnd(x, y, width, height))
            {
                return false;
            }

            tree.Root = AddToNode(x, y, width, height, value, root, tree, unique, indexName);
            return true;
        }

        private static MXCIFQuadTreeNode<object> AddToNode(
            double x, double y,
            double width, double height,
            object value,
            MXCIFQuadTreeNode<object> node,
            MXCIFQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object>)
            {
                var leaf = (MXCIFQuadTreeNodeLeaf<object>) node;

                if (leaf.Count < tree.LeafCapacity || node.Level >= tree.MaxTreeHeight)
                {
                    // can be multiple as value can be a collection
                    var numAdded = AddToData(leaf, x, y, width, height, value, unique, indexName);
                    leaf.IncCount(numAdded);

                    if (leaf.Count <= tree.LeafCapacity || node.Level >= tree.MaxTreeHeight)
                    {
                        return leaf;
                    }
                }

                node = Subdivide(leaf, tree, unique, indexName);
            }

            var branch = (MXCIFQuadTreeNodeBranch<object>) node;
            AddToBranch(branch, x, y, width, height, value, tree, unique, indexName);
            return node;
        }

        private static void AddToBranch(
            MXCIFQuadTreeNodeBranch<object> branch,
            double x, double y,
            double width, double height,
            object value,
            MXCIFQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            var quadrant = branch.Bb.GetQuadrantApplies(x, y, width, height);
            if (quadrant == QuadrantAppliesEnum.NW)
            {
                branch.Nw = AddToNode(x, y, width, height, value, branch.Nw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.NE)
            {
                branch.Ne = AddToNode(x, y, width, height, value, branch.Ne, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SW)
            {
                branch.Sw = AddToNode(x, y, width, height, value, branch.Sw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SE)
            {
                branch.Se = AddToNode(x, y, width, height, value, branch.Se, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SOME)
            {
                var numAdded = AddToData(branch, x, y, width, height, value, unique, indexName);
                branch.IncCount(numAdded);
            }
            else
            {
                throw new IllegalStateException("Applies to none");
            }
        }

        private static MXCIFQuadTreeNode<object> Subdivide(
            MXCIFQuadTreeNodeLeaf<object> leaf,
            MXCIFQuadTree<object> tree,
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
            var nw = new MXCIFQuadTreeNodeLeaf<object>(bbNW, leaf.Level + 1, null, 0);
            var ne = new MXCIFQuadTreeNodeLeaf<object>(bbNE, leaf.Level + 1, null, 0);
            var sw = new MXCIFQuadTreeNodeLeaf<object>(bbSW, leaf.Level + 1, null, 0);
            var se = new MXCIFQuadTreeNodeLeaf<object>(bbSE, leaf.Level + 1, null, 0);
            var branch = new MXCIFQuadTreeNodeBranch<object>(leaf.Bb, leaf.Level, null, 0, nw, ne, sw, se);

            var data = leaf.Data;
            if (data is XYWHRectangleMultiType)
            {
                var rectangle = (XYWHRectangleMultiType) data;
                Subdivide(rectangle, branch, tree, unique, indexName);
            }
            else
            {
                var collection = (ICollection<XYWHRectangleMultiType>) data;
                foreach (var rectangle in collection)
                {
                    Subdivide(rectangle, branch, tree, unique, indexName);
                }
            }

            return branch;
        }

        private static void Subdivide(
            XYWHRectangleMultiType rectangle,
            MXCIFQuadTreeNodeBranch<object> branch,
            MXCIFQuadTree<object> tree,
            bool unique,
            string indexName)
        {
            var x = rectangle.X;
            var y = rectangle.Y;
            var w = rectangle.W;
            var h = rectangle.H;
            var quadrant = branch.Bb.GetQuadrantApplies(x, y, w, h);
            switch (quadrant)
            {
                case QuadrantAppliesEnum.NW:
                    branch.Nw = AddToNode(x, y, w, h, rectangle, branch.Nw, tree, unique, indexName);
                    break;
                case QuadrantAppliesEnum.NE:
                    branch.Ne = AddToNode(x, y, w, h, rectangle, branch.Ne, tree, unique, indexName);
                    break;
                case QuadrantAppliesEnum.SW:
                    branch.Sw = AddToNode(x, y, w, h, rectangle, branch.Sw, tree, unique, indexName);
                    break;
                case QuadrantAppliesEnum.SE:
                    branch.Se = AddToNode(x, y, w, h, rectangle, branch.Se, tree, unique, indexName);
                    break;
                case QuadrantAppliesEnum.SOME:
                    var numAdded = AddToData(branch, x, y, w, h, rectangle, unique, indexName);
                    branch.IncCount(numAdded);
                    break;
                default:
                    throw new IllegalStateException("No intersection");
            }
        }

        public static int AddToData(
            MXCIFQuadTreeNode<object> node,
            double x, double y,
            double width, double height,
            object value,
            bool unique,
            string indexName)
        {
            var currentValue = node.Data;

            // value can be multitype itself since we may subdivide-add and don't want to allocate a new object
            if (value is XYWHRectangleMultiType rectangle)
            {
                if (!rectangle.CoordinateEquals(x, y, width, height))
                {
                    throw new IllegalStateException();
                }

                if (currentValue == null)
                {
                    node.Data = rectangle;
                    return rectangle.Count();
                }

                if (currentValue is XYWHRectangleMultiType otherXYWHR)
                {
                    if (otherXYWHR.CoordinateEquals(x, y, width, height))
                    {
                        if (unique)
                        {
                            throw HandleUniqueViolation(indexName, otherXYWHR);
                        }

                        otherXYWHR.AddMultiType(rectangle);
                        return rectangle.Count();
                    }

                    var collectionInner = new LinkedList<XYWHRectangleMultiType>();
                    collectionInner.AddLast(otherXYWHR);
                    collectionInner.AddLast(rectangle);
                    node.Data = collectionInner;
                    return rectangle.Count();
                }

                var collectionXYWH = (ICollection<XYWHRectangleMultiType>) currentValue;
                foreach (var other in collectionXYWH)
                {
                    if (other.CoordinateEquals(x, y, width, height))
                    {
                        if (unique)
                        {
                            throw HandleUniqueViolation(indexName, other);
                        }

                        other.AddMultiType(rectangle);
                        return rectangle.Count();
                    }
                }

                collectionXYWH.Add(rectangle);
                return rectangle.Count();
            }

            if (currentValue == null)
            {
                var point = new XYWHRectangleMultiType(x, y, width, height, value);
                node.Data = point;
                return 1;
            }

            if (currentValue is XYWHRectangleMultiType otherInnerX)
            {
                if (otherInnerX.CoordinateEquals(x, y, width, height))
                {
                    if (unique)
                    {
                        throw HandleUniqueViolation(indexName, otherInnerX);
                    }

                    otherInnerX.AddSingleValue(value);
                    return 1;
                }

                var collectionXY = new LinkedList<XYWHRectangleMultiType>();
                collectionXY.AddLast(otherInnerX);
                collectionXY.AddLast(new XYWHRectangleMultiType(x, y, width, height, value));
                node.Data = collectionXY;
                return 1;
            }

            var collection = (ICollection<XYWHRectangleMultiType>) currentValue;
            foreach (XYWHRectangleMultiType other in collection)
            {
                if (other.CoordinateEquals(x, y, width, height))
                {
                    if (unique)
                    {
                        throw HandleUniqueViolation(indexName, other);
                    }

                    other.AddSingleValue(value);
                    return 1;
                }
            }

            collection.Add(new XYWHRectangleMultiType(x, y, width, height, value));
            return 1;
        }

        private static EPException HandleUniqueViolation(string indexName, XYWHRectangleMultiType other)
        {
            return PropertyIndexedEventTableUnique.HandleUniqueIndexViolation(indexName,
                "(" + other.X + "," + other.Y + "," + other.W + "," + other.H + ")");
        }
    }
} // end of namespace
