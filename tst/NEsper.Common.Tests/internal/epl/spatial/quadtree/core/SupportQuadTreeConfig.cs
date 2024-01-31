///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportQuadTreeConfig
    {
        public SupportQuadTreeConfig(
            double x,
            double y,
            double width,
            double height,
            int leafCapacity,
            int maxTreeHeight)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            LeafCapacity = leafCapacity;
            MaxTreeHeight = maxTreeHeight;
        }

        public double X { get; }

        public double Y { get; }

        public double Width { get; }

        public double Height { get; }

        public int LeafCapacity { get; }

        public int MaxTreeHeight { get; }

        public double MaxX => X + Width;

        public double MaxY => Y + Height;
    }
} // end of namespace
