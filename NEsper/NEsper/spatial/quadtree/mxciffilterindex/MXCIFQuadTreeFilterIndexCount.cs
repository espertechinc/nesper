///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.filter;
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexCount
    {
        public static int Count(MXCIFQuadTree<Object> quadTree)
        {
            return Count(quadTree.Root);
        }

        private static int Count(MXCIFQuadTreeNode<Object> node)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object> leaf)
            {
                return CountData(leaf.Data);
            }

            MXCIFQuadTreeNodeBranch<Object> branch = (MXCIFQuadTreeNodeBranch<Object>) node;
            return Count(branch.Nw) + Count(branch.Ne) + Count(branch.Sw) + Count(branch.Se) + CountData(branch.Data);
        }

        private static int CountData(Object data)
        {
            if (data == null)
            {
                return 0;
            }

            if (data is XYWHRectangleWValue<object>)
            {
                return CountCallbacks(data);
            }

            var coll = (ICollection<XYWHRectangleWValue<object>>) data;
            int count = 0;
            foreach (var p in coll)
            {
                count += CountCallbacks(p.Value);
            }

            return count;
        }

        private static int CountCallbacks(Object points)
        {
            if (points is FilterHandleSetNode)
            {
                return ((FilterHandleSetNode) points).FilterCallbackCount;
            }

            if (points is FilterParamIndexBase)
            {
                return ((FilterParamIndexBase) points).Count;
            }

            return 1;
        }
    }
} // end of namespace
