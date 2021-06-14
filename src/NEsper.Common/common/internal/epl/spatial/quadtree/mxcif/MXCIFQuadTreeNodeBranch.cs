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
    public class MXCIFQuadTreeNodeBranch : MXCIFQuadTreeNode
    {
        public MXCIFQuadTreeNode Ne { get; internal set; }
        public MXCIFQuadTreeNode Nw { get; internal set; }
        public MXCIFQuadTreeNode Se { get; internal set; }
        public MXCIFQuadTreeNode Sw { get; internal set; }

        public MXCIFQuadTreeNodeBranch(
            BoundingBox bb,
            int level,
            object data,
            int dataCount,
            MXCIFQuadTreeNode nw,
            MXCIFQuadTreeNode ne,
            MXCIFQuadTreeNode sw,
            MXCIFQuadTreeNode se)
            : base(bb, level, data, dataCount)
        {
            Nw = nw;
            Ne = ne;
            Sw = sw;
            Se = se;
        }
    }
} // end of namespace