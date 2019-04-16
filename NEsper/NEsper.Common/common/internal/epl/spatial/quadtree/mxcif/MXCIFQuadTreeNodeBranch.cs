///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif
{
    public class MXCIFQuadTreeNodeBranch<L> : MXCIFQuadTreeNode<L>
    {
        internal MXCIFQuadTreeNode<L> Ne;
        internal MXCIFQuadTreeNode<L> Nw;
        internal MXCIFQuadTreeNode<L> Se;
        internal MXCIFQuadTreeNode<L> Sw;

        public MXCIFQuadTreeNodeBranch(
            BoundingBox bb,
            int level,
            L data,
            int dataCount,
            MXCIFQuadTreeNode<L> nw,
            MXCIFQuadTreeNode<L> ne,
            MXCIFQuadTreeNode<L> sw,
            MXCIFQuadTreeNode<L> se)
            : base(bb, level, data, dataCount)
        {
            Nw = nw;
            Ne = ne;
            Sw = sw;
            Se = se;
        }
    }
} // end of namespace