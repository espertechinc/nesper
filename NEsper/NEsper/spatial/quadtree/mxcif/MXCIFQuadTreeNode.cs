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
    public abstract class MXCIFQuadTreeNode<TL>
    {
        protected MXCIFQuadTreeNode(BoundingBox bb, int level, TL data, int count)
        {
            Bb = bb;
            Level = level;
            Data = data;
            Count = count;
        }

        internal BoundingBox Bb; // { get; }
        internal int Level; // { get; }
        internal TL Data; // { get; set; }
        internal int Count; // { get; set; }

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