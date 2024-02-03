///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexCount
    {
        public static int Count(MXCIFQuadTree quadTree)
        {
            return Count(quadTree.Root);
        }

        private static int Count(MXCIFQuadTreeNode node)
        {
            if (node is MXCIFQuadTreeNodeLeaf leaf) {
                return CountData(leaf.Data);
            }

            var branch = (MXCIFQuadTreeNodeBranch)node;
            return Count(branch.Nw) + Count(branch.Ne) + Count(branch.Sw) + Count(branch.Se) + CountData(branch.Data);
        }

        private static int CountData(object data)
        {
            if (data == null) {
                return 0;
            }

            if (data is XYWHRectangleWValue) {
                return CountCallbacks(data);
            }
            else if (data is ICollection<XYWHRectangleWValue> collection) {
                var count = 0;
                foreach (var p in collection) {
                    count += CountCallbacks(p.Value);
                }

                return count;
            }
            else {
                var actualType = data.GetType().FullName;
                throw new IllegalStateException($"unknown type \"{actualType}\" for data");
            }
        }

        private static int CountCallbacks(object points)
        {
            if (points is FilterHandleSize size) {
                return size.FilterCallbackCount;
            }

            return 1;
        }
    }
} // end of namespace