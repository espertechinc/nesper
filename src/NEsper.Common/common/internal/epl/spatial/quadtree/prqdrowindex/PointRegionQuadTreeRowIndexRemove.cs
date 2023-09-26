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

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    public class PointRegionQuadTreeRowIndexRemove
    {
        /// <summary>
        /// Remove value.
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="value">value to remove</param>
        /// <param name="tree">quadtree</param>
        public static void Remove(
            double x,
            double y,
            object value,
            PointRegionQuadTree<object> tree)
        {
            var root = tree.Root;
            var replacement = RemoveFromNode(x, y, value, root, tree);
            tree.Root = replacement;
        }

        private static PointRegionQuadTreeNode RemoveFromNode(
            double x,
            double y,
            object value,
            PointRegionQuadTreeNode node,
            PointRegionQuadTree<object> tree)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object> leaf) {
                var removed = RemoveFromPoints(x, y, value, leaf.Points);
                if (removed) {
                    leaf.DecCount();
                    if (leaf.Count == 0) {
                        leaf.Points = null;
                    }
                }

                return leaf;
            }

            var branch = (PointRegionQuadTreeNodeBranch)node;
            var quadrant = node.Bb.GetQuadrant(x, y);
            switch (quadrant) {
                case QuadrantEnum.NW:
                    branch.Nw = RemoveFromNode(x, y, value, branch.Nw, tree);
                    break;

                case QuadrantEnum.NE:
                    branch.Ne = RemoveFromNode(x, y, value, branch.Ne, tree);
                    break;

                case QuadrantEnum.SW:
                    branch.Sw = RemoveFromNode(x, y, value, branch.Sw, tree);
                    break;

                default:
                    branch.Se = RemoveFromNode(x, y, value, branch.Se, tree);
                    break;
            }

            if (!(branch.Nw is PointRegionQuadTreeNodeLeaf<object> nwLeaf) ||
                !(branch.Ne is PointRegionQuadTreeNodeLeaf<object> neLeaf) ||
                !(branch.Sw is PointRegionQuadTreeNodeLeaf<object> swLeaf) ||
                !(branch.Se is PointRegionQuadTreeNodeLeaf<object> seLeaf)) {
                return branch;
            }

            var total = nwLeaf.Count + neLeaf.Count + swLeaf.Count + seLeaf.Count;
            if (total >= tree.LeafCapacity) {
                return branch;
            }

            var collection = new LinkedList<XYPointMultiType>();
            var count = MergeChildNodes(collection, nwLeaf.Points);
            count += MergeChildNodes(collection, neLeaf.Points);
            count += MergeChildNodes(collection, swLeaf.Points);
            count += MergeChildNodes(collection, seLeaf.Points);
            return new PointRegionQuadTreeNodeLeaf<object>(branch.Bb, branch.Level, collection, count);
        }

        private static bool RemoveFromPoints(
            double x,
            double y,
            object value,
            object points)
        {
            if (points == null) {
                return false;
            }

            if (!(points is ICollection<XYPointMultiType> collection)) {
                var point = (XYPointMultiType)points;
                if (point.X == x && point.Y == y) {
                    var removed = point.Remove(value);
                    if (removed) {
                        return true;
                    }
                }

                return false;
            }

            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext()) {
                var point = enumerator.Current;
                if (point.X == x && point.Y == y) {
                    var removed = point.Remove(value);
                    if (removed) {
                        if (point.IsEmpty()) {
                            collection.Remove(point);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private static int MergeChildNodes(
            ICollection<XYPointMultiType> target,
            object points)
        {
            if (points == null) {
                return 0;
            }

            if (points is XYPointMultiType p1) {
                target.Add(p1);
                return p1.Count();
            }

            var coll = (ICollection<XYPointMultiType>)points;
            var total = 0;
            foreach (var p in coll) {
                target.Add(p);
                total += p.Count();
            }

            return total;
        }
    }
} // end of namespace