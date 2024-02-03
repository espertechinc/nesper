///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion
{
    public abstract class PointRegionQuadTreeNodeLeafOpaque : PointRegionQuadTreeNode
    {
        public PointRegionQuadTreeNodeLeafOpaque(
            BoundingBox bb,
            int level) : base(bb, level)
        {
        }

        public abstract object OpaquePoints { get; }
    }
}