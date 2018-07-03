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
        public static int Count<TL>(MXCIFQuadTree<TL> quadTree)
        {
            return Count(quadTree.Root);
        }

        private static int Count<TL>(MXCIFQuadTreeNode<TL> node)
        {
            if (node is MXCIFQuadTreeNodeLeaf<TL> leaf)
            {
                return CountData<TL>(leaf.Data);
            }

            var branch = (MXCIFQuadTreeNodeBranch<TL>) node;
            return Count(branch.Nw) +
                   Count(branch.Ne) + 
                   Count(branch.Sw) + 
                   Count(branch.Se) + 
                   CountData<TL>(branch.Data);
        }

        private static int CountData<TL>(Object data)
        {
            if (data == null)
            {
                return 0;
            }

            if (data is XYWHRectangleWValue<TL>)
            {
                return CountCallbacks(data);
            }

            var coll = (ICollection<XYWHRectangleWValue<TL>>) data;
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
