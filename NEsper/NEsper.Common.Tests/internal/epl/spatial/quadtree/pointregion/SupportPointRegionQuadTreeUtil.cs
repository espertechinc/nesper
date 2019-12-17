///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion
{
    public class SupportPointRegionQuadTreeUtil
    {
        public static readonly SupportQuadTreeUtil.Factory<PointRegionQuadTree<object>> POINTREGION_FACTORY = config =>
            PointRegionQuadTreeFactory<object>.Make(
                config.X,
                config.Y,
                config.Width,
                config.Height,
                config.LeafCapacity,
                config.MaxTreeHeight);

        public static string PrintPoint(
            double x,
            double y)
        {
            return "(" + x + "," + y + ")";
        }

        public static PointRegionQuadTreeNodeLeaf<object> NavigateLeaf(
            PointRegionQuadTree<object> tree,
            string directions)
        {
            return (PointRegionQuadTreeNodeLeaf<object>) Navigate(tree, directions);
        }

        public static PointRegionQuadTreeNodeLeaf<object> NavigateLeaf(
            PointRegionQuadTreeNode node,
            string directions)
        {
            return (PointRegionQuadTreeNodeLeaf<object>) Navigate(node, directions);
        }

        public static PointRegionQuadTreeNodeBranch NavigateBranch(
            PointRegionQuadTree<object> tree,
            string directions)
        {
            return (PointRegionQuadTreeNodeBranch) Navigate(tree, directions);
        }

        public static PointRegionQuadTreeNode Navigate(
            PointRegionQuadTree<object> tree,
            string directions)
        {
            return Navigate(tree.Root, directions);
        }

        public static PointRegionQuadTreeNode Navigate(
            PointRegionQuadTreeNode current,
            string directions)
        {
            if (string.IsNullOrEmpty(directions))
            {
                return current;
            }

            var split = directions.SplitCsv();
            for (var i = 0; i < split.Length; i++)
            {
                var branch = (PointRegionQuadTreeNodeBranch) current;
                switch (split[i])
                {
                    case "nw":
                        current = branch.Nw;
                        break;

                    case "ne":
                        current = branch.Ne;
                        break;

                    case "sw":
                        current = branch.Sw;
                        break;

                    case "se":
                        current = branch.Se;
                        break;

                    default:
                        throw new ArgumentException("Invalid direction " + split[i]);
                }
            }

            return current;
        }
    }
} // end of namespace
