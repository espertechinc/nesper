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
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    public class MXCIFQuadTreeRowIndexAdd
    {
        /// <summary>
        ///     Add value.
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
        public static bool Add(
            double x,
            double y,
            double width,
            double height,
            object value,
            MXCIFQuadTree tree,
            bool unique,
            string indexName)
        {
            var root = tree.Root;
            if (!root.Bb.IntersectsBoxIncludingEnd(x, y, width, height)) {
                return false;
            }

            var replacement = AddToNode(x, y, width, height, value, root, tree, unique, indexName);
            tree.Root = replacement;
            return true;
        }

        private static MXCIFQuadTreeNode AddToNode(
            double x,
            double y,
            double width,
            double height,
            object value,
            MXCIFQuadTreeNode node,
            MXCIFQuadTree tree,
            bool unique,
            string indexName)
        {
            if (node is MXCIFQuadTreeNodeLeaf leaf) {
                if (leaf.Count < tree.LeafCapacity || leaf.Level >= tree.MaxTreeHeight) {
                    // can be multiple as value can be a collection
                    var numAdded = AddToData(leaf, x, y, width, height, value, unique, indexName);
                    leaf.IncCount(numAdded);

                    if (leaf.Count <= tree.LeafCapacity || leaf.Level >= tree.MaxTreeHeight) {
                        return leaf;
                    }
                }

                node = Subdivide(leaf, tree, unique, indexName);
            }

            var branch = (MXCIFQuadTreeNodeBranch)node;
            AddToBranch(branch, x, y, width, height, value, tree, unique, indexName);
            return node;
        }

        private static void AddToBranch(
            MXCIFQuadTreeNodeBranch branch,
            double x,
            double y,
            double width,
            double height,
            object value,
            MXCIFQuadTree tree,
            bool unique,
            string indexName)
        {
            var quadrant = branch.Bb.GetQuadrantApplies(x, y, width, height);
            if (quadrant == QuadrantAppliesEnum.NW) {
                branch.Nw = AddToNode(x, y, width, height, value, branch.Nw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.NE) {
                branch.Ne = AddToNode(x, y, width, height, value, branch.Ne, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SW) {
                branch.Sw = AddToNode(x, y, width, height, value, branch.Sw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SE) {
                branch.Se = AddToNode(x, y, width, height, value, branch.Se, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SOME) {
                var numAdded = AddToData(branch, x, y, width, height, value, unique, indexName);
                branch.IncCount(numAdded);
            }
            else {
                throw new IllegalStateException("Applies to none");
            }
        }

        private static MXCIFQuadTreeNode Subdivide(
            MXCIFQuadTreeNodeLeaf leaf,
            MXCIFQuadTree tree,
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
            MXCIFQuadTreeNode nw = new MXCIFQuadTreeNodeLeaf(bbNW, leaf.Level + 1, null, 0);
            MXCIFQuadTreeNode ne = new MXCIFQuadTreeNodeLeaf(bbNE, leaf.Level + 1, null, 0);
            MXCIFQuadTreeNode sw = new MXCIFQuadTreeNodeLeaf(bbSW, leaf.Level + 1, null, 0);
            MXCIFQuadTreeNode se = new MXCIFQuadTreeNodeLeaf(bbSE, leaf.Level + 1, null, 0);
            var branch = new MXCIFQuadTreeNodeBranch(leaf.Bb, leaf.Level, null, 0, nw, ne, sw, se);

            var data = leaf.Data;
            if (data is XYWHRectangleMultiType type) {
                Subdivide(type, branch, tree, unique, indexName);
            }
            else {
                var collection = (ICollection<XYWHRectangleMultiType>)data;
                foreach (var rectangle in collection) {
                    Subdivide(rectangle, branch, tree, unique, indexName);
                }
            }

            return branch;
        }

        private static void Subdivide(
            XYWHRectangleMultiType rectangle,
            MXCIFQuadTreeNodeBranch branch,
            MXCIFQuadTree tree,
            bool unique,
            string indexName)
        {
            var x = rectangle.X;
            var y = rectangle.Y;
            var w = rectangle.W;
            var h = rectangle.H;
            var quadrant = branch.Bb.GetQuadrantApplies(x, y, w, h);
            if (quadrant == QuadrantAppliesEnum.NW) {
                branch.Nw = AddToNode(x, y, w, h, rectangle, branch.Nw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.NE) {
                branch.Ne = AddToNode(x, y, w, h, rectangle, branch.Ne, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SW) {
                branch.Sw = AddToNode(x, y, w, h, rectangle, branch.Sw, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SE) {
                branch.Se = AddToNode(x, y, w, h, rectangle, branch.Se, tree, unique, indexName);
            }
            else if (quadrant == QuadrantAppliesEnum.SOME) {
                var numAdded = AddToData(branch, x, y, w, h, rectangle, unique, indexName);
                branch.IncCount(numAdded);
            }
            else {
                throw new IllegalStateException("No intersection");
            }
        }

        public static int AddToData(
            MXCIFQuadTreeNode node,
            double x,
            double y,
            double width,
            double height,
            object value,
            bool unique,
            string indexName)
        {
            var currentValue = node.Data;

            // value can be multitype itself since we may subdivide-add and don't want to allocate a new object
            if (value is XYWHRectangleMultiType rectangle) {
                if (!rectangle.CoordinateEquals(x, y, width, height)) {
                    throw new IllegalStateException();
                }

                if (currentValue == null) {
                    node.Data = rectangle;
                    return rectangle.Count();
                }

                if (currentValue is XYWHRectangleMultiType type) {
                    if (type.CoordinateEquals(x, y, width, height)) {
                        if (unique) {
                            throw HandleUniqueViolation(indexName, type);
                        }

                        type.AddMultiType(rectangle);
                        return rectangle.Count();
                    }

                    ICollection<XYWHRectangleMultiType> collectionX = new List<XYWHRectangleMultiType>();
                    collectionX.Add(type);
                    collectionX.Add(rectangle);
                    node.Data = collectionX;
                    return rectangle.Count();
                }

                var collectionY = (ICollection<XYWHRectangleMultiType>)currentValue;
                foreach (var other in collectionY) {
                    if (other.CoordinateEquals(x, y, width, height)) {
                        if (unique) {
                            throw HandleUniqueViolation(indexName, other);
                        }

                        other.AddMultiType(rectangle);
                        return rectangle.Count();
                    }
                }

                collectionY.Add(rectangle);
                return rectangle.Count();
            }

            if (currentValue == null) {
                node.Data = new XYWHRectangleMultiType(x, y, width, height, value);
                return 1;
            }

            if (currentValue is XYWHRectangleMultiType item) {
                if (item.CoordinateEquals(x, y, width, height)) {
                    if (unique) {
                        throw HandleUniqueViolation(indexName, item);
                    }

                    item.AddSingleValue(value);
                    return 1;
                }

                ICollection<XYWHRectangleMultiType> collectionZ = new List<XYWHRectangleMultiType>();
                collectionZ.Add(item);
                collectionZ.Add(new XYWHRectangleMultiType(x, y, width, height, value));
                node.Data = collectionZ;
                return 1;
            }

            var collection = (ICollection<XYWHRectangleMultiType>)currentValue;
            foreach (var other in collection) {
                if (other.CoordinateEquals(x, y, width, height)) {
                    if (unique) {
                        throw HandleUniqueViolation(indexName, other);
                    }

                    other.AddSingleValue(value);
                    return 1;
                }
            }

            collection.Add(new XYWHRectangleMultiType(x, y, width, height, value));
            return 1;
        }

        private static EPException HandleUniqueViolation(
            string indexName,
            XYWHRectangleMultiType other)
        {
            return PropertyHashedEventTableUnique.HandleUniqueIndexViolation(
                indexName,
                $"({other.X.RenderAny()},{other.Y.RenderAny()},{other.W.RenderAny()},{other.H.RenderAny()})");
        }
    }
} // end of namespace