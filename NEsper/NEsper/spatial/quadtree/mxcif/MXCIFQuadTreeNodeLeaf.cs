///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.spatial.quadtree.core;

namespace com.espertech.esper.spatial.quadtree.mxcif
{
    internal class MXCIFQuadTreeNodeLeaf<L> : MXCIFQuadTreeNode<L>
    {
        internal MXCIFQuadTreeNodeLeaf(BoundingBox bb, int level, L data, int dataCount)
            : base(bb, level, data, dataCount)
        {
        }
    }
} // end of namespace