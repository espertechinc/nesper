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
using com.espertech.esper.compat.logging;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.spatial.quadtree.mxcifrowindex
{
    public class MXCIFQuadTreeRowIndexQuery
    {
        public static ICollection<object> QueryRange(
            MXCIFQuadTree<object> quadTree, 
            double x, double y, 
            double width, double height)
        {
            return QueryNode(quadTree.Root, x, y, width, height, null);
        }
    
        private static ICollection<object> QueryNode(
            MXCIFQuadTreeNode<object> node,
            double x, double y, 
            double width, double height,
            ICollection<object> result)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object> leaf) {
                return Visit(leaf, x, y, width, height, result);
            }
    
            var branch = (MXCIFQuadTreeNodeBranch<object>) node;
            result = Visit(branch, x, y, width, height, result);
            result = QueryNode(branch.Nw, x, y, width, height, result);
            result = QueryNode(branch.Ne, x, y, width, height, result);
            result = QueryNode(branch.Sw, x, y, width, height, result);
            result = QueryNode(branch.Se, x, y, width, height, result);
            return result;
        }
    
        private static ICollection<object> Visit(
            MXCIFQuadTreeNode<object> node, 
            double x, double y, 
            double width, double height,
            ICollection<object> result)
        {
            var data = node.Data;
            if (data == null) {
                return result;
            }
            if (data is XYWHRectangleMultiType point) {
                return Visit(point, x, y, width, height, result);
            }

            var collection = (ICollection<XYWHRectangleMultiType>) data;
            foreach (XYWHRectangleMultiType rectangle in collection) {
                result = Visit(rectangle, x, y, width, height, result);
            }
            return result;
        }
    
        private static ICollection<object> Visit(
            XYWHRectangleMultiType rectangle, 
            double x, double y, 
            double width, double height,
            ICollection<object> result)
        {
            if (!BoundingBox.IntersectsBoxIncludingEnd(x, y, x + width, y + height, rectangle.X, rectangle.Y, rectangle.W, rectangle.H)) {
                return result;
            }
            if (result == null) {
                result = new ArrayDeque<object>(4);
            }
            rectangle.CollectInto(result);
            return result;
        }
    }
} // end of namespace
