///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexTraverse
    {
        public static void Traverse(
            MXCIFQuadTree quadtree,
            Consumer<object> consumer)
        {
            Traverse(quadtree.Root, consumer);
        }

        public static void Traverse(
            MXCIFQuadTreeNode node,
            Consumer<object> consumer)
        {
            if (node is MXCIFQuadTreeNodeLeaf leaf) {
                TraverseData(leaf.Data, consumer);
                return;
            }

            var branch = (MXCIFQuadTreeNodeBranch)node;
            TraverseData(branch.Data, consumer);
            Traverse(branch.Nw, consumer);
            Traverse(branch.Ne, consumer);
            Traverse(branch.Sw, consumer);
            Traverse(branch.Se, consumer);
        }

        private static void TraverseData(
            object data,
            Consumer<object> consumer)
        {
            if (data == null) {
                return;
            }

            if (!(data is ICollection<object> collection)) {
                Visit(data, consumer);
                return;
            }

            foreach (var datapoint in collection) {
                Visit(datapoint, consumer);
            }
        }

        private static void Visit(
            object data,
            Consumer<object> consumer)
        {
            if (data is XYWHRectangleWValue value) {
                consumer.Invoke(value.Value);
            }
            else {
                var multiType = data as XYWHRectangleMultiType;
                if (multiType?.Multityped is ICollection<object> collection) {
                    foreach (var datapoint in collection) {
                        Visit(datapoint, consumer);
                    }
                }
            }
        }
    }
} // end of namespace