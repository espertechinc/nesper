///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.spatial.quadtree.core;
using com.espertech.esper.spatial.quadtree.pointregion;

//import static com.espertech.esper.spatial.quadtree.prqdfilterindex.PointRegionQuadTreeFilterIndexCheckBB.checkBB;

namespace com.espertech.esper.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexDelete<TL>
    {
        public static void Delete(double x, double y, PointRegionQuadTree<object> tree)
        {
            var root = tree.Root;
            PointRegionQuadTreeFilterIndexCheckBB.CheckBB(root.Bb, x, y);
            var replacement = DeleteFromNode(x, y, root, tree);
            tree.Root = replacement;
        }

        private static PointRegionQuadTreeNode DeleteFromNode(
            double x, double y,
            PointRegionQuadTreeNode node,
            PointRegionQuadTree<object> tree)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object> leaf)
            {
                var removed = DeleteFromPoints(x, y, leaf.Points);
                if (removed)
                {
                    leaf.DecCount();
                    if (leaf.Count == 0) leaf.Points = null;
                }

                return leaf;
            }

            var branch = (PointRegionQuadTreeNodeBranch) node;
            var quadrant = node.Bb.GetQuadrant(x, y);
            if (quadrant == QuadrantEnum.NW)
                branch.Nw = DeleteFromNode(x, y, branch.Nw, tree);
            else if (quadrant == QuadrantEnum.NE)
                branch.Ne = DeleteFromNode(x, y, branch.Ne, tree);
            else if (quadrant == QuadrantEnum.SW)
                branch.Sw = DeleteFromNode(x, y, branch.Sw, tree);
            else
                branch.Se = DeleteFromNode(x, y, branch.Se, tree);

            if (!(branch.Nw is PointRegionQuadTreeNodeLeaf<object> nwLeaf) ||
                !(branch.Ne is PointRegionQuadTreeNodeLeaf<object> neLeaf) ||
                !(branch.Sw is PointRegionQuadTreeNodeLeaf<object> swLeaf) ||
                !(branch.Se is PointRegionQuadTreeNodeLeaf<object> seLeaf))
                return branch;

            var total = nwLeaf.Count + neLeaf.Count + swLeaf.Count + seLeaf.Count;
            if (total >= tree.LeafCapacity) return branch;

            var collection = new List<XYPointWValue<TL>>();
            var count = MergeChildNodes(collection, nwLeaf.Points);
            count += MergeChildNodes(collection, neLeaf.Points);
            count += MergeChildNodes(collection, swLeaf.Points);
            count += MergeChildNodes(collection, seLeaf.Points);
            return new PointRegionQuadTreeNodeLeaf<object>(branch.Bb, branch.Level, collection, count);
        }

        private static bool DeleteFromPoints(double x, double y, object points)
        {
            XYPointWValue<TL> point;

            if (points == null) return false;
            if (points is ICollection<XYPointWValue<TL>> collection)
            {
                var enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    point = enumerator.Current;
                    if (point.X == x && point.Y == y)
                    {
                        collection.Remove(point);
                        return true;
                    }
                }

                return false;
            }

            point = (XYPointWValue<TL>) points;
            return point.X == x && point.Y == y;
        }

        private static int MergeChildNodes(ICollection<XYPointWValue<TL>> target, object points)
        {
            if (points == null) return 0;
            if (points is XYPointWValue<TL>)
            {
                var p = (XYPointWValue<TL>) points;
                target.Add(p);
                return 1;
            }

            var coll = (ICollection<XYPointWValue<TL>>) points;
            foreach (var p in coll) target.Add(p);
            return coll.Count;
        }
    }
} // end of namespace