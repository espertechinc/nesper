///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.spatial.quadtree.mxcifrowindex
{
    public class MXCIFQuadTreeRowIndexRemove
    {
        /// <summary>
        /// Remove value.
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="value">value to remove</param>
        /// <param name="tree">quadtree</param>
        public static void Remove(double x, double y, double width, double height, object value,
            MXCIFQuadTree<object> tree)
        {
            var root = tree.Root;
            var replacement = RemoveFromNode(x, y, width, height, value, root, tree);
            tree.Root = replacement;
        }

        private static MXCIFQuadTreeNode<object> RemoveFromNode(double x, double y, double width, double height,
            object value, MXCIFQuadTreeNode<object> node, MXCIFQuadTree<object> tree)
        {

            if (node is MXCIFQuadTreeNodeLeaf<object> leaf)
            {
                var removed = RemoveFromPoints(x, y, width, height, value, leaf.Data);
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

            var branch = (MXCIFQuadTreeNodeBranch<object>) node;
            var quadrant = node.Bb.GetQuadrantApplies(x, y, width, height);
            switch (quadrant)
            {
                case QuadrantAppliesEnum.NW:
                    branch.Nw = RemoveFromNode(x, y, width, height, value, branch.Nw, tree);
                    break;
                case QuadrantAppliesEnum.NE:
                    branch.Ne = RemoveFromNode(x, y, width, height, value, branch.Ne, tree);
                    break;
                case QuadrantAppliesEnum.SW:
                    branch.Sw = RemoveFromNode(x, y, width, height, value, branch.Sw, tree);
                    break;
                case QuadrantAppliesEnum.SE:
                    branch.Se = RemoveFromNode(x, y, width, height, value, branch.Se, tree);
                    break;
                case QuadrantAppliesEnum.SOME:
                    var removed = RemoveFromPoints(x, y, width, height, value, branch.Data);
                    if (removed)
                    {
                        branch.DecCount();
                        if (branch.Count == 0)
                        {
                            branch.Data = null;
                        }
                    }

                    break;
            }

            if (!(branch.Nw is MXCIFQuadTreeNodeLeaf<object>) ||
                !(branch.Ne is MXCIFQuadTreeNodeLeaf<object>) ||
                !(branch.Sw is MXCIFQuadTreeNodeLeaf<object>) ||
                !(branch.Se is MXCIFQuadTreeNodeLeaf<object>))
            {
                return branch;
            }

            var nwLeaf = (MXCIFQuadTreeNodeLeaf<object>) branch.Nw;
            var neLeaf = (MXCIFQuadTreeNodeLeaf<object>) branch.Ne;
            var swLeaf = (MXCIFQuadTreeNodeLeaf<object>) branch.Sw;
            var seLeaf = (MXCIFQuadTreeNodeLeaf<object>) branch.Se;
            var total = branch.Count + nwLeaf.Count + neLeaf.Count + swLeaf.Count + seLeaf.Count;
            if (total >= tree.LeafCapacity)
            {
                return branch;
            }

            var collection = new LinkedList<XYWHRectangleMultiType>();
            var count = MergeChildNodes(collection, branch.Data);
            count += MergeChildNodes(collection, nwLeaf.Data);
            count += MergeChildNodes(collection, neLeaf.Data);
            count += MergeChildNodes(collection, swLeaf.Data);
            count += MergeChildNodes(collection, seLeaf.Data);
            return new MXCIFQuadTreeNodeLeaf<object>(branch.Bb, branch.Level, collection, count);
        }

        private static bool RemoveFromPoints(double x, double y, double width, double height, object value, object data)
        {
            if (data == null)
            {
                return false;
            }

            if (!(data is ICollection<XYWHRectangleMultiType> collection))
            {
                var rectangle = (XYWHRectangleMultiType) data;
                if (rectangle.CoordinateEquals(x, y, width, height))
                {
                    var removed = rectangle.Remove(value);
                    if (removed)
                    {
                        return true;
                    }
                }

                return false;
            }

            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var rectangle = enumerator.Current;
                if (rectangle.CoordinateEquals(x, y, width, height))
                {
                    var removed = rectangle.Remove(value);
                    if (removed)
                    {
                        if (rectangle.IsEmpty())
                        {
                            collection.Remove(rectangle);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private static int MergeChildNodes(ICollection<XYWHRectangleMultiType> target, object data)
        {
            if (data == null)
            {
                return 0;
            }

            if (data is XYWHRectangleMultiType dataR)
            {
                target.Add(dataR);
                return dataR.Count();
            }

            var coll = (ICollection<XYWHRectangleMultiType>) data;
            var total = 0;
            foreach (var r in coll)
            {
                target.Add(r);
                total += r.Count();
            }

            return total;
        }
    }
} // end of namespace
