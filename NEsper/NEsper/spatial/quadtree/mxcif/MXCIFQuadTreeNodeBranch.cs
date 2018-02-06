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
    public class MXCIFQuadTreeNodeBranch<L> : MXCIFQuadTreeNode<L>
    {
        public MXCIFQuadTreeNodeBranch(
            BoundingBox bb, int level, L data, int dataCount, 
            MXCIFQuadTreeNode<L> nw, MXCIFQuadTreeNode<L> ne, MXCIFQuadTreeNode<L> sw, MXCIFQuadTreeNode<L> se)
            : base(bb, level, data, dataCount)
        {
            Nw = nw;
            Ne = ne;
            Sw = sw;
            Se = se;
        }

        public MXCIFQuadTreeNode<L> Nw { get; set; }

        public MXCIFQuadTreeNode<L> Ne { get; set; }

        public MXCIFQuadTreeNode<L> Sw { get; set; }

        public MXCIFQuadTreeNode<L> Se { get; set; }
    }
} // end of namespace