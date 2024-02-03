///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexEmpty
    {
        public static bool IsEmpty(MXCIFQuadTree quadTree)
        {
            return IsEmpty(quadTree.Root);
        }

        public static bool IsEmpty(MXCIFQuadTreeNode node)
        {
            if (node is MXCIFQuadTreeNodeLeaf leaf) {
                return leaf.Data == null;
            }

            return false;
        }
    }
} // end of namespace