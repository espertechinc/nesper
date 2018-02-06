///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.spatial.quadtree.mxciffilterindex
{
    public class MXCIFQuadTreeFilterIndexEmpty
    {
        public static bool IsEmpty(MXCIFQuadTree<object> quadTree)
        {
            return IsEmpty(quadTree.Root);
        }

        public static bool IsEmpty(MXCIFQuadTreeNode<object> node)
        {
            if (node is MXCIFQuadTreeNodeLeaf<object> leaf)
            {
                return leaf.Data == null;
            }

            return false;
        }
    }
} // end of namespace
