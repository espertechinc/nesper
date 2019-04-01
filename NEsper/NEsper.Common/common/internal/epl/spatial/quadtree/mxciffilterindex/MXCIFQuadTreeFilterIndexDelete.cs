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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex.MXCIFQuadTreeFilterIndexCheckBB;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexDelete
    {
        public static void Delete(double x, double y, double width, double height, MXCIFQuadTree<object> tree)
        {
            MXCIFQuadTreeNode<object> root = tree.Root;
            CheckBB(root.Bb, x, y, width, height);
            MXCIFQuadTreeNode<object> replacement = DeleteFromNode(x, y, width, height, root, tree);
            tree.Root = replacement;
        }

        private static MXCIFQuadTreeNode<object> DeleteFromNode(
            double x, 
            double y, 
            double width,
            double height,
            MXCIFQuadTreeNode<object> node,
            MXCIFQuadTree<object> tree)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object>)
            {
                var leaf = (MXCIFQuadTreeNodeLeaf<object>)node;
                var removed = DeleteFromData(x, y, width, height, leaf.Data);
                if (removed)
                {
                    leaf.DecCount();
                    if (leaf.Count == 0)
                    {
                        leaf.Data = null;
                    }
                }
                return leaf;
            }

            var branch = (MXCIFQuadTreeNodeBranch<object>)node;
            var quadrant = node.Bb.GetQuadrantApplies(x, y, width, height);
            if (quadrant == QuadrantAppliesEnum.NW)
            {
                branch.Nw = DeleteFromNode(x, y, width, height, branch.Nw, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.NE)
            {
                branch.Ne = DeleteFromNode(x, y, width, height, branch.Ne, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SW)
            {
                branch.Sw = DeleteFromNode(x, y, width, height, branch.Sw, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SE)
            {
                branch.Se = DeleteFromNode(x, y, width, height, branch.Se, tree);
            }
            else if (quadrant == QuadrantAppliesEnum.SOME)
            {
                var removed = DeleteFromData(x, y, width, height, branch.Data);
                if (removed)
                {
                    branch.DecCount();
                    if (branch.Count == 0)
                    {
                        branch.Data = null;
                    }
                }
            }

            if (!(branch.Nw is MXCIFQuadTreeNodeLeaf<object>) || 
                !(branch.Ne is MXCIFQuadTreeNodeLeaf<object>) || 
                !(branch.Sw is MXCIFQuadTreeNodeLeaf<object>) || 
                !(branch.Se is MXCIFQuadTreeNodeLeaf<object>))
            {
                return branch;
            }
            var nwLeaf = (MXCIFQuadTreeNodeLeaf<object>)branch.Nw;
            var neLeaf = (MXCIFQuadTreeNodeLeaf<object>)branch.Ne;
            var swLeaf = (MXCIFQuadTreeNodeLeaf<object>)branch.Sw;
            var seLeaf = (MXCIFQuadTreeNodeLeaf<object>)branch.Se;
            var total = nwLeaf.Count + neLeaf.Count + swLeaf.Count + seLeaf.Count + branch.Count;
            if (total >= tree.LeafCapacity)
            {
                return branch;
            }

            ICollection<XYWHRectangleWValue<L>> collection = new LinkedList<>();
            var count = MergeChildNodes(collection, branch.Data);
            count += MergeChildNodes(collection, nwLeaf.Data);
            count += MergeChildNodes(collection, neLeaf.Data);
            count += MergeChildNodes(collection, swLeaf.Data);
            count += MergeChildNodes(collection, seLeaf.Data);
            return new MXCIFQuadTreeNodeLeaf<object>(branch.Bb, branch.Level, collection, count);
        }

        private static bool DeleteFromData<L>(double x, double y, double width, double height, object data)
        {
            if (data == null)
            {
                return false;
            }
            if (!(data is ICollection<XYWHRectangleWValue<L>>))
            {
                XYWHRectangleWValue<L> rectangle = (XYWHRectangleWValue<L>)data;
                return rectangle.CoordinateEquals(x, y, width, height);
            }
            ICollection<XYWHRectangleWValue<L>> collection = (ICollection<XYWHRectangleWValue<L>>)data;
            foreach (var rectangles in collection)
            {
                if (rectangles.CoordinateEquals(x, y, width, height))
                {
                    it.Remove();
                    return true;
                }
            }
            return false;
        }

        private static int MergeChildNodes<L>(ICollection<XYWHRectangleWValue<L>> target, object data)
        {
            if (data == null)
            {
                return 0;
            }
            if (data is XYWHRectangleWValue<L>)
            {
                XYWHRectangleWValue<L> p = (XYWHRectangleWValue<L>)data;
                target.Add(p);
                return 1;
            }
            ICollection<XYWHRectangleWValue<L>> coll = (ICollection<XYWHRectangleWValue<L>>)data;
            target.AddAll(coll);
            return coll.Count;
        }
    }
} // end of namespace