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

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif
{
    public class SupportMXCIFQuadTreeUtil
    {
        public static readonly SupportQuadTreeUtil.Factory<MXCIFQuadTree<object>> MXCIF_FACTORY = config =>
            MXCIFQuadTreeFactory<object>.Make(
                config.X,
                config.Y,
                config.Width,
                config.Height,
                config.LeafCapacity,
                config.MaxTreeHeight);

        public static MXCIFQuadTreeNodeLeaf<object> NavigateLeaf(
            MXCIFQuadTree<object> tree,
            string directions)
        {
            return (MXCIFQuadTreeNodeLeaf<object>) Navigate(tree, directions);
        }

        public static MXCIFQuadTreeNodeLeaf<object> NavigateLeaf(
            MXCIFQuadTreeNode<object> node,
            string directions)
        {
            return (MXCIFQuadTreeNodeLeaf<object>) Navigate(node, directions);
        }

        public static MXCIFQuadTreeNodeBranch<object> NavigateBranch(
            MXCIFQuadTree<object> tree,
            string directions)
        {
            return (MXCIFQuadTreeNodeBranch<object>) Navigate(tree, directions);
        }

        public static MXCIFQuadTreeNode<object> Navigate(
            MXCIFQuadTree<object> tree,
            string directions)
        {
            return Navigate(tree.Root, directions);
        }

        public static MXCIFQuadTreeNode<object> Navigate(
            MXCIFQuadTreeNode<object> current,
            string directions)
        {
            if (string.IsNullOrEmpty(directions))
            {
                return current;
            }

            var split = directions.SplitCsv();
            for (var i = 0; i < split.Length; i++) {
                var branch = (MXCIFQuadTreeNodeBranch<object>) current;
                switch (split[i]) {
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
