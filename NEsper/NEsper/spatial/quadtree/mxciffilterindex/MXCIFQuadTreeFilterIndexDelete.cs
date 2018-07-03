///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.mxcif;
using com.espertech.esper.spatial.quadtree.mxcifrowindex;

// import static com.espertech.esper.spatial.quadtree.mxcifrowindex.MXCIFQuadTreeFilterIndexCheckBB.checkBB;

namespace com.espertech.esper.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexDelete<TL>
    {
        public static void Delete(double x, double y, double width, double height, MXCIFQuadTree<object> tree)
        {
            MXCIFQuadTreeNode<object> root = tree.Root;
            MXCIFQuadTreeFilterIndexCheckBB.CheckBB(root.Bb, x, y, width, height);
            tree.Root = DeleteFromNode(x, y, width, height, root, tree);
        }

        private static MXCIFQuadTreeNode<object> DeleteFromNode(
            double x, double y, 
            double width, double height,
            MXCIFQuadTreeNode<object> node, 
            MXCIFQuadTree<object> tree)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object> leaf)
            {
                var removed = DeleteFromData(x, y, width, height, leaf.Data);
                if (removed)
                {
                    leaf.DecCount();
                    if (leaf.Count == 0) leaf.Data = null;
                }

                return leaf;
            }

            var branch = (MXCIFQuadTreeNodeBranch<object>) node;
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
                    if (branch.Count == 0) branch.Data = null;
                }
            }

            if (!(branch.Nw is MXCIFQuadTreeNodeLeaf<object> nwLeaf) ||
                !(branch.Ne is MXCIFQuadTreeNodeLeaf<object> neLeaf) ||
                !(branch.Sw is MXCIFQuadTreeNodeLeaf<object> swLeaf) ||
                !(branch.Se is MXCIFQuadTreeNodeLeaf<object> seLeaf))
                return branch;

            var total = nwLeaf.Count + neLeaf.Count + swLeaf.Count + seLeaf.Count + branch.Count;
            if (total >= tree.LeafCapacity) return branch;

            var collection = new LinkedList<XYWHRectangleWValue<TL>>();
            var count = MergeChildNodes(collection, branch.Data);
            count += MergeChildNodes(collection, nwLeaf.Data);
            count += MergeChildNodes(collection, neLeaf.Data);
            count += MergeChildNodes(collection, swLeaf.Data);
            count += MergeChildNodes(collection, seLeaf.Data);
            return new MXCIFQuadTreeNodeLeaf<object>(branch.Bb, branch.Level, collection, count);
        }

        private static bool DeleteFromData(double x, double y, double width, double height, object data)
        {
            if (data == null) return false;
            if (!(data is ICollection<XYWHRectangleWValue<TL>>))
            {
                var rectangle = (XYWHRectangleWValue<TL>) data;
                return rectangle.CoordinateEquals(x, y, width, height);
            }

            var collection = (ICollection<XYWHRectangleWValue<TL>>) data;
            foreach(var rectangles in collection)
            {
                if (rectangles.CoordinateEquals(x, y, width, height))
                {
                    collection.Remove(rectangles);
                    return true;
                }
            }

            return false;
        }

        private static int MergeChildNodes(ICollection<XYWHRectangleWValue<TL>> target, object data)
        {
            if (data == null) return 0;
            if (data is XYWHRectangleWValue<TL> dataRect)
            {
                target.Add(dataRect);
                return 1;
            }

            var coll = (ICollection<XYWHRectangleWValue<TL>>) data;
            target.AddAll(coll);
            return coll.Count;
        }
    }
} // end of namespace