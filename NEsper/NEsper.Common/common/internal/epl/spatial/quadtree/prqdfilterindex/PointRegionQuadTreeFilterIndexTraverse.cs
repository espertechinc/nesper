///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class PointRegionQuadTreeFilterIndexTraverse
    {
        public static void Traverse(
            PointRegionQuadTree<object> quadtree,
            Consumer<object> consumer)
        {
            Traverse(quadtree.Root, consumer);
        }

        public static void Traverse(
            PointRegionQuadTreeNode node,
            Consumer<object> consumer)
        {
            if (node is PointRegionQuadTreeNodeLeaf<object>) {
                var leaf = (PointRegionQuadTreeNodeLeaf<object>) node;
                TraverseData(leaf.Points, consumer);
                return;
            }

            var branch = (PointRegionQuadTreeNodeBranch) node;
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

            if (!(data is ICollection<object>)) {
                Visit(data, consumer);
                return;
            }

            ICollection<object> collection = (ICollection<object>) data;
            foreach (var datapoint in collection) {
                Visit(datapoint, consumer);
            }
        }

        private static void Visit(
            object data,
            Consumer<object> consumer)
        {
            if (data is XYPointWValue<object>) {
                consumer.Invoke(((XYPointWValue<object>) data).Value);
            }
            else if (data is XYPointMultiType) {
                var multiType = (XYPointMultiType) data;
                if (multiType.Multityped is ICollection<object>) {
                    ICollection<object> collection = (ICollection<object>) multiType.Multityped;
                    foreach (var datapoint in collection) {
                        Visit(datapoint, consumer);
                    }
                }
            }
        }
    }
} // end of namespace