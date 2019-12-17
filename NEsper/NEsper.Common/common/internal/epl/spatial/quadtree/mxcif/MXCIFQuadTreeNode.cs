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
    public abstract class MXCIFQuadTreeNode
    {
        protected MXCIFQuadTreeNode(
            BoundingBox bb,
            int level,
            object data,
            int count)
        {
            Bb = bb;
            Level = level;
            Data = data;
            Count = count;
        }

        public BoundingBox Bb { get; }
        public int Level { get; }
        public object Data;
        public int Count;

        public void IncCount(int numAdded)
        {
            Count += numAdded;
        }

        public void DecCount()
        {
            Count--;
        }
    }
} // end of namespace