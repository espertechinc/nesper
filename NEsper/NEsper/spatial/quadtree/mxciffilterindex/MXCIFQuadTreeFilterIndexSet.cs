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
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.mxcif;
using com.espertech.esper.spatial.quadtree.mxcifrowindex;

namespace com.espertech.esper.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexSet
    {
        public static void Set<TL>(
            double x, 
            double y, 
            double width, 
            double height, 
            TL value, 
            MXCIFQuadTree<object> tree)
        {
            MXCIFQuadTreeNode<object> root = tree.Root;
            MXCIFQuadTreeFilterIndexCheckBB.CheckBB(root.Bb, x, y, width, height);
            var replacement = SetOnNode(x, y, width, height, value, root, tree);
            tree.Root = replacement;
        }
    
        private static MXCIFQuadTreeNode<object> SetOnNode<TL>(
            double x, 
            double y, 
            double width, 
            double height, 
            TL value, 
            MXCIFQuadTreeNode<object> node, 
            MXCIFQuadTree<object> tree)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object> leaf) {
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
    
        private static void AddToBranch<TL>(
            MXCIFQuadTreeNodeBranch<object> branch, 
            double x, 
            double y, 
            double width, 
            double height, 
            TL value, 
            MXCIFQuadTree<object> tree)
        {
            var quadrant = branch.Bb.GetQuadrantApplies(x, y, width, height);
            if (quadrant == QuadrantAppliesEnum.NW) {
                branch.Nw = SetOnNode(x, y, width, height, value, branch.Nw, tree);
            } else if (quadrant == QuadrantAppliesEnum.NE) {
                branch.Ne = SetOnNode(x, y, width, height, value, branch.Ne, tree);
            } else if (quadrant == QuadrantAppliesEnum.SW) {
                branch.Sw = SetOnNode(x, y, width, height, value, branch.Sw, tree);
            } else if (quadrant == QuadrantAppliesEnum.SE) {
                branch.Se = SetOnNode(x, y, width, height, value, branch.Se, tree);
            } else if (quadrant == QuadrantAppliesEnum.SOME) {
                var count = SetOnNode(branch, x, y, width, height, value);
                branch.IncCount(count);
            } else {
                throw new IllegalStateException("Quandrant not applies to any");
            }
        }
    
        private static MXCIFQuadTreeNode<object> Subdivide(
            MXCIFQuadTreeNodeLeaf<object> leaf, 
            MXCIFQuadTree<object> tree)
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
    
            var rectangles = leaf.Data;
            if (rectangles is XYWHRectangleWValue<object>) {
                var rectangle = (XYWHRectangleWValue<object>) rectangles;
                Subdivide(rectangle, branch, tree);
            } else {
                var collection = (ICollection<XYWHRectangleWValue<object>>) rectangles;
                foreach (var rectangle in collection) {
                    Subdivide(rectangle, branch, tree);
                }
            }
            return branch;
        }
    
        private static void Subdivide(
            XYWHRectangleWValue<object> rectangle, 
            MXCIFQuadTreeNodeBranch<object> branch, 
            MXCIFQuadTree<object> tree)
        {
            var x = rectangle.X;
            var y = rectangle.Y;
            var w = rectangle.W;
            var h = rectangle.H;
            var quadrant = branch.Bb.GetQuadrantApplies(x, y, w, h);
            if (quadrant == QuadrantAppliesEnum.NW) {
                branch.Nw = SetOnNode(x, y, w, h, rectangle, branch.Nw, tree);
            } else if (quadrant == QuadrantAppliesEnum.NE) {
                branch.Ne = SetOnNode(x, y, w, h, rectangle, branch.Ne, tree);
            } else if (quadrant == QuadrantAppliesEnum.SW) {
                branch.Sw = SetOnNode(x, y, w, h, rectangle, branch.Sw, tree);
            } else if (quadrant == QuadrantAppliesEnum.SE) {
                branch.Se = SetOnNode(x, y, w, h, rectangle, branch.Se, tree);
            } else if (quadrant == QuadrantAppliesEnum.SOME) {
                var numAdded = SetOnNode(branch, x, y, w, h, rectangle);
                branch.IncCount(numAdded);
            } else {
                throw new IllegalStateException("No intersection");
            }
        }

        private static int SetOnNode<TL>(
            MXCIFQuadTreeNode<object> node,
            double x,
            double y,
            double width,
            double height,
            TL value)
        {
            var currentValue = node.Data;

#if false
            if (value is XYWHRectangleWValue<object> rectangleXY) {
                if (!rectangleXY.CoordinateEquals(x, y, width, height)) {
                    throw new IllegalStateException();
                }
                if (currentValue == null) {
                    node.Data = rectangleXY;
                    return 1;
                }
                if (currentValue is XYWHRectangleWValue<object>) {
                    var other = (XYWHRectangleWValue<object>) currentValue;
                    if (other.CoordinateEquals(x, y, width, height)) {
                        other.Value = value;
                        return 0; // replaced
                    }
                    var collectionZ = new List<object>();
                    collectionZ.Add(other);
                    collectionZ.Add(rectangleXY);
                    node.Data = collectionZ;
                    return 1;
                }
                var collectionY = (ICollection<XYWHRectangleWValue<object>>) currentValue;
                foreach (var other in collectionY) {
                    if (other.CoordinateEquals(x, y, width, height)) {
                        other.Value = value;
                        return 0;
                    }
                }
                collectionY.Add(rectangleXY);
                return 1;
            }
#endif

            if (currentValue == null)
            {
                var point = new XYWHRectangleWValue<object>(x, y, width, height, value);
                node.Data = point;
                return 1;
            }

            if (currentValue is XYWHRectangleWValue<object> otherXY)
            {
                if (otherXY.CoordinateEquals(x, y, width, height))
                {
                    otherXY.Value = value;
                    return 0;
                }

                var collectionX = new List<object>();
                collectionX.Add(otherXY);
                collectionX.Add(new XYWHRectangleWValue<object>(x, y, width, height, value));
                node.Data = collectionX;
                return 1;
            }

            var collection = (ICollection<XYWHRectangleWValue<object>>) currentValue;
            foreach (var other in collection)
            {
                if (other.CoordinateEquals(x, y, width, height))
                {
                    other.Value = value;
                    return 0;
                }
            }

            collection.Add(new XYWHRectangleWValue<object>(x, y, width, height, value));
            return 1;
        }
    }
} // end of namespace
