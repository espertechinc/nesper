///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexCount
    {
        public static int Count(MXCIFQuadTree<object> quadTree)
        {
            return Count(quadTree.Root);
        }

        private static int Count(MXCIFQuadTreeNode<object> node)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object>)
            {
                var leaf = (MXCIFQuadTreeNodeLeaf<object>)node;
                return CountData(leaf.Data);
            }
            var branch = (MXCIFQuadTreeNodeBranch<object>)node;
            return Count(branch.Nw) + Count(branch.Ne) + Count(branch.Sw) + Count(branch.Se) + CountData(branch.Data);
        }

        private static int CountData(object data)
        {
            if (data == null)
            {
                return 0;
            }
            if (data is XYWHRectangleWValue<object>)
            {
                return CountCallbacks(data);
            }
            ICollection<XYWHRectangleWValue<object>> coll = (ICollection<XYWHRectangleWValue<object>>)data;
            int count = 0;
            foreach (XYWHRectangleWValue<object> p in coll)
            {
                count += CountCallbacks(p.Value);
            }
            return count;
        }

        private static int CountCallbacks(object points)
        {
            if (points is FilterHandleSize)
            {
                return ((FilterHandleSize)points).FilterCallbackCount;
            }
            return 1;
        }
    }
} // end of namespace